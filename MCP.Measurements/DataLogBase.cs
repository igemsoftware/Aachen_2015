using DynamicDataDisplay.Markers.DataSources;
using MCP.Curves;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Serialization;

namespace MCP.Measurements
{
    public class DataLogBase
    {
        public string _FilePath { get; set; }



        //private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        //[XmlIgnore]
        //public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; } }

        private ObservableCollection<DataPoint> _SensorDataSet = new ObservableCollection<DataPoint>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<DataPoint> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; } }

        

        [XmlIgnore]
        public EnumerableDataSource DataSource { get; set; }
        [XmlIgnore]
        public DateTime StartTime { get; set; }

        private bool IsPlotActivated { get; set; }


        public DataLogBase()
        {
            DataSource = new EnumerableDataSource(SensorDataSet);
            DataSource.DataToPoint = new Func<object, Point>(dp => new Point(((dp as DataPoint).Time - StartTime).TotalSeconds, (dp as DataPoint).YValue));
        }

        public void Initialize(params string[] headers)
        {
            bool existed = File.Exists(_FilePath);
            if (!existed)
            {
                StreamWriter writer = File.CreateText(_FilePath);
                writer.WriteLine(string.Join("\t", headers));
                writer.Flush();
                writer.Dispose();
            }
        }

        public virtual bool AddRawData(DataPoint data)
        {
            WriteLine(data.ToString());
            if (!IsPlotActivated)
                return true;
            SensorDataSet.Add(data);
            return true;
        }
        public void WriteLine(string text)
        {
            StreamWriter writer = File.AppendText(_FilePath);
            writer.WriteLine(text);
            writer.Flush();
            writer.Dispose();
        }

        public void ActivatePlot()
        {
            IsPlotActivated = true;
            SensorDataSet.Clear();
            string[] lines = File.ReadAllLines(_FilePath);
            for (int i = 1; i < lines.Length; i++)
            {
                DataPoint dp = new DataPoint(lines[i]);
                SensorDataSet.Add(dp);
            }
        }
        public void DeactivatePlot()
        {
            IsPlotActivated = false;
            SensorDataSet.Clear();
        }
    }
}
