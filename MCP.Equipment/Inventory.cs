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
        private FileSystemWatcher sensorWatcher = null;
        private string pumpDirectory = null;
        private string reactorDirectory = null;
        private string sensorDirectory = null;

        public static Inventory Current { get; set; }
        

        private Dictionary<string, PumpInformation> _Pumps = new Dictionary<string, PumpInformation>();
        public Dictionary<string, PumpInformation> Pumps { get { return _Pumps; } set { _Pumps = value; OnPropertyChanged(); } }

        private Dictionary<string, BiomassSensorInformation> _BiomassSensors = new Dictionary<string, BiomassSensorInformation>();
        public Dictionary<string, BiomassSensorInformation> BiomassSensors { get { return _BiomassSensors; } set { _BiomassSensors = value; OnPropertyChanged(); } }

        private Dictionary<string, GasSensorInformation> _GasSensors = new Dictionary<string, GasSensorInformation>();
        public Dictionary<string, GasSensorInformation> GasSensors { get { return _GasSensors; } set { _GasSensors = value; OnPropertyChanged(); } }
        

        private Dictionary<ParticipantID, ReactorInformation> _Reactors = new Dictionary<ParticipantID, ReactorInformation>();
        public Dictionary<ParticipantID, ReactorInformation> Reactors { get { return _Reactors; } set { _Reactors = value; OnPropertyChanged(); } }


        #region Commands
        private RelayCommand _ImportPumpCommand;
        public RelayCommand ImportPumpCommand { get { return _ImportPumpCommand; } set { _ImportPumpCommand = value; OnPropertyChanged(); } }

        private RelayCommand _ImportSensorCommand;
        public RelayCommand ImportSensorCommand { get { return _ImportSensorCommand; } set { _ImportSensorCommand = value; OnPropertyChanged(); } }

        
        private RelayCommand _AddReactorCommand;
        public RelayCommand AddReactorCommand { get { return _AddReactorCommand; } set { _AddReactorCommand = value; OnPropertyChanged(); } }

        private RelayCommand _ImportReactorCommand;
        public RelayCommand ImportReactorCommand { get { return _ImportReactorCommand; } set { _ImportReactorCommand = value; OnPropertyChanged(); } }
        
		//TODO: commands to delete pumps or reactors
			
        #endregion


        public Inventory()
        {
            Current = this;
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
            ImportSensorCommand = new RelayCommand(delegate
            {
                OpenFileDialog ofd = new OpenFileDialog() { Filter = "Biomass Sensor Calibration Files|*.biomass" };
                DialogResult result = ofd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    FileInfo fi = new FileInfo(ofd.FileName);
                    try
                    {
                        File.Copy(ofd.FileName, Path.Combine(sensorDirectory, fi.Name));
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
        public void Initialize(string pumpPath, string reactorPath, string sensorPath)
        {
            if (!Directory.Exists(pumpPath) || !Directory.Exists(reactorPath) || !Directory.Exists(sensorPath))
                return;
            pumpDirectory = pumpPath;
            pumpWatcher = new FileSystemWatcher(pumpPath, "*.pump");
            pumpWatcher.Renamed += pumpWatcher_Changed;
            pumpWatcher.Created += pumpWatcher_Changed;
            pumpWatcher.Deleted += pumpWatcher_Changed;
            pumpWatcher.EnableRaisingEvents = true;
            ScanPumpDirectory();
            //
            reactorDirectory = reactorPath;
            reactorWatcher = new FileSystemWatcher(reactorPath, "*.reactor");
            reactorWatcher.Renamed += reactorWatcher_Changed;
            reactorWatcher.Created += reactorWatcher_Changed;
            reactorWatcher.Deleted += reactorWatcher_Changed;
            reactorWatcher.EnableRaisingEvents = true;
            ScanReactorDirectory();
            //
            sensorDirectory = sensorPath;
            sensorWatcher = new FileSystemWatcher(sensorPath, "*.*"); //TDO: filter for .biomass or .sensor
            sensorWatcher.Renamed += sensorWatcher_Changed;
            sensorWatcher.Created += sensorWatcher_Changed;
            sensorWatcher.Deleted += sensorWatcher_Changed;
            sensorWatcher.EnableRaisingEvents = true;
            ScanSensorDirectory();
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
                            ResponseCurve = pump.ResponseCurve
                        };
                        PumpInformationWindow piw = new PumpInformationWindow("Edit Pump", false, newPump);
                        piw.Show();
                        await piw.WaitTask;
                        if (piw.Confirmed)
                        {
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
        }
        public async void sensorWatcher_Changed(object sender, FileSystemEventArgs e)
        {
            await Task.Delay(100);//this is to avoid file access conflicts
            ScanSensorDirectory();
        }
        private void ScanSensorDirectory()
        {
            Dictionary<string, BiomassSensorInformation> newBiomassSensors = new Dictionary<string, BiomassSensorInformation>();
            Dictionary<string, GasSensorInformation> newGasSensors = new Dictionary<string, GasSensorInformation>();

            foreach (string file in Directory.EnumerateFiles(sensorDirectory, "*.biomass"))
            {
                BiomassSensorInformation sensor = BiomassSensorInformation.LoadFromFile(file);
                if (sensor != null)
                {
                    sensor.EditSensorCommand = new RelayCommand(async delegate
                    {
                        BiomassSensorInformationWindow bsiw = new BiomassSensorInformationWindow("Sensor Calibration", false, sensor);
                        bsiw.Show();
                        await bsiw.WaitTask;
                    });
                    newBiomassSensors.Add(sensor.SensorID, sensor);
                }

            }
            foreach (string file in Directory.EnumerateFiles(sensorDirectory, "*.sensor"))
            {
                GasSensorInformation sensor = GasSensorInformation.LoadFromFile(file);
                if (sensor != null)
                {
                    sensor.EditSensorCommand = new RelayCommand(async delegate
                    {
                        GasSensorInformationWindow bsiw = new GasSensorInformationWindow("Sensor Calibration", false, sensor);
                        bsiw.Show();
                        await bsiw.WaitTask;
                    });
                    newGasSensors.Add(sensor.SensorID, sensor);
                }
            }

            BiomassSensors = newBiomassSensors;
            GasSensors = newGasSensors;
        }
    }

}