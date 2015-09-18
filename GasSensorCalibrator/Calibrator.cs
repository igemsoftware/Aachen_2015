using MCP.Equipment;
using MCP.Measurements;
using MCP.Curves;
using MCP.Protocol;
using Microsoft.Research.DynamicDataDisplay.DataSources;
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
using System.Windows.Forms;
using MCP.Calibration;

namespace GasSensorCalibrator
{
    public class Calibrator : PropertyChangedBase
    {
        #region Commands
        private RelayCommand _StartOverCommand;
        public RelayCommand StartOverCommand { get { return _StartOverCommand; } set { _StartOverCommand = value; OnPropertyChanged(); } }
        private RelayCommand _FinalizeCommand;
        public RelayCommand FinalizeCommand { get { return _FinalizeCommand; } set { _FinalizeCommand = value; OnPropertyChanged(); } }
        private RelayCommand _OpenFileCommand;
        public RelayCommand OpenFileCommand { get { return _OpenFileCommand; } set { _OpenFileCommand = value; OnPropertyChanged(); } }
        #endregion

        private ObservableCollection<Subcalibration> _Subcalibrations = new ObservableCollection<Subcalibration>();
        public ObservableCollection<Subcalibration> Subcalibrations { get { return _Subcalibrations; } set { _Subcalibrations = value; OnPropertyChanged(); } }

        private CalibrationMode _CalibrationMode = CalibrationMode.Standard;
        public CalibrationMode CalibrationMode { get { return _CalibrationMode; } set { _CalibrationMode = value; OnPropertyChanged(); } }

        private CalibrationTarget _CalibrationTarget = CalibrationTarget.Oxygen;
        public CalibrationTarget CalibrationTarget { get { return _CalibrationTarget; } set { _CalibrationTarget = value; OnPropertyChanged(); } }

        public string CalibrationTargetSymbol
        {
            get
            {
                switch (_CalibrationTarget)
                {
                    case CalibrationTarget.Pump:
                        return null;
                    case CalibrationTarget.Stirrer:
                        return DimensionSymbol.Agitation_Rate;
                    case CalibrationTarget.OD:
                        return null;
                    case CalibrationTarget.Biomass:
                        return null;
                    case CalibrationTarget.Oxygen:
                        return DimensionSymbol.O2_Saturation;
                    case CalibrationTarget.Carbon_Dioxide:
                        return DimensionSymbol.CO2_Saturation;
                    case CalibrationTarget.CHx:
                        return DimensionSymbol.CHx_Saturation;
                    default:
                        return null;
                }
            }
        }
        //TODO: can CalibrationTarget be replaced with the DimensionSymbol?


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
                foreach (GasSensorResponseData gsrd in CalibrationProfiles.Profiles[CalibrationTarget][CalibrationMode])
                    Subcalibrations.Add(new Subcalibration(gsrd, CalibrationProfiles.Symbols[CalibrationTarget], CalibrationProfiles.Units[CalibrationTarget]) { Target = CalibrationTarget });
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
            OpenFileCommand = new RelayCommand(delegate
            {
                LoadCalibrationFile();
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
            GasSensorInformation si = new GasSensorInformation();
            foreach (Subcalibration sub in Subcalibrations)
                if (!double.IsNaN(sub.ResponsePoint.Analog))
                    si.ResponseCurve.Add(sub.ResponsePoint);
            GasSensorInformationWindow gsiw = new GasSensorInformationWindow("Save Sensor Calibration", true, si);
            gsiw.Show();
            await gsiw.WaitTask;
            if (gsiw.Confirmed)
            {
                System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
                fbd.ShowDialog();
                if (Directory.Exists(fbd.SelectedPath))
                {
                    si.SaveTo(fbd.SelectedPath);
                }
            }
        }
        private void LoadCalibrationFile()
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "OD Sensor Files|*.biomass" };
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(ofd.FileName);
                try
                {
                    GasSensorInformation si = GasSensorInformation.LoadFromFile(ofd.FileName);
                    Subcalibrations.Clear();
                    foreach (GasSensorResponseData gsrd in si.ResponseCurve)
                    {
                        Subcalibrations.Add(new Subcalibration(gsrd, CalibrationProfiles.Symbols[CalibrationTarget], CalibrationProfiles.Units[CalibrationTarget]) { Target = CalibrationTarget });
                        foreach (Subcalibration sub in Subcalibrations)
                        {
                            sub.RequestCapture += sub_RequestCapture;
                            sub.CaptureEnded += sub_CaptureEnded;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Task mb = CustomMessageBox.ShowAsync("Can't import", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
                }
            }
        }


    }
}