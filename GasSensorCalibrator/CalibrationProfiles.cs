using MCP.Calibration;
using MCP.Curves;
using MCP.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GasSensorCalibrator
{
    public static class CalibrationProfiles
    {
        public static Dictionary<CalibrationTarget, Dictionary<CalibrationMode, List<GasSensorResponseData>>> Profiles = new Dictionary<CalibrationTarget, Dictionary<CalibrationMode, List<GasSensorResponseData>>>();
        public static Dictionary<CalibrationTarget, string> Symbols = new Dictionary<CalibrationTarget, string>();
        public static Dictionary<CalibrationTarget, string> Units = new Dictionary<CalibrationTarget, string>();

        static CalibrationProfiles()
        {
            Profiles.Add(CalibrationTarget.Oxygen, new Dictionary<CalibrationMode, List<GasSensorResponseData>>());
            Profiles.Add(CalibrationTarget.Carbon_Dioxide, new Dictionary<CalibrationMode, List<GasSensorResponseData>>());
            Profiles.Add(CalibrationTarget.CHx, new Dictionary<CalibrationMode, List<GasSensorResponseData>>());
            Symbols.Add(CalibrationTarget.Oxygen, DimensionSymbol.O2_Saturation);
            Symbols.Add(CalibrationTarget.Carbon_Dioxide, DimensionSymbol.CO2_Saturation);
            Symbols.Add(CalibrationTarget.CHx, DimensionSymbol.CO2_Saturation);
            Units.Add(CalibrationTarget.Oxygen, Unit.Percent);
            Units.Add(CalibrationTarget.Carbon_Dioxide, Unit.Percent);
            Units.Add(CalibrationTarget.CHx, Unit.Percent);
            AddO2Profiles();
            AddCO2Profiles();
            AddCHxProfiles();
        }
        private static void AddO2Profiles()
        {
            foreach (CalibrationMode mode in Enum.GetValues(typeof(CalibrationMode)))
            {
                List<GasSensorResponseData> subcals = new List<GasSensorResponseData>();
                switch (mode)
                {
                    case CalibrationMode.Debug:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 10 });
                        subcals.Add(new GasSensorResponseData() { Percent = 21, CalibrationDuration = 10 });
                        break;
                    case CalibrationMode.Quick:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 30 });
                        subcals.Add(new GasSensorResponseData() { Percent = 21, CalibrationDuration = 30 });
                        break;
                    case CalibrationMode.Standard:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 21, CalibrationDuration = 60 });
                        break;
                    case CalibrationMode.Precise:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 5, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 10, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 15, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 21, CalibrationDuration = 60 });
                        break;
                }
                Profiles[CalibrationTarget.Oxygen].Add(mode, subcals);
            }
        }
        private static void AddCO2Profiles()
        {
            foreach (CalibrationMode mode in Enum.GetValues(typeof(CalibrationMode)))
            {
                List<GasSensorResponseData> subcals = new List<GasSensorResponseData>();
                switch (mode)
                {
                    case CalibrationMode.Debug:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 10 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.4, CalibrationDuration = 10 });
                        break;
                    case CalibrationMode.Quick:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 30 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.4, CalibrationDuration = 30 });
                        break;
                    case CalibrationMode.Standard:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.4, CalibrationDuration = 60 });
                        break;
                    case CalibrationMode.Precise:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.1, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.25, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.5, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.1, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.5, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.75, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 1, CalibrationDuration = 60 });
                        break;
                }
                Profiles[CalibrationTarget.Carbon_Dioxide].Add(mode, subcals);
            }
        }
        private static void AddCHxProfiles()
        {
            foreach (CalibrationMode mode in Enum.GetValues(typeof(CalibrationMode)))
            {
                List<GasSensorResponseData> subcals = new List<GasSensorResponseData>();
                switch (mode)
                {
                    case CalibrationMode.Debug:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 10 });
                        subcals.Add(new GasSensorResponseData() { Percent = 10, CalibrationDuration = 10 });
                        break;
                    case CalibrationMode.Quick:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 30 });
                        subcals.Add(new GasSensorResponseData() { Percent = 10, CalibrationDuration = 30 });
                        break;
                    case CalibrationMode.Standard:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 10, CalibrationDuration = 60 });
                        break;
                    case CalibrationMode.Precise:
                        subcals.Add(new GasSensorResponseData() { Percent = 0, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.1, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.25, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 0.5, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 1, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 5, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 7.5, CalibrationDuration = 60 });
                        subcals.Add(new GasSensorResponseData() { Percent = 10, CalibrationDuration = 60 });
                        break;
                }
                Profiles[CalibrationTarget.CHx].Add(mode, subcals);
            }
        }

    }
}
