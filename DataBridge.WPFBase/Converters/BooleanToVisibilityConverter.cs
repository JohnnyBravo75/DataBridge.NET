namespace DataBridge.GUI.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility result;

            if (parameter != null && (parameter.Equals("Invert") || parameter.Equals("Inverse")))
            {
                if ((bool)value)
                {
                    result = Visibility.Collapsed;
                }
                else
                {
                    result = Visibility.Visible;
                }
            }
            else
            {
                if ((bool)value)
                {
                    result = Visibility.Visible;
                }
                else
                {
                    result = Visibility.Collapsed;
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

