using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Accord.Statistics.Models.Regression.Linear;

namespace MCP.Curves
{
    public class ResponseData
    {
        [XmlAttribute]
        public double Setpoint { get; set; }
        [XmlAttribute]
        public double Response { get; set; }

        public ResponseData()
        {

        }
    }
    public static class ResponseDataExtensions
    {
        public static double LinearTransformResponseToSetpoint(this List<ResponseData> data, double response)
        {
            // linear regression without the intercept term (see http://en.wikipedia.org/wiki/Simple_linear_regression#Linear_regression_without_the_intercept_term)
            double products = data.Select(d => d.Setpoint * d.Response).Sum();
            double squares = data.Select(d => d.Response * d.Response).Sum();
            return response * (products / squares);
        }
    }
}
