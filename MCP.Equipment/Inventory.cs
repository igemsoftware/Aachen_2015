using MCP.Protocol;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using TCD;
using TCD.Controls;

namespace MCP.Equipment
{
    public class Inventory : PropertyChangedBase
    {
        private FileSystemWatcher pumpWatcher = null;
        private FileSystemWatcher reactorWatcher = null;
        private string pumpDirectory = null;
        private string reactorDirectory = null;

        public static Inventory Current { get; set; }
        

        private Dictionary<string, PumpInformation> _Pumps = new Dictionary<string, PumpInformation>();
        public Dictionary<string, PumpInformation> Pumps { get { return _Pumps; } set { _Pumps = value; OnPropertyChanged(); } }

        private Dictionary<ParticipantID, ReactorInformation> _Reactors = new Dictionary<ParticipantID, ReactorInformation>();
        public Dictionary<ParticipantID, ReactorInformation> Reactors { get { return _Reactors; } set { _Reactors = value; OnPropertyChanged(); } }

        #region Commands
        private RelayCommand _AddPumpCommand;
        public RelayCommand AddPumpCommand { get { return _AddPumpCommand; } set { _AddPumpCommand = value; OnPropertyChanged(); } }

        private RelayCommand _ImportPumpCommand;
        public RelayCommand ImportPumpCommand { get { return _ImportPumpCommand; } set { _ImportPumpCommand = value; OnPropertyChanged(); } }

        private RelayCommand _AddReactorCommand;
        public RelayCommand AddReactorCommand { get { return _AddReactorCommand; } set { _AddReactorCommand = value; OnPropertyChanged(); } }

        private RelayCommand _ImportReactorCommand;
        public RelayCommand ImportReactorCommand { get { return _ImportReactorCommand; } set { _ImportReactorCommand = value; OnPropertyChanged(); } }
        
		//TODO: commands to delete pumps or reactors
			
        #endregion


        public Inventory()
        {
            Current = this;
            AddPumpCommand = new RelayCommand(async delegate
            {
                PumpInformation newPump = new PumpInformation();
                PumpInformationWindow piw = new PumpInformationWindow("Add New Pump", true) { DataContext = newPump };
                piw.Show();
                await piw.WaitTask;
                if (piw.Confirmed)
                {
                    newPump.SaveTo(pumpDirectory);
                }
            });
            ImportPumpCommand = new RelayCommand(delegate
            {
                OpenFileDialog ofd = new OpenFileDialog() { Filter = "Pump Calibration Files|*.pump" };
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    try
                    {
                        File.Copy(ofd.FileName, Path.Combine(pumpDirectory, fi.Name));
                    }
                    catch (Exception ex)
                    {
                        Task mb = CustomMessageBox.ShowAsync("Can't import", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
                    }
                }
            });
            AddReactorCommand = new RelayCommand(async delegate
            {
                ReactorInformation newReactor = new ReactorInformation();
                ReactorInformationWindow piw = new ReactorInformationWindow("Add New Reactor", true, Pumps.Keys) { DataContext = newReactor };
                piw.Show();
                await piw.WaitTask;
                if (piw.Confirmed)
                {
                    newReactor.SaveTo(reactorDirectory);
                }
            });
            ImportReactorCommand = new RelayCommand(delegate
            {
                OpenFileDialog ofd = new OpenFileDialog() { Filter = "Reactor Files|*.reactor" };
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    try
                    {
                        File.Copy(ofd.FileName, Path.Combine(reactorDirectory, fi.Name));
                    }
                    catch (Exception ex)
                    {
                        Task mb = CustomMessageBox.ShowAsync("Can't import", "There was an error:\r\n\r\n" + ex.Message, System.Windows.MessageBoxImage.Error, 0, "Ok");
                    }
                }
            });
        }
        public void Initialize(string pumpPath, string reactorPath)
        {
            if (string.IsNullOrWhiteSpace(pumpPath) || string.IsNullOrWhiteSpace(reactorPath))
                return;
            pumpDirectory = pumpPath;
            pumpWatcher = new FileSystemWatcher();
            pumpWatcher.Path = pumpPath;
            pumpWatcher.Renamed += pumpWatcher_Changed;
            pumpWatcher.Created += pumpWatcher_Changed;
            pumpWatcher.Deleted += pumpWatcher_Changed;
            pumpWatcher.Filter = "*.pump";
            pumpWatcher.EnableRaisingEvents = true;
            ScanPumpDirectory();
            //
            reactorDirectory = reactorPath;
            reactorWatcher = new FileSystemWatcher();
            reactorWatcher.Path = reactorPath;
            reactorWatcher.Renamed += reactorWatcher_Changed;
            reactorWatcher.Created += reactorWatcher_Changed;
            reactorWatcher.Deleted += reactorWatcher_Changed;
            reactorWatcher.Filter = "*.reactor";
            reactorWatcher.EnableRaisingEvents = true;
            ScanReactorDirectory();
        }

