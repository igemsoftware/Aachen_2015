using MCP.Calibration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PumpCalibrator
{
    public static class CalibrationProfiles
    {
        public static Dictionary<CalibrationTarget, Dictionary<CalibrationMode, List<int[]>>> Profiles = new Dictionary<CalibrationTarget, Dictionary<CalibrationMode, List<int[]>>>();
        public static Dictionary<CalibrationTarget, string> Symbols = new Dictionary<CalibrationTarget, string>();
        public static Dictionary<CalibrationTarget, string> Units = new Dictionary<CalibrationTarget, string>();

        static CalibrationProfiles()
        {
            Profiles.Add(CalibrationTarget.Pump, new Dictionary<CalibrationMode, List<int[]>>());
            Profiles.Add(CalibrationTarget.Stirrer, new Dictionary<CalibrationMode, List<int[]>>());
            Symbols.Add(CalibrationTarget.Pump, "pump1");
            Symbols.Add(CalibrationTarget.Stirrer, "n");
            Units.Add(CalibrationTarget.Pump, "sph");
            Units.Add(CalibrationTarget.Stirrer, "rpm");
            AddPumpProfiles();
            AddStirrerProfiles();
        }
        private static void AddPumpProfiles()
        {
            foreach (CalibrationMode mode in Enum.GetValues(typeof(CalibrationMode)))
            {
                List<int[]> subcals = new List<int[]>();
                switch (mode)
                {
                    case CalibrationMode.Debug:
                        subcals.Add(new int[] { 5000, 30 });
                        subcals.Add(new int[] { 10000, 20 });
                        subcals.Add(new int[] { 20000, 15 });
                        subcals.Add(new int[] { 40000, 10 });
                        break;
                    case CalibrationMode.Quick:
                        subcals.Add(new int[] { 5000, 240 });
                        subcals.Add(new int[] { 10000, 120 });
                        subcals.Add(new int[] { 20000, 60 });
                        subcals.Add(new int[] { 40000, 30 });
                        break;
                    case CalibrationMode.Standard:
                        subcals.Add(new int[] { 1000, 1200 });
                        subcals.Add(new int[] { 5000, 340 });
                        subcals.Add(new int[] { 7500, 240 });
                        subcals.Add(new int[] { 10000, 180 });
                        subcals.Add(new int[] { 15000, 120 });
                        subcals.Add(new int[] { 20000, 90 });
                        subcals.Add(new int[] { 30000, 60 });
                        subcals.Add(new int[] { 40000, 45 });
                        break;
                    case CalibrationMode.Precise:
                        subcals.Add(new int[] { 1000, 1200 });
                        subcals.Add(new int[] { 1500, 960 });
                        subcals.Add(new int[] { 3000, 480 });
                        subcals.Add(new int[] { 5000, 340 });
                        subcals.Add(new int[] { 7500, 240 });
                        subcals.Add(new int[] { 10000, 180 });
                        subcals.Add(new int[] { 15000, 120 });
                        subcals.Add(new int[] { 20000, 90 });
                        subcals.Add(new int[] { 30000, 60 });
                        subcals.Add(new int[] { 40000, 45 });
                        break;
                    case CalibrationMode.AerationPrecise:
                        subcals.Add(new int[] {   40000, 15 });
                        subcals.Add(new int[] {   70000, 15 });
                        subcals.Add(new int[] {  150000, 15 });
                        subcals.Add(new int[] {  300000, 15 });
                        subcals.Add(new int[] {  600000, 15 });
                        subcals.Add(new int[] {  900000, 15 });
                        subcals.Add(new int[] { 1200000, 15 });
                        subcals.Add(new int[] { 1500000, 15 });
                        subcals.Add(new int[] { 2000000, 15 });
                        subcals.Add(new int[] { 2500000, 15 });
                        break;
                }
                Profiles[CalibrationTarget.Pump].Add(mode, subcals);
            }
        }
        private static void AddStirrerProfiles()
        {
            foreach (CalibrationMode mode in Enum.GetValues(typeof(CalibrationMode)))
            {
                List<int[]> subcals = new List<int[]>();
                switch (mode)
                {
                    case CalibrationMode.Debug:
                        subcals.Add(new int[] { 100, 20 });
                        subcals.Add(new int[] { 400, 10 });
                        subcals.Add(new int[] { 800, 10 });
                        subcals.Add(new int[] { 1900, 10 });
                        break;
                    case CalibrationMode.Quick:
                        subcals.Add(new int[] { 100, 30 });
                        subcals.Add(new int[] { 400, 20 });
                        subcals.Add(new int[] { 800, 15 });
                        subcals.Add(new int[] { 1900, 10 });
                        break;
                    case CalibrationMode.Standard:
                        subcals.Add(new int[] { 100, 40 });
                        subcals.Add(new int[] { 200, 25 });
                        subcals.Add(new int[] { 300, 20 });
                        subcals.Add(new int[] { 400, 15 });
                        subcals.Add(new int[] { 550, 15 });
                        subcals.Add(new int[] { 700, 10 });
                        subcals.Add(new int[] { 900, 10 });
                        subcals.Add(new int[] { 1100, 10 });
                        subcals.Add(new int[] { 1500, 10 });
                        subcals.Add(new int[] { 1900, 10 });
                        break;
                    case CalibrationMode.Precise:
                        subcals.Add(new int[] { 100, 45 });
                        subcals.Add(new int[] { 150, 30 });
                        subcals.Add(new int[] { 200, 20 });
                        subcals.Add(new int[] { 250, 20 });
                        subcals.Add(new int[] { 300, 15 });
                        subcals.Add(new int[] { 400, 10 });
                        subcals.Add(new int[] { 500, 10 });
                        subcals.Add(new int[] { 600, 10 });
                        subcals.Add(new int[] { 700, 10 });
                        subcals.Add(new int[] { 800, 10 });
                        subcals.Add(new int[] { 900, 10 });
                        subcals.Add(new int[] { 1000, 10 });
                        subcals.Add(new int[] { 1300, 10 });
                        subcals.Add(new int[] { 1600, 10 });
                        subcals.Add(new int[] { 1900, 10 });
                        break;
                }
                Profiles[CalibrationTarget.Stirrer].Add(mode, subcals);
            }
        }
    }
}
