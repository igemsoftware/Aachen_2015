using MCP.Curves;
using MCP.Protocol;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TCD;

namespace ODCalibrator
{
    public class ViewModel : PropertyChangedBase
    {
        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }
       
        private Calibrator _Calibrator = new Calibrator();
        public Calibrator Calibrator { get { return _Calibrator; } set { _Calibrator = value; OnPropertyChanged(); } }

        public static ViewModel Current { get; private set; }

        #region Data
        private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; } }

        private ObservableCollection<DataPoint> _SensorDataSet = new ObservableCollection<DataPoint>();//contains all datapoints
        public ObservableCollection<DataPoint> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; } }

        public EnumerableDataSource<DataPoint> DataSource { get; set; }

        private DateTime StartTime { get; set; }
        #endregion


        #region Debug Properties
#if DEBUG
        public bool IsDebugMode { get { return true; } }
#else
        public bool IsDebugMode { get { return false; } }
#endif
        private bool _IsRandomizerEnabled;
        public bool IsRandomizerEnabled { get { return _IsRandomizerEnabled; } set { _IsRandomizerEnabled = value; OnPropertyChanged(); } }

        private static Random rnd = new Random();

        #endregion

        public ViewModel()
        {
            Current = this;
            PrimarySerial.NewMessageReceived += PrimarySerial_NewMessageReceived;
            if (IsDebugMode)
            {
                DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                dt.Tick += delegate
                {
                    if (IsRandomizerEnabled)
                    {
                        double val = rnd.NextDouble();
                        if (Calibrator.ActiveCalibrationSub != null)
                            val = val * 10 + (double)Calibrator.ActiveCalibrationSub.ResponsePoint.OD * 150 + 120;
                        Message msg = new Message(ParticipantID.Reactor_1, ParticipantID.MCP, MessageType.Data, string.Format("{0}\t{1}\t{2}", DimensionSymbol.Biomass, val, Unit.Biomass));
                        PrimarySerial.InterpretMessage(msg.Raw);
                    }
                };
                dt.Start();
            }
            //data collection
            StartTime = DateTime.Now;
            DataSource = SensorDataCollection.AsDataSource<DataPoint>();
            DataSource.SetXMapping(x => (x.Time - StartTime).TotalSeconds);
            DataSource.SetYMapping(y => y.YValue);
        }


        private void PrimarySerial_NewMessageReceived(object sender, Message message)
        {
            try
            {
                switch (message.MessageType)
                {
                    case MessageType.Data:
                        if (message.Contents[0] == DimensionSymbol.Biomass)
                        {
                            double val = Convert.ToDouble(message.Contents[1]);
                            DataPoint dp = new DataPoint(DateTime.Now, val);
                            SensorDataSet.Add(dp);
                            SensorDataCollection.Add(dp);
                            if (Calibrator.ActiveCalibrationSub != null)
                                Calibrator.ActiveCalibrationSub.AddPoint(new DataPoint(DateTime.Now, val));
                        }
                        break;
                    case MessageType.Command:
                        break;
                    case MessageType.DataFormat:
                        break;
                    case MessageType.CommandFormat:
                        break;
                    default:
                        break;
                }
            }
            catch
            {

            }

        }
    }
}