        public async void pumpWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(100);//this is to avoid file access conflicts
            ScanPumpDirectory();
        }
        private void ScanPumpDirectory()
        {
            Dictionary<string, PumpInformation> newPumps = new Dictionary<string, PumpInformation>();
            foreach (string file in Directory.EnumerateFiles(pumpDirectory, "*.pump"))
            {
                PumpInformation pump = PumpInformation.LoadFromFile(file);
                if (pump != null)
                {
                    pump.EditPumpCommand = new RelayCommand(async delegate
                    {
                        PumpInformation newPump = new PumpInformation()
                        {
                            PumpID = pump.PumpID,
                            SpecificPumpingRate = pump.SpecificPumpingRate
                        };
                        PumpInformationWindow piw = new PumpInformationWindow("Edit Pump", false) { DataContext = newPump };
                        piw.Show();
                        await piw.WaitTask;
                        if (piw.Confirmed)
                        {
                            pump.SpecificPumpingRate = newPump.SpecificPumpingRate;
                            pump.SaveTo(pumpDirectory);
                        }
                    });
                    newPumps.Add(pump.PumpID, pump);
                }
            }
            Pumps = newPumps;
        }
        public async void reactorWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(100);//this is to avoid file access conflicts
            ScanReactorDirectory();
        }
        private void ScanReactorDirectory()
        {
            Dictionary<ParticipantID, ReactorInformation> newReactors = new Dictionary<ParticipantID, ReactorInformation>();
            foreach (string file in Directory.EnumerateFiles(reactorDirectory, "*.reactor"))
            {
                ReactorInformation reactor = ReactorInformation.LoadFromFile(file);
                if (reactor != null)
                {
                    reactor.EditReactorCommand = new RelayCommand(async delegate
                    {
                        ReactorInformation newReactor = new ReactorInformation()
                        {
                            ParticipantID = reactor.ParticipantID,
                            FeedPumpID = reactor.FeedPumpID,
                            AerationPumpID = reactor.AerationPumpID,
                            HarvestPumpID = reactor.HarvestPumpID
                        };
                        ReactorInformationWindow riw = new ReactorInformationWindow("Edit Reactor", false, Pumps.Keys) { DataContext = newReactor };
                        riw.Show();
                        await riw.WaitTask;
                        if (riw.Confirmed)
                        {
                            reactor.FeedPumpID = newReactor.FeedPumpID;
                            reactor.AerationPumpID = newReactor.AerationPumpID;
                            reactor.HarvestPumpID = newReactor.HarvestPumpID;
                            reactor.SaveTo(reactorDirectory);
                        }
                    });
                    newReactors.Add(reactor.ParticipantID, reactor);
                }
            }
            Reactors = newReactors;

            //Dictionary<ParticipantID, ReactorInformation> reactors = new Dictionary<ParticipantID, ReactorInformation>();
            //foreach (string file in Directory.EnumerateFiles(reactorDirectory, "*.reactor"))
            //{
            //    FileInfo fi = new FileInfo(file);
            //    ReactorInformation ri = ReactorInformation.LoadFromFile(file);
            //    reactors.Add(ri.ParticipantID, ri);
            //}
            ////add or update all reactors
            //List<ParticipantID> extraReactors = new List<ParticipantID>();
            //foreach (var kvp in Reactors)
            //{
            //    if (reactors.ContainsKey(kvp.Key))
            //    {
            //        Reactors[kvp.Key] = reactors[kvp.Key];
            //        reactors.Remove(kvp.Key);
            //    }
            //    else
            //    {
            //        extraReactors.Add(kvp.Key);
            //    }
            //}
            ////remove extra reactors
            //foreach (ParticipantID pi in extraReactors)
            //{
            //    Reactors.Remove(pi);
            //}
        }
    }
}
