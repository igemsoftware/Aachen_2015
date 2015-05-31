using MCP.Curves;
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
#if DEBUG
        public bool IsDebugMode { get { return true; } }
#else
        public bool IsDebugMode { get { return false; } }
#endif
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
                    if (!IsRandomizerEnabled || Calibrator.ActiveCalibrationSub == null)
                        return;
                    double val = 0;
                    switch (Calibrator.CalibrationTarget)
                    {
                        case CalibrationTarget.Pump:
                            weight = weight + rnd.NextDouble() * (double)Calibrator.ActiveCalibrationSub.Setpoint / 500000;
                            val = weight;
                            break;
                        case CalibrationTarget.Stirrer:
                            val = (0.7 * rnd.NextDouble() + 0.3) * (double)Calibrator.ActiveCalibrationSub.Setpoint / 60;
                            break;
                    }
                    if (Calibrator.ActiveCalibrationSub != null)
                        Calibrator.ActiveCalibrationSub.AddPoint(new RawData(val, DateTime.Now));
                };
                dt.Start();
            }
        }


        private void PrimarySerial_NewMessageReceived(object sender, Message message)
        {
            try
            {
                switch (message.MessageType)
                {
                    case MessageType.Data:
                        if (message.Contents[0] == "scale")
                        {
                            double val = Convert.ToDouble(message.Contents[1].Substring(0,9));
                            if (Calibrator.ActiveCalibrationSub != null)
                                Calibrator.ActiveCalibrationSub.AddPoint(new RawData(val, DateTime.Now));
                        }
                        else if (message.Contents[0] == "signals")
                        {
                            int val = Convert.ToInt32(message.Contents[1]);
                            if (Calibrator.ActiveCalibrationSub != null)
                                Calibrator.ActiveCalibrationSub.AddPoint(new RawData(val, DateTime.Now));
                        }
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
            catch
            {

            }
            
        }
    }
}
