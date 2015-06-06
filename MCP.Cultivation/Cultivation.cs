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

        private CultivationLog _CultivationLog;
        [XmlIgnore]
        public CultivationLog CultivationLog { get { return _CultivationLog; } set { _CultivationLog = value; OnPropertyChanged(); } }

        private string _BaseDirectory;
        [XmlIgnore]
        public string BaseDirectory { get { return _BaseDirectory; } set { _BaseDirectory = value; OnPropertyChanged(); CultivationLog = new CultivationLog(value); } }

       
        	
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



        public Cultivation()
        {

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
