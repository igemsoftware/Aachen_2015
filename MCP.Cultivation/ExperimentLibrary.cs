using MCP.Cultivation;
using MCP.Equipment;
using MCP.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TCD;

namespace MCP.Cultivation
{
    public class ExperimentLibrary : PropertyChangedBase
    {
        private FileSystemWatcher experimentWatcher = null;
        private string experimentsDirectory = null;
        private DispatcherTimer scanTimer = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(200) };//the Experiments collection can't be changed from the thread where the FileSystemWatcher changes are handled - this solution works

        private ObservableCollection<Experiment> _Experiments = new ObservableCollection<Experiment>();
        public ObservableCollection<Experiment> Experiments { get { return _Experiments; } set { _Experiments = value; OnPropertyChanged(); } }

        private RelayCommand _AddExperimentCommand;
        public RelayCommand AddExperimentCommand { get { return _AddExperimentCommand; } set { _AddExperimentCommand = value; OnPropertyChanged(); } }

        
        public ExperimentLibrary()
        {
            AddExperimentCommand = new RelayCommand(async delegate
            {
                Experiment exp = new Experiment();
                ExperimentInformationWindow eiw = new ExperimentInformationWindow("Create Experimet", true) { DataContext = exp };
                eiw.Show();
                await eiw.WaitTask;
                if (eiw.Confirmed)
                {
                    exp.BaseDirectory = Path.Combine(experimentsDirectory, exp.DisplayName);//this authorizes the experiment to be saved or save itself (when a property changes)
                    exp.Save();
                }
            });
            scanTimer.Tick += delegate 
            {
                scanTimer.Stop();
                ScanExperimentsDirectory();
            };
        }

        public void Initialize(string experimentsPath)
        {
            if (string.IsNullOrWhiteSpace(experimentsPath))
                return;
            experimentsDirectory = experimentsPath;
            experimentWatcher = new FileSystemWatcher(experimentsPath, "*");
            experimentWatcher.Renamed += delegate { scanTimer.Start(); };
            experimentWatcher.Created += delegate { scanTimer.Start(); };
            experimentWatcher.Deleted += delegate { scanTimer.Start(); };
            experimentWatcher.EnableRaisingEvents = true;
            ScanExperimentsDirectory();
        }


        private void ScanExperimentsDirectory()
        {
            //in order to preserve the ObservableCollection we only want to add new experiments, or remove experiments where the folder was deleted
            //first get all subdirectories in the experiments directory
            List<string> dirs = Directory.EnumerateDirectories(experimentsDirectory).ToList();
            //remove all dirs which are already loaded and remove experiments that got deleted
            for (int i = 0; i < Experiments.Count; i++)
            {
                if (dirs.Contains(Experiments[i].BaseDirectory))//these experiments are already loaded and can be ignored
                {
                    dirs.Remove(Experiments[i].BaseDirectory);
                }
                else//unload/remove experiments where the folder does not exist anymore
                {
                    Experiments.RemoveAt(i);
                    i--;
                }
            }
            //add new experiments
            foreach (string dir in dirs)
            {
                Experiment exp = Experiment.LoadFromDirectory(dir);
                exp.EditExperimentCommand = new RelayCommand(async delegate
                {
                    Experiment newexp = new Experiment() { Title = exp.Title, Description = exp.Description, Date = exp.Date };
                    ExperimentInformationWindow eiw = new ExperimentInformationWindow("Edit Experimet", false) { DataContext = newexp };
                    eiw.Show();
                    await eiw.WaitTask;
                    if (eiw.Confirmed)
                    {
                        exp.Description = newexp.Description;
                        //TODO: save involved reactors
                        exp.Save();
                    }
                });
                if (exp != null)
                    Experiments.Add(exp);
            }
        }




    }
}
