using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Web.UI;

namespace DataBridge.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// String formatting with named variables
        /// Samples:
        ///
        /// MembershipUser user = Membership.GetUser();
        /// Status.Text = "{UserName} last logged in at {LastLoginDate}".FormatWith(user);
        ///
        /// "{CurrentTime} - {ProcessName}".FormatWith(new { CurrentTime = DateTime.Now, ProcessName = p.ProcessName });
        ///
        /// </summary>
        /// <param name="format"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public static string FormatWith(this string format, object source)
        {
            return FormatWith(format, null, source);
        }

        public static string FormatWith(this string format, IFormatProvider provider, object source)
        {
            if (format == null)
                throw new ArgumentNullException("format");

            Regex r = new Regex(@"(?<start>\{)+(?<property>[\w\.\[\]]+)(?<format>:[^}]+)?(?<end>\})+",
              RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            List<object> values = new List<object>();
            string rewrittenFormat = r.Replace(format, delegate (Match m)
            {
                Group startGroup = m.Groups["start"];
                Group propertyGroup = m.Groups["property"];
                Group formatGroup = m.Groups["format"];
                Group endGroup = m.Groups["end"];

                values.Add((propertyGroup.Value == "0")
                  ? source
                  : DataBinder.Eval(source, propertyGroup.Value));

                return new string('{', startGroup.Captures.Count) + (values.Count - 1) + formatGroup.Value
                  + new string('}', endGroup.Captures.Count);
            });

            return string.Format(provider, rewrittenFormat, values.ToArray());
        }

        /// <summary>
        /// A safe ToString() method, returns an empty string on null
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public static string ToStringOrEmpty(this object value)
        {
            return ((object)value ?? string.Empty).ToString();
        }

        public static string ToUpperOrEmpty(this string str)
        {
            return (str ?? string.Empty).ToUpper();
        }

        public static string ToLowerOrEmpty(this string str)
        {
            return (str ?? string.Empty).ToLower();
        }

        /// <summary>
        /// Takes the specified the string.
        /// </summary>
        /// <param name="theString">The string.</param>
        /// <param name="count">The count.</param>
        /// <param name="ellipsis">if set to <c>true</c> [ellipsis].</param>
        /// <returns></returns>
        public static string Take(this string theString, int count, bool ellipsis)
        {
            return Take(theString, 0, count, ellipsis);
        }

        /// <summary>
        /// Like linq take - takes the first x characters
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="startindex">The startindex.</param>
        /// <param name="count">The count.</param>
        /// <param name="ellipsis">if set to <c>true</c> [ellipsis].</param>
        /// <returns></returns>
        public static string Take(this string str, int startindex, int count, bool ellipsis)
        {
            if (str.Length < startindex)
            {
                return "";
            }

            int lengthToTake = Math.Min(count, str.Length);
            var cutDownString = str.Substring(startindex, Math.Min(lengthToTake, str.Length - startindex));

            if (ellipsis && lengthToTake < str.Length)
            {
                cutDownString += "...";
            }

            return cutDownString;
        }


        /// <summary>
        /// To the title case.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string ToTitleCase(this string str)
        {
            if (str == null)
            {
                return str;
            }

            // Does not work!!
            // return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(str);

            string[] words = str.Split(' ');
            for (int i = 0; i < words.Length; i++)
            {
                if (words[i].Length == 0)
                {
                    continue;
                }

                char firstChar = char.ToUpper(words[i][0]);
                string rest = "";

                if (words[i].Length > 1)
                {
                    rest = words[i].Substring(1).ToLower();
                }

                words[i] = firstChar + rest;
            }

            return string.Join(" ", words);
        }

        /// <summary>
        /// Appends the specified a string to another strin, with the given separator (default is Environment.NewLine)
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="append">The append.</param>
        /// <param name="separator">The separator.</param>
        /// <returns></returns>
        public static string Append(this string str, string append, string separator = "\r\n")
        {
            if (!string.IsNullOrEmpty(str) && !string.IsNullOrEmpty(append) && !string.IsNullOrEmpty(separator))
            {
                str += separator;
            }

            str += append;
            return str;
        }

        /// <summary>
        /// Like the original builtin Contains(), but with a stringcomparasion option for caseinsensitve comparing.
        /// Checks, if the given string is in the original string.
        /// Ift can be used with a stringcomparision option, for caseinsensitive checking
        /// e.g.  str.Contains("test", StringComparison.OrdinalIgnoreCase);
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="toCheck">To check.</param>
        /// <param name="comparisonMode">The comp.</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified STR]; otherwise, <c>false</c>.
        /// </returns>
        public static bool Contains(this string str, string toCheck, StringComparison comparisonMode = StringComparison.InvariantCulture)
        {
            if (toCheck == null || str == null)
            {
                return false;
            }

            if (toCheck == string.Empty || str == string.Empty)
            {
                return true;
            }

            return str.IndexOf(toCheck, comparisonMode) >= 0;
        }

        public static bool ContainsIgnoreCase(this string str, string toCheck)
        {
            return str.Contains(toCheck, StringComparison.InvariantCultureIgnoreCase);
        }

        public static bool ContainsAny(this string str, params string[] toCheck)
        {
            return str.ContainsAny(toCheck, true);
        }

        /// <summary>
        /// checks, if one of the given strings is in the original string
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="toCheck">To check.</param>
        /// <param name="matchCase">if set to <c>true</c> [match case].</param>
        /// <param name="wholeString">if set to <c>true</c> [whole string].</param>
        /// <returns>
        ///   <c>true</c> if [contains] [the specified STR]; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAny(this string str, string[] toCheck, bool matchCase = true, bool wholeString = false)
        {
            if (toCheck == null || str == null)
            {
                return false;
            }

            if (toCheck.Length == 0 || str == string.Empty)
            {
                return true;
            }

            foreach (var currStr in toCheck)
            {
                if (wholeString)
                {
                    if (str == currStr)
                    {
                        return true;
                    }
                }
                else
                {
                    if (matchCase)
                    {
                        if (!string.IsNullOrEmpty(currStr) && str.IndexOf(currStr, StringComparison.Ordinal) >= 0)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(currStr) && str.IndexOf(currStr, StringComparison.OrdinalIgnoreCase) >= 0)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Determines whether the specified string contains any of the given string in the array.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="toCheck">To array of strings to check.</param>
        /// <param name="pos">The found position.</param>
        /// <param name="length">The found length.</param>
        /// <param name="matchCase">if set to <c>true</c> [match case].</param>
        /// <returns>
        ///   <c>true</c> if the specified string contains any of the given string in the array; otherwise, <c>false</c>.
        /// </returns>
        public static bool ContainsAny(this string str, string[] toCheck, out int pos, out int length, bool matchCase = true)
        {
            length = 0;
            pos = -1;
            bool result = false;
            int idx = 0;
            while (idx < toCheck.Length)
            {
                int foundPos = -1;
                if (matchCase)
                {
                    foundPos = str.IndexOf(toCheck[idx], StringComparison.Ordinal);
                }
                else
                {
                    foundPos = str.IndexOf(toCheck[idx], StringComparison.OrdinalIgnoreCase);
                }

                if (foundPos != -1)
                {
                    length = toCheck[idx].Length;
                    pos = foundPos;
                    result = true;
                }
                break;
            }

            idx = idx + 1;

            return result;
        }

        /// <summary>
        /// Chars at.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public static string CharAt(this string str, int index)
        {
            if (index >= str.Length)
            {
                return "";
            }

            return str[index].ToString();
        }

        /// <summary>
        /// Like SubString, but does not throw an error, if the string is shorter than the given length
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="length">number of chars to take</param>
        /// <returns>
        /// the cutted string
        /// </returns>
        public static string Truncate(this string str, int length)
        {
            return Take(str, length, false);
        }

        /// <summary>
        /// Tries the sub string.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <param name="startindex">The startindex.</param>
        /// <param name="length">The length.</param>
        /// <returns></returns>
        public static string Truncate(this string str, int startindex, int length)
        {
            return Take(str, startindex, length, false);
        }

        /// <summary>
        /// Tries the trim.
        /// </summary>
        /// <param name="str">The STR.</param>
        /// <returns></returns>
        public static string TryTrim(this string str)
        {
            if (str == null)
            {
                return str;
            }

            return str.Trim();
        }
    }
}