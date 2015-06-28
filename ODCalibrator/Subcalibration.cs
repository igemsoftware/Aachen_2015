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

namespace ODCalibrator
{
    public class Subcalibration : PropertyChangedBase
    {
        #region Events
        //OnRequestCapture
        public delegate void AddOnRequestCaptureDelegate(Subcalibration sender, EventArgs e);
        public event AddOnRequestCaptureDelegate RequestCapture;
        private void OnRequestCaptureEvent(Subcalibration sender, EventArgs e)
        {
            if (RequestCapture != null)
                RequestCapture(sender, e);
        }
        //OnCaptureEnded
        public delegate void AddOnCaptureEndedDelegate(Subcalibration sender, EventArgs e);
        public event AddOnCaptureEndedDelegate CaptureEnded;
        private void OnCaptureEndedEvent(Subcalibration sender, EventArgs e)
        {
            if (CaptureEnded != null)
                CaptureEnded(sender, e);
        }
        #endregion

        #region Commands
        private RelayCommand _CaptureCommand;
        public RelayCommand CaptureCommand { get { return _CaptureCommand; } set { _CaptureCommand = value; OnPropertyChanged(); } }
        private RelayCommand _AbortCommand;
        public RelayCommand AbortCommand { get { return _AbortCommand; } set { _AbortCommand = value; OnPropertyChanged(); } }
        #endregion

        #region Data Collection
        private ObservableCollection<DataPoint> _SensorDataSet = new ObservableCollection<DataPoint>();//contains all datapoints
        public ObservableCollection<DataPoint> SensorDataSet { get { return _SensorDataSet; } set { _SensorDataSet = value; } }
        

        private DataPoint _Result;
        public DataPoint Result { get { return _Result; } set { _Result = value; OnPropertyChanged(); } }
        #endregion

        #region Stuff to Run
        private static int InitialDelay = 1;
        private Task waitTask;
        private DateTime StartTime { get; set; }

        private SubcalibrationState _State = SubcalibrationState.Idle;
        public SubcalibrationState State
        {
            get { return _State; }
            set
            {
                _State = value;
                OnPropertyChanged();
                CaptureCommand.RaiseCanExecuteChanged();
                AbortCommand.RaiseCanExecuteChanged();
            }
        }
			
        public double ProgressPercent
        {
            get
            {
                return Math.Min((DateTime.Now - StartTime).TotalSeconds / (Duration + InitialDelay) * 100, 100);
            }
            set
            {

            }
        }

        #endregion

        #region Specificity and Settings
        public double Setpoint { get; set; }
        public double CDW { get; set; }
        public double Duration { get; private set; }

        private string Symbol;
        private string Unit;
        private CalibrationTarget _Target;
        public CalibrationTarget Target { get { return _Target; } set { _Target = value; OnPropertyChanged(); } }
        #endregion

        public Subcalibration(double setpoint, double duration, string symbol, string unit)
        {
            this.Setpoint = setpoint;
            this.Duration = duration;
            this.Symbol = symbol;
            this.Unit = unit;
            CaptureCommand = new RelayCommand(delegate
            {
                OnRequestCaptureEvent(this, new EventArgs());
            }, () => State != SubcalibrationState.Running);
            AbortCommand = new RelayCommand(delegate
            {
                SensorDataSet.Clear();
                if (!waitTask.IsCompleted)
                    waitTask.Start();
            }, () => State == SubcalibrationState.Running);
        }

        public async Task<bool> RunAsync()
        {
            //prepare
            waitTask = new Task(async delegate { await Task.Delay(1); });
            //begin
            State = SubcalibrationState.Running;
            StartTime = DateTime.Now;
            await Task.Delay(InitialDelay * 1000);//always wait a few seconds to give the hardware time to react
            //reset all caches
            SensorDataSet.Clear();
            //start the calibration interval
            StartTimer();
            await waitTask;//wait for the interval to finish
            //calculate the result
            if (SensorDataSet.Count < 2)
            {
                Result = null;
                State = SubcalibrationState.Idle;
                StartTime = new DateTime();
            }
            else
            {
                Result = new DataPoint(Setpoint, SensorDataSet.Select(x => x.YValue));
                State = SubcalibrationState.Complete;
                StartTime = new DateTime();
            }
            OnCaptureEndedEvent(this, new EventArgs());
            return true;
        }
        private async void StartTimer()
        {
            await Task.Delay(1000 * (int)Duration);
            waitTask.Start();
        }

        public void AddPoint(DataPoint data)
        {
            SensorDataSet.Add(data);
        }
        public void Tick()
        {
            OnPropertyChanged("ProgressPercent");
        }
    }
}