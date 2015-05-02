using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Linq;
using System.Windows.Data;
using MCP.Protocol;

namespace MasterControlProgram
{
    public class UIConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            switch (parameter as string)
            {
                case "BaudRate":
                    if (value is int)
                        return (BaudRate)value;
                    else
                        return (int)value;
                case "ReactorID": return string.Format("Reactor {0}", value);
                case "IsLegalFilenameString":
                    if (string.IsNullOrWhiteSpace(value as string))
                        return false;
                    char[] invalid = System.IO.Path.GetInvalidFileNameChars();
                    foreach (char c in value as string)
                        if (invalid.Contains(c))
                            return false;
                    return true;
                case "IsParticipantIDOfReactor": return ((int)value >= (int)ParticipantID.Reactor_1);
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
