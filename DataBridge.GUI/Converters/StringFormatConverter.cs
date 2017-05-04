namespace DataBridge.GUI.Converters
{
    using System;
    using System.Windows.Data;

    public class StringFormatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
            {
                return null;
            }

            string param = parameter as string;
            if (string.IsNullOrEmpty(param))
            {
                return value;
            }

            if (param.StartsWith("{}"))
            {
                param = param.Replace("{}", "");
            }

            return String.Format(param, value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}