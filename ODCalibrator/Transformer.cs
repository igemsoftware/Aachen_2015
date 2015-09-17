using MCP.Curves;
using MCP.Equipment;
using MCP.Measurements;
using MCP.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCD;
using TCD.Controls;

namespace ODCalibrator
{
    public class Transformer : PropertyChangedBase
    {
        #region Commands
        private RelayCommand _OpenFileCommand;
        public RelayCommand OpenFileCommand { get { return _OpenFileCommand; } set { _OpenFileCommand = value; OnPropertyChanged(); } }

        private RelayCommand _TransformToODCommand;
        public RelayCommand TransformToODCommand { get { return _TransformToODCommand; } set { _TransformToODCommand = value; OnPropertyChanged(); } }

        private RelayCommand _LoadRawFileCommand;
        public RelayCommand LoadRawFileCommand { get { return _LoadRawFileCommand; } set { _LoadRawFileCommand = value; OnPropertyChanged(); } }

        #endregion

        public string CalibrationFileInfoText
        {
            get
            {
                if (BiomassSensorInfo == null)
                    return "please load a calibration file";
                else if (BiomassSensorInfo.ResponseRegressionOD == null)
                    return "no regression available";
                else
                    return BiomassSensorInfo.ResponseRegressionOD.ToString().Replace("y(x)", "OD(x)");
            }
        }

        public string InfoRaw
        {
            get
            {
                return string.Format("{0} data points.", RawDataPoints.Count);
            }
        }
      
        private BiomassSensorInformation _BiomassSensorInfo;
        public BiomassSensorInformation BiomassSensorInfo
        {
            get { return _BiomassSensorInfo; }
            set
            {
                _BiomassSensorInfo = value;
                OnPropertyChanged();
                OnPropertyChanged("CalibrationFileInfoText");
                TransformToODCommand.RaiseCanExecuteChanged();
            }
        }

        private ObservableCollection<DataPoint> _RawDataPoints = new ObservableCollection<DataPoint>();
        public ObservableCollection<DataPoint> RawDataPoints { get { return _RawDataPoints; } set { _RawDataPoints = value; OnPropertyChanged(); } }
			
        public Transformer()
        {
            OpenFileCommand = new RelayCommand(delegate
            {
                LoadCalibrationFile();
                //TODO: load calibration file implementieren
                //      response curve regression aus der datei laden
                //      file info text anzeigen
                //      laden von log-dateien implementieren
                //      transformation implementieren
                //      speichern implementieren
            });
            LoadRawFileCommand = new RelayCommand(delegate
                {
                    LoadLogFile(RawDataPoints);
                    TransformToODCommand.RaiseCanExecuteChanged();
                    OnPropertyChanged("InfoRaw");
                });
             TransformToODCommand = new RelayCommand(delegate
                {
                    SimulatePostprocessing();
                }, new Func<bool>(() => BiomassSensorInfo != null && RawDataPoints.Count > 0));
        }

        private void SimulatePostprocessing()
        {
            SaveFileDialog sfd = new SaveFileDialog() { Filter = "Log File|*.log" };
            DialogResult result = sfd.ShowDialog();
            if (result == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(sfd.FileName);
                try
                {
                    BiomassLog log = new BiomassLog(sfd.FileName, DimensionSymbol.Turbidity, Unit.None);

                    foreach (DataPoint point in RawDataPoints)
                    {
                        log.AddRawData(new DataPoint(point.Time, BiomassSensorInfo.CaluclateOD(point.YValue)));
                    }
                }
                catch (Exception ex)
                {
                    Task mb = CustomMessageBox.ShowAsync("Can't import", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
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
                    BiomassSensorInfo = BiomassSensorInformation.LoadFromFile(ofd.FileName);
                }
                catch (Exception ex)
                {
                    Task mb = CustomMessageBox.ShowAsync("Can't import", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
                }
            }
        }

        private void LoadLogFile(ObservableCollection<DataPoint> targetCollection)
        {
            OpenFileDialog ofd = new OpenFileDialog() { Filter = "Log Files|*.log" };
            DialogResult result = ofd.ShowDialog();
            if (result == DialogResult.OK)
            {
                FileInfo fi = new FileInfo(ofd.FileName);
                try
                {
                    RawDataPoints.Clear();
                    LoadPointsFromFile(ofd.FileName, targetCollection);
                }
                catch (Exception ex)
                {
                    Task mb = CustomMessageBox.ShowAsync("Can't import", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
                }
            }
        }
			
        private void LoadPointsFromFile(string filepath, ObservableCollection<DataPoint> targetCollection)
        {
            string[] lines = File.ReadAllLines(filepath);
            for (int i = 1; i < lines.Length; i++)
            {
                DataPoint dp = new DataPoint(lines[i]);
                try
                {
                    targetCollection.Add(dp);
                }
                catch
                {
                    //TODO: this feels dirty
                }
            }
        }

    }
}
