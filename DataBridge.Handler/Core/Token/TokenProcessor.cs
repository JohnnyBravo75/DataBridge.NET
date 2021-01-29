using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading;
using DataBridge.Extensions;

namespace DataBridge
{
    public class TokenProcessor
    {
        private const string TOKEN_START = "{";
        private const string TOKEN_END = "}";

        public static string ReplaceTokens(string str, IDictionary<string, object> parameters = null)
        {
            string replacedStr = str;
            bool wasReplaced = ReplaceTokens(str, parameters, out replacedStr);
            return replacedStr;
        }

        public static bool ReplaceTokens(string str, IDictionary<string, object> parameters, out string replacedStr)
        {
            // perhaps better way: https://stackoverflow.com/questions/733378/whats-a-good-way-of-doing-string-templating-in-net

            replacedStr = str;
            bool wasReplaced = false;

            if (string.IsNullOrEmpty(str))
            {
                return false;
            }

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    if (replacedStr.Contains(BuildToken(parameter.Key)))
                    {
                        replacedStr = replacedStr.Replace(BuildToken(parameter.Key), parameter.Value.ToStringOrEmpty());
                        wasReplaced = true;
                    }
                }
            }

            foreach (var dateFmt in DateFormats)
            {
                if (replacedStr.Contains(BuildToken(dateFmt)))
                {
                    replacedStr = replacedStr.Replace(BuildToken(dateFmt), DateTime.Now.ToString(dateFmt));
                    wasReplaced = true;
                }
            }

            return wasReplaced;
        }

        public static string ReplaceToken(string str, string tokenName, object tokenValue)
        {
            var dict = new Dictionary<string, object>()
            {
                { tokenName, tokenValue }
            };

            return ReplaceTokens(str, dict);
        }

        /// <summary>
        /// Parses the token values out of a string
        /// </summary>
        /// <param name="str">The string e.g. PREF_LongNameWithSomething_1234.txt</param>
        /// <param name="template">The template e.g. PREF_LongName{SubName}_{Number}.{Ext}</param>
        /// <returns></returns>
        public static IDictionary<string, object> ParseTokenValues(string str, string template)
        {
            var tokenValues = new Dictionary<string, object>();
            if (string.IsNullOrEmpty(str) || string.IsNullOrEmpty(template))
            {
                return tokenValues;
            }

            var tokenNames = ParseTokens(template);

            // convert string template into regex for parsing
            string templateRegex = template;

            foreach (var tokenName in tokenNames)
            {
                // PREF_LongName{SubName}_{Number}.{Ext}
                // PREF_LongName(?<SubName>[\w\[\]]+)_(?<Number>[\w\[\]]+).(?<Ext>[\w\[\]]+)
                templateRegex = templateRegex.Replace(TOKEN_START + tokenName + TOKEN_END, @"(?<" + tokenName + @">[\w\[\]]+)");
            }

            var regex = new Regex(templateRegex, RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            Match match = regex.Match(str);

            if (match.Success)
            {
                foreach (string groupName in regex.GetGroupNames())
                {
                    if (tokenNames.Contains(groupName))
                    {
                        tokenValues.AddOrUpdate(groupName, match.Groups[groupName].Value);
                    }
                }
            }

            return tokenValues;
        }

        public static IList<string> ParseTokens(string str)
        {
            var tokens = new List<string>();

            var regex = new Regex(@"(?<start>\" + TOKEN_START + @")+(?<name>[\w\.\[\]]+)(?<end>\" + TOKEN_END + @")+", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            var matches = regex.Matches(str);
            foreach (Match match in matches)
            {
                Group startGroup = match.Groups["start"];
                Group propertyGroup = match.Groups["name"];
                Group endGroup = match.Groups["end"];

                tokens.Add(propertyGroup.Value);
            }

            return tokens;
        }

        private static List<string> dateFormats = null;

        private static List<string> DateFormats
        {
            get
            {
                if (dateFormats == null)
                {
                    dateFormats = new List<string>()
                        {
                            "yyyy", "yy",
                            "MM", "MMM", "MMMM",
                            "dd", "ddd", "dddd",
                            "d","D","f","F",
                            "g","G","m","M",
                            "s","t","T","o","O",
                            "y","Y","u","U",
                            "yyyyMMdd", "yyMM", "yyMMdd", "yyyy-MM-dd"
                        };

                    dateFormats.AddRange(DateTimeFormatInfo.CurrentInfo.GetAllDateTimePatterns('d'));

                    var currentCulture = Thread.CurrentThread.CurrentCulture;

                    dateFormats.AddRange(currentCulture.DateTimeFormat.GetAllDateTimePatterns());
                }

                return dateFormats;
            }
        }

        public static string BuildToken(string name)
        {
            return $"{TOKEN_START}{name}{TOKEN_END}";
        }

        public static string EvaluateExpression(string str, IDictionary<string, object> parameters, bool throwOnError = true)
        {
            if (parameters == null || string.IsNullOrEmpty(str))
            {
                return str;
            }

            foreach (var v in parameters)
            {
                str = str.Replace(BuildToken(v.Key), v.Key);
            }

            var lambdaParser = new NReco.Linq.LambdaParser();

            try
            {
                var result = lambdaParser.Eval(str, parameters);
                return result.ToStringOrEmpty();
            }
            catch (Exception)
            {
                if (throwOnError)
                {
                    throw;
                }

                return str;
            }
        }

        public static string ExpandEnvironmentVariables(string str)
        {
            if (EnvironmentHelper.ContainsEnvironmentVariables(str))
            {
                return EnvironmentHelper.ExpandEnvironmentVariables(str);
            }

            return str;
        }
    }
}