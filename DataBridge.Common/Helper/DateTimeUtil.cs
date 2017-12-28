using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using DataBridge.Extensions;

namespace DataBridge.Helper
{
    public class DateTimeUtil
    {
        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        private static readonly double MaxUnixSeconds = (DateTime.MaxValue - UnixEpoch).TotalSeconds;

        public static string[][] TimeZones = new string[][]
        {
         new string[] {"ACDT", "+10:30", "Australian Central Daylight Time"},
         new string[] {"ACST", "+09:30", "Australian Central Standard Time"},
         new string[] {"ADT", "-03:00", "(US) Atlantic Daylight Time"},
         new string[] {"AEDT", "+11:00", "Australian East Daylight Time"},
         new string[] {"AEST", "+10:00", "Australian East Standard Time"},
         new string[] {"AHDT", "-09:00", ""},
         new string[] {"AHST", "-10:00", ""},
         new string[] {"AST", "-04:00", "(US) Atlantic Standard Time"},
         new string[] {"AT", "-02:00", "Azores Time"},
         new string[] {"AWDT", "+09:00", "Australian West Daylight Time"},
         new string[] {"AWST", "+08:00", "Australian West Standard Time"},
         new string[] {"BAT", "+03:00", "Bhagdad Time"},
         new string[] {"BDST", "+02:00", "British Double Summer Time"},
         new string[] {"BET", "-11:00", "Bering Standard Time"},
         new string[] {"BST", "-03:00", "Brazil Standard Time"},
         new string[] {"BT", "+03:00", "Baghdad Time"},
         new string[] {"BZT2", "-03:00", "Brazil Zone 2"},
         new string[] {"CADT", "+10:30", "Central Australian Daylight Time"},
         new string[] {"CAST", "+09:30", "Central Australian Standard Time"},
         new string[] {"CAT", "-10:00", "Central Alaska Time"},
         new string[] {"CCT", "+08:00", "China Coast Time"},
         new string[] {"CDT", "-05:00", "(US) Central Daylight Time"},
         new string[] {"CED", "+02:00", "Central European Daylight"},
         new string[] {"CET", "+01:00", "Central European Time"},
         new string[] {"CEST", "+02:00", "Central European Summer Time"},
         new string[] {"CST", "-06:00", "(US) Central Standard Time"},
         new string[] {"EAST", "+10:00", "Eastern Australian Standard Time"},
         new string[] {"EDT", "-04:00", "(US) Eastern Dayligh Timet"},
         new string[] {"EED", "+03:00", "Eastern European Daylight"},
         new string[] {"EET", "+02:00", "Eastern Europe Time"},
         new string[] {"EEST", "+03:00", "Eastern Europe Summer Time"},
         new string[] {"EST", "-05:00", "(US) Eastern Standard Time"},
         new string[] {"FST", "+02:00", "French Summer Time"},
         new string[] {"FWT", "+01:00", "French Winter Time"},
         new string[] {"GMT", "-00:00", "Greenwich Mean Time"},
         new string[] {"GST", "+10:00", "Guam Standard"},
         new string[] {"HDT", "-09:00", "Hawaii Daylight Time"},
         new string[] {"HST", "-10:00", "Hawaii Standard Time"},
         new string[] {"IDLE", "+12:00", "Internation Date Line East"},
         new string[] {"IDLW", "-12:00", "Internation Date Line West"},
         new string[] {"IST", "+05:30", "Indian Standard Time"},
         new string[] {"IT", "+03:30", "Iran Time"},
         new string[] {"JST", "+09:00", "Japan Standard"},
         new string[] {"JT", "+07:00", "Java Time"},
         new string[] {"MDT", "-06:00", "(US) Mountain Daylight"},
         new string[] {"MED", "+02:00", "Middle European Daylight"},
         new string[] {"MET", "+01:00", "Middle European Time"},
         new string[] {"MEST", "+02:00", "Middle European Summer Time"},
         new string[] {"MESZ", "+02:00", "Middle European Summer Zeit"},
         new string[] {"MEWT", "+01:00", "Middle European Winter Time"},
         new string[] {"MEWZ", "+01:00", "Middle European Winter Zeit"},
         new string[] {"MST", "-07:00", "(US) Mountain Standard Time"},
         new string[] {"MT", "+08:00", "Moluccas Time"},
         new string[] {"NDT", "-02:30", "Newfoundland Daylight Time"},
         new string[] {"NFT", "-03:30", "Newfoundland Time"},
         new string[] {"NT", "-11:00", "Nome Time"},
         new string[] {"NST", "+06:30", "North Sumatra Time"},
         new string[] {"NZ", "+11:00", "New Zealand"},
         new string[] {"NZST", "+12:00", "New Zealand Standard Time"},
         new string[] {"NZDT", "+13:00", "New Zealand Daylight Time"},
         new string[] {"NZT", "+12:00", "New Zealand"},
         new string[] {"PDT", "-07:00", "(US) Pacific Daylight"},
         new string[] {"PST", "-08:00", "(US) Pacific Standard"},
         new string[] {"ROK", "+09:00", "Republic of Korea"},
         new string[] {"SAD", "+10:00", "South Australia Daylight"},
         new string[] {"SAST", "+09:00", "South Australia Standard"},
         new string[] {"SAT", "+09:00", "South Australia Standard"},
         new string[] {"SDT", "+10:00", "South Australia Daylight Time"},
         new string[] {"SST", "+02:00", "Swedish Summer Time"},
         new string[] {"SWT", "+01:00", "Swedish Winter Time"},
         new string[] {"USZ3", "+04:00", "USSR Zone 3"},
         new string[] {"USZ4", "+05:00", "USSR Zone 4"},
         new string[] {"USZ5", "+06:00", "USSR Zone 5"},
         new string[] {"USZ6", "+07:00", "USSR Zone 6"},
         new string[] {"UT", "-00:00", "Universal Coordinated"},
         new string[] {"UTC", "-00:00", "Universal Coordinated"},
         new string[] {"UZ10", "+11:00", "USSR Zone 10"},
         new string[] {"WAT", "-01:00", "West Africa Time"},
         new string[] {"WET", "-00:00", "West European Time"},
         new string[] {"WET", "-10:00", "West European Time"},
         new string[] {"WST", "+08:00", "West Australian Standard Time"},
         new string[] {"YDT", "-08:00", "Yukon Daylight Time"},
         new string[] {"YST", "-09:00", "Yukon Standard Time"},
         new string[] {"ZP4", "+04:00", "USSR Zone 3"},
         new string[] {"ZP5", "+05:00", "USSR Zone 4"},
         new string[] {"ZP6", "+06:00", "USSR Zone 5"}
        };

