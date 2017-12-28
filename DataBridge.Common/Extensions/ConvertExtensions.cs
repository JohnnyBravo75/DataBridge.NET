namespace DataBridge.Extensions
{
    using System;
    using System.Globalization;

    public static class ConvertExtensions
    {
        /// <summary>
        /// Returns True if the type can get Null as a value (is a reference type and not a value one)
        /// </summary>
        public static bool IsNullable(Type type)
        {
            if (!type.IsGenericType)
            {
                return false;
            }

            Type typeDef = type.GetGenericTypeDefinition();
            return (typeDef == typeof(Nullable<>));
        }

        /// <summary>
        /// Converts to a given type
        /// </summary>
        /// <typeparam name="T">the target type to convert</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="throwExceptionOnError">if set to <c>true</c> [throw exception on error].</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>
        /// the converted value
        /// </returns>
        public static T ConvertTo<T>(this object value, CultureInfo culture = null, bool throwExceptionOnError = false)
        {
            try
            {
                if (value == null || string.IsNullOrEmpty(value.ToString()))
                {
                    return default(T);
                }

                Type type = typeof(T);

                if (IsNullable(type))
                {
                    type = Nullable.GetUnderlyingType(type);
                }

                return (T)Convert.ChangeType(value, type, culture);
            }
            catch
            {
                if (throwExceptionOnError)
                {
                    throw;
                }
                else
                {
                    return default(T);
                }
            }
        }

        /// <summary>
        /// Converts to a given type or returns on NULL a default value.
        /// </summary>
        /// <typeparam name="T">the target type to convert</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="defaultValue">The default value, when NULL</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>
        /// the converted value
        /// </returns>
        public static T ConvertToOrDefault<T>(this object value, T defaultValue, CultureInfo culture = null)
        {
            try
            {
                T convertedValue = ConvertTo<T>(value, culture);

                if (convertedValue != null)
                {
                    return convertedValue;
                }
                else
                {
                    return defaultValue;
                }
            }
            catch
            {
                return defaultValue;
            }
        }

        /// <summary>
        /// Changes the type (with extended handling for booleans and empty strings).
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="tgtType">Type of the TGT.</param>
        /// <param name="culture">The culture.</param>
        /// <returns></returns>
        public static object ChangeTypeExtended(object value, Type tgtType, CultureInfo culture = null)
        {
            if (tgtType == null)
            {
                return value;
            }

            object tgtValue = null;

            if ((tgtType == typeof(bool) || tgtType == typeof(bool?)) && value is string)
            {
                // Special handling for Boolean
                string strValue = (value as string).Trim().ToLower();
                if (strValue == "1" || strValue == "true" || strValue == "yes" || strValue == "y")
                {
                    tgtValue = true;
                }
                else
                {
                    tgtValue = false;
                }
            }
            else if (tgtType != typeof(string) && string.IsNullOrWhiteSpace(value.ToStringOrEmpty()))
            {
                // special handling for whitespace strings, for non string types
                tgtValue = Activator.CreateInstance(tgtType);
            }
            else
            {
                tgtValue = ConvertExtensions.ChangeType(value, tgtType, culture);
            }

            return tgtValue;
        }

        /// <summary>
        /// Changes the type to the given target type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The target type.</param>
        /// <param name="culture">The culture information.</param>
        /// <returns>
        /// the converted value
        /// </returns>
        public static object ChangeType(object value, Type type, CultureInfo culture = null)
        {
            if (type == null)
            {
                return value;
            }

            if (IsNullable(type))
            {
                type = Nullable.GetUnderlyingType(type);
            }

            if (value == null && type.IsGenericType)
            {
                return Activator.CreateInstance(type);
            }

            if (value == null)
            {
                return null;
            }

            if (type == value.GetType())
            {
                return value;
            }

            if (type.IsEnum)
            {
                if (value is string)
                {
                    return Enum.Parse(type, value as string, true);
                }
                else
                {
                    return Enum.ToObject(type, value);
                }
            }

            if (!type.IsInterface && type.IsGenericType)
            {
                Type innerType = type.GetGenericArguments()[0];
                object innerValue = ChangeType(value, innerType, culture);
                return Activator.CreateInstance(type, new object[] { innerValue });
            }

            if (value is string && type == typeof(Guid))
            {
                return new Guid(value as string);
            }

            if (value is string && type == typeof(Version))
            {
                return new Version(value as string);
            }

            if (!(value is IConvertible))
            {
                return value;
            }

            return Convert.ChangeType(value, type, culture);
        }
    }
}