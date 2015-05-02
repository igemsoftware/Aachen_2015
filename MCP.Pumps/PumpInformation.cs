using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using TCD.Controls;

namespace MCP.Pumps
{
    public class PumpInformation : PropertyChangedBase
    {
        private string _PumpID;
        public string PumpID { get { return _PumpID; } set { _PumpID = value; OnPropertyChanged(); } }

        private double _SpecificPumpingRate;
        /// <summary>
        /// specific pumping rate [steps/ml]
        /// </summary>
        public double SpecificPumpingRate { get { return _SpecificPumpingRate; } set { _SpecificPumpingRate = value; OnPropertyChanged(); } }

        private RelayCommand _EditPumpCommand;
        [XmlIgnore]
        public RelayCommand EditPumpCommand { get { return _EditPumpCommand; } set { _EditPumpCommand = value; OnPropertyChanged(); } }
        
			

        public PumpInformation()
        {

        }

        public void SaveTo(string folder)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(PumpInformation));
                TextWriter textWriter = new StreamWriter(Path.Combine(folder, PumpID + ".pump"));
                serializer.Serialize(textWriter, this);
                textWriter.Close();
                textWriter.Dispose();
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't save", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
        }
        public static PumpInformation LoadFromFile(string file)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(PumpInformation));
                TextReader textReader = new StreamReader(file);
                PumpInformation pumpInfo = (PumpInformation)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                return pumpInfo;
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't load", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
            return null;
        }
    }
}
