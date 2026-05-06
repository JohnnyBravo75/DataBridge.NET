using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DataBridge.GUI.Converters
{
    /// <summary>
    /// Converts a list of validation messages to a status brush.
    /// Empty = Green (valid), non-empty = Red (invalid).
    /// </summary>
    public class ValidationStatusToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messages = value as IList<string>;
            if (messages == null || messages.Count == 0)
            {
                return new SolidColorBrush(Color.FromRgb(60, 160, 80));  // Green
            }
            return new SolidColorBrush(Color.FromRgb(200, 50, 50));      // Red
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts a list of validation messages to a tooltip string.
    /// </summary>
    public class ValidationStatusToTooltipConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messages = value as IList<string>;
            if (messages == null || messages.Count == 0)
            {
                return "Konfiguration OK";
            }
            return string.Join("\n", messages);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
