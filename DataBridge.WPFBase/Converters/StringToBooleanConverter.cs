namespace DataBridge.GUI.Converters
{
    using System;
    using System.Windows.Data;

    public class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value != null)
            {
                string toConvert = value.ToString();

                if (toConvert != null)
                {
                    if (toConvert.ToLower() == "true" || toConvert.ToLower() == "y")
                    {
                        return true;
                    }
                    else if (toConvert.ToLower() == "false" || toConvert.ToLower() == "n")
                    {
                        return false;
                    }
                
                    return toConvert;                    
                 }

                return null;
            }
            else
            {
                return false;
            }
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
