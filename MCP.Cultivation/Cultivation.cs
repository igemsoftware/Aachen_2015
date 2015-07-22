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
using MCP.Curves;


namespace MCP.Cultivation
{
    public class Cultivation : PropertyChangedBase
    {
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
        public double AerationRateSetpoint { get { return _AerationRateSetpoint; } set { _AerationRateSetpoint = Math.Round(value, 1); OnPropertyChanged(); } }

        private double _DilutionRateSetpoint = 0;
        /// <summary>
        /// desired dilution rate [culture volumes per hour]
        /// </summary>
        public double DilutionRateSetpoint { get { return _DilutionRateSetpoint; } set { _DilutionRateSetpoint = Math.Round(value, 2); OnPropertyChanged(); } }

        private double _CultureVolume = 10;
        /// <summary>
        /// culture volume [ml]
        /// </summary>
        [XmlElement]
        public double CultureVolume { get { return _CultureVolume; } set { _CultureVolume = Math.Round(value, 1); OnPropertyChanged(); } }

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

        private bool _IsRunning;
        public bool IsRunning { get { return _IsRunning; } set { _IsRunning = value; OnPropertyChanged(); } }
        
			
        #endregion

        #region Calculated Properties (Ignored)
        [XmlIgnore]
        public double FeedPumpSPH
        {
            get
            {
                return Reactor.FeedPump.CalculateSetpoint(DilutionRateSetpoint * CultureVolume);
            }
        }
        [XmlIgnore]
        private double AerationPumpSPH
        {
            get
            {
                return Reactor.AerationPump.CalculateSetpoint(AerationRateSetpoint * 60 * CultureVolume);
            }
        }
        [XmlIgnore]
        private double HarvestPumpSPH
        {
            get
            {
                return Reactor.HarvestPump.CalculateSetpoint(DilutionRateSetpoint * CultureVolume * 1.15);
            }
        }
        #endregion

        #region Ignored Properties
        private ReactorInformation _Reactor;
        [XmlIgnore]
        public ReactorInformation Reactor { get { return _Reactor; } set { _Reactor = value; OnPropertyChanged(); } }

        private Dictionary<string, DataLiveLog> _LiveLogs = new Dictionary<string, DataLiveLog>();
        [XmlIgnore]
        public Dictionary<string, DataLiveLog> LiveLogs { get { return _LiveLogs; } set { _LiveLogs = value; OnPropertyChanged(); } }

        private Dictionary<string, DataPostprocessingLog> _PostprocessingLogs = new Dictionary<string, DataPostprocessingLog>();
        [XmlIgnore]
        public Dictionary<string, DataPostprocessingLog> PostprocessingLogs { get { return _PostprocessingLogs; } set { _PostprocessingLogs = value; OnPropertyChanged(); } }
        

        private string _BaseDirectory;
        [XmlIgnore]
        public string BaseDirectory { get { return _BaseDirectory; } set { _BaseDirectory = value; OnPropertyChanged(); InitializeLogs(); } }

        //TODO: StabilityIndicator is not the only relevant parameter - also consider if the Reactor even has a calibrated sensor!!!!
        [XmlIgnore]
        private bool[] StabilityIndicators = new bool[] { false, false, false };
        #endregion

        #region Commands
        private RelayCommand _StartCultivationCommand;
        [XmlIgnore]
        public RelayCommand StartCultivationCommand { get { return _StartCultivationCommand; } set { _StartCultivationCommand = value; OnPropertyChanged(); } }

        private RelayCommand _StopCultivationCommand;
        [XmlIgnore]
        public RelayCommand StopCultivationCommand { get { return _StopCultivationCommand; } set { _StopCultivationCommand = value; OnPropertyChanged(); } }

        private RelayCommand _ChangeParametersCommand;
        [XmlIgnore]
        public RelayCommand ChangeParametersCommand { get { return _ChangeParametersCommand; } set { _ChangeParametersCommand = value; OnPropertyChanged(); } }
        #endregion

