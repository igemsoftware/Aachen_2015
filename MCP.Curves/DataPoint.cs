using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;

namespace MCP.Curves
{
    public class DataPoint : PropertyChangedBase
    {
        private double _YValue;
        public double YValue { get { return _YValue; } set { _YValue = value; OnPropertyChanged(); } }

        private double _XValue;
        public double XValue { get { return _XValue; } set { _XValue = value; OnPropertyChanged(); } }

        private DateTime _Time;
        public DateTime Time { get { return _Time; } set { _Time = value; OnPropertyChanged(); } }

        private double _StandardDeviation;
        public double StandardDeviation { get { return _StandardDeviation; } set { _StandardDeviation = value; OnPropertyChanged(); } }

        public DataPoint(double yvalue, double xvalue)
        {
            this.YValue = yvalue;
            this.XValue = xvalue;
        }
        public DataPoint(DateTime time, double value)
        {
            this.YValue = value;
            this.Time = time;
        }
        public DataPoint(string raw)
        {
            string[] text = raw.Split('\t');
            if (text.Length >= 1)
                this.Time = Convert.ToDateTime(text[0]);
            if (text.Length >= 2)
                this.YValue = Convert.ToDouble(text[1]);
            if (text.Length >= 3)
                this.StandardDeviation = Convert.ToDouble(text[2]);
        }
        public DataPoint(DateTime intervalStart, DateTime intervalEnd, IEnumerable<double> data)
        {
            this.Time = intervalStart + TimeSpan.FromMilliseconds((intervalEnd - intervalStart).TotalMilliseconds / 2);//the time value is in the middle of the interval
            double[] avAndStd = AverageAndStd(data);
            this.YValue = avAndStd[0];
            this.StandardDeviation = avAndStd[1];
        }
        public DataPoint(double xvalue, IEnumerable<double> yvalues)
        {
            this.XValue = xvalue;
            double[] avAndStd = AverageAndStd(yvalues);
            this.YValue = avAndStd[0];
            this.StandardDeviation = avAndStd[1];
        }

        public override string ToString()
        {
            return string.Format("{0}\t{1}\t{2}", Time.ToString("dd.MM.yyyy HH:mm:ss"), YValue, StandardDeviation);//TODO: make Time.ToString or something that is okay with Excel
        }


        public static double[] AverageAndStd(IEnumerable<double> data)
        {
            double average = data.Average(d => d);
            double std = double.NaN;
            if (data.Count() > 2) // for less, std will stay NaN
            {
                double sumOfSquaresOfDifferences = data.Select(val => (val - average) * (val - average)).Sum();
                std = Math.Sqrt(sumOfSquaresOfDifferences / (data.Count() - 1));
            }
            return new double[] { average, std };
        }
    }
}
