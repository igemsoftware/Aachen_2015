using MCP.Calibration;
using MCP.Curves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODCalibrator
{
    public static class CalibrationProfiles
    {
        public static Dictionary<CalibrationTarget, Dictionary<CalibrationMode, List<BiomassResponseData>>> Profiles = new Dictionary<CalibrationTarget, Dictionary<CalibrationMode, List<BiomassResponseData>>>();
        public static Dictionary<CalibrationTarget, string> Symbols = new Dictionary<CalibrationTarget, string>();
        public static Dictionary<CalibrationTarget, string> Units = new Dictionary<CalibrationTarget, string>();

        static CalibrationProfiles()
        {
            Profiles.Add(CalibrationTarget.Biomass, new Dictionary<CalibrationMode, List<BiomassResponseData>>());
            Symbols.Add(CalibrationTarget.Biomass, "X");
            Units.Add(CalibrationTarget.Biomass, "variable");
        }
        
    }
}
