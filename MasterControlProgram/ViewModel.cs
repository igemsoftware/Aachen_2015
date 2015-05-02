using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TCD;
using MCP.Equipment;
using MCP.Protocol;
using MCP.Cultivation;


namespace MasterControlProgram
{
    public class ViewModel : PropertyChangedBase
    {
        private MCPSettings _MCPSettings = new MCPSettings();
        public MCPSettings MCPSettings { get { return _MCPSettings; } set { _MCPSettings = value; OnPropertyChanged(); } }

        private Inventory _Inventory = new Inventory();
        public Inventory Inventory { get { return _Inventory; } set { _Inventory = value; OnPropertyChanged(); } }

        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }


        private ExperimentLibrary _ExperimentLibrary = new ExperimentLibrary();
        public ExperimentLibrary ExperimentLibrary { get { return _ExperimentLibrary; } set { _ExperimentLibrary = value; OnPropertyChanged(); } }
        
				

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
            //
            MCPSettings.HomeDirectoryChanged += MCPSettings_HomeDirectoryChanged;
            Inventory.Initialize(MCPSettings.PumpDirectoryPath, MCPSettings.ReactorDirectoryPath);
        }

        private void MCPSettings_HomeDirectoryChanged(object sender, EventArgs e)
        {
            Inventory.Initialize(MCPSettings.PumpDirectoryPath, MCPSettings.ReactorDirectoryPath);
        }

        private void ScanForExperiments()
        {

        }

    }
}

