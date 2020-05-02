namespace DataBridge.GUI.Converters
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    public class ObjectToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            Visibility result;

            // check the types
            if (value is string)
            {
                // string
                result = (!string.IsNullOrEmpty(value as string) ? Visibility.Visible : Visibility.Collapsed);
            }
            else if (value is int)
            {
                // integer
                result = ((int)value > 0 ? Visibility.Visible : Visibility.Collapsed);
            }
            else if (value is bool)
            {
                // boolean
                result = ((bool)value == true ? Visibility.Visible : Visibility.Collapsed);
            }
            else
            {
                // object
                result = (value != null ? Visibility.Visible : Visibility.Collapsed);
            }

            // invert the result
            if (parameter != null && (parameter.Equals("Invert") || parameter.Equals("Inverse")))
            {
                result = (result == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible);
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new ArgumentException("Der übergebene Wert ist kein Boolean", "value");
        }
    }
}