namespace DataBridge.Extensions
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public static class TypeExtensions
    {
        public static string GetNameAndAssembly(this Type type)
        {
            return type.FullName + ", " + type.Assembly.GetName().Name;
        }

        public static bool HasDefaultConstructor(this Type type)
        {
            if (type.IsValueType)
            {
                return true;
            }

            return (type.GetConstructor(BindingFlags.Public | BindingFlags.Instance, null, new Type[0], null) != null);
        }

        public static bool ImplementsGenericDefinition(this Type type, Type genericInterfaceDefinition)
        {
            Type implementingType;
            return ImplementsGenericDefinition(type, genericInterfaceDefinition, out implementingType);
        }

        public static bool ImplementsGenericDefinition(this Type type, Type genericInterfaceDefinition, out Type implementingType)
        {
            if (!genericInterfaceDefinition.IsInterface || !genericInterfaceDefinition.IsGenericTypeDefinition)
                throw new ArgumentNullException("genericInterfaceDefinition", "'" + genericInterfaceDefinition.ToString() + "' is not a generic interface definition.");

            if (type.IsInterface)
            {
                if (type.IsGenericType)
                {
                    Type interfaceDefinition = type.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = type;
                        return true;
                    }
                }
            }

            foreach (Type i in type.GetInterfaces())
            {
                if (i.IsGenericType)
                {
                    Type interfaceDefinition = i.GetGenericTypeDefinition();

                    if (genericInterfaceDefinition == interfaceDefinition)
                    {
                        implementingType = i;
                        return true;
                    }
                }
            }

            implementingType = null;
            return false;
        }

        public static bool InheritsGenericDefinition(this Type type, Type genericClassDefinition)
        {
            Type implementingType;
            return InheritsGenericDefinition(type, genericClassDefinition, out implementingType);
        }

        public static bool InheritsGenericDefinition(this Type type, Type genericClassDefinition, out Type implementingType)
        {
            if (!genericClassDefinition.IsClass || !genericClassDefinition.IsGenericTypeDefinition)
                throw new ArgumentNullException("genericClassDefinition", "'" + genericClassDefinition.ToString() + "' is not a generic class definition.");

            return InheritsGenericClassDefinitionInternal(type, genericClassDefinition, out implementingType);
        }

        public static bool IsInstantiatableType(this Type type)
        {
            if (type.IsAbstract || type.IsInterface || type.IsArray || type.IsGenericTypeDefinition || type == typeof(void))
                return false;

            if (!HasDefaultConstructor(type))
                return false;

            return true;
        }

        public static bool IsNullable(this Type type)
        {
            if (type.IsValueType)
            {
                return IsNullableType(type);
            }

            return true;
        }

        public static bool IsNullableType(this Type type)
        {
            return (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>));
        }

        public static bool IsType<T>(this Type type)
        {
            return type.IsType(typeof(T));
        }

        public static bool IsType(this Type type, Type test)
        {
            return test.IsAssignableFrom(type);
        }

        public static object New(this Type type, params object[] args)
        {
            return Activator.CreateInstance(type, args);
        }

        public static T New<T>(this Type type, params object[] args)
        {
            return (T)New(type, args);
        }

        private static bool InheritsGenericClassDefinitionInternal(Type currentType, Type genericClassDefinition, out Type implementingType)
        {
            if (currentType.IsGenericType)
            {
                Type currentGenericClassDefinition = currentType.GetGenericTypeDefinition();

                if (genericClassDefinition == currentGenericClassDefinition)
                {
                    implementingType = currentType;
                    return true;
                }
            }

            if (currentType.BaseType == null)
            {
                implementingType = null;
                return false;
            }

            return InheritsGenericClassDefinitionInternal(currentType.BaseType, genericClassDefinition, out implementingType);
        }

        //public static Type GetElementType(this Type type)
        //{
        //    Type enumerableType = type.GetInterfaces().FirstOrDefault(x => x.IsGenericEnumerable());

        //    if (enumerableType != null)
        //    {
        //        Type[] genericArguments = enumerableType.GetGenericArguments();
        //        return genericArguments[0];
        //    }

        //    // return 'object' for a non-generic IEnumerable
        //    if (typeof(IEnumerable).IsAssignableFrom(type))
        //    {
        //        return typeof(object);
        //    }

        //    return type;
        //}

        public static bool IsGenericEnumerable(this Type type)
        {
            return type.IsGenericType &&
                   type.GetGenericTypeDefinition() == typeof(IEnumerable<>);
        }

        public static bool IsEnumberable(this Type type)
        {
            // string is enumerable (array of char), but this is not what we want
            if (type == typeof(string))
            {
                return false;
            }

            if (IsGenericEnumerable(type))
            {
                return true;
            }

            return type.GetInterfaces().Any(i => i == typeof(IEnumerable));
        }

        public static object GetDefaultValue(this Type t)
        {
            if (t.IsValueType && Nullable.GetUnderlyingType(t) == null)
                return Activator.CreateInstance(t);
            else
                return null;
        }

    }
}