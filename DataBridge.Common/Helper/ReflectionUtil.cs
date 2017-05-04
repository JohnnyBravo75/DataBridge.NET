using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace DataBridge.Helper
{
    public static class ReflectionUtil
    {
        public static object GetValueByPath(object obj, string path)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            if (path == null) throw new ArgumentNullException("path");

            Type currentType = obj.GetType();

            foreach (string propertyName in path.Split('.'))
            {
                if (currentType != null)
                {
                    PropertyInfo property = null;
                    int brackStart = propertyName.IndexOf("[");
                    int brackEnd = propertyName.IndexOf("]");

                    property = currentType.GetProperty(brackStart > 0 ? propertyName.Substring(0, brackStart) : propertyName);
                    obj = property.GetValue(obj, null);

                    if (brackStart > 0)
                    {
                        string index = propertyName.Substring(brackStart + 1, brackEnd - brackStart - 1);
                        foreach (Type iType in obj.GetType().GetInterfaces())
                        {
                            if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IDictionary<,>))
                            {
                                obj = typeof(ReflectionUtil).GetMethod("GetDictionaryElement")
                                                             .MakeGenericMethod(iType.GetGenericArguments())
                                                             .Invoke(null, new object[] { obj, index });
                                break;
                            }
                            if (iType.IsGenericType && iType.GetGenericTypeDefinition() == typeof(IList<>))
                            {
                                obj = typeof(ReflectionUtil).GetMethod("GetListElement")
                                                             .MakeGenericMethod(iType.GetGenericArguments())
                                                             .Invoke(null, new object[] { obj, index });
                                break;
                            }
                        }
                    }

                    currentType = obj != null ? obj.GetType() : null; //property.PropertyType;
                }
                else return null;
            }
            return obj;
        }

        public static TValue GetDictionaryElement<TKey, TValue>(IDictionary<TKey, TValue> dict, object index)
        {
            TKey key = (TKey)Convert.ChangeType(index, typeof(TKey), null);
            return dict[key];
        }

        public static T GetListElement<T>(IList<T> list, object index)
        {
            return list[Convert.ToInt32(index)];
        }

        public static object GetPropertyValue(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return null;
            }

            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                return null;
            }

            return propertyInfo.GetValue(obj, null);
        }

        public static T GetPropertyValue<T>(object obj, string propertyName)
        {
            if (obj == null || string.IsNullOrEmpty(propertyName))
            {
                return default(T);
            }

            PropertyInfo propertyInfo = obj.GetType().GetProperty(propertyName);
            if (propertyInfo == null)
            {
                return default(T);
            }

            return (T)propertyInfo.GetValue(obj, null);
        }

        [DebuggerStepThrough]
        public static bool HasProperty(object obj, string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(obj)[propertyName] == null)
            {
                return false;
            }

            return true;
        }

        public static object GetProperties(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return obj.GetType().GetProperties();
        }
    }
}