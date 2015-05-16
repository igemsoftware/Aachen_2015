using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;

namespace MCP.Measurements
{
    public class DataPoint : PropertyChangedBase
    {
        private double _Value;
        public double Value { get { return _Value; } set { _Value = value; OnPropertyChanged("Value"); } }

        private DateTime _Time;
        public DateTime Time { get { return _Time; } set { _Time = value; OnPropertyChanged("Time"); } }

        private double _StandardDeviation;
        public double StandardDeviation { get { return _StandardDeviation; } set { _StandardDeviation = value; OnPropertyChanged("StandardDeviation"); } }



        public DataPoint(DateTime time, double value)
        {
            this.Time = time;
            this.Value = value;
        }
        public DataPoint(DateTime intervalStart, DateTime intervalEnd, List<double> data)
        {
            this.Time = intervalStart + TimeSpan.FromMilliseconds((intervalEnd - intervalStart).TotalMilliseconds / 2);//the time value is in the middle of the interval
            this.Value = data.Average(d => d);
            if (data.Count > 2)
            {
                double sumOfSquaresOfDifferences = data.Select(val => (val - this.Value) * (val - this.Value)).Sum();
                this.StandardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / (data.Count - 1));
            }            
        }
    }
}
