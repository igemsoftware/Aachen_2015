using MCP.Calibration;
using MCP.Protocol;
using System;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Data;

namespace PumpCalibrator
{
    public class UIConverter : IValueConverter
    {
        private static char[] InvalidFileNameCharacters = System.IO.Path.GetInvalidFileNameChars();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (parameter as string)
            {
                case "BaudRate":
                    if (value is int)
                        return (BaudRate)value;
                    else
                        return (int)value;
                case "IsLegalFilenameString":
                    if (string.IsNullOrWhiteSpace(value as string))
                        return false;
                    foreach (char c in value as string)
                        if (InvalidFileNameCharacters.Contains(c))
                            return false;
                    return true;
                case "BoolToVisibility": return ((bool)value == true) ? Visibility.Visible : Visibility.Collapsed;
                case "TargetToAppTitle": return string.Format("{0} Calibrator", value);
                case "TargetToChartTitle": return string.Format("{0} Calibration Profile", value);
                case "TargetToYAxis":
                    if (value == null)
                        return null;
                    switch ((CalibrationTarget)value)
                    {
                        case CalibrationTarget.Pump:
                            return "pumped volume   [ml]";
                        case CalibrationTarget.Stirrer:
                            return "rotations   [-]";
                        default:
                            return null;
                    }
                case "TimeSpanToString":
                    string result = string.Empty;
                    TimeSpan val = (TimeSpan)value;
                    if (val.Hours > 0)
                        result += string.Format("{0} hours ", val.Hours);
                    if (val.Minutes > 0)
                        result += string.Format("{0} minutes and ", val.Minutes);
                    if (val.Seconds > 0)
                        result += string.Format("{0} seconds", val.Seconds);
                        return result;
                case "IsNotNull": return (value != null);
                case "IsNull": return (value == null);
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (parameter as string)
            {
                case "BaudRate":
                    if (value is int)
                        return (BaudRate)value;
                    else
                        return (int)value;
                default:
                    return null;
            }
        }
    }
}
