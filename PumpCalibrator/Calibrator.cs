using MCP.Equipment;
using MCP.Measurements;
using MCP.Curves;
using MCP.Protocol;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TCD;
using TCD.Controls;

namespace PumpCalibrator
{
    public class Calibrator : PropertyChangedBase
    {
        #region Commands
        private RelayCommand _StartCalibrationCommand;
        public RelayCommand StartCalibrationCommand { get { return _StartCalibrationCommand; } set { _StartCalibrationCommand = value; OnPropertyChanged(); } }
        
			
        private RelayCommand _AbortCalibrationCommand;
        public RelayCommand AbortCalibrationCommand { get { return _AbortCalibrationCommand; } set { _AbortCalibrationCommand = value; OnPropertyChanged(); } }
        
			
        #endregion

        private ObservableCollection<Subcalibration> _Subcalibrations = new ObservableCollection<Subcalibration>();
        public ObservableCollection<Subcalibration> Subcalibrations { get { return _Subcalibrations; } set { _Subcalibrations = value; OnPropertyChanged(); } }

        private CalibrationFluid _CalibrationFluid = CalibrationFluid.Water;//TODO: actually implement this
        public CalibrationFluid CalibrationFluid { get { return _CalibrationFluid; } set { _CalibrationFluid = value; OnPropertyChanged(); } }
        
        private CalibrationTarget _CalibrationTarget = CalibrationTarget.Pump;
        public CalibrationTarget CalibrationTarget { get { return _CalibrationTarget; } set { _CalibrationTarget = value; OnPropertyChanged(); } }
        
        private CalibrationMode _CalibrationMode = CalibrationMode.Standard;
        public CalibrationMode CalibrationMode { get { return _CalibrationMode; } set { _CalibrationMode = value; OnPropertyChanged(); } }

        
        private Subcalibration _ActiveCalibrationSub;
        public Subcalibration ActiveCalibrationSub
        {
            get { return _ActiveCalibrationSub; }
            set
            {
                _ActiveCalibrationSub = value;
                OnPropertyChanged();
                StartCalibrationCommand.RaiseCanExecuteChanged();
                AbortCalibrationCommand.RaiseCanExecuteChanged();
            }
        }


        public double ProgressPercent
        {
            get
            {
                if (ActiveCalibrationSub == null)
                    return 0;
                int totalSeconds = 0;
                double completedSeconds = 0;
                foreach (Subcalibration sub in Subcalibrations)
                {
                    totalSeconds += sub.Duration;
                    completedSeconds += sub.Duration * sub.ProgressPercent / 100;
                }
                if (totalSeconds == 0)
                    return 0;
                return completedSeconds / totalSeconds * 100;
            }
        }

        public TimeSpan RemainingCalibrationTime
        {
            get
            {
                if (ActiveCalibrationSub == null)
                    return TimeSpan.FromSeconds(0);
                int totalSeconds = 0;
                double completedSeconds = 0;
                foreach (Subcalibration sub in Subcalibrations)
                {
                    totalSeconds += sub.Duration;
                    completedSeconds += sub.Duration * sub.ProgressPercent / 100;
                }
                return TimeSpan.FromSeconds(totalSeconds - completedSeconds);
            }
        }

        private DispatcherTimer progressTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
			

        public Calibrator()
        {
            StartCalibrationCommand = new RelayCommand(async delegate
                {
                    SpeechIO.Speak(string.Format("Starting {0} calibration.", CalibrationMode));
                    Subcalibrations.Clear();
                    foreach (int[] pair in CalibrationProfiles.Profiles[CalibrationTarget][CalibrationMode])
                        Subcalibrations.Add(new Subcalibration(pair[0], pair[1], CalibrationProfiles.Symbols[CalibrationTarget], CalibrationProfiles.Units[CalibrationTarget]));
                    foreach (Subcalibration sub in Subcalibrations)
                    {
                        ActiveCalibrationSub = sub;
                        if (!await sub.RunAsync())
                        {
                            SpeechIO.Speak("Calibration cancelled.");
                            ActiveCalibrationSub = null;
                            return;
                        }    
                    }
                    ActiveCalibrationSub = null;
                    SpeechIO.Speak("Calibration finished.");
                    PrepareResults();
                }, () => ActiveCalibrationSub == null);
            AbortCalibrationCommand = new RelayCommand(delegate
                {
                    foreach (Subcalibration sub in Subcalibrations)
                        sub.Abort();
                }, () => ActiveCalibrationSub != null);
            progressTimer.Tick += delegate
            {
                OnPropertyChanged("ProgressPercent");
                OnPropertyChanged("RemainingCalibrationTime");
            };
            progressTimer.Start();
        }
        private async void PrepareResults()
        {
            switch (CalibrationTarget)
            {
                case CalibrationTarget.Pump:
                    PumpInformation newPump = new PumpInformation();
                    foreach (Subcalibration sub in Subcalibrations)
                        newPump.ResponseCurve.Add(new ResponseData() { Setpoint = sub.Setpoint, Response = sub.AbsoluteChangePerHour });
                    PumpInformationWindow piw = new PumpInformationWindow("Save Pump Calibration", true, newPump);
                    piw.Show();
                    await piw.WaitTask;
                    if (piw.Confirmed)
                    {
                        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                        fbd.ShowDialog();
                        if (Directory.Exists(fbd.SelectedPath))
                        {
                            newPump.SaveTo(fbd.SelectedPath);
                        }
                    }
                    break;
                case CalibrationTarget.Stirrer:
                    System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                    sfd.AddExtension = true;
                    sfd.Filter = "log files (*.log)|*.log";
                    sfd.ShowDialog();
                    if (!string.IsNullOrWhiteSpace(sfd.FileName))
                    {
                        try
                        {
                            var writer = File.CreateText(sfd.FileName);
                            writer.WriteLine("Setpoint   [n]\tResponse   [rpm]");
                            foreach (Subcalibration sub in Subcalibrations)
                                writer.WriteLine(string.Format("{0}\t{1}", sub.Setpoint, sub.IncrementalChangePerMinute));
                            writer.Flush();
                            writer.Dispose();
                        }
                        catch
                        {

                        }
                    }
                    break;
            }
        }

        
        
    }
}