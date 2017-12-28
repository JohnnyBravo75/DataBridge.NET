using System;
using System.Globalization;
using System.Threading;

namespace DataBridge.Handler.Services.Converters
{
    public class NumberFormatConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                if (culture == null)
                {
                    // culture = CultureInfo.InvariantCulture;
                    culture = Thread.CurrentThread.CurrentCulture;
                }

                // new culture in which the number separators can be changed

                var parseCulture = new CultureInfo(culture.Name);

                if (parameter is string && !string.IsNullOrEmpty(parameter as string))
                {
                    parseCulture.NumberFormat.NumberDecimalSeparator = (parameter as string)[0].ToString();
                    parseCulture.NumberFormat.NumberGroupSeparator = (parameter as string)[1].ToString();
                }
                NumberStyles style = NumberStyles.Any;

                if (targetType == typeof(decimal))
                {
                    decimal returnDecimal;
                    decimal.TryParse(value as string, style, parseCulture, out returnDecimal);
                    return returnDecimal;
                }
                else if (targetType == typeof(int))
                {
                    int returnInt;
                    int.TryParse(value as string, style, parseCulture, out returnInt);
                    return returnInt;
                }
                else if (targetType == typeof(float))
                {
                    float returnFloat;
                    float.TryParse(value as string, style, parseCulture, out returnFloat);
                    return returnFloat;
                }
                else if (targetType == typeof(double))
                {
                    double returnDouble;
                    double.TryParse(value as string, style, parseCulture, out returnDouble);
                    return returnDouble;
                }
            }
            return value;
        }
    }
}