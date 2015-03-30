using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TCD;

namespace MasterControlProgram
{
    public class ViewModel : PropertyChangedBase
    {
        private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; OnPropertyChanged("SensorDataCollection"); } }

        private ObservableCollection<SensorData> _SensorDataSet = new ObservableCollection<SensorData>();//contains all datapoints
        public ObservableCollection<SensorData> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; OnPropertyChanged("SensorDataSet"); } }
        
        private EnumerableDataSource<SensorData> _DataSource;
        public EnumerableDataSource<SensorData> DataSource { get { return _DataSource; } set { _DataSource = value; OnPropertyChanged("DataSource"); } }

        private DateTime start = DateTime.Now;

        public ViewModel()
        {
            DataSource = new EnumerableDataSource<SensorData>(SensorDataCollection);
            DataSource.SetXMapping(x => x.Time);
            DataSource.SetYMapping(y => y.Value);

            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            dt.Tick += delegate 
            {
                SensorData data = new SensorData((DateTime.Now - start).TotalSeconds, new Random().NextDouble());
                SensorDataSet.Add(data);
                SensorDataCollection.Add(data);
            };
            dt.Start();
        }
    }
}
