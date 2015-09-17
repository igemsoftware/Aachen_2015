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
using System.Windows;

namespace ODCalibrator
{
    public class Calibrator : PropertyChangedBase
    {
        #region Commands
        private RelayCommand _FinalizeCommand;
        public RelayCommand FinalizeCommand { get { return _FinalizeCommand; } set { _FinalizeCommand = value; OnPropertyChanged(); } }

        private RelayCommand _CopyCommand;
        public RelayCommand CopyCommand { get { return _CopyCommand; } set { _CopyCommand = value; OnPropertyChanged(); } }

        private RelayCommand _OpenFileCommand;
        public RelayCommand OpenFileCommand { get { return _OpenFileCommand; } set { _OpenFileCommand = value; OnPropertyChanged(); } }
        #endregion

        private ObservableCollection<Subcalibration> _Subcalibrations = new ObservableCollection<Subcalibration>();
        public ObservableCollection<Subcalibration> Subcalibrations { get { return _Subcalibrations; } set { _Subcalibrations = value; OnPropertyChanged(); } }

        private int _TotalCalibrationPoints = 7;
        public int TotalCalibrationPoints { get { return _TotalCalibrationPoints; } set { _TotalCalibrationPoints = value; OnPropertyChanged(); UpdateNumberOfSubcalibrations(); } }
        

        private CalibrationTarget _CalibrationTarget = CalibrationTarget.Biomass;
        public CalibrationTarget CalibrationTarget { get { return _CalibrationTarget; } set { _CalibrationTarget = value; OnPropertyChanged(); } }



        private Subcalibration _ActiveCalibrationSub;
        public Subcalibration ActiveCalibrationSub
        {
            get { return _ActiveCalibrationSub; }
            set
            {
                _ActiveCalibrationSub = value;
                OnPropertyChanged();
            }
        }



        private DispatcherTimer progressTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(100) };


        public Calibrator()
        {
            FinalizeCommand = new RelayCommand(delegate
            {
                var si = PrepareResults();
                ShowResults(si);
            });
            CopyCommand = new RelayCommand(delegate
            {
                var si = PrepareResults();
                // now copy the response points 
                var outputTabbed = "Raw\tsRaw\tOD\r\n";
                foreach (BiomassResponseData rd in si.ResponseCurve)
	            {
		            outputTabbed += string.Format("{0}\t{1}\t{2}\r\n", rd.Analog, rd.AnalogStd, rd.OD);
                }                    
                outputTabbed = outputTabbed.TrimEnd('\n', '\r');
                try
                {
                    System.Windows.Clipboard.SetDataObject(outputTabbed, true);
                }
                catch (Exception ex)
                {
                    Task t = CustomMessageBox.ShowAsync("Error", "Export to Clipboard failed.\n\nPlease try again.", MessageBoxImage.Warning, 0, "Ok");
                }
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
            UpdateNumberOfSubcalibrations();
        }

        private void UpdateNumberOfSubcalibrations()
        {
            if (Subcalibrations.Count > TotalCalibrationPoints) // case 1: zu viele -> range löschen
            {
                //remove extra subcalibrations
                for (int i = TotalCalibrationPoints; i < Subcalibrations.Count; i++)
                    Subcalibrations.RemoveAt(i);
            }
            else if (TotalCalibrationPoints > Subcalibrations.Count) // case 2: zu wenige -> hinzufügen
            {
                for (int i = Subcalibrations.Count; i < TotalCalibrationPoints; i++)
                {
                    BiomassResponseData brd = new BiomassResponseData() { OD = 0.1 * Math.Pow(2, i), CalibrationDuration = 20 };
                    Subcalibration sub = new Subcalibration(brd, CalibrationProfiles.Symbols[CalibrationTarget], CalibrationProfiles.Units[CalibrationTarget]) { Target = CalibrationTarget };
                    sub.RequestCapture += sub_RequestCapture;
                    sub.CaptureEnded += sub_CaptureEnded;
                    Subcalibrations.Add(sub);
                }
            }
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

        private BiomassSensorInformation PrepareResults()
        {
            BiomassSensorInformation si = new BiomassSensorInformation();
            foreach (Subcalibration sub in Subcalibrations)
                if (!double.IsNaN(sub.ResponsePoint.Analog))
                    si.ResponseCurve.Add(sub.ResponsePoint);
            return si;
        }
        private async void ShowResults(BiomassSensorInformation si)
        {
            BiomassSensorInformationWindow piw = new BiomassSensorInformationWindow("Save Sensor Calibration", true, si);
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
                    BiomassSensorInformation si = BiomassSensorInformation.LoadFromFile(ofd.FileName);
                    Subcalibrations.Clear();
                    foreach (BiomassResponseData brd in si.ResponseCurve)
                    {
                        Subcalibrations.Add(new Subcalibration(brd, CalibrationProfiles.Symbols[CalibrationTarget], CalibrationProfiles.Units[CalibrationTarget]) { Target = CalibrationTarget });
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