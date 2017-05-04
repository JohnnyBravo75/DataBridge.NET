namespace DataBridge.GUI.Converters
{
    using System;
    using System.Windows.Data;

    public class StringTruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string str = value.ToString();

                if (parameter != null)
                {
                    var maxLength = System.Convert.ToInt32(parameter);

                    str = str.Length > maxLength ? str.Substring(0, maxLength) + "..." : str;
                }

                return str;
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is bool)
            {
                return ((bool)value).ToString();
            }

            throw new ArgumentException("Der übergebene Wert ist kein Boolean", "value");
        }
    }
}