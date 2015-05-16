using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCP.Measurements
{
    public class RawData
    {
        public double Value { get; set; }
        public DateTime Time { get; set; }

        public RawData(double value, DateTime time)
        {
            this.Value = value;
            this.Time = time;
        }
        public override string ToString()
        {
            return string.Format("{0}\t{1}", Time, Value);
        }
    }
}
