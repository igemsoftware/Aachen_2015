using MCP.Calibration;
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

namespace GasSensorCalibrator
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
        #endregion

        #region Stuff to Run and Settings
        //Tasks and Delays
        private static int InitialDelay = 1;
        private Task finishTask;

        //Interval-related
        private DateTime StartTime { get; set; }

        //Properties
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
                return Math.Min((DateTime.Now - StartTime).TotalSeconds / (ResponsePoint.CalibrationDuration + InitialDelay) * 100, 100);
            }
            set
            {

            }
        }

        public BiomassResponseData ResponsePoint { get; set; }

        private string Symbol;
        private string Unit;

        private CalibrationTarget _Target;
        public CalibrationTarget Target { get { return _Target; } set { _Target = value; OnPropertyChanged(); } }
        #endregion

        public Subcalibration(BiomassResponseData sensorRD, string symbol, string unit)
        {
            this.ResponsePoint = sensorRD;
            this.Symbol = symbol;
            this.Unit = unit;
            CaptureCommand = new RelayCommand(delegate
            {
                OnRequestCaptureEvent(this, new EventArgs());
            }, () => State != SubcalibrationState.Running);
            AbortCommand = new RelayCommand(delegate
            {
                SensorDataSet.Clear();
                if (!finishTask.IsCompleted)
                    finishTask.Start();
            }, () => State == SubcalibrationState.Running);
        }

        public async Task<bool> RunAsync()
        {
            //prepare
            finishTask = new Task(async delegate { await Task.Delay(1); });
            //begin
            State = SubcalibrationState.Running;
            StartTime = DateTime.Now;
            await Task.Delay(InitialDelay * 1000);//always wait a few seconds to give the hardware time to react
            //reset all caches
            SensorDataSet.Clear();
            //start the calibration interval
            StartTimer();
            await finishTask;//wait for the interval to finish
            //calculate the result
            if (SensorDataSet.Count < 2)
            {
                ResponsePoint.Analog = double.NaN;
                ResponsePoint.AnalogStd = double.NaN;
                State = SubcalibrationState.Idle;
                StartTime = new DateTime();
            }
            else
            {
                double[] avAndStd = DataPoint.AverageAndStd(SensorDataSet.Select(x => x.YValue));
                ResponsePoint.Analog = avAndStd[0];
                ResponsePoint.AnalogStd = avAndStd[1];
                State = SubcalibrationState.Complete;
                StartTime = new DateTime();
            }
            OnPropertyChanged("ResponsePoint");
            OnCaptureEndedEvent(this, new EventArgs());
            return true;
        }
        private async void StartTimer()
        {
            while ((DateTime.Now - StartTime).TotalSeconds < ResponsePoint.CalibrationDuration && !finishTask.IsCompleted)
            {
                await Task.Delay(10);
            }
            try
            {
                finishTask.Start();
            }
            catch
            {

            }
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