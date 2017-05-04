using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using DataBridge.Utils;

namespace DataBridge.Extensions
{
    using System;
    using System.IO;
    using System.Xml.Serialization;

    public static class ObjectExtensions
    {
        private static Dictionary<Type, XmlSerializer> serializers = new Dictionary<Type, XmlSerializer>();

        /// <summary>
        /// Clones an object with the XmlSerializer
        /// </summary>
        /// <typeparam name="T">the object type</typeparam>
        /// <param name="obj">the object</param>
        /// <returns>the clone</returns>
        public static T Clone<T>(this T obj)
        {
            T result = default(T);
            using (Stream memStream = new MemoryStream())
            {
                XmlSerializer serializer = null;
                serializers.TryGetValue(typeof(T), out serializer);
                if (serializer == null)
                {
                    Type[] knownTypes = KnownTypesProvider.GetKnownTypes(null, excludeNameSpacePrefixes: new string[] { }).ToArray();
                    serializer = new XmlSerializer(typeof(T), knownTypes);
                    serializers.Add(typeof(T), serializer);
                }
                serializer.Serialize(memStream, obj);
                memStream.Seek(0, SeekOrigin.Begin);
                result = (T)serializer.Deserialize(memStream);
            }

            return result;
        }

        public static T To<T>(this object value, bool throwOnError = false) where T : IConvertible
        {
            var type = typeof(T);

            // If the type is nullable and the result should be null, set a null value.
            if (type.IsNullable() && (value == null || value == DBNull.Value))
            {
                return default(T);
            }

            // Convert.ChangeType fails on Nullable<T> types.  We want to try to cast to the underlying type anyway.
            var underlyingType = Nullable.GetUnderlyingType(type) ?? type;

            try
            {
                return (T)Convert.ChangeType(value, underlyingType);
            }
            catch (Exception ex)
            {
                if (!throwOnError)
                {
                    return default(T);
                }

                throw;
            }
        }

        //public static bool IsNullOrEmpty(this ICollection obj)
        //{
        //    return (obj == null || obj.Count == 0);
        //}

        //public static bool IsNullOrEmpty(this object obj)
        //{
        //    if (obj == null)
        //    {
        //        return true;
        //    }
        //    if (obj is ICollection)
        //    {
        //        if ((obj as ICollection).Count == 0)
        //        {
        //            return true;
        //        }
        //    }

        //    return false;
        //}
    }

    public static class FormattableObject
    {
        /// <summary>
        /// A formatted ToString
        ///
        /// Example:
        /// p.ToString("{Money:C} {LastName}, {ScottName} {BirthDate}");
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="formatStr"></param>
        /// <returns></returns>
        public static string ToString(this object obj, string formatStr)
        {
            return ToString(obj, formatStr, null);
        }

        /// <summary>
        /// A formatted ToString
        ///
        /// Example:
        /// p.ToString("{Money:C} {LastName}, {ScottName} {BirthDate}",new System.Globalization.CultureInfo("zh-hk"));
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="formatStr"></param>
        /// <param name="formatProvider"></param>
        /// <returns></returns>
        public static string ToString(this object obj, string formatStr, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(formatStr))
            {
                return obj.ToString();
            }

            StringBuilder sb = new StringBuilder();
            Type type = obj.GetType();
            Regex reg = new Regex(@"({)([^}]+)(})", RegexOptions.IgnoreCase);
            MatchCollection mc = reg.Matches(formatStr);

            int startIndex = 0;
            foreach (Match match in mc)
            {
                Group group = match.Groups[2]; //it's second in the match between { and }
                int length = group.Index - startIndex - 1;
                sb.Append(formatStr.Substring(startIndex, length));

                string toGet = String.Empty;
                string toFormat = String.Empty;
                int formatIndex = group.Value.IndexOf(":"); //formatting would be to the right of a :
                if (formatIndex == -1) //no formatting, no worries
                {
                    toGet = group.Value;
                }
                else //pickup the formatting
                {
                    toGet = group.Value.Substring(0, formatIndex);
                    toFormat = group.Value.Substring(formatIndex + 1);
                }

                //first try properties
                PropertyInfo retrievedProperty = type.GetProperty(toGet);
                Type retrievedType = null;
                object retrievedObject = null;
                if (retrievedProperty != null)
                {
                    retrievedType = retrievedProperty.PropertyType;
                    retrievedObject = retrievedProperty.GetValue(obj, null);
                }
                else //try fields
                {
                    FieldInfo retrievedField = type.GetField(toGet);
                    if (retrievedField != null)
                    {
                        retrievedType = retrievedField.FieldType;
                        retrievedObject = retrievedField.GetValue(obj);
                    }
                }

                if (retrievedType != null) //Cool, we found something
                {
                    string result = String.Empty;
                    if (toFormat == String.Empty) //no format info
                    {
                        result = retrievedType.InvokeMember("ToString",
                          BindingFlags.Public | BindingFlags.NonPublic |
                          BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                          , null, retrievedObject, null) as string;

                        //result = retrievedObject.ToString();
                    }
                    else //format info
                    {
                        result = retrievedType.InvokeMember("ToString",
                          BindingFlags.Public | BindingFlags.NonPublic |
                          BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.IgnoreCase
                          , null, retrievedObject, new object[] { toFormat, formatProvider }) as string;

                        //result = retrievedObject.ToString(toFormat, formatProvider);
                    }
                    if (result != null)
                    {
                        sb.Append(result);
                    }
                }
                else //didn't find a property with that name, so be gracious and put it back
                {
                    sb.Append("{");
                    sb.Append(group.Value);
                    sb.Append("}");
                }
                startIndex = group.Index + group.Length + 1;
            }
            if (startIndex < formatStr.Length) //include the rest (end) of the string
            {
                sb.Append(formatStr.Substring(startIndex));
            }
            return sb.ToString();
        }
    }
}