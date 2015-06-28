using MCP.Curves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace MCP.Measurements
{
    public class DataPostprocessingLog : DataLogBase
    {
        private List<DataPoint> _RawDataCache = new List<DataPoint>();
        public List<DataPoint> RawDataCache { get { return _RawDataCache; } set { _RawDataCache = value; } }

        private int _MinimumSamples;
        private int _MinimumTime; // Neu
        private System.Timers.Timer timer; // Neu
        private bool _MeasurementTimeElapsed = false; // Neu

        private PostprocessingMode _PostprocessingMode;



        public DataPostprocessingLog(string path, string symbol, string unit, PostprocessingMode mode)
        {
            this._FilePath = path;
            this._PostprocessingMode = mode;
            switch (mode)
            {
                case PostprocessingMode.Biomass:
                    _MinimumSamples = 50;//TODO: implement Biomass postprocessing
                    _MinimumTime = 30;
                    timer = new System.Timers.Timer(_MinimumTime * 1000);
                    timer.AutoReset = false;
                    timer.Elapsed += new ElapsedEventHandler(TimerElapsed);
                    timer.Enabled = true;
                    break;
                case PostprocessingMode.Offgas:
                    _MinimumSamples = 500;//TODO: implement Offgas postprocessing
                    break;
                default:
                    break;
            }
            string title = string.Format("{0} [{1}]", symbol, unit);
            Initialize("Time", title, "s_" + title);
        }

        public override void AddRawData(DataPoint data)
        {
            RawDataCache.Add(data);
            TryToAccumulate();
        }

        private void TryToAccumulate()
        {
            if (RawDataCache.Count < _MinimumSamples || !_MeasurementTimeElapsed) // changed
                return;
            _MeasurementTimeElapsed = false;
            timer.Stop(); // Resets the timer
            timer.Start();

            DataPoint point = null;
            switch (_PostprocessingMode)
            {
                case PostprocessingMode.Biomass:
                    //TODO: implement Biomass analysis
                    double average = 0;

                    for (int i = 0; i < RawDataCache.Count; i++)
                    {
                        average += RawDataCache.ElementAt(i).YValue;
                    }
                    average /= RawDataCache.Count;
                    RawDataCache.Clear();

                    point = new DataPoint(DateTime.Now, average);

                    break;
                case PostprocessingMode.Offgas:
                    //TODO: implement Offgas analysis
                    break;
            }
            // if the analysis was successful, we should now have a DataPoint
            if (point != null)
            {
                WriteLine(point.ToString());
                RawDataCache.Clear();
            }
        }

        private void TimerElapsed(object source, ElapsedEventArgs e) // Neu
        {
            _MeasurementTimeElapsed = true;
        }
    }
    public enum PostprocessingMode
    {
        Biomass,
        Offgas
    }
}
