using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ODCalibrator
{
    public class CalibrationPoint
    {
        public double Input { get; set; }
        public double Interval { get; set; }

        public CalibrationPoint()
        {

        }
        public CalibrationPoint(double input, double interval)
        {
            this.Input = input;
            this.Interval = interval;
        }
    }
}
