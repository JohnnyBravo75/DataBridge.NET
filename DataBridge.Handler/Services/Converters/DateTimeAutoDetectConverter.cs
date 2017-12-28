using System;
using System.Globalization;
using DataBridge.Helper;

namespace DataBridge.Handler.Services.Converters
{
    public class DateTimeAutoDetectConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                string twoLetterCountryCode = "";

                if (parameter is string && !string.IsNullOrEmpty(parameter as string) && (parameter as string).Length == 2)
                {
                    // parameter contains countrycode e.g. "DE"
                    twoLetterCountryCode = parameter as string;
                }
                else if (culture != null)
                {
                    // get countrycode from culture e.g. "DE"
                    var region = new RegionInfo(culture.LCID);
                    twoLetterCountryCode = region.TwoLetterISORegionName;
                }

                DateTime returnValue;

                returnValue = DateTimeUtil.TryParseDate(value as string, twoLetterCountryCode);
                return returnValue;
            }

            return value;
        }
    }
}