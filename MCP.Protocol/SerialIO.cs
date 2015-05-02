using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;
using TCD.Controls;

namespace MCP.Protocol
{
    public class SerialIO : PropertyChangedBase
    {
        #region Commands
        public RelayCommand CommandRefreshPorts { get; set; }
        #endregion

        private string[] _PortNames;
        public string[] PortNames { get { return _PortNames; } set { _PortNames = value; OnPropertyChanged("PortNames"); } }

        private int _SelectedPort;
        public int SelectedPort { get { return _SelectedPort; } set { _SelectedPort = value; OnPropertyChanged("SelectedPort"); ChangePort(); } }

        private SerialPort _ActivePort;
        public SerialPort ActivePort { get { return _ActivePort; } set { _ActivePort = value; OnPropertyChanged("ActivePort"); } }

        public SerialIO()
        {
            //ports
            CommandRefreshPorts = new RelayCommand(delegate
            {
                var ports = SerialPort.GetPortNames().ToList();
                PortNames = ports.ToArray();
                SelectedPort = 0;
            });
            CommandRefreshPorts.Execute(null);
        }
        private void ChangePort()
        {
            try
            {
                if (ActivePort != null)
                    ActivePort.Close();
                ActivePort = new SerialPort(PortNames[SelectedPort], (int)BaudRate._9600);
                ActivePort.DataReceived += ActivePort_DataReceived;
                ActivePort.Open();
            }
            catch (Exception ex)
            {
                if (!(ex is UnauthorizedAccessException))
                    CustomMessageBox.ShowAsync("Exception occured", ex.Message, System.Windows.MessageBoxImage.Error, 0, "Close");
            }

        }

        private void ActivePort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                //TODO: use the Dispatcher
            }
            catch { }
        }

        public void SendString(string text)
        {
            try
            {
                ActivePort.WriteLine(text);
                System.Diagnostics.Debug.WriteLine("> " + text);
            }
            catch 
            {
                if (Debugger.IsAttached)
                    throw;
            }
        }

    }
}
