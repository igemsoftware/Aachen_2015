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
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Collections.ObjectModel;
using MCP.Curves;

namespace MCP.Equipment
{
    [XmlTypeAttribute(AnonymousType = true)]
    public class SensorInformation : PropertyChangedBase
    {
        private string _SensorID;
        public string SensorID { get { return _SensorID; } set { _SensorID = value; OnPropertyChanged(); } }

        private List<ResponseData> _ResponseCurve = new List<ResponseData>();
        /// <summary>
        /// What is the response of the pump [ml/h] to a certain setpoint [sph] ?
        /// </summary>
        [XmlArray]
        public List<ResponseData> ResponseCurve { get { return _ResponseCurve; } set { _ResponseCurve = value; OnPropertyChanged(); } }
        //TODO: how to incorporate OD %% CDW calibration curves into one?

        private RelayCommand _EditSensorCommand;
        [XmlIgnore]
        public RelayCommand EditSensorCommand { get { return _EditSensorCommand; } set { _EditSensorCommand = value; OnPropertyChanged(); } }

        #region Drawing the Response Curve
        private ResponseDataPointCollection _ResponseDataCollection = new ResponseDataPointCollection();//contains only recent datapoints
        [XmlIgnore]
        public ResponseDataPointCollection ResponseDataCollection { get { return _ResponseDataCollection; } set { _ResponseDataCollection = value; } }

        private ObservableCollection<ResponseData> _ResponseDataSet = new ObservableCollection<ResponseData>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<ResponseData> ResponseDataSet { get { return _ResponseDataSet; } set { _ResponseDataSet = value; } }

        [XmlIgnore]
        public EnumerableDataSource<ResponseData> DataSource { get; set; }
        #endregion



        public SensorInformation()
        {
            ResponseCurve = new List<ResponseData>();
            DataSource = new EnumerableDataSource<ResponseData>(ResponseDataCollection);
            DataSource.SetXMapping(x => x.Setpoint);
            DataSource.SetYMapping(y => y.Response);
        }
        public void LoadResponseCurve()
        {
            foreach (ResponseData ri in ResponseCurve)
            {
                ResponseDataCollection.Add(ri);
                ResponseDataSet.Add(ri);
            }
        }

        public double CalculateSetpoint(double mlPerHour)
        {
            return ResponseCurve.LinearTransformResponseToSetpoint(mlPerHour);
        }

        public void SaveTo(string folder)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(SensorInformation));
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
        public static SensorInformation LoadFromFile(string file)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(PumpInformation));
                TextReader textReader = new StreamReader(file);
                SensorInformation sensorInfo = (SensorInformation)deserializer.Deserialize(textReader);
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
}
