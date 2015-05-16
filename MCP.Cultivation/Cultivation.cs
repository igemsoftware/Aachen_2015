using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCP.Equipment;
using MCP.Protocol;
using TCD;
using System.Xml.Serialization;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using System.IO;
using TCD.Controls;
using MCP.Measurements;


namespace MCP.Cultivation
{
    public class Cultivation : PropertyChangedBase
    {
        #region Ignored Properties
        private ReactorInformation _Reactor;
        [XmlIgnore]
        public ReactorInformation Reactor { get { return _Reactor; } set { _Reactor = value; OnPropertyChanged(); } }

        private CultivationLog _CultivationLog;
        [XmlIgnore]
        public CultivationLog CultivationLog { get { return _CultivationLog; } set { _CultivationLog = value; OnPropertyChanged(); } }

        private string _BaseDirectory;
        [XmlIgnore]
        public string BaseDirectory { get { return _BaseDirectory; } set { _BaseDirectory = value; OnPropertyChanged(); } }
        
        #endregion

        #region Commands
        private RelayCommand _ChangeParametersCommand;
        [XmlIgnore]
        public RelayCommand ChangeParametersCommand { get { return _ChangeParametersCommand; } set { _ChangeParametersCommand = value; OnPropertyChanged(); } }
        #endregion

        #region Serialized Properties (Control Parameters)
        private int _AgitationRateSetpoint = 500;
        /// <summary>
        /// desired agitation rate [rpm]
        /// </summary>
        public int AgitationRateSetpoint { get { return _AgitationRateSetpoint; } set { _AgitationRateSetpoint = value; OnPropertyChanged(); } }

        private double _AerationRateSetpoint = 1;
        /// <summary>
        /// desired aeration rate [vvm]
        /// </summary>
        public double AerationRateSetpoint { get { return _AerationRateSetpoint; } set { _AerationRateSetpoint = value; OnPropertyChanged(); } }

        private double _DilutionRateSetpoint = 0;
        /// <summary>
        /// desired dilution rate [culture volumes per hour]
        /// </summary>
        public double DilutionRateSetpoint { get { return _DilutionRateSetpoint; } set { _DilutionRateSetpoint = value; OnPropertyChanged(); } }

        private double _CultureVolume = 10;
        /// <summary>
        /// culture volume [ml]
        /// </summary>
        [XmlElement]
        public double CultureVolume { get { return _CultureVolume; } set { _CultureVolume = value; OnPropertyChanged(); } }

        private string _CultureDescription;
        /// <summary>
        /// Describes the strain or sample in the cultivation.
        /// </summary>
        public string CultureDescription { get { return _CultureDescription; } set { _CultureDescription = value; OnPropertyChanged(); } }

        private DateTime _StartTime;
        /// <summary>
        /// When was the experiment started?
        /// </summary>
        public DateTime StartTime { get { return _StartTime; } set { _StartTime = value; OnPropertyChanged(); } }
        
			
        #endregion

        #region Events
        //OnNewMessageToSend
        public delegate void AddOnNewMessageToSendDelegate(object sender, Message message);
        public event AddOnNewMessageToSendDelegate NewMessageToSend;
        public void OnNewMessageToSendEvent(object sender, Message message)
        {
            if (NewMessageToSend != null)
                NewMessageToSend(sender, message);
        }
        #endregion


        private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        [XmlIgnore]
        public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; OnPropertyChanged("SensorDataCollection"); } }

        private ObservableCollection<DataPoint> _SensorDataSet = new ObservableCollection<DataPoint>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<DataPoint> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; OnPropertyChanged("SensorDataSet"); } }

        private EnumerableDataSource<DataPoint> _DataSource;
        [XmlIgnore]
        public EnumerableDataSource<DataPoint> DataSource { get { return _DataSource; } set { _DataSource = value; OnPropertyChanged("DataSource"); } }

        private static Random rnd = new Random();


        public Cultivation()
        {
            //TODO: implement all the graphs
            DataSource = new EnumerableDataSource<DataPoint>(SensorDataCollection);
            DataSource.SetXMapping(x => (x.Time - StartTime).TotalSeconds);
            DataSource.SetYMapping(y => y.Value);

            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            dt.Tick += delegate
            {
                DataPoint data = new DataPoint(DateTime.Now, rnd.NextDouble());
                SensorDataSet.Add(data);
                SensorDataCollection.Add(data);
                if (CultivationLog != null)
                    CultivationLog.LogData(DimensionSymbol.Agitation_Rate, new RawData(rnd.NextDouble(), DateTime.Now));
            };
            dt.Start();
        }


        public void Initialize(string baseDirectory)
        {
            BaseDirectory = baseDirectory;
            CultivationLog = new CultivationLog(baseDirectory);
        }

        private void SendSetpointUpdate()
        {
            if (Reactor.FeedPump != null)
                OnNewMessageToSendEvent(this, new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Feed_Rate, CalculateFeedPumpSPH().ToString(), Unit.SPH));
            if (Reactor.AerationPump != null)
                OnNewMessageToSendEvent(this, new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Aeration_Rate, CalculateAerationPumpSPH().ToString(), Unit.SPH));
            if (Reactor.HarvestPump != null)
                OnNewMessageToSendEvent(this, new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Harvest_Rate, CalculateHarvestPumpSPH().ToString(), Unit.SPH));
            OnNewMessageToSendEvent(this, new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Agitation_Rate, AgitationRateSetpoint.ToString(), Unit.RPM));
        }

        private double CalculateFeedPumpSPH()
        {
            return DilutionRateSetpoint * CultureVolume * Reactor.FeedPump.SpecificPumpingRate;
        }
        private double CalculateAerationPumpSPH()
        {
            return AerationRateSetpoint * 60 * CultureVolume * Reactor.AerationPump.SpecificPumpingRate;
        }
        private double CalculateHarvestPumpSPH()
        {
            return DilutionRateSetpoint * CultureVolume * Reactor.HarvestPump.SpecificPumpingRate * 1.15;
        }

        public void Save()
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(Cultivation));
                TextWriter textWriter = new StreamWriter(Path.Combine(BaseDirectory, Reactor.ParticipantID.ToString() + ".cultivation"));
                serializer.Serialize(textWriter, this);
                textWriter.Close();
                textWriter.Dispose();
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't save", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
        }
        public static Cultivation LoadFromFile(string file)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(Cultivation));
                TextReader textReader = new StreamReader(file);
                Cultivation cultureInfo = (Cultivation)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                cultureInfo.Initialize(new FileInfo(file).Directory.FullName);
                return cultureInfo;
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't load", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
            return null;
        }

    }
}
