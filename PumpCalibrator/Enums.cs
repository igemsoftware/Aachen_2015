using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD.Controls;

namespace PumpCalibrator
{
    public enum CalibrationMode
    {
        [Display(Name = "Debug")]
        Debug,
        [Display(Name = "Quick")]
        Quick,
        [Display(Name = "Standard")]
        Standard,
        [Display(Name = "Precise")]
        Precise
    }
    public enum CalibrationTarget
    {
        Pump,
        Stirrer
    }
}
