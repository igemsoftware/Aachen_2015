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

        private Experiment _ActiveExperiment = null;
        public Experiment ActiveExperiment { get { return _ActiveExperiment; } set { _ActiveExperiment = value; OnPropertyChanged(); } }
        //TODO: remove the ActiveExperiment property and give each Cultivation its own IsRunning property.
        //TODO: before a Cultivation can be marked as active, check if another Cultivation is using a same reactor
        //TODO: make a Dictionary<ParticipantID, Cultivation> to keep track which reactor is in use and also to forward incoming messages
        //TODO: save the IsRunning property to the Cultivation file
        //TODO: when loading the Experiments, check if there's a collision between reactors that are in use. If so show a CustomMessageBox to decide which cultivation to stop
        //      Advantages: this way multiple experiments can run at the same time, independent from each other
        //TODO: when a Cultivation is started for the first time, save the start time
        //TODO: when a Cultivation is started but there's already data, ask the user what to do (Delete/Keep)

			

        private RelayCommand _AddExperimentCommand;
        public RelayCommand AddExperimentCommand { get { return _AddExperimentCommand; } set { _AddExperimentCommand = value; OnPropertyChanged(); } }

        
        public ExperimentLibrary()
        {
            AddExperimentCommand = new RelayCommand(async delegate
            {
                Experiment exp = new Experiment();
                ExperimentInformationWindow eiw = new ExperimentInformationWindow("Create Experimet", true) { DataContext = exp };
                List<ParticipantID> selectedReactors = await eiw.ShowAsync();
                if (eiw.Confirmed)
                {
                    foreach (ParticipantID selectedReactor in selectedReactors)
                        Directory.CreateDirectory(Path.Combine(experimentsDirectory, exp.DisplayName, selectedReactor.ToString()));
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
            if (!Directory.Exists(experimentsPath))
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
                    if (ActiveExperiment == Experiments[i])
                        ActiveExperiment = null;
                    Experiments.RemoveAt(i);
                    i--;
                }
            }
            //add new experiments
            foreach (string dir in dirs)
            {
                Experiment exp = Experiment.LoadFromDirectory(dir);
                if (exp != null)
                {
                    exp.EditExperimentCommand = new RelayCommand(async delegate
                    {
                        Experiment newexp = new Experiment() { Title = exp.Title, Description = exp.Description, Date = exp.Date, Cultivations = exp.Cultivations };
                        ExperimentInformationWindow eiw = new ExperimentInformationWindow("Edit Experimet", false) { DataContext = newexp };
                        await eiw.ShowAsync();
                        if (eiw.Confirmed)
                        {
                            exp.Description = newexp.Description;
                            exp.Save();
                        }
                    });
                    Experiments.Add(exp);
                }
            }
        }




    }
}
