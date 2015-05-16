using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;
using TCD.Controls;
using MCP.Protocol;
using System.IO;

namespace MCP.Measurements
{
    public class CultivationLog : PropertyChangedBase
    {
        private Dictionary<string, DataLogBase> _Logs = new Dictionary<string, DataLogBase>();
        public Dictionary<string, DataLogBase> Logs { get { return _Logs; } set { _Logs = value; OnPropertyChanged(); } }
        
		
        #region Private Stuff
        private string _BaseDirectory { get; set; }

        #endregion


        public CultivationLog(string baseDir)
        {
            _BaseDirectory = baseDir;
            //initialize postprocessing logs
            Logs.Add(DimensionSymbol.Biomass_Concentration, new DataPostprocessingLog(Path.Combine(_BaseDirectory, "Biomass.log"), DimensionSymbol.Biomass_Concentration, Unit.Percent, PostprocessingMode.Biomass));
            Logs.Add(DimensionSymbol.O2_Saturation, new DataPostprocessingLog(Path.Combine(_BaseDirectory, "O2.log"), DimensionSymbol.O2_Saturation, Unit.Percent, PostprocessingMode.Offgas));
            Logs.Add(DimensionSymbol.CO2_Saturation, new DataPostprocessingLog(Path.Combine(_BaseDirectory, "CO2.log"), DimensionSymbol.CO2_Saturation, Unit.Percent, PostprocessingMode.Offgas));
            Logs.Add(DimensionSymbol.CHx_Saturation, new DataPostprocessingLog(Path.Combine(_BaseDirectory, "CHx.log"), DimensionSymbol.CHx_Saturation, Unit.Percent, PostprocessingMode.Offgas));
            //initialize live logs
            Logs.Add(DimensionSymbol.Agitation_Rate, new DataLiveLog(Path.Combine(_BaseDirectory, "Agitation.log"), DimensionSymbol.Agitation_Rate, Unit.RPM));
            Logs.Add(DimensionSymbol.Aeration_Rate, new DataLiveLog(Path.Combine(_BaseDirectory, "Aeration.log"), DimensionSymbol.Aeration_Rate, Unit.VVM));
            Logs.Add(DimensionSymbol.Dilution_Rate, new DataLiveLog(Path.Combine(_BaseDirectory, "DilutionRate.log"), DimensionSymbol.Dilution_Rate, Unit.PerHour));
            Logs.Add(DimensionSymbol.Temperature, new DataLiveLog(Path.Combine(_BaseDirectory, "Temperature.log"), DimensionSymbol.Temperature, Unit.Celsius));
        }

        public void ReceiveMessage(Message msg)
        {
            switch (msg.MessageType)
            {
                case MessageType.Data:
                    LogData(msg.Contents);
                    break;
                case MessageType.Command:
                    break;
                case MessageType.DataFormat:
                    break;
                case MessageType.CommandFormat:
                    break;
                default:
                    break;
            }
        }
        public void LogData(string[] data)
        {
            if (Logs.ContainsKey(data[0]))//TODO: check for the right unit
                Logs[data[0]].AddRawData(new RawData(Convert.ToDouble(data[1]), DateTime.Now));
        }
    }
}
