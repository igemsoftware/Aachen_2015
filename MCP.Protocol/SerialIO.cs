using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using TCD;
using TCD.Controls;

namespace MCP.Protocol
{
    public class SerialIO : PropertyChangedBase
    {
        #region Commands
        public RelayCommand CommandRefreshPorts { get; set; }
        #endregion

        #region Events
        //OnNewMessageReceived
        public delegate void AddOnNewMessageReceivedDelegate(object sender, Message message);
        public event AddOnNewMessageReceivedDelegate NewMessageReceived;
        private void OnNewMessageReceivedEvent(object sender, Message message)
        {
            if (NewMessageReceived != null)
                NewMessageReceived(sender, message);
        }
        #endregion

        private string[] _PortNames;
        public string[] PortNames { get { return _PortNames; } set { _PortNames = value; OnPropertyChanged("PortNames"); } }

        private int _SelectedPort;
        public int SelectedPort { get { return _SelectedPort; } set { _SelectedPort = value; OnPropertyChanged("SelectedPort"); ChangePort(); } }

        private SerialPort _ActivePort;
        public SerialPort ActivePort { get { return _ActivePort; } set { _ActivePort = value; OnPropertyChanged("ActivePort"); } }
        public static SerialIO Current { get; private set; }


        public SerialIO()
        {
            Current = this;
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
                if (PortNames.Length <= SelectedPort)
                    return;
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
                string raw = ActivePort.ReadLine();
                InterpretMessage(raw);                
            }
            catch { }
        }
        public void InterpretMessage(string raw)
        {
            Message msg = new Message(raw);
            System.Diagnostics.Debug.WriteLine(msg.ToString());
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() =>
            {
                OnNewMessageReceivedEvent(this, msg);
            }));
        }

        public bool SendMessage(Message msg)
        {
            System.Diagnostics.Debug.WriteLine(msg.ToString());
            return SendString(msg.Raw);
        }
        public bool SendString(string text)
        {
            if (ActivePort == null)
                return false;
            try
            {
                ActivePort.WriteLine(text);
                return true;
            }
            catch 
            {
                //if (Debugger.IsAttached)
                //    throw;
                return false;
            }
        }

    }
}
