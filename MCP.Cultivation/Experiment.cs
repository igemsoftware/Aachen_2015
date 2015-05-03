using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;
using TCD;
using TCD.Controls;

namespace MCP.Cultivation
{
    public class Experiment : PropertyChangedBase
    {
        #region Ignored Properties
        private string _BaseDirectory;
        [XmlIgnore]
        public string BaseDirectory { get { return _BaseDirectory; } set { _BaseDirectory = value; OnPropertyChanged(); } }
        
        [XmlIgnore]
        public string DisplayName { get { return string.Format("{0} {1}", Date.ToString("yy-MM-dd"), Title); } }

        private RelayCommand _EditExperimentCommand;
        [XmlIgnore]
        public RelayCommand EditExperimentCommand { get { return _EditExperimentCommand; } set { _EditExperimentCommand = value; OnPropertyChanged(); } }
        
			
        #endregion

        #region Serialized Properties
        private DateTime _Date = DateTime.Today;
        [XmlElement]
        public DateTime Date { get { return _Date; } set { _Date = value; OnPropertyChanged(); OnPropertyChanged("DisplayName"); } }//TODO: implement to change the experimen date afterwards
        
        private string _Title;
        [XmlElement]
        public string Title { get { return _Title; } set { _Title = value; OnPropertyChanged(); OnPropertyChanged("DisplayName"); } }//TODO: implement to change the experiment title afterwards

        private string _Description;
        [XmlElement]
        public string Description { get { return _Description; } set { _Description = value; OnPropertyChanged(); saveTimer.Start(); } }
        #endregion

        #region Private Stuff
        private DispatcherTimer saveTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
        #endregion


        private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        [XmlIgnore]
        public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; OnPropertyChanged("SensorDataCollection"); } }

        private ObservableCollection<SensorData> _SensorDataSet = new ObservableCollection<SensorData>();//contains all datapoints
        [XmlIgnore]
        public ObservableCollection<SensorData> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; OnPropertyChanged("SensorDataSet"); } }

        private EnumerableDataSource<SensorData> _DataSource;
        [XmlIgnore]
        public EnumerableDataSource<SensorData> DataSource { get { return _DataSource; } set { _DataSource = value; OnPropertyChanged("DataSource"); } }

        private DateTime start = DateTime.Now;



        public Experiment()
        {
            saveTimer.Tick += delegate
            {
                Save();
            };





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
        public override string ToString()
        {
            return DisplayName;
        }

        public void Save()
        {
            if (string.IsNullOrWhiteSpace(BaseDirectory))
                return;
            try
            {
                DirectoryInfo di = new DirectoryInfo(BaseDirectory);
                if (!di.Exists)
                    Directory.CreateDirectory(BaseDirectory);
                string expFileName = Path.Combine(BaseDirectory, DisplayName + ".experiment");
                XmlSerializer serializer = new XmlSerializer(typeof(Experiment));
                TextWriter textWriter = new StreamWriter(expFileName);
                serializer.Serialize(textWriter, this);
                textWriter.Close();
                textWriter.Dispose();
                saveTimer.Stop();
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't save", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
        }
        public static Experiment LoadFromDirectory(string dir)
        {
            try
            {
                DirectoryInfo di = new DirectoryInfo(dir);
                string expFileName = Path.Combine(dir, di.Name + ".experiment");
                XmlSerializer deserializer = new XmlSerializer(typeof(Experiment));
                TextReader textReader = new StreamReader(expFileName);
                Experiment experiment = (Experiment)deserializer.Deserialize(textReader);
                textReader.Close();
                textReader.Dispose();
                experiment.BaseDirectory = dir;
                return experiment;
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't load", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
            return null;
        }

    }
}
