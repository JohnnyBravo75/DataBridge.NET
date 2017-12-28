using System;
using System.Globalization;
using System.Linq;

namespace DataBridge.Handler.Services.Converters
{
    public class NumberAutoDetectConverter : ValueConverterBase
    {
        public override object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string)
            {
                string input = value as string;
                double result;
                string output;

                if (string.IsNullOrWhiteSpace(input))
                {
                    return 0d;
                }

                // Taken from https://stackoverflow.com/questions/1354924/how-do-i-parse-a-string-with-a-decimal-point-to-a-double

                // Check if last seperator==groupSeperator
                string groupSep = CultureInfo.CurrentCulture.NumberFormat.NumberGroupSeparator;
                if (input.LastIndexOf(groupSep, StringComparison.InvariantCulture) + 4 == input.Length)
                {
                    bool tryParse = double.TryParse(input, NumberStyles.Any, CultureInfo.CurrentCulture, out result);
                    result = tryParse ? result : 0d;
                }
                else
                {
                    // Unify string (no spaces, only . )
                    output = input.Trim().Replace(" ", string.Empty).Replace(",", ".");

                    // Split it on points
                    string[] split = output.Split('.');

                    if (split.Length > 1)
                    {
                        // Take all parts except last
                        output = string.Join(string.Empty, split.Take(split.Length - 1).ToArray());

                        // Combine token parts with last part
                        output = string.Format("{0}.{1}", output, split.Last());
                    }
                    // Parse double invariant
                    result = double.Parse(output, CultureInfo.InvariantCulture);
                }

                return result;
            }

            return value;
        }
    }
}