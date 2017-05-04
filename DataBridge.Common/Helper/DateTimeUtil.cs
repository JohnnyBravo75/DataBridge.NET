using System;
using System.Globalization;

namespace DataBridge.Helper
{
    public static class DateTimeUtil
    {
        public static DateTime? TryParseExact(string s, string format, IFormatProvider provider = null)
        {
            if (provider == null)
            {
                provider = CultureInfo.InvariantCulture;
            }

            DateTime date;
            if (DateTime.TryParseExact(s, format, provider, DateTimeStyles.None, out date))
            {
                return date;
            }
            else
            {
                return null;
            }
        }

        public static DateTime? TryParse(string text)
        {
            DateTime date;
            if (DateTime.TryParse(text, out date))
            {
                return date;
            }
            else
            {
                return null;
            }
        }
    }
}