        #region Events
        //OnOffgasCollected
        public delegate void AddOnOffgasCollectedDelegate(ParticipantID sender);
        public static event AddOnOffgasCollectedDelegate OffgasCollected;
        private static void OnOffgasCollectedEvent(ParticipantID sender)
        {
            if (OffgasCollected != null)
                OffgasCollected(sender);
        }
        #endregion


        public Cultivation()
        {

        }
        private void InitializeLogs()
        {
            //initialize postprocessing logs - they will handle collection, analysis and regression
            PostprocessingLogs.Add(DimensionSymbol.Turbidity, new BiomassLog(Path.Combine(_BaseDirectory, "OD.log"), DimensionSymbol.Turbidity, Unit.None));
            PostprocessingLogs.Add(DimensionSymbol.Biomass_Concentration, new BiomassLog(Path.Combine(_BaseDirectory, "CDW.log"), DimensionSymbol.Biomass_Concentration, Unit.BiomassConcentration));
            PostprocessingLogs.Add(DimensionSymbol.O2_Saturation, new OffgasLog(Path.Combine(_BaseDirectory, "O2.log"), DimensionSymbol.O2_Saturation, Unit.Percent));
            PostprocessingLogs.Add(DimensionSymbol.CO2_Saturation, new OffgasLog(Path.Combine(_BaseDirectory, "CO2.log"), DimensionSymbol.CO2_Saturation, Unit.Percent));
            PostprocessingLogs.Add(DimensionSymbol.CHx_Saturation, new OffgasLog(Path.Combine(_BaseDirectory, "CHx.log"), DimensionSymbol.CHx_Saturation, Unit.Percent));

            //initialize live logs - they do not process data
            LiveLogs.Add(DimensionSymbol.Agitation_Rate, new DataLiveLog(Path.Combine(_BaseDirectory, "Agitation.log"), DimensionSymbol.Agitation_Rate, Unit.RPM));
            LiveLogs.Add(DimensionSymbol.Aeration_Rate, new DataLiveLog(Path.Combine(_BaseDirectory, "Aeration.log"), DimensionSymbol.Aeration_Rate, Unit.VVM));
            LiveLogs.Add(DimensionSymbol.Dilution_Rate, new DataLiveLog(Path.Combine(_BaseDirectory, "DilutionRate.log"), DimensionSymbol.Dilution_Rate, Unit.PerHour));
            LiveLogs.Add(DimensionSymbol.Temperature, new DataLiveLog(Path.Combine(_BaseDirectory, "Temperature.log"), DimensionSymbol.Temperature, Unit.Celsius));
        }


