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

        public int MinimumSamples;
        public int MinimumTime; // Neu
        private System.Timers.Timer timer; // Neu
        private bool _MeasurementTimeElapsed = false; // Neu




        public void ResetInterval()
        {
            RawDataCache.Clear();
            timer = new System.Timers.Timer(MinimumTime * 1000);
            timer.AutoReset = false;
            timer.Elapsed += delegate { _MeasurementTimeElapsed = true; };
            timer.Enabled = true;
        }

        public override bool AddRawData(DataPoint data)
        {
            RawDataCache.Add(data);
            //TODO: the oder here has to be optimized:
            //at least _MinimumSamples
            //try to detect a stable signal until the timer elapses
            //check if there's ENOUGH data or we waited for too long
            if (RawDataCache.Count > MinimumSamples || _MeasurementTimeElapsed) // changed
            {
                //reset the measurement interval
                _MeasurementTimeElapsed = false;
                timer.Stop();
                timer.Start();
                //now compute the signal
                bool res = ComputeSignal();
                RawDataCache.Clear();
                return res;
            }
            return false;
        }

        public virtual bool ComputeSignal()
        {
           //implement this in the deriving classes for GasSensors or BiomassSensors
            throw new NotImplementedException();
        }
    }
}
