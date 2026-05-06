using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace DataBridge.GUI.Converters
{
    /// <summary>
    /// Converts a Directions enum value to a display color for parameter badges.
    /// In = Blue, Out = Green, InOut = Purple
    /// </summary>
    public class DirectionToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Directions)
            {
                switch ((Directions)value)
                {
                    case Directions.In:
                        return new SolidColorBrush(Color.FromRgb(70, 130, 180));   // SteelBlue
                    case Directions.Out:
                        return new SolidColorBrush(Color.FromRgb(60, 160, 80));    // Green
                    case Directions.InOut:
                        return new SolidColorBrush(Color.FromRgb(140, 80, 180));   // Purple
                }
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