        private static string[] dateFormats = new string[]
        {   "MM/dd/yyyy",
                    "MM/dd/yyyy HH:mm:ss",
                    "dd/MM/yyyy",
                    "dd/MM/yyyy HH:mm:ss",
                    "dd.MM.yyyy",
                    "dd.MM.yyyy HH:mm:ss",
                    "yyyyMMdd",
                    "yyMMdd",
                    "yyyy-MM-dd",
                    "yy-MM-dd",
                    "dd-MM-yyyy",
                    "dd-MM-yyyy HH:mm:ss",
                    "dd/MM/yyyy",
                    "MM-dd-yyyy",
                    "dd-MM-yyyy",
                    "dd/MM/yyyy HH:mm::ss",
                    "dd.MM.yy"
        };

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

        public static DateTime UnixTimeStampToDateTime(double unixTimeStamp)
        {
            return unixTimeStamp > MaxUnixSeconds
            ? UnixEpoch.AddMilliseconds(unixTimeStamp)
            : UnixEpoch.AddSeconds(unixTimeStamp);
        }

        public static double DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - UnixEpoch).TotalSeconds;
        }

        public static double DateTimeToJulian(DateTime date)
        {
            int month = date.Month;
            int day = date.Day;
            int year = date.Year;

            if (month < 3)
            {
                month = month + 12;
                year = year - 1;
            }

            double julianDay = day + (153 * month - 457) / 5 + 365 * year + (year / 4) - (year / 100) + (year / 400) + 1721119;
            return julianDay;
        }

        public static DateTime JulianToDateTime(double julianDate)
        {
            DateTime date;
            double dblA, dblB, dblC, dblD, dblE, dblF;
            double dblZ, dblW, dblX;
            int day, month, year;
            try
            {
                dblZ = Math.Floor(julianDate + 0.5);
                dblW = Math.Floor((dblZ - 1867216.25) / 36524.25);
                dblX = Math.Floor(dblW / 4);
                dblA = dblZ + 1 + dblW - dblX;
                dblB = dblA + 1524;
                dblC = Math.Floor((dblB - 122.1) / 365.25);
                dblD = Math.Floor(365.25 * dblC);
                dblE = Math.Floor((dblB - dblD) / 30.6001);
                dblF = Math.Floor(30.6001 * dblE);
                day = Convert.ToInt32(dblB - dblD - dblF);
                if (dblE > 13)
                {
                    month = Convert.ToInt32(dblE - 13);
                }
                else
                {
                    month = Convert.ToInt32(dblE - 1);
                }

                if ((month == 1) || (month == 2))
                {
                    year = Convert.ToInt32(dblC - 4715);
                }
                else
                {
                    year = Convert.ToInt32(dblC - 4716);
                }
                date = new DateTime(year, month, day);
                return date;
            }
            catch (Exception ex)
            {
                date = new DateTime(0);
            }
            return date;
        }

        /// <summary>
        /// Detects the CultureInfo by checking Month and DayNames
        /// </summary>
        /// <param name="dateString"></param>
        /// <returns></returns>
        public static CultureInfo DetectCultureInfobyMonthOrDayNames(string dateString)
        {
            CultureInfo detectedCultureInfo = null;

            foreach (CultureInfo ci in CultureInfo.GetCultures(CultureTypes.AllCultures))
            {
                // check by the month names
                if (dateString.ContainsAny(ci.DateTimeFormat.MonthNames))
                {
                    detectedCultureInfo = ci;
                    break;
                }

                // check by the day names
                if (dateString.ContainsAny(ci.DateTimeFormat.DayNames))
                {
                    detectedCultureInfo = ci;
                    break;
                }
            }

            return detectedCultureInfo;
        }

        /// <summary>
        /// Converts a time zone info to the time offset.
        /// e.g "GMT" -> "-00:00"
        /// </summary>
        /// <param name="timeZoneAbbreviation">The time zone abbreviation.</param>
        /// <returns></returns>
        public static string TimeZoneToOffset(string timeZoneAbbreviation)
        {
            for (int i = 0; i < TimeZones.Length; i++)
            {
                string timeZoneAbbr = ((string)((string[])TimeZones.GetValue(i)).GetValue(0));
                string timeZoneOffSet = ((string)((string[])TimeZones.GetValue(i)).GetValue(1));

                if (timeZoneAbbr == timeZoneAbbreviation)
                {
                    return timeZoneOffSet;
                }
            }

            return string.Empty;

            //return TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).ToString();
        }

        /// <summary>
        /// Replaces the time zone info with the time offset.
        /// e.g. "Fri Jan 2011 15:00:39 GMT" -> "Fri Jan 2011 15:00:39 -00:00"
        /// </summary>
        /// <param name="dateString">The date string.</param>
        /// <returns></returns>
        public static string ReplaceTimeZoneWithOffset(string dateString)
        {
            for (int i = 0; i < TimeZones.Length; i++)
            {
                // "GMT"
                string timeZoneAbbr = ((string)((string[])TimeZones.GetValue(i)).GetValue(0));

                // "-00:00"
                string timeZoneOffSet = ((string)((string[])TimeZones.GetValue(i)).GetValue(1));

                // ends with timezone offset e.g. "Fri Jan 2011 15:00:39 GMT"?
                if (dateString.EndsWith(timeZoneAbbr))
                {
                    // replace "GMT" -> "-00:00"
                    dateString = dateString.Replace(timeZoneAbbr, timeZoneOffSet);
                    break;
                }
                else
                {
                    // find the timezone offset e.g. "Fri Jan 14 2011 15:00:39 GMT-0800" -> "-0800"
                    Match match = Regex.Match(dateString, timeZoneAbbr + @"\s*((\+|\-)?[0-9]{2}:?[0-9]{2})$", RegexOptions.IgnoreCase);
                    if (match.Success)
                    {
                        // remove a plus e.g.  "+0800" -> "0800"
                        if (timeZoneOffSet.StartsWith("+"))
                        {
                            timeZoneOffSet = timeZoneOffSet.Replace("+", "");
                        }

                        TimeSpan spanTZ = TimeSpan.Parse(timeZoneOffSet);

                        string existingOffSet = match.Value.Replace(timeZoneAbbr, "");

                        // remove a plus e.g. "+0800" -> "0800"
                        if (existingOffSet.StartsWith("+"))
                        {
                            existingOffSet = existingOffSet.Replace("+", "");
                        }

                        if (!existingOffSet.Contains(":"))
                        {
                            if (existingOffSet.StartsWith("+") || existingOffSet.StartsWith("-"))
                            {
                                // e.g. "-0800" -> "-08:00"
                                existingOffSet = existingOffSet.Substring(0, 3) + ":" + existingOffSet.Substring(3, 2);
                            }
                            else
                            {
                                // e.g. "0800" -> "08:00"
                                existingOffSet = existingOffSet.Substring(0, 2) + ":" + existingOffSet.Substring(2, 2);
                            }
                        }

                        TimeSpan spanEx = TimeSpan.Parse(existingOffSet);
                        TimeSpan spanComplete = spanTZ.Add(spanEx);
                        string tzComplete = (spanComplete < TimeSpan.Zero ? "-" : "") + spanComplete.ToString("hh\\:mm");

                        // replace in the original string
                        dateString = dateString.Substring(0, match.Index) + tzComplete + dateString.Substring(match.Index + match.Length);
                    }
                }
            }

            return dateString;
        }

        /// <summary>
        /// Converts a date from one timzone to a date in another timezone.
        /// The are in registry under "HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Time Zones"
        /// </summary>
        /// <param name="date">The date.</param>
        /// <param name="targetTimezone">The target timezone (e.g. EST, CEST,..).</param>
        /// <returns></returns>
        public static DateTime ToTimezone(DateTime date, string targetTimezone)
        {
            TimeZoneInfo tzTo = TimeZoneInfo.FindSystemTimeZoneById(targetTimezone);
            DateTime output = TimeZoneInfo.ConvertTimeFromUtc(date.ToUniversalTime(), tzTo);
            return output;
        }

        public static DateTime TryParseDate(string date, string twoLetterCountryCode = null)
        {
            var result = DateTime.MinValue;
            IFormatProvider formatProvider = CultureInfo.InvariantCulture;

            // TimeZone abbreviated name codes are not supported in .NET (RFC822 date formats)!
            // e.g. "Fri Jan 2011 15:00:39 GMT" -> "Fri Jan 2011 15:00:39 -00:00"
            date = DateTimeUtil.ReplaceTimeZoneWithOffset(date);

            // replace the UTC (which cannot be parsed) with the ISO-8601 conform 'Z' which means Zulu.
            //if (date.EndsWith(" UTC"))
            //{
            //    date = date.Replace(" UTC", "Z");
            //}

            // parse with the given culture
            if (result == DateTime.MinValue && !string.IsNullOrEmpty(twoLetterCountryCode))
            {
                var givenCultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures).Where(c => c.Name.EndsWith(twoLetterCountryCode));

                foreach (var culture in givenCultureInfos)
                {
                    DateTime.TryParse(date, culture, DateTimeStyles.None, out result);

                    if (result != DateTime.MinValue)
                    {
                        break;
                    }
                }
            }

            // parse with the invariant culture
            if (result == DateTime.MinValue)
            {
                if (date.EndsWith(" PM") || date.EndsWith(" AM") || date.Contains("/"))
                {
                    // american-english format
                    formatProvider = new CultureInfo("en-US");
                }

                DateTime.TryParse(date, formatProvider, DateTimeStyles.None, out result);
            }

            // parse in the current culture
            if (result == DateTime.MinValue)
            {
                formatProvider = CultureInfo.CurrentCulture;
                DateTime.TryParse(date, formatProvider, DateTimeStyles.None, out result);
            }

            // parse individual formats
            if (result == DateTime.MinValue)
            {
                foreach (var format in dateFormats)
                {
                    if (DateTime.TryParseExact(date, format, formatProvider, DateTimeStyles.None, out result))
                    {
                        break;
                    }
                }
            }

            // try to detect the cultureinfo
            if (result == DateTime.MinValue)
            {
                var detectedCulture = DateTimeUtil.DetectCultureInfobyMonthOrDayNames(date);
                if (detectedCulture != null)
                {
                    DateTime.TryParse(date, detectedCulture, DateTimeStyles.None, out result);
                }
            }

            return result;
        }

        public static DateTime? UtcStringToLocalDate(string utcDateString)
        {
            if (utcDateString == null)
            {
                return null;
            }
            return DateTimeOffset.Parse(utcDateString).UtcDateTime.ToLocalTime();
        }

        public static string LocalDateToUtcString(DateTime? date)
        {
            if (date == null)
            {
                return "";
            }

            if (!date.HasValue)
            {
                return "";
            }

            return LocalDateToUtcString(date.Value);
        }

        public static string LocalDateToUtcString(DateTime date)
        {
            var utcCreateDate = date.ToUniversalTime();
            return utcCreateDate.ToString("s", CultureInfo.InvariantCulture);
        }
    }
}
