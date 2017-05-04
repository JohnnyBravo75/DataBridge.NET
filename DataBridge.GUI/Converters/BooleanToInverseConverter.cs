namespace DataBridge.GUI.Converters
{
    using System;
    using System.Windows.Data;

    public class BooleanToInverseConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !((bool)value);
            }

            throw new NotSupportedException("Dieser Converter kann nur Boolean zu Boolean konvertieren!");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return !((bool)value);
            }

            throw new NotSupportedException("Dieser Converter kann nur Boolean zu Boolean konvertieren!");
        }
    }
}
