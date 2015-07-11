using MCP.Curves;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using TCD;
using TCD.Controls;

namespace MCP.Equipment
{
    public class GasSensorInformation : PropertyChangedBase
    {
        private string _SensorID;
        public string SensorID { get { return _SensorID; } set { _SensorID = value; OnPropertyChanged(); } }

        private SensorType _SensorType;
        public SensorType SensorType { get { return _SensorType; } set { _SensorType = value; OnPropertyChanged(); } }
        

        private List<GasSensorResponseData> _ResponseCurve = new List<GasSensorResponseData>();
        /// <summary>
        /// What actual Vol.% value correlates to the analog sensor values?
        /// </summary>
        [XmlArray]
        public List<GasSensorResponseData> ResponseCurve { get { return _ResponseCurve; } set { _ResponseCurve = value; OnPropertyChanged(); } }

        private RelayCommand _EditSensorCommand;
        [XmlIgnore]
        public RelayCommand EditSensorCommand { get { return _EditSensorCommand; } set { _EditSensorCommand = value; OnPropertyChanged(); } }

        #region Drawing the Response Curve
        private GasSensorResponseDataCollection _ResponseDataCollection = new GasSensorResponseDataCollection();//contains only recent datapoints
        [XmlIgnore]
        public GasSensorResponseDataCollection ResponseDataCollection { get { return _ResponseDataCollection; } set { _ResponseDataCollection = value; } }

        private ObservableCollection<GasSensorResponseData> _ResponseDataSet = new ObservableCollection<GasSensorResponseData>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<GasSensorResponseData> ResponseDataSet { get { return _ResponseDataSet; } set { _ResponseDataSet = value; } }

        [XmlIgnore]
        public EnumerableDataSource<GasSensorResponseData> DataSource { get; set; }
        #endregion

        public GasSensorInformation()
        {
            ResponseCurve = new List<GasSensorResponseData>();
            DataSource = new EnumerableDataSource<GasSensorResponseData>(ResponseDataCollection);
            DataSource.SetXMapping(x => x.Analog);
            DataSource.SetYMapping(y => y.Percent);
        }
        public void LoadResponseCurve()
        {
            foreach (GasSensorResponseData ri in ResponseCurve)
            {
                ResponseDataCollection.Add(ri);
                ResponseDataSet.Add(ri);
            }
        }

        public double CaluclatePercent(double analog)
        {
            return ResponseCurve.LinearTransformAnalogToPercent(analog);
        }

        public void SaveTo(string folder)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(GasSensorInformation));
                TextWriter textWriter = new StreamWriter(Path.Combine(folder, SensorID + ".sensor"));
                serializer.Serialize(textWriter, this);
                textWriter.Close();
                textWriter.Dispose();
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't save", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
        }
        public static GasSensorInformation LoadFromFile(string file)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(GasSensorInformation));
                TextReader textReader = new StreamReader(file);
                GasSensorInformation sensorInfo = (GasSensorInformation)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                return sensorInfo;
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't load", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
            return null;
        }

    }
    public enum SensorType
    {
        Oxygen, Carbon_Dioxide
    }
}
