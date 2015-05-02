using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;
using System.Windows.Forms;
using System.IO;

namespace MasterControlProgram
{
    public class MCPSettings : PropertyChangedBase
    {
        #region Events
        //OnHomeDirectoryChanged
        public delegate void AddOnHomeDirectoryChangedDelegate(object sender, EventArgs e);
        public event AddOnHomeDirectoryChangedDelegate HomeDirectoryChanged;
        private void OnHomeDirectoryChangedEvent(object sender, EventArgs e)
        {
            if (HomeDirectoryChanged != null)
                HomeDirectoryChanged(sender, e);
        }
        #endregion

        public string HomeDirectoryPath { get { return Properties.Settings.Default.HomeDirectoryPath; } set { Properties.Settings.Default.HomeDirectoryPath = value; OnPropertyChanged(); SaveSettings(); } }
        public string PumpDirectoryPath { get { return Path.Combine(HomeDirectoryPath, "Pumps"); } }
        public string ReactorDirectoryPath { get { return Path.Combine(HomeDirectoryPath, "Reactors"); } }
        public string ExperimentsDirectoryPath { get { return Path.Combine(HomeDirectoryPath, "Experiments"); } }
			

        private RelayCommand _ChangeHomeDirectoryCommand;
        public RelayCommand ChangeHomeDirectoryCommand { get { return _ChangeHomeDirectoryCommand; } set { _ChangeHomeDirectoryCommand = value; OnPropertyChanged(); } }

        private RelayCommand _BrowseHomeDirectoryCommand;
        public RelayCommand BrowseHomeDirectoryCommand { get { return _BrowseHomeDirectoryCommand; } set { _BrowseHomeDirectoryCommand = value; OnPropertyChanged(); } }
        
			
        public MCPSettings()
        {
            ChangeHomeDirectoryCommand = new RelayCommand(delegate
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                if (!string.IsNullOrWhiteSpace(HomeDirectoryPath))
                    fbd.SelectedPath = HomeDirectoryPath;
                else
                    fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                fbd.ShowDialog();
                if (!string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    if (fbd.SelectedPath.EndsWith("MCP"))
                        HomeDirectoryPath = fbd.SelectedPath;
                    else
                        HomeDirectoryPath = Path.Combine(fbd.SelectedPath, "MCP");
                InitializeHomeDirectory();
            });
            BrowseHomeDirectoryCommand = new RelayCommand(delegate { System.Diagnostics.Process.Start(HomeDirectoryPath); });
            InitializeHomeDirectory();
        }
        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }
        private void InitializeHomeDirectory()
        {
            if (!Directory.Exists(HomeDirectoryPath))
                Directory.CreateDirectory(HomeDirectoryPath);
            if (!Directory.Exists(PumpDirectoryPath))
                Directory.CreateDirectory(PumpDirectoryPath);
            if (!Directory.Exists(ReactorDirectoryPath))
                Directory.CreateDirectory(ReactorDirectoryPath);
            if (!Directory.Exists(ExperimentsDirectoryPath))
                Directory.CreateDirectory(ExperimentsDirectoryPath);

        }
    }
}
