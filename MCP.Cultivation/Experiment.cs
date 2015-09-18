using MCP.Equipment;
using MCP.Protocol;
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
        public string BaseDirectory { get { return _BaseDirectory; } set { _BaseDirectory = value; OnPropertyChanged(); LoadCultivations(); } }
        
        [XmlIgnore]
        public string DisplayName { get { return string.Format("{0} {1}", Date.ToString("yy-MM-dd"), Title); } }

        private ObservableCollection<Cultivation> _Cultivations = new ObservableCollection<Cultivation>();
        [XmlIgnore]
        public ObservableCollection<Cultivation> Cultivations { get { return _Cultivations; } set { _Cultivations = value; OnPropertyChanged(); } }
        #endregion

        #region Commands
        private RelayCommand _EditExperimentCommand;
        [XmlIgnore]
        public RelayCommand EditExperimentCommand { get { return _EditExperimentCommand; } set { _EditExperimentCommand = value; OnPropertyChanged(); } }

        private RelayCommand _DeleteExperimentCommand;
        [XmlIgnore]
        public RelayCommand DeleteExperimentCommand { get { return _DeleteExperimentCommand; } set { _DeleteExperimentCommand = value; OnPropertyChanged(); } }
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


        public Experiment()
        {
            saveTimer.Tick += delegate
            {
                Save();
            };
            DeleteExperimentCommand = new RelayCommand(async delegate
            {
                bool delete = (await CustomMessageBox.ShowAsync("Deleting an Experiment", string.Format("Do you want to delete {0} ?", DisplayName), System.Windows.MessageBoxImage.Warning, 0, "Keep the experiment", string.Format("Delete {0}", this.Title)) == 1);
                if (delete)
                    try
                    {
                        Directory.Delete(BaseDirectory, true);
                    }
                    catch { }
            });
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
        private async void LoadCultivations()
        {
            if (!Directory.Exists(BaseDirectory))
                return;
            try
            {
                foreach (string dir in Directory.GetDirectories(BaseDirectory, "Reactor_*"))
                {
                    DirectoryInfo di = new DirectoryInfo(dir);
                    Cultivation c = null;
                    if (File.Exists(Path.Combine(dir, di.Name + ".cultivation")))
                        c = Cultivation.LoadFromFile(Path.Combine(dir, di.Name + ".cultivation"));
                    else
                        c = new Cultivation() { BaseDirectory = dir };
                    ParticipantID reactor = (ParticipantID)Enum.Parse(typeof(ParticipantID), di.Name);
                    if (!Inventory.Current.Reactors.ContainsKey(reactor))
                    {
                        await CustomMessageBox.ShowAsync("Reactor Missing", string.Format("A cultivation can't be loaded because {0} was not found.\r\nPlease import or create the reactor and then restart the app.", reactor.GetValueName()), System.Windows.MessageBoxImage.Error, 0, "Understood");
                    }
                    else
                    {
                        c.Reactor = Inventory.Current.Reactors[reactor];
                        c.ChangeParametersCommand = new RelayCommand(async delegate
                        {
                            Cultivation newCultivation = new Cultivation()
                            {
                                Reactor = c.Reactor,
                                DilutionRateSetpoint = c.DilutionRateSetpoint,
                                AgitationRateSetpoint = c.AgitationRateSetpoint,
                                AerationRateSetpoint = c.AerationRateSetpoint,
                                CultureVolume = c.CultureVolume,
                                CultureDescription = c.CultureDescription
                            };
                            SetpointWindow sw = new SetpointWindow() { DataContext = newCultivation };
                            sw.Show();
                            await sw.WaitTask;
                            if (sw.Confirmed)
                            {
                                c.DilutionRateSetpoint = newCultivation.DilutionRateSetpoint;
                                c.AgitationRateSetpoint = newCultivation.AgitationRateSetpoint;
                                c.AerationRateSetpoint = newCultivation.AerationRateSetpoint;
                                c.CultureVolume = newCultivation.CultureVolume;
                                c.CultureDescription = newCultivation.CultureDescription;
                                c.Save();
                                c.SendSetpointUpdate();
                            }
                        });
                        Cultivations.Add(c);
                    }
                }
            }
            catch (Exception ex)
            {
                Task mb = CustomMessageBox.ShowAsync("Can't load", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
            }
        }

        
    }
}
