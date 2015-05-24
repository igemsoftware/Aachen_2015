using MCP.Curves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCP.Measurements
{
    public class DataPostprocessingLog : DataLogBase
    {
        private List<RawData> _RawDataCache = new List<RawData>();
        public List<RawData> RawDataCache { get { return _RawDataCache; } set { _RawDataCache = value; } }

        private int _MinimumSamples;
        private PostprocessingMode _PostprocessingMode;



        public DataPostprocessingLog(string path, string symbol, string unit, PostprocessingMode mode)
        {
            this._FilePath = path;
            this._PostprocessingMode = mode;
            switch (mode)
            {
                case PostprocessingMode.Biomass:
                    _MinimumSamples = 1000;//TODO: implement Biomass postprocessing
                    break;
                case PostprocessingMode.Offgas:
                    _MinimumSamples = 500;//TODO: implement Offgas postprocessing
                    break;
                default:
                    break;
            }
            string title = string.Format("{0} [{1}]", symbol, unit);
            InitializeAsync("Time", title, "s_" + title);
        }

        public override void AddRawData(RawData data)
        {
            RawDataCache.Add(data);
            TryToAccumulate();
        }

        private void TryToAccumulate()
        {
            if (RawDataCache.Count < _MinimumSamples)
                return;
            DataPoint point = null;
            switch (_PostprocessingMode)
            {
                case PostprocessingMode.Biomass:
                    //TODO: implement Biomass analysis
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
    }
    public enum PostprocessingMode
    {
        Biomass,
        Offgas
    }
}
