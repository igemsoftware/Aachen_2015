using MCP.Measurements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCP.Equipment;
using MCP.Curves;
using MCP.Protocol;
using System.IO;

namespace MCP.Cultivation
{
    public class BiomassLog : DataPostprocessingLog
    {
        public BiomassLog(string path, string symbol, string unit)
        {
            this._FilePath = path;

            MinimumSamples = 60; // only one of these criteria has to be fullfilled
            MinimumTime = 180;
            ResetInterval();

            string title = string.Format("{0} [{1}]", symbol, unit);
            this.Initialize("Time", title, "s_" + title);
        }

        public override bool ComputeSignal()
        {
            DataPoint dp = null;

            //TODO: analyze the signal to look for stability

            //take the average...
            dp = new DataPoint(RawDataCache.First().Time, RawDataCache.Last().Time, this.RawDataCache.Select(p => p.YValue));

            if (dp != null)
            {
                this.WriteLine(dp.ToString());
                this.SensorDataSet.Add(dp);
                return true;
            }
            return false;
        }
    }
}
