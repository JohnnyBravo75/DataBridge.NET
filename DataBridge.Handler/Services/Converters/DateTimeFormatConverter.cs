using System;
using System.Globalization;
using System.Threading;

namespace DataBridge.Handler.Services.Converters
{
    public class DateTimeFormatConverter : ValueConverterBase
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

                DateTime returnValue;

                if (parameter is string && !string.IsNullOrEmpty(parameter as string))
                {
                    DateTime.TryParseExact(value as string, parameter as string, culture, DateTimeStyles.None, out returnValue);
                    return returnValue;
                }
            }

            return value;
        }
    }
}