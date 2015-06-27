using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODCalibrator
{
    public static class CalibrationProfiles
    {
        public static Dictionary<CalibrationTarget, Dictionary<CalibrationMode, List<double[]>>> Profiles = new Dictionary<CalibrationTarget, Dictionary<CalibrationMode, List<double[]>>>();
        public static Dictionary<CalibrationTarget, string> Symbols = new Dictionary<CalibrationTarget, string>();
        public static Dictionary<CalibrationTarget, string> Units = new Dictionary<CalibrationTarget, string>();

        static CalibrationProfiles()
        {
            Profiles.Add(CalibrationTarget.OD, new Dictionary<CalibrationMode, List<double[]>>());
            Profiles.Add(CalibrationTarget.Biomass, new Dictionary<CalibrationMode, List<double[]>>());
            Symbols.Add(CalibrationTarget.OD, "OD");
            Symbols.Add(CalibrationTarget.Biomass, "X");
            Units.Add(CalibrationTarget.OD, "sph");
            Units.Add(CalibrationTarget.Biomass, "mgCDW/mL");
            AddODProfiles();
            AddBiomassProfiles();
        }
        private static void AddODProfiles()
        {
            foreach (CalibrationMode mode in Enum.GetValues(typeof(CalibrationMode)))
            {
                List<double[]> subcals = new List<double[]>();
                switch (mode)
                {
                    case CalibrationMode.Debug:
                        subcals.Add(new double[] { 0, 10 });
                        subcals.Add(new double[] { 0.5, 10 });
                        subcals.Add(new double[] { 1.0, 10 });
                        subcals.Add(new double[] { 1.5, 10 });
                        break;
                    case CalibrationMode.Quick:
                        subcals.Add(new double[] { 0, 10 });
                        subcals.Add(new double[] { 0.5, 10 });
                        subcals.Add(new double[] { 1.0, 10 });
                        subcals.Add(new double[] { 1.5, 10 });
                        subcals.Add(new double[] { 2.0, 10 });
                        subcals.Add(new double[] { 3.0, 10 });
                        break;
                    case CalibrationMode.Standard:
                        subcals.Add(new double[] { 0, 15 });
                        subcals.Add(new double[] { 0.25, 15 });
                        subcals.Add(new double[] { 0.50, 15 });
                        subcals.Add(new double[] { 0.75, 15 });
                        subcals.Add(new double[] { 1.00, 15 });
                        subcals.Add(new double[] { 1.50, 15 });
                        subcals.Add(new double[] { 2.00, 15 });
                        subcals.Add(new double[] { 2.50, 15 });
                        subcals.Add(new double[] { 3.00, 15 });
                        break;
                    case CalibrationMode.Precise:
                        subcals.Add(new double[] { 0, 20 });
                        subcals.Add(new double[] { 0.25, 20 });
                        subcals.Add(new double[] { 0.50, 20 });
                        subcals.Add(new double[] { 0.75, 20 });
                        subcals.Add(new double[] { 1.00, 20 });
                        subcals.Add(new double[] { 1.50, 20 });
                        subcals.Add(new double[] { 2.00, 20 });
                        subcals.Add(new double[] { 2.50, 20 });
                        subcals.Add(new double[] { 3.00, 20 });
                        subcals.Add(new double[] { 4.00, 20 });
                        break;
                }
                Profiles[CalibrationTarget.OD].Add(mode, subcals);
            }
        }
        private static void AddBiomassProfiles()
        {
            foreach (CalibrationMode mode in Enum.GetValues(typeof(CalibrationMode)))
            {
                List<double[]> subcals = new List<double[]>();
                switch (mode)
                {
                    case CalibrationMode.Debug:
                        subcals.Add(new double[] { 100, 20 });
                        subcals.Add(new double[] { 400, 10 });
                        subcals.Add(new double[] { 800, 10 });
                        subcals.Add(new double[] { 1900, 10 });
                        break;
                    case CalibrationMode.Quick:
                        subcals.Add(new double[] { 100, 30 });
                        subcals.Add(new double[] { 400, 20 });
                        subcals.Add(new double[] { 800, 15 });
                        subcals.Add(new double[] { 1900, 10 });
                        break;
                    case CalibrationMode.Standard:
                        subcals.Add(new double[] { 100, 40 });
                        subcals.Add(new double[] { 200, 25 });
                        subcals.Add(new double[] { 300, 20 });
                        subcals.Add(new double[] { 400, 15 });
                        subcals.Add(new double[] { 550, 15 });
                        subcals.Add(new double[] { 700, 10 });
                        subcals.Add(new double[] { 900, 10 });
                        subcals.Add(new double[] { 1100, 10 });
                        subcals.Add(new double[] { 1500, 10 });
                        subcals.Add(new double[] { 1900, 10 });
                        break;
                    case CalibrationMode.Precise:
                        subcals.Add(new double[] { 100, 45 });
                        subcals.Add(new double[] { 150, 30 });
                        subcals.Add(new double[] { 200, 20 });
                        subcals.Add(new double[] { 250, 20 });
                        subcals.Add(new double[] { 300, 15 });
                        subcals.Add(new double[] { 400, 10 });
                        subcals.Add(new double[] { 500, 10 });
                        subcals.Add(new double[] { 600, 10 });
                        subcals.Add(new double[] { 700, 10 });
                        subcals.Add(new double[] { 800, 10 });
                        subcals.Add(new double[] { 900, 10 });
                        subcals.Add(new double[] { 1000, 10 });
                        subcals.Add(new double[] { 1300, 10 });
                        subcals.Add(new double[] { 1600, 10 });
                        subcals.Add(new double[] { 1900, 10 });
                        break;
                }
                Profiles[CalibrationTarget.Biomass].Add(mode, subcals);
            }
        }
    }
}
