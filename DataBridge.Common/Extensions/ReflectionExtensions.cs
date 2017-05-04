namespace DataBridge.Extensions
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;

    public static class ReflectionExtensions
    {
        public static object GetPropertyValue(this object obj, string propertyName)
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

        public static T GetPropertyValue<T>(this object obj, string propertyName)
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
        public static bool HasProperty(this object obj, string propertyName)
        {
            // Verify that the property name matches a real,
            // public, instance property on this object.
            if (TypeDescriptor.GetProperties(obj)[propertyName] == null)
            {
                return false;
            }

            return true;
        }

        public static object GetProperties(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            return obj.GetType().GetProperties();
        }
    }
}