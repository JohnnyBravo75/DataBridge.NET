namespace DataBridge.Extensions
{
    using System;

    public static class ConvertExtensions
    {
        /// <summary>
        /// Returns True if the type can get Null as a value (is a reference type and not a value one)
        /// </summary>
        private static bool IsNullable(Type type)
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
        /// <returns>the converted value</returns>
        public static T ConvertTo<T>(this object value, bool throwExceptionOnError = false)
        {
            try
            {
                if (value == null)
                {
                    return default(T);
                }

                Type t = typeof(T);

                return (T)ConvertExtensions.ChangeType(value, t);
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
        /// <returns>
        /// the converted value
        /// </returns>
        public static T ConvertToOrDefault<T>(this object value, T defaultValue)
        {
            try
            {
                T convertedValue = ConvertTo<T>(value);

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
        /// Changes the type to the given target type.
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="type">The target type.</param>
        /// <returns>the converted value</returns>
        public static object ChangeType(object value, Type type)
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
                object innerValue = ChangeType(value, innerType);
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

            return Convert.ChangeType(value, type, null);
        }
    }
}