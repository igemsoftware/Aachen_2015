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
    public class BiomassSensorInformation : PropertyChangedBase
    {
        private string _SensorID;
        public string SensorID { get { return _SensorID; } set { _SensorID = value; OnPropertyChanged(); } }

        private List<BiomassResponseData> _ResponseCurve = new List<BiomassResponseData>();
        /// <summary>
        /// What actual OD/CDW correlates to the analog sensor values?
        /// </summary>
        [XmlArray]
        public List<BiomassResponseData> ResponseCurve { get { return _ResponseCurve; } set { _ResponseCurve = value; OnPropertyChanged(); } }
        
        private RelayCommand _EditSensorCommand;
        [XmlIgnore]
        public RelayCommand EditSensorCommand { get { return _EditSensorCommand; } set { _EditSensorCommand = value; OnPropertyChanged(); } }

        #region Drawing the Response Curve
        private BiomassResponseDataPointCollection _ResponseDataCollection = new BiomassResponseDataPointCollection();//contains only recent datapoints
        [XmlIgnore]
        public BiomassResponseDataPointCollection ResponseDataCollection { get { return _ResponseDataCollection; } set { _ResponseDataCollection = value; } }

        private ObservableCollection<BiomassResponseData> _ResponseDataSet = new ObservableCollection<BiomassResponseData>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<BiomassResponseData> ResponseDataSet { get { return _ResponseDataSet; } set { _ResponseDataSet = value; } }

        [XmlIgnore]
        public EnumerableDataSource<BiomassResponseData> ODDataSource { get; set; }
        [XmlIgnore]
        public EnumerableDataSource<BiomassResponseData> CDWDataSource { get; set; }
        #endregion



        public BiomassSensorInformation()
        {
            ResponseCurve = new List<BiomassResponseData>();
            ODDataSource = new EnumerableDataSource<BiomassResponseData>(ResponseDataCollection);
            ODDataSource.SetXMapping(x => x.Analog);
            ODDataSource.SetYMapping(y => y.OD);
            CDWDataSource = new EnumerableDataSource<BiomassResponseData>(ResponseDataCollection);
            CDWDataSource.SetXMapping(x => x.Analog);
            CDWDataSource.SetYMapping(y => y.CDW);
        }
        public void LoadResponseCurve()
        {
            foreach (BiomassResponseData ri in ResponseCurve)
            {
                ResponseDataCollection.Add(ri);
                ResponseDataSet.Add(ri);
            }
        }

        public double CaluclateOD(double analog)
        {
            return ResponseCurve.LinearTransformAnalogToOD(analog);
        }
        public double CaluclateCDW(double analog)
        {
            return ResponseCurve.LinearTransformAnalogToCDW(analog);
        }

        public void SaveTo(string folder)
        {
            try
            {
                XmlSerializer serializer = new XmlSerializer(typeof(BiomassSensorInformation));
                TextWriter textWriter = new StreamWriter(Path.Combine(folder, SensorID + ".biomass"));
                serializer.Serialize(textWriter, this);
                textWriter.Close();
                textWriter.Dispose();
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't save", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
        }
        public static BiomassSensorInformation LoadFromFile(string file)
        {
            try
            {
                XmlSerializer deserializer = new XmlSerializer(typeof(BiomassSensorInformation));
                TextReader textReader = new StreamReader(file);
                BiomassSensorInformation sensorInfo = (BiomassSensorInformation)deserializer.Deserialize(textReader);
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
