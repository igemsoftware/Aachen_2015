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
    public class PumpInformation : PropertyChangedBase
    {
        private string _PumpID;
        public string PumpID { get { return _PumpID; } set { _PumpID = value; OnPropertyChanged(); } }

        private List<ResponseData> _ResponseCurve = new List<ResponseData>();
        /// <summary>
        /// What is the response of the pump [ml/h] to a certain setpoint [sph] ?
        /// </summary>
        [XmlArray]
        public List<ResponseData> ResponseCurve { get { return _ResponseCurve; } set { _ResponseCurve = value; OnPropertyChanged(); } }
        

        private double _SpecificPumpingRate;
        /// <summary>
        /// specific pumping rate [steps/ml]
        /// </summary>
        public double SpecificPumpingRate { get { return _SpecificPumpingRate; } set { _SpecificPumpingRate = value; OnPropertyChanged(); } }

        private RelayCommand _EditPumpCommand;
        [XmlIgnore]
        public RelayCommand EditPumpCommand { get { return _EditPumpCommand; } set { _EditPumpCommand = value; OnPropertyChanged(); } }

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



        public PumpInformation()
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
