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
using TCD.Controls;

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


        private async void ScanExperimentsDirectory()
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
                    await LookForConflictsAsync(exp);
                    foreach (Cultivation c in exp.Cultivations)
                    {
                        ParticipantID reactorInQuestion = c.Reactor.ParticipantID;
                        c.StartCultivationCommand = new RelayCommand(async delegate
                        {
                            Experiment conflictingExperiment;
                            Cultivation conflicting = FindRunningCultivation(c.Reactor.ParticipantID, out conflictingExperiment);
                            if (conflicting == null)
                            {
                                c.IsRunning = true;
                            }
                            else//ask the user to stop the other cultivation
                            {
                                int result = await CustomMessageBox.ShowAsync(
                                    "Conflict - Reactor already in use",
                                    string.Format("{0} is already in use by {1}\r\n\r\nDo you want to stop the other cultivation?", reactorInQuestion.GetValueName(), conflictingExperiment.ToString()),
                                    System.Windows.MessageBoxImage.Warning,
                                    1,
                                    string.Format("Stop {0}", conflictingExperiment.ToString()),
                                    "Cancel");
                                if (result == 0)//if he wants to proceed and stop the other experiment
                                {
                                    conflicting.IsRunning = false;
                                    conflicting.Save();
                                    c.IsRunning = true;
                                }
                            }
                            if (c.IsRunning)//everything is GO - start the culture and save the time
                                c.StartTime = DateTime.Now;
                            c.Save();
                        }); 
                        c.StopCultivationCommand = new RelayCommand(delegate
                        {
                            c.IsRunning = false;
                            c.Save();
                        });
                    }
                    Experiments.Add(exp);
                }
            }
        }
        private async Task LookForConflictsAsync(Experiment exp)
        {
            foreach (Cultivation c in exp.Cultivations)
            {
                ParticipantID reactorInQuestion = c.Reactor.ParticipantID;
                if (c.IsRunning)//the loaded experiment is running -> make sure that there's no conflict
                {
                    Experiment conflictingExperiment;
                    Cultivation conflicting = FindRunningCultivation(reactorInQuestion, out conflictingExperiment);
                    if (conflicting != null)//if we don't know of any other experiment that is using the same reactor
                    {
                        //ask the user to choose
                        int result = await CustomMessageBox.ShowAsync(
                            "Conflict - Reactor already in use",
                            string.Format("{0} is already in use by {1}\r\n\r\nPlease select the experiment that you want to prioritize.", reactorInQuestion.GetValueName(), conflictingExperiment),
                            System.Windows.MessageBoxImage.Warning,
                            0,
                            conflictingExperiment.ToString(),
                            exp.ToString());
                        if (result == 0)//if he wants to keep the already loaded experiment running
                        {
                            c.IsRunning = false;//stop the new one
                            c.Save();
                        }
                        else//stop the other cultivation and overwrite the reservation
                        {
                            conflicting.IsRunning = false;
                            conflicting.Save();
                        }
                    }

                }
            }
        }

        public Cultivation FindRunningCultivation(ParticipantID reactor, out Experiment conflictingExperiment)
        {
            foreach (Experiment exp in Experiments)
            {
                foreach (Cultivation culti in exp.Cultivations)
                {
                    if (culti.IsRunning && culti.Reactor.ParticipantID == reactor)
                    {
                        conflictingExperiment = exp;
                        return culti;
                    }
                }
            }
            conflictingExperiment = null;
            return null;
        }

    }
}
