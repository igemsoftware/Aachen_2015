using MCP.Calibration;
using MCP.Curves;
using MCP.Measurements;
using MCP.Protocol;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using TCD;

namespace PumpCalibrator
{
    public class Subcalibration : PropertyChangedBase
    {
        #region Data Collection
        private SensorDataPointCollection _SensorDataCollection = new SensorDataPointCollection();//contains only recent datapoints
        public SensorDataPointCollection SensorDataCollection { get { return _SensorDataCollection; } set { _SensorDataCollection = value; } }

        private ObservableCollection<DataPoint> _SensorDataSet = new ObservableCollection<DataPoint>();//contains all datapoints
        public ObservableCollection<DataPoint> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; } }
        public EnumerableDataSource<DataPoint> DataSource { get; set; }

        /// <summary>
        /// Returns the difference between the last and the first divided by the time difference.
        /// </summary>
        public double AbsoluteChangePerHour
        {
            get
            {
                DataPoint start = SensorDataSet.FirstOrDefault();
                DataPoint end = SensorDataSet.LastOrDefault();
                if (start == null || (end.Time - start.Time).TotalHours == 0)
                    return double.NaN;
                return (end.YValue - start.YValue) / (end.Time - start.Time).TotalHours;
            }
        }

        /// <summary>
        /// Returns this sum of all collected data points divided by the time difference.
        /// </summary>
        public double IncrementalChangePerMinute
        {
            get
            {
                DataPoint start = SensorDataSet.FirstOrDefault();
                DataPoint end = SensorDataSet.LastOrDefault();
                double sum = SensorDataSet.Sum(d => d.YValue);//calculate the sum of all data points
                if (start == null || (end.Time - start.Time).TotalMinutes == 0)
                    return double.NaN;
                return sum / (end.Time - start.Time).TotalMinutes;
            }
        }
        #endregion

        #region Screensaver-Deactivation
        [DllImport("kernel32.dll")]
        private static extern uint SetThreadExecutionState(uint esFlags);
        private const uint ES_CONTINUOUS = 0x80000000;
        private const uint ES_SYSTEM_REQUIRED = 0x00000001;
        private const uint ES_DISPLAY_REQUIRED = 0x00000002;
        #endregion

        #region Stuff to Run
        private static int InitialDelay = 4;
        private Task initialDelay = new Task(async delegate { await Task.Delay(InitialDelay); });
        private Task waitTask = new Task(async delegate { await Task.Delay(1); });
        private DateTime StartTime { get; set; }
        private bool _RanToCompletion = false;
        public bool RanToCompletion { get { return _RanToCompletion; } set { _RanToCompletion = value; OnPropertyChanged(); } }
        public double ProgressPercent
        {
            get
            {
                if (SensorDataSet.Count == 0)
                    return 0;
                return Math.Min((DateTime.Now - StartTime).TotalSeconds / (Duration + InitialDelay) * 100, 100);
            }
        }

        #endregion

        #region Specificity and Settings
        public int Setpoint { get; private set; }
        public int Duration { get; private set; }
        private string Symbol;
        private string Unit;
        private CalibrationTarget _Target;
        public CalibrationTarget Target { get { return _Target; } set { _Target = value; OnPropertyChanged(); } }
        
			
        #endregion

        public Subcalibration(int setpoint, int duration, string symbol, string unit)
        {
            this.Setpoint = setpoint;
            this.Duration = duration;
            this.Symbol = symbol;
            this.Unit = unit;
            DataSource = new EnumerableDataSource<DataPoint>(SensorDataCollection);
            DataSource.SetXMapping(x => (x.Time - SensorDataSet.First().Time).TotalSeconds);
            DataSource.SetYMapping(y => y.YValue - SensorDataSet.First().YValue);
        }

        public async Task<bool> RunAsync()
        {
            //deactivate screensaver
            SetThreadExecutionState(ES_CONTINUOUS | ES_SYSTEM_REQUIRED | ES_DISPLAY_REQUIRED);
            //set the new setpoint
            ViewModel.Current.PrimarySerial.SendMessage(new Message(ParticipantID.MCP, ParticipantID.Reactor_1, MessageType.Command, Symbol, ((int)Setpoint).ToString(), Unit));
            StartTime = DateTime.Now;
            initialDelay.Start();
            await initialDelay;//always wait a few seconds to give the hardware time to react
            //reset all caches
            SensorDataCollection.Clear();
            SensorDataSet.Clear();
            DataSource.SetXMapping(x => (x.Time - SensorDataSet.First().Time).TotalSeconds);
            DataSource.SetYMapping(y => y.YValue - SensorDataSet.First().YValue);
            //start the calibration interval
            StartTimer();
            await waitTask;//wait for the interval to finish
            //calculate the result
            ViewModel.Current.PrimarySerial.SendMessage(new Message(ParticipantID.MCP, ParticipantID.Reactor_1, MessageType.Command, Symbol, "0", Unit));
            //if (double.IsNaN(AbsoluteChangePerHour) || AbsoluteChangePerHour == 0)//without a response you can't calibrate
            //    return false;
            RanToCompletion = true;
            //re-activate screensaver
            SetThreadExecutionState(ES_CONTINUOUS);
            return true;
        }
        public void Abort()
        {
            SensorDataCollection.Clear();
            SensorDataSet.Clear();
            if (!waitTask.IsCompleted)
                waitTask.Start();
        }
        private async void StartTimer()
        {
            await Task.Delay(1000 * Duration);
            if (!waitTask.IsCompleted)
                waitTask.Start();
        }

        public void AddPoint(DataPoint data)
        {
            if (!initialDelay.IsCompleted)
                return;
            SensorDataSet.Add(data);
            if (Target == CalibrationTarget.Stirrer)
            {
                DataPoint prev = SensorDataCollection.LastOrDefault();
                double preval = 0;
                if (prev != null)
                    preval = prev.YValue;
                DataPoint data2 = new DataPoint(data.Time, preval + data.YValue);
                SensorDataCollection.Add(data2);
            }
            else
                SensorDataCollection.Add(data);
        }
    }
}