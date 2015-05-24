using MCP.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using System.Xml.Schema;
using TCD;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using TCD.Controls;
using MCP.Equipment;

namespace MCP.Equipment
{
    [Serializable]
    public class ReactorInformation : PropertyChangedBase
    {
        private ParticipantID _ParticipantID;
        [XmlElement]
        public ParticipantID ParticipantID { get { return _ParticipantID; } set { _ParticipantID = value; OnPropertyChanged(); } }

        
        #region Pumps
        private string _FeedPumpID = string.Empty;
        [XmlElement]
        public string FeedPumpID { get { return _FeedPumpID; } set { _FeedPumpID = value; OnPropertyChanged(); } }
        [XmlIgnore]
        public PumpInformation FeedPump { get { return (Inventory.Current.Pumps.ContainsKey(_FeedPumpID) ? Inventory.Current.Pumps[_FeedPumpID] : null); } }

        private string _AerationPumpID = string.Empty;
        [XmlElement]
        public string AerationPumpID { get { return _AerationPumpID; } set { _AerationPumpID = value; OnPropertyChanged(); } }
        [XmlIgnore]
        public PumpInformation AerationPump { get { return (Inventory.Current.Pumps.ContainsKey(_AerationPumpID) ? Inventory.Current.Pumps[_AerationPumpID] : null); } }

        private string _HarvestPumpID = string.Empty;
        [XmlElement]
        public string HarvestPumpID { get { return _HarvestPumpID; } set { _HarvestPumpID = value; OnPropertyChanged(); } }
        [XmlIgnore]
        public PumpInformation HarvestPump { get { return (Inventory.Current.Pumps.ContainsKey(_HarvestPumpID) ? Inventory.Current.Pumps[_HarvestPumpID] : null); } }
        #endregion

        #region Commands
        private RelayCommand _EditReactorCommand;
        [XmlIgnore]
        public RelayCommand EditReactorCommand { get { return _EditReactorCommand; } set { _EditReactorCommand = value; OnPropertyChanged(); } }
        
			
        #endregion

        public ReactorInformation()
        {

        }

        public void SaveTo(string folder)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(ReactorInformation));
                TextWriter textWriter = new StreamWriter(Path.Combine(folder, ParticipantID.ToString() + ".reactor"));
                serializer.Serialize(textWriter, this);
                textWriter.Close();
                textWriter.Dispose();
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't save", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
        }
        public static ReactorInformation LoadFromFile(string file)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(ReactorInformation));
                TextReader textReader = new StreamReader(file);
                ReactorInformation reactorInfo = (ReactorInformation)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                return reactorInfo;
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't load", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
            return null;
        }

    }
}
