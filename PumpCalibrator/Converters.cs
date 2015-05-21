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
