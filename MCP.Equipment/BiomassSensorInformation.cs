﻿using System;
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
using Accord.Statistics.Models.Regression.Linear;
using DynamicDataDisplay.Markers.DataSources;
using System.Windows;

namespace MCP.Equipment
{
    [XmlTypeAttribute(AnonymousType = true)]
    public class BiomassSensorInformation : PropertyChangedBase
    {
        private string _SensorID = null;
        public string SensorID { get { return _SensorID; } set { _SensorID = value; OnPropertyChanged(); } }

        private double _AirThreshold;
        [XmlElement]
        public double AirThreshold { get { return _AirThreshold; } set { _AirThreshold = value; OnPropertyChanged(); } }
        
        private List<BiomassResponseData> _ResponseCurve = new List<BiomassResponseData>();
        /// <summary>
        /// What actual OD/CDW correlates to the analog sensor values?
        /// </summary>
        [XmlArray]
        public List<BiomassResponseData> ResponseCurve { get { return _ResponseCurve; } set { _ResponseCurve = value; OnPropertyChanged(); } }

        private PolynomialRegression _ResponseRegressionOD;
        [XmlIgnore]
        public PolynomialRegression ResponseRegressionOD { get { return _ResponseRegressionOD; } set { _ResponseRegressionOD = value; OnPropertyChanged(); } }
        
        private RelayCommand _EditSensorCommand;
        [XmlIgnore]
        public RelayCommand EditSensorCommand { get { return _EditSensorCommand; } set { _EditSensorCommand = value; OnPropertyChanged(); } }

        #region Drawing the Response Curve
        //private BiomassResponseDataCollection _ResponseDataCollection = new BiomassResponseDataCollection();//contains only recent datapoints
        //[XmlIgnore]
        //public BiomassResponseDataCollection ResponseDataCollection { get { return _ResponseDataCollection; } set { _ResponseDataCollection = value; } }

        private ObservableCollection<BiomassResponseData> _ResponseDataSet = new ObservableCollection<BiomassResponseData>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<BiomassResponseData> ResponseDataSet { get { return _ResponseDataSet; } set { _ResponseDataSet = value; } }

        [XmlIgnore]
        public EnumerableDataSource ODDataSource { get; set; }
        [XmlIgnore]
        public EnumerableDataSource CDWDataSource { get; set; }
        #endregion



        public BiomassSensorInformation()
        {
            ResponseCurve = new List<BiomassResponseData>();
            ODDataSource = new EnumerableDataSource(ResponseDataSet);
            ODDataSource.DataToPoint = new Func<object, Point>(rd => new Point((rd as BiomassResponseData).Analog, (rd as BiomassResponseData).OD));
            CDWDataSource = new EnumerableDataSource(ResponseDataSet);
            CDWDataSource.DataToPoint = new Func<object, Point>(rd => new Point((rd as BiomassResponseData).Analog, (rd as BiomassResponseData).CDW));
        }
        public void LoadResponseCurve()
        {
            foreach (BiomassResponseData ri in ResponseCurve)
            {
                ResponseDataSet.Add(ri);
            }
            // do the polynomial regression
            ResponseRegressionOD = PolynomialRegression.FromData(3, ResponseCurve.Select(p => p.Analog).ToArray(), ResponseCurve.Select(p => p.OD).ToArray());
        }

        public double CaluclateOD(double analog)
        {
            return ResponseRegressionOD.Compute(analog);
            //return ResponseCurve.ExponentialTransformAnalogToOD(analog);
        }
        public double CaluclateCDW(double analog)
        {
            return ResponseCurve.ExponentialTransformAnalogToCDW(analog);
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
                sensorInfo.LoadResponseCurve();
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
