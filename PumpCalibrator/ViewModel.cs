using MCP.Measurements;
using MCP.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using TCD;

namespace PumpCalibrator
{
    public class ViewModel : PropertyChangedBase
    {
        private SerialIO _PrimarySerial = new SerialIO();
        public SerialIO PrimarySerial { get { return _PrimarySerial; } set { _PrimarySerial = value; OnPropertyChanged(); } }

        private Calibrator _Calibrator = new Calibrator();
        public Calibrator Calibrator { get { return _Calibrator; } set { _Calibrator = value; OnPropertyChanged(); } }

        public static ViewModel Current { get; private set; }


        #region Debug Properties
        public bool IsDebugMode { get { return System.Diagnostics.Debugger.IsAttached; } }

        private bool _IsRandomizerEnabled;
        public bool IsRandomizerEnabled { get { return _IsRandomizerEnabled; } set { _IsRandomizerEnabled = value; OnPropertyChanged(); } }

        private static Random rnd = new Random();
        private double weight = 0;

        #endregion

        public ViewModel()
        {
            Current = this;
            PrimarySerial.NewMessageReceived += PrimarySerial_NewMessageReceived;

            if (IsDebugMode)
            {
                DispatcherTimer dt = new DispatcherTimer() { Interval = TimeSpan.FromSeconds(1) };
                dt.Tick += delegate
                {
                    if (!IsRandomizerEnabled)
                        return;
                    weight = weight + rnd.NextDouble();
                    Debug.WriteLine(weight);
                    Calibrator.AddPoint(new RawData(weight, DateTime.Now));
                };
                dt.Start();
            }
        }


        private void PrimarySerial_NewMessageReceived(object sender, Message message)
        {
            switch (message.MessageType)
            {
                case MessageType.Data:
                    double val = Convert.ToDouble(message.Contents);
                    Calibrator.AddPoint(new RawData(val, DateTime.Now));
                    break;
                case MessageType.Command:
                    break;
                case MessageType.DataFormat:
                    break;
                case MessageType.CommandFormat:
                    break;
                default:
                    break;
            }
        }
    }
}
