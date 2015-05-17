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
using MCP.Measurements;


namespace MasterControlProgram
{
    public class ViewModel : PropertyChangedBase
    {
        private MCPSettings _MCPSettings = new MCPSettings();
        public MCPSettings MCPSettings { get { return _MCPSettings; } set { _MCPSettings = value; OnPropertyChanged(); } }

        private Inventory _Inventory = new Inventory();
        public Inventory Inventory { get { return _Inventory; } set { _Inventory = value; OnPropertyChanged(); } }

        private ExperimentLibrary _ExperimentLibrary = new ExperimentLibrary();
        public ExperimentLibrary ExperimentLibrary { get { return _ExperimentLibrary; } set { _ExperimentLibrary = value; OnPropertyChanged(); } }

        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }

        #region Debug Properties
        public bool IsDebugMode { get { return System.Diagnostics.Debugger.IsAttached; } }

        private bool _IsRandomizerEnabled;
        public bool IsRandomizerEnabled { get { return _IsRandomizerEnabled; } set { _IsRandomizerEnabled = value; OnPropertyChanged(); } }

        private static Random rnd = new Random();
			
        #endregion




        public ViewModel()
        {
            MCPSettings.HomeDirectoryChanged += MCPSettings_HomeDirectoryChanged;
            Inventory.Initialize(MCPSettings.PumpDirectoryPath, MCPSettings.ReactorDirectoryPath);
            ExperimentLibrary.Initialize(MCPSettings.ExperimentsDirectoryPath);
            PrimarySerial.NewMessageReceived += PrimarySerial_NewMessageReceived;
            if (IsDebugMode)
                StartRandomValuesGenerator();
        }

        private void PrimarySerial_NewMessageReceived(object sender, Message message)
        {
            Experiment receivingExperiment;
            Cultivation receiver = ExperimentLibrary.FindRunningCultivation(message.Sender, out receivingExperiment);
            if (receiver == null)
                return;
            receiver.CultivationLog.ReceiveMessage(message);
        }

        private void MCPSettings_HomeDirectoryChanged(object sender, EventArgs e)
        {
            Inventory.Initialize(MCPSettings.PumpDirectoryPath, MCPSettings.ReactorDirectoryPath);
            ExperimentLibrary.Initialize(MCPSettings.ExperimentsDirectoryPath);
        }


        private void StartRandomValuesGenerator()
        {
            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(500) };
            dt.Tick += delegate
            {
                if (IsRandomizerEnabled)
                {
                    Message msg = new Message(ParticipantID.Reactor_1, ParticipantID.MCP, MessageType.Data, string.Format("{0}\t{1}\t{2}", DimensionSymbol.Temperature, 36 + 2 * rnd.NextDouble(), Unit.Celsius));
                    PrimarySerial.InterpretMessage(msg.Raw);
                }                
            };
            dt.Start();
        }

    }
}

