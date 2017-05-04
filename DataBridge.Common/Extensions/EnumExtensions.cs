using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBridge.Extensions
{
    public static class EnumExtensions
    {
        public static string Description(this Enum enumValue)
        {
            return EnumHelper.GetDescription(enumValue);
        }

        public static bool HasFlag<T>(this T value, T flag) where T : struct
        {
            long lValue = Convert.ToInt64(value);
            long lFlag = Convert.ToInt64(flag);
            return (lValue & lFlag) != 0;
        }

        public static IEnumerable<T> GetFlags<T>(this T value) where T : struct
        {
            foreach (T flag in Enum.GetValues(typeof(T)).Cast<T>())
            {
                if (value.HasFlag(flag))
                    yield return flag;
            }
        }
    }
}