        public async void SendSetpointUpdate()
        {
            if (Reactor == null)
                return;
            if (!IsRunning)
            {
                int sel = await CustomMessageBox.ShowAsync("Not Running", "This cultivation is not running at the moment.\r\n\r\nDo you want to start it now?", System.Windows.MessageBoxImage.Information, 1, "Start Now", "Okay");
                if (sel == 0)
                    StartCultivationCommand.Execute(null);
            }
            if (IsRunning)
            {
                if (Reactor.FeedPump != null)
                    SerialIO.Current.SendMessage(new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Feed_Rate, FeedPumpSPH.ToString("0"), Unit.SPH));
                if (Reactor.AerationPump != null)
                    SerialIO.Current.SendMessage(new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Aeration_Rate, AerationPumpSPH.ToString("0"), Unit.SPH));
                if (Reactor.HarvestPump != null)
                    SerialIO.Current.SendMessage(new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Harvest_Rate, HarvestPumpSPH.ToString("0"), Unit.SPH));
                SerialIO.Current.SendMessage(new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Agitation_Rate, AgitationRateSetpoint.ToString(), Unit.RPM));
            }
        }
        public void StopCultivation()
        {
            //stop all motors
            if (Reactor.FeedPump != null)
                SerialIO.Current.SendMessage(new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Feed_Rate, "0", Unit.SPH));
            if (Reactor.AerationPump != null)
                SerialIO.Current.SendMessage(new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Aeration_Rate, "0", Unit.SPH));
            if (Reactor.HarvestPump != null)
                SerialIO.Current.SendMessage(new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Harvest_Rate, "0", Unit.SPH));
            SerialIO.Current.SendMessage(new Message(ParticipantID.MCP, Reactor.ParticipantID, MessageType.Command, DimensionSymbol.Agitation_Rate, "0", Unit.RPM));
        }

        public void ReceiveMessage(Message msg)
        {
            switch (msg.MessageType)
            {
                case MessageType.Data:
                    ProcessData(msg.Contents);
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
        private void ProcessData(string[] contents)
        {
            switch (contents[0])
            {
                case DimensionSymbol.Temperature:
                case DimensionSymbol.Aeration_Rate:
                case DimensionSymbol.Agitation_Rate:
                case DimensionSymbol.Dilution_Rate:
                case DimensionSymbol.Feed_Rate:
                case DimensionSymbol.Harvest_Rate:
                    LiveLogs[contents[0]].AddRawData(new DataPoint(DateTime.Now, Convert.ToDouble(contents[1])));
                    break;
                case DimensionSymbol.Biomass:
                    if (Reactor.BiomassSensor != null)
                    {
                        //signal processing (and logging) has to be performed with the transformed value. Otherwise there is no way to calculate the Std on the processing result
                        PostprocessingLogs[DimensionSymbol.Turbidity].AddRawData(new DataPoint(DateTime.Now, Reactor.BiomassSensor.CaluclateOD(Convert.ToDouble(contents[1]))));
                        PostprocessingLogs[DimensionSymbol.Biomass_Concentration].AddRawData(new DataPoint(DateTime.Now, Reactor.BiomassSensor.CaluclateCDW(Convert.ToDouble(contents[1]))));
                    }
                    break;
                case DimensionSymbol.O2_Saturation:
                    if (Reactor.OxygenSensor != null)
                    {
                        if (PostprocessingLogs[contents[0]].AddRawData(new DataPoint(DateTime.Now, Reactor.OxygenSensor.CalculatePercent(Convert.ToDouble(contents[1])))))//add the new point, try to analyze/accumulate
                            StabilityIndicators[0] = true;//if a new point was logged, we're not longer limited by this offgas component
                    }
                    else
                        StabilityIndicators[0] = true;//when the're is no sensor, mark the signal as stable
                    break;
                case DimensionSymbol.CO2_Saturation:
                    if (Reactor.CarbonDioxideSensor != null)
                    {
                        if (PostprocessingLogs[contents[0]].AddRawData(new DataPoint(DateTime.Now, Reactor.CarbonDioxideSensor.CalculatePercent(Convert.ToDouble(contents[1])))))
                            StabilityIndicators[1] = true;
                    }
                    else
                        StabilityIndicators[1] = true;
                    break;
                case DimensionSymbol.CHx_Saturation:
                    if (Reactor.CHxSensor != null)
                    {
                        if (PostprocessingLogs[contents[0]].AddRawData(new DataPoint(DateTime.Now, Reactor.CHxSensor.CalculatePercent(Convert.ToDouble(contents[1])))))
                            StabilityIndicators[2] = true;
                    }
                    else
                        StabilityIndicators[2] = true;
                    break;
                default:
                    break;
            }
            //as soon as all available sensors have detected stability...
            if (StabilityIndicators[0] && StabilityIndicators[1] && StabilityIndicators[2])
            {
                OnOffgasCollectedEvent(Reactor.ParticipantID);//signal that we're ready to hand over for the next reactor
                StabilityIndicators = new bool[] { false, false, false };
            }
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
                cultureInfo.BaseDirectory = new FileInfo(file).Directory.FullName;
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
