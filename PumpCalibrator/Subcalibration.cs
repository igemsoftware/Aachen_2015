using MCP.Curves;
using MCP.Measurements;
using MCP.Protocol;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

        private ObservableCollection<RawData> _SensorDataSet = new ObservableCollection<RawData>();//contains all datapoints
        public ObservableCollection<RawData> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; } }
        public EnumerableDataSource<RawData> DataSource { get; set; }

        public double ChangePerHour
        {
            get
            {
                RawData start = SensorDataSet.FirstOrDefault();
                RawData end = SensorDataSet.LastOrDefault();
                if (start == null || (end.Time - start.Time).TotalHours == 0)
                    return double.NaN;
                return (end.Value - start.Value) / (end.Time - start.Time).TotalHours;
            }
        }
        public double ChangePerMinute
        {
            get
            {
                RawData start = SensorDataSet.FirstOrDefault();
                RawData end = SensorDataSet.LastOrDefault();
                if (start == null || (end.Time - start.Time).TotalMinutes == 0)
                    return double.NaN;
                return (end.Value - start.Value) / (end.Time - start.Time).TotalMinutes;
            }
        }
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
        #endregion

        public Subcalibration(int setpoint, int duration, string symbol, string unit)
        {
            this.Setpoint = setpoint;
            this.Duration = duration;
            this.Symbol = symbol;
            this.Unit = unit;
            DataSource = new EnumerableDataSource<RawData>(SensorDataCollection);
            DataSource.SetXMapping(x => (x.Time - SensorDataSet.First().Time).TotalSeconds);
            DataSource.SetYMapping(y => y.Value - SensorDataSet.First().Value);
        }

        public async Task<bool> RunAsync()
        {
            //set the new setpoint
            ViewModel.Current.PrimarySerial.SendMessage(new Message(ParticipantID.MCP, ParticipantID.Reactor_1, MessageType.Command, Symbol, ((int)Setpoint).ToString(), Unit));
            StartTime = DateTime.Now;
            initialDelay.Start();
            await initialDelay;//always wait a few seconds to give the hardware time to react
            //reset all caches
            SensorDataCollection.Clear();
            SensorDataSet.Clear();
            DataSource.SetXMapping(x => (x.Time - SensorDataSet.First().Time).TotalSeconds);
            DataSource.SetYMapping(y => y.Value - SensorDataSet.First().Value);
            //start the calibration interval
            StartTimer();
            await waitTask;//wait for the interval to finish
            //calculate the result
            ViewModel.Current.PrimarySerial.SendMessage(new Message(ParticipantID.MCP, ParticipantID.Reactor_1, MessageType.Command, Symbol, "0", Unit));
            if (double.IsNaN(ChangePerHour) || ChangePerHour == 0)//without a response you can't calibrate
                return false;
            RanToCompletion = true;
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

        public void AddPoint(RawData data)
        {
            if (!initialDelay.IsCompleted)
                return;
            SensorDataSet.Add(data);
            SensorDataCollection.Add(data);
        }
    }
}