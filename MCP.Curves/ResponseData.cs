using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Accord.Statistics.Models.Regression.Linear;
using TCD;

namespace MCP.Curves
{
    public class PumpResponseData
    {
        [XmlAttribute]
        public double Setpoint { get; set; }
        [XmlAttribute]
        public double Response { get; set; }

        [XmlAttribute]
        public double Std { get; set; }

        public PumpResponseData()
        {

        }
    }
    public class SensorResponseData : PropertyChangedBase
    {
        private double _Analog = double.NaN;
        [XmlAttribute]
        public double Analog { get { return _Analog; } set { _Analog = value; OnPropertyChanged(); } }


        private double _AnalogStd = double.NaN;
        [XmlAttribute]
        public double AnalogStd { get { return _AnalogStd; } set { _AnalogStd = value; OnPropertyChanged(); } }

        private double _CalibrationDuration = double.NaN;
        [XmlAttribute]
        public double CalibrationDuration { get { return _CalibrationDuration; } set { _CalibrationDuration = value; OnPropertyChanged(); } }

        public SensorResponseData()
        {

        }
    }
    public class BiomassResponseData : SensorResponseData
    {
        private double _OD = double.NaN;
        [XmlAttribute]
        public double OD { get { return _OD; } set { _OD = value; OnPropertyChanged(); } }

        private double _CDW = double.NaN;
        [XmlAttribute]
        public double CDW { get { return _CDW; } set { _CDW = value; OnPropertyChanged(); } }

        public BiomassResponseData()
        {

        }
    }
    public class GasSensorResponseData : SensorResponseData
    {
        public double Percent { get; set; }

        public GasSensorResponseData()
        {

        }
    }
    public static class RegressionExtensions
    {
        public static double LinearTransformResponseToSetpoint(this List<PumpResponseData> curve, double response)
        {
            // linear regression without the intercept term (see http://en.wikipedia.org/wiki/Simple_linear_regression#Linear_regression_without_the_intercept_term)
            double products = curve.Select(d => d.Setpoint * d.Response).Sum();
            double squares = curve.Select(d => d.Response * d.Response).Sum();
            return response * (products / squares);
        }
        public static double LinearTransformAnalogToOD(this List<BiomassResponseData> curve, double analog)
        {
            SimpleLinearRegression slr = SimpleLinearRegression.FromData(curve.Select(p => p.Analog).ToArray(), curve.Select(p => p.OD).ToArray());
            return slr.Compute(analog);
        }
        public static double ExponentialTransformAnalogToOD(this List<BiomassResponseData> curve, double analog)
        {
            PolynomialRegression pr = PolynomialRegression.FromData(3, curve.Select(p => p.Analog).ToArray(), curve.Select(p => p.OD).ToArray());
            return pr.Compute(analog);
        }
        public static double LinearTransformAnalogToCDW(this List<BiomassResponseData> curve, double analog)
        {
            SimpleLinearRegression slr = SimpleLinearRegression.FromData(curve.Select(p => p.Analog).ToArray(), curve.Select(p => p.CDW).ToArray());
            return slr.Compute(analog);
        }
        public static double ExponentialTransformAnalogToCDW(this List<BiomassResponseData> curve, double analog)
        {
            PolynomialRegression pr = PolynomialRegression.FromData(3, curve.Select(p => p.Analog).ToArray(), curve.Select(p => p.CDW).ToArray());
            return pr.Compute(analog);
        }
        public static double LinearTransformAnalogToPercent(this List<GasSensorResponseData> curve, double analog)
        {
            SimpleLinearRegression slr = SimpleLinearRegression.FromData(curve.Select(p => p.Analog).ToArray(), curve.Select(p => p.Percent).ToArray());
            return slr.Compute(analog);
        }
        public static double[] ChangePerHourByLinearRegression(this IEnumerable<DataPoint> data)
        {
            if (data.Count() < 6)
                return new double[] { double.NaN, double.NaN };
            //split the dataset into three sets
            List<DataPoint> set1 = new List<DataPoint>();
            List<DataPoint> set2 = new List<DataPoint>();
            List<DataPoint> set3 = new List<DataPoint>();
            for (int i = 0; i < data.Count(); i += 3)
                set1.Add(data.ElementAt(i));
            for (int i = 1; i < data.Count(); i += 3)
                set2.Add(data.ElementAt(i));
            for (int i = 2; i < data.Count(); i += 3)
                set3.Add(data.ElementAt(i));
            //calculate three Linear Regressions
            DateTime start = data.FirstOrDefault().Time;
            SimpleLinearRegression slr1 = SimpleLinearRegression.FromData(set1.Select(d => (d.Time - start).TotalHours).ToArray(), set1.Select(d => d.YValue).ToArray());
            SimpleLinearRegression slr2 = SimpleLinearRegression.FromData(set2.Select(d => (d.Time - start).TotalHours).ToArray(), set2.Select(d => d.YValue).ToArray());
            SimpleLinearRegression slr3 = SimpleLinearRegression.FromData(set3.Select(d => (d.Time - start).TotalHours).ToArray(), set3.Select(d => d.YValue).ToArray());
            //return the average and standard deviation of the slopes
            double[] slopes = new double[] { slr1.Slope, slr2.Slope, slr3.Slope };
            return new double[] { slopes.Average(), slopes.StdDev() };
        }
        public static double StdDev(this IEnumerable<double> values)
        {
            double ret = double.NaN;
            if (values.Count() > 2)
            {
                //Compute the Average      
                double avg = values.Average();
                //Perform the Sum of (value-avg)_2_2      
                double sum = values.Sum(d => Math.Pow(d - avg, 2));
                //Put it all together      
                ret = Math.Sqrt((sum) / (values.Count() - 1));
            }
            return ret;
        }

        public static double TransformForward(this PolynomialRegression reg, double x)
        {
            double result = 0;
            int pow = reg.Coefficients.Length - 1;
            foreach (double coefficient in reg.Coefficients)
            {
                result += coefficient * Math.Pow(x, pow);
                pow--;
            }
            return result;
        }
    }
}
