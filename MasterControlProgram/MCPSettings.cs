/*
    MCP Bioreactor Control Software
    Copyright (C) 2015 iGEM Aachen (Michael Osthege, Sebastian Siegel, Tanya Bafna, Sayantan Dutta)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;
using System.Windows.Forms;
using System.IO;
using TCD.Controls;
using System.Windows.Threading;

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

        public string HomeDirectoryPath
        {
            get { return Properties.Settings.Default.HomeDirectoryPath; }
            set
            {
                Properties.Settings.Default.HomeDirectoryPath = value;
                OnPropertyChanged();
                SaveSettings();
                InitializeHomeDirectory();
                OnHomeDirectoryChangedEvent(this, new EventArgs());
            }
        }
        public string PumpDirectoryPath { get { return Path.Combine(HomeDirectoryPath, "Pumps"); } }
        public string SensorDirectoryPath { get { return Path.Combine(HomeDirectoryPath, "Sensors"); } }
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
                string previousPath = HomeDirectoryPath;
                FolderBrowserDialog fbd = new FolderBrowserDialog() { Description = "Select Home Directory" };
                if (!string.IsNullOrWhiteSpace(previousPath))
                    fbd.SelectedPath = previousPath;
                else
                    fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                fbd.ShowDialog();
                if (fbd.SelectedPath != previousPath)
                {
                    if (fbd.SelectedPath.EndsWith("MCP"))
                        HomeDirectoryPath = fbd.SelectedPath;
                    else
                        HomeDirectoryPath = Path.Combine(fbd.SelectedPath, "MCP");
                }
            });
            BrowseHomeDirectoryCommand = new RelayCommand(delegate
            {
                try
                {
                    System.Diagnostics.Process.Start(HomeDirectoryPath);
                }
                catch { }
            });
            //
            DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromMilliseconds(20) };
            dt.Tick += async delegate
            {
                dt.Stop();
                if (string.IsNullOrWhiteSpace(HomeDirectoryPath))
                {
                    int sel = await CustomMessageBox.ShowAsync("Setup", "Welcome to the MCP!\r\n\r\nBefore you can start you must set a home directory where files will be saved.\r\n\r\nYou may set or change the home directory under \"Settings\".", System.Windows.MessageBoxImage.Information, 0, "Select Home Directory", "Okay");
                    if (sel == 0)
                        ChangeHomeDirectoryCommand.Execute(null);
                }
                else
                    InitializeHomeDirectory();
            };
            dt.Start();
        }

        private void SaveSettings()
        {
            Properties.Settings.Default.Save();
        }
        private void InitializeHomeDirectory()
        {
            if (string.IsNullOrWhiteSpace(HomeDirectoryPath))
                return;
            if (!Directory.Exists(HomeDirectoryPath))
                Directory.CreateDirectory(HomeDirectoryPath);
            if (!Directory.Exists(PumpDirectoryPath))
                Directory.CreateDirectory(PumpDirectoryPath);
            if (!Directory.Exists(SensorDirectoryPath))
                Directory.CreateDirectory(SensorDirectoryPath);
            if (!Directory.Exists(ReactorDirectoryPath))
                Directory.CreateDirectory(ReactorDirectoryPath);
            if (!Directory.Exists(ExperimentsDirectoryPath))
                Directory.CreateDirectory(ExperimentsDirectoryPath);
        }
    }
}
