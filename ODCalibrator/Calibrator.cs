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
using MCP.NUI;

namespace ODCalibrator
{
    public class Calibrator : PropertyChangedBase
    {
        #region Commands
        private RelayCommand _StartOverCommand;
        public RelayCommand StartOverCommand { get { return _StartOverCommand; } set { _StartOverCommand = value; OnPropertyChanged(); } }
        private RelayCommand _FinalizeCommand;
        public RelayCommand FinalizeCommand { get { return _FinalizeCommand; } set { _FinalizeCommand = value; OnPropertyChanged(); } }
        #endregion

        private ObservableCollection<Subcalibration> _Subcalibrations = new ObservableCollection<Subcalibration>();
        public ObservableCollection<Subcalibration> Subcalibrations { get { return _Subcalibrations; } set { _Subcalibrations = value; OnPropertyChanged(); } }

        private CalibrationTarget _CalibrationTarget = CalibrationTarget.OD;
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
                StartOverCommand.RaiseCanExecuteChanged();
            }
        }



        private DispatcherTimer progressTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };
			

        public Calibrator()
        {
            StartOverCommand = new RelayCommand(delegate
                {
                    Subcalibrations.Clear();
                    foreach (double[] pair in CalibrationProfiles.Profiles[CalibrationTarget][CalibrationMode])
                        Subcalibrations.Add(new Subcalibration(pair[0], pair[1], CalibrationProfiles.Symbols[CalibrationTarget], CalibrationProfiles.Units[CalibrationTarget]) { Target = CalibrationTarget });
                    foreach (Subcalibration sub in Subcalibrations)
                    {
                        sub.RequestCapture += sub_RequestCapture;
                        sub.CaptureEnded += sub_CaptureEnded;
                    }
                }, () => ActiveCalibrationSub == null);
            FinalizeCommand = new RelayCommand(delegate
            {
                PrepareResults();
            });
            progressTimer.Tick += delegate
            {
                foreach (Subcalibration sub in Subcalibrations)
                    sub.Tick();
            };
            progressTimer.Start();
            //Initialize
            StartOverCommand.Execute(null);
        }

        private async void sub_RequestCapture(Subcalibration sender, EventArgs e)
        {
            if (ActiveCalibrationSub != null)
                ActiveCalibrationSub.AbortCommand.Execute(null);
            await Task.Delay(200);
            ActiveCalibrationSub = sender;
            await ActiveCalibrationSub.RunAsync();
        }
        private void sub_CaptureEnded(Subcalibration sender, EventArgs e)
        {
            ActiveCalibrationSub = null;
        }

        private async void PrepareResults()
        {
            switch (CalibrationTarget)
            {
                case CalibrationTarget.OD:
                    SensorInformation si = new SensorInformation();
                    foreach (Subcalibration sub in Subcalibrations)
                        if (sub.Result != null)
                            si.ResponseCurve.Add(new ResponseData() { Setpoint = sub.Setpoint, Response = sub.Result.YValue });
                    SensorInformationWindow piw = new SensorInformationWindow("Save Sensor Calibration", true, si);
                    piw.Show();
                    await piw.WaitTask;
                    if (piw.Confirmed)
                    {
                        System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                        fbd.ShowDialog();
                        if (Directory.Exists(fbd.SelectedPath))
                        {
                            si.SaveTo(fbd.SelectedPath);
                        }
                    }
                    break;
                case CalibrationTarget.Biomass:
                    //System.Windows.Forms.SaveFileDialog sfd = new System.Windows.Forms.SaveFileDialog();
                    //sfd.AddExtension = true;
                    //sfd.Filter = "log files (*.log)|*.log";
                    //sfd.ShowDialog();
                    //if (!string.IsNullOrWhiteSpace(sfd.FileName))
                    //{
                    //    try
                    //    {
                    //        var writer = File.CreateText(sfd.FileName);
                    //        writer.WriteLine("Setpoint   [n]\tResponse   [rpm]");
                    //        foreach (Subcalibration sub in Subcalibrations)
                    //            writer.WriteLine(string.Format("{0}\t{1}", sub.Setpoint, sub.IncrementalChangePerMinute));
                    //        writer.Flush();
                    //        writer.Dispose();
                    //    }
                    //    catch
                    //    {

                    //    }
                    //}
                    break;
            }
        }

        
        
    }
}