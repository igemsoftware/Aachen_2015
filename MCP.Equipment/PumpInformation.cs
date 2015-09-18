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
using DynamicDataDisplay.Markers.DataSources;
using System.Windows;

namespace MCP.Equipment
{
    [XmlTypeAttribute(AnonymousType = true)]
    public class PumpInformation : PropertyChangedBase
    {
        private string _PumpID;
        public string PumpID { get { return _PumpID; } set { _PumpID = value; OnPropertyChanged(); } }

        private List<PumpResponseData> _ResponseCurve = new List<PumpResponseData>();
        /// <summary>
        /// What is the response of the pump [ml/h] to a certain setpoint [sph] ?
        /// </summary>
        [XmlArray]
        public List<PumpResponseData> ResponseCurve { get { return _ResponseCurve; } set { _ResponseCurve = value; OnPropertyChanged(); } }
        

        private RelayCommand _EditPumpCommand;
        [XmlIgnore]
        public RelayCommand EditPumpCommand { get { return _EditPumpCommand; } set { _EditPumpCommand = value; OnPropertyChanged(); } }

        #region Drawing the Response Curve
        //private PumpResponseDataCollection _ResponseDataCollection = new PumpResponseDataCollection();//contains only recent datapoints
        //[XmlIgnore]
        //public PumpResponseDataCollection ResponseDataCollection { get { return _ResponseDataCollection; } set { _ResponseDataCollection = value; } }

        private ObservableCollection<PumpResponseData> _ResponseDataSet = new ObservableCollection<PumpResponseData>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<PumpResponseData> ResponseDataSet { get { return _ResponseDataSet; } set { _ResponseDataSet = value; } }

        [XmlIgnore]
        public EnumerableDataSource DataSource { get; set; }
        #endregion



        public PumpInformation()
        {
            ResponseCurve = new List<PumpResponseData>();
            DataSource = new EnumerableDataSource(ResponseDataSet);
            DataSource.DataToPoint = new Func<object,System.Windows.Point>(rd => new Point((rd as PumpResponseData).Setpoint, (rd as PumpResponseData).Response));
        }
        public void LoadResponseCurve()
        {
            foreach (PumpResponseData ri in ResponseCurve)
            {
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
