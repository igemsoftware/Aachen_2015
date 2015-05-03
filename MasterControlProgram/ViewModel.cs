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

        private ExperimentLibrary _ExperimentLibrary = new ExperimentLibrary();
        public ExperimentLibrary ExperimentLibrary { get { return _ExperimentLibrary; } set { _ExperimentLibrary = value; OnPropertyChanged(); } }

        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }




        

        public ViewModel()
        {
            MCPSettings.HomeDirectoryChanged += MCPSettings_HomeDirectoryChanged;
            Inventory.Initialize(MCPSettings.PumpDirectoryPath, MCPSettings.ReactorDirectoryPath);
            ExperimentLibrary.Initialize(MCPSettings.ExperimentsDirectoryPath);
        }

        private void MCPSettings_HomeDirectoryChanged(object sender, EventArgs e)
        {
            Inventory.Initialize(MCPSettings.PumpDirectoryPath, MCPSettings.ReactorDirectoryPath);
            ExperimentLibrary.Initialize(MCPSettings.ExperimentsDirectoryPath);
        }

    }
}

