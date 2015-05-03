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
        private static char[] InvalidPathCharacters = System.IO.Path.GetInvalidPathChars();
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
                case "ReactorID": return string.Format("Reactor {0}", value);
                case "IsLegalFilenameString":
                    if (string.IsNullOrWhiteSpace(value as string))
                        return false;
                    foreach (char c in value as string)
                        if (InvalidFileNameCharacters.Contains(c))
                            return false;
                    return true;
                case "IsLegalPathString":
                    if (string.IsNullOrWhiteSpace(value as string))
                        return false;
                    foreach (char c in value as string)
                        if (InvalidPathCharacters.Contains(c))
                            return false;
                    return true;
                case "IsValidExperimentDisplayName":
                    if (string.IsNullOrWhiteSpace(value as string) || (value as string).Length < 10)
                        return false;
                    foreach (char c in value as string)
                        if (InvalidPathCharacters.Contains(c))
                            return false;
                    return true;
                case "IsParticipantIDOfReactor": return ((int)value >= (int)ParticipantID.Reactor_1);
                case "IsNotNull": return (value != null);
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
