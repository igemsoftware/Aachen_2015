using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCP.Measurements
{
    public class DataLogBase
    {
        internal string _FilePath { get; set; }
        internal StreamWriter _Writer { get; set; }


        internal async void InitializeAsync(params string[] headers)
        {
            bool existed = File.Exists(_FilePath);
            _Writer = File.CreateText(_FilePath);
            _Writer.AutoFlush = true;//TODO: is AutoFlush on live data really a good idea?
            if (!existed)
                await _Writer.WriteLineAsync(string.Join("\t", headers));
        }

        public virtual void AddRawData(RawData data)
        {
            _Writer.WriteLineAsync(data.ToString());
        }
    }
}
