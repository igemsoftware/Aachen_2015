using MCP.Equipment;
using MCP.Measurements;
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
using TCD;

namespace PumpCalibrator
{
    public class Calibrator : PropertyChangedBase
    {
        private RelayCommand _StartCalibrationCommand;
        public RelayCommand StartCalibrationCommand { get { return _StartCalibrationCommand; } set { _StartCalibrationCommand = value; OnPropertyChanged(); } }

        private RelayCommand _FinishCalibrationCommand;
        public RelayCommand FinishCalibrationCommand { get { return _FinishCalibrationCommand; } set { _FinishCalibrationCommand = value; OnPropertyChanged(); } }
        
			

        private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; } }

        private ObservableCollection<RawData> _SensorDataSet = new ObservableCollection<RawData>();//contains all datapoints
        public ObservableCollection<RawData> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; } }


        public DateTime StartTime { get; set; }
        public double StartWeight { get; set; }

        //private Dictionary<PumpingSpeed,double> _SpecificPumpingRates;
        //public Dictionary<PumpingSpeed,double> SpecificPumpingRates { get { return _SpecificPumpingRates; } set { _SpecificPumpingRates = value; OnPropertyChanged(); } }

        private PumpingSpeed _CurrentSpeed;
        public PumpingSpeed CurrentSpeed { get { return _CurrentSpeed; } set { _CurrentSpeed = value; OnPropertyChanged(); } }
        
			
			
        public EnumerableDataSource<RawData> DataSource { get; set; }
			

        public Calibrator()
        {
            DataSource = new EnumerableDataSource<RawData>(SensorDataCollection);
            DataSource.SetXMapping(x => (x.Time - StartTime).TotalSeconds);
            DataSource.SetYMapping(y => y.Value - StartWeight);
            //
            StartCalibrationCommand = new RelayCommand(delegate
                {
                    SpeechIO.Speak(string.Format("Starting calibration at {0} speed.", CurrentSpeed));
                    Start();
                });
            FinishCalibrationCommand = new RelayCommand(async delegate
                {
                    SpeechIO.Speak("Calibration finished.");
                    RawData start = SensorDataSet.First();
                    RawData end = SensorDataSet.Last();
                    double pumpedVolume = (end.Value - start.Value);
                    if (pumpedVolume <= 0)
                        return;
                    double specificPumpingRate = (int)CurrentSpeed * (end.Time - start.Time).TotalHours / pumpedVolume;
                    //SpecificPumpingRates.Add(CurrentSpeed, specificPumpingRate);
                    PumpInformation newPump = new PumpInformation() { SpecificPumpingRate = specificPumpingRate };
                    PumpInformationWindow piw = new PumpInformationWindow("Save Pump Calibration", true) { DataContext = newPump };
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
                });
            //Speech
            //SpeechIO.Recognizer.SpeechRecognized += Recognizer_SpeechRecognized;
        }
        private void Recognizer_SpeechRecognized(object sender, System.Speech.Recognition.SpeechRecognizedEventArgs e)
        {
            switch(e.Result.Text.ToLower())
            {
                case "start calibration":
                    StartCalibrationCommand.Execute(null);
                    break;
                case "finish calibration":
                    FinishCalibrationCommand.Execute(null);
                    break;
            }
        }

        public void Start()
        {
            SensorDataCollection.Clear();
            StartTime = DateTime.Now;
            ViewModel.Current.PrimarySerial.SendMessage(new Message(ParticipantID.MCP, ParticipantID.Reactor_1, MessageType.Command, string.Format("100000")));
        }
        public void AddPoint(RawData data)
        {
            SensorDataSet.Add(data);
            SensorDataCollection.Add(data);
        }
    }
    public enum PumpingSpeed
    {
        Slow = 300000,
        Medium = 200000,
        Fast = 100000
    }
}
