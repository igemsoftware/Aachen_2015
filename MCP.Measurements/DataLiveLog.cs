using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCP.Measurements
{
    public class DataLiveLog : DataLogBase
    {
        public DataLiveLog(string path, string symbol, string unit)
        {
            this._FilePath = path;
            string title = string.Format("{0} [{1}]", symbol, unit);
            Initialize("Time", title, "s_" + title);
        }
    }
}
