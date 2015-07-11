using MCP.Calibration;
using MCP.Curves;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasSensorCalibrator
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
            AddODProfiles();
        }
        private static void AddODProfiles()
        {
            foreach (CalibrationMode mode in Enum.GetValues(typeof(CalibrationMode)))
            {
                List<BiomassResponseData> subcals = new List<BiomassResponseData>();
                switch (mode)
                {
                    case CalibrationMode.Debug:
                        subcals.Add(new BiomassResponseData() { OD = 0, CalibrationDuration = 10 });
                        subcals.Add(new BiomassResponseData() { OD = 0.5, CalibrationDuration = 10 });
                        subcals.Add(new BiomassResponseData() { OD = 1.0, CalibrationDuration = 10 });
                        subcals.Add(new BiomassResponseData() { OD = 1.5, CalibrationDuration = 10 });
                        break;
                    case CalibrationMode.Quick:
                        subcals.Add(new BiomassResponseData() { OD = 0, CalibrationDuration = 10 });
                        subcals.Add(new BiomassResponseData() { OD = 0.5, CalibrationDuration = 10 });
                        subcals.Add(new BiomassResponseData() { OD = 1.0, CalibrationDuration = 10 });
                        subcals.Add(new BiomassResponseData() { OD = 1.5, CalibrationDuration = 10 });
                        subcals.Add(new BiomassResponseData() { OD = 2.0, CalibrationDuration = 10 });
                        subcals.Add(new BiomassResponseData() { OD = 3.0, CalibrationDuration = 10 });
                        break;
                    case CalibrationMode.Standard:
                        subcals.Add(new BiomassResponseData() { OD = 0, CalibrationDuration = 15 });
                        subcals.Add(new BiomassResponseData() { OD = 0.25, CalibrationDuration = 15 });
                        subcals.Add(new BiomassResponseData() { OD = 0.50, CalibrationDuration = 15 });
                        subcals.Add(new BiomassResponseData() { OD = 0.75, CalibrationDuration = 15 });
                        subcals.Add(new BiomassResponseData() { OD = 1.00, CalibrationDuration = 15 });
                        subcals.Add(new BiomassResponseData() { OD = 1.50, CalibrationDuration = 15 });
                        subcals.Add(new BiomassResponseData() { OD = 2.00, CalibrationDuration = 15 });
                        subcals.Add(new BiomassResponseData() { OD = 2.50, CalibrationDuration = 15 });
                        subcals.Add(new BiomassResponseData() { OD = 3.00, CalibrationDuration = 15 });
                        break;
                    case CalibrationMode.Precise:
                        subcals.Add(new BiomassResponseData() { OD = 0, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 0.25, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 0.50, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 0.75, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 1.00, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 1.50, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 2.00, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 2.50, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 3.00, CalibrationDuration = 20 });
                        subcals.Add(new BiomassResponseData() { OD = 4.00, CalibrationDuration = 20 });
                        break;
                }
                Profiles[CalibrationTarget.Biomass].Add(mode, subcals);
            }
        }

    }
}
