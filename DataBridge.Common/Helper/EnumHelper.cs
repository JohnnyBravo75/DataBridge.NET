using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DataBridge.Extensions;

namespace DataBridge
{
    public static class EnumHelper
    {
        /// <summary>
        /// Gets the description.
        /// </summary>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>the description</returns>
        public static string GetDescription(object enumValue)
        {
            FieldInfo fi = enumValue.GetType().GetField(enumValue.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

            string description = (attributes.Length > 0) ? attributes[0].Description : enumValue.ToString();
            return description;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetAllValuesAndDescriptions<TEnum>() where TEnum : struct, IConvertible, IComparable, IFormattable
        {
            if (!typeof(TEnum).IsEnum)
            {
                throw new ArgumentException("TEnum must be an Enumeration type");
            }

            return from e in Enum.GetValues(typeof(TEnum)).Cast<Enum>()
                   select new KeyValuePair<string, string>(e.ToString(), e.Description());
        }

        /// <summary>
        /// Gets all enum values.
        /// </summary>
        /// <typeparam name="T">the type of the enum</typeparam>
        /// <returns>the enum values</returns>
        public static IQueryable GetAllEnumValues<T>()
        {
            IQueryable retVal = null;

            Type targetType = typeof(T);
            if (targetType.IsEnum)
            {
                retVal = Enum.GetValues(targetType).AsQueryable();
            }

            return retVal;
        }

        public static IEnumerable<T> GetValues<T>()
        {
            return Enum.GetValues(typeof(T)).Cast<T>();
        }

        /// <summary>
        /// Gets the enumeration.
        /// </summary>
        /// <typeparam name="T">the type of the enum</typeparam>
        /// <param name="enumName">Name of the enum.</param>
        /// <returns>the enumeration object</returns>
        public static object GetEnumeration<T>(string enumName)
        {
            return Enum.Parse(typeof(T), enumName, true);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <typeparam name="T">the type of the enum</typeparam>
        /// <param name="enumValue">The enum value.</param>
        /// <returns>the name</returns>
        public static object GetName<T>(object enumValue)
        {
            return Enum.GetName(typeof(T), enumValue);
        }
    }
}