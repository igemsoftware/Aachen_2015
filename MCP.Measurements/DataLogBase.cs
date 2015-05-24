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

namespace MCP.Measurements
{
    public class DataLogBase
    {
        internal string _FilePath { get; set; }



        private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        [XmlIgnore]
        public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; } }

        private ObservableCollection<RawData> _SensorDataSet = new ObservableCollection<RawData>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<RawData> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; } }

        

        [XmlIgnore]
        public EnumerableDataSource<RawData> DataSource { get; set; }
        [XmlIgnore]
        public DateTime StartTime { get; set; }


        public DataLogBase()
        {
            DataSource = new EnumerableDataSource<RawData>(SensorDataCollection);
            DataSource.SetXMapping(x => (x.Time - StartTime).TotalSeconds);
            DataSource.SetYMapping(y => y.Value);
        }

        internal async void InitializeAsync(params string[] headers)
        {
            bool existed = File.Exists(_FilePath);
            if (!existed)
            {
                StreamWriter writer = File.CreateText(_FilePath);
                await writer.WriteLineAsync(string.Join("\t", headers));
                await writer.FlushAsync();
                writer.Dispose();
            }
        }

        public virtual void AddRawData(RawData data)
        {
            WriteLine(data.ToString());
            SensorDataSet.Add(data);
            SensorDataCollection.Add(data);
        }
        internal async void WriteLine(string text)
        {
            StreamWriter writer = File.AppendText(_FilePath);
            await writer.WriteLineAsync(text);
            await writer.FlushAsync();
            writer.Dispose();
        }
    }
}
