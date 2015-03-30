using Microsoft.Research.DynamicDataDisplay.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TCD;

namespace MasterControlProgram
{
    public class SensorData : PropertyChangedBase
    {
        private double _Value;
        public double Value { get { return _Value; } set { _Value = value; OnPropertyChanged("Value"); } }

        private double _Time;
        public double Time { get { return _Time; } set { _Time = value; OnPropertyChanged("Time"); } }

        private double _StandardDeviation;
        public double StandardDeviation { get { return _StandardDeviation; } set { _StandardDeviation = value; OnPropertyChanged("StandardDeviation"); } }



        public SensorData(double time, double value)
        {
            this.Time = time;
            this.Value = value;
        }
        public SensorData(List<double[]> data)
        {
            this.Time = data.Average(d => d[0]);
            this.Value = data.Average(d => d[1]);
            double sumOfSquaresOfDifferences = data.Select(val => (val[1] - this.Value) * (val[1] - this.Value)).Sum();
            this.StandardDeviation = Math.Sqrt(sumOfSquaresOfDifferences / (data.Count - 1));
        }
    }
    public class SensorDataPointCollection : RingArray<SensorData>
    {
        //private const int TOTAL_POINTS = ViewModel.SecondsToShow * 1000 / ViewModel.UpdateInterval;
        private const int TOTAL_POINTS = 1000;

        public SensorDataPointCollection()
            : base(TOTAL_POINTS) // here i set how much values to show 
        {
        }
    }
}
