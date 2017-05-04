namespace DataBridge.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;

    public class GenericFactory
    {
        // ***********************Fields***********************

        private static List<Type> typeCache = new List<Type>();

        // ***********************Functions***********************

        /// <summary>
        /// Create an instance of an object by a type name and a given interface
        /// </summary>
        /// <typeparam name="T">the interface type</typeparam>
        /// <param name="assembly">the assembly in which the type is</param>
        /// <param name="typeName">type name</param>
        /// <param name="parameters">parameters</param>
        /// <returns>the object</returns>
        public static T GetInstance<T>(Assembly assembly, string typeName, params object[] parameters)
        {
            return GetInstance<T>(assembly, typeName, true, null, parameters);
        }

        public static T TryGetInstance<T>(Assembly assembly, string typeName, params object[] parameters)
        {
            return GetInstance<T>(assembly, typeName, false, null, parameters);
        }

        public static T GetInstance<T>(Assembly assembly, string typeName, Type[] genericTypes, params object[] parameters)
        {
            return GetInstance<T>(assembly, typeName, true, genericTypes, parameters);
        }

        public static T TryGetInstance<T>(Assembly assembly, string typeName, Type[] genericTypes, params object[] parameters)
        {
            return GetInstance<T>(assembly, typeName, false, genericTypes, parameters);
        }

        /// <summary>
        /// Create an instance of an object by a type name and a given interface
        /// </summary>
        /// <typeparam name="T">the interface type</typeparam>
        /// <param name="assembly">the assembly in which the type is</param>
        /// <param name="typeName">type name</param>
        /// <param name="thowOnError">if set to <c>true</c> [thow on error].</param>
        /// <param name="genericTypes">The generic types.</param>
        /// <param name="parameters">parameters</param>
        /// <returns>
        /// the created instance object
        /// </returns>
        /// <exception cref="System.ArgumentNullException">
        /// assembly
        /// or
        /// type
        /// </exception>
        /// <exception cref="System.ArgumentException"></exception>
        private static T GetInstance<T>(Assembly assembly, string typeName, bool thowOnError, Type[] genericTypes, params object[] parameters)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            // look for a normal type
            Type type = typeCache.FirstOrDefault(d => d.Name == typeName && d.IsGenericType == false);

            if (type == null)
            {
                var types = assembly.GetTypes();
                type = types.FirstOrDefault(d => d.Name == typeName && d.IsGenericType == false);

                if (type != null)
                {
                    // when the type was found, add all types from the assembly to the cache
                    typeCache.AddRange(types);
                }
            }

            if (type == null)
            {
                // look for a generic type
                type = typeCache.FirstOrDefault(d => d.Name == typeName + "`1" && d.IsGenericType);

                if (type == null)
                {
                    type = assembly.GetTypes().FirstOrDefault(d => d.Name == typeName + "`1" && d.IsGenericType);
                }

                if (type != null)
                {
                    type = type.MakeGenericType(genericTypes);
                }
            }

            if (type == null)
            {
                if (thowOnError)
                {
                    throw new ArgumentNullException("type", string.Format("The type '{0}' implementing '{1}' was not found in the assembly '{2}'", typeName, typeof(T).ToString(), assembly.ToString()));
                }
                else
                {
                    return default(T);
                }
            }

            try
            {
                return (T)Activator.CreateInstance(type, parameters);
            }
            catch (Exception ex)
            {
                if (thowOnError)
                {
                    throw new ArgumentException(string.Format("Could not create type '{0}'", typeName), ex);
                }
                else
                {
                    return default(T);
                }
            }
        }

        /// <summary>
        /// Create an instance of an object by a type name (looks first for the fullname, otherwise for the shortname) and a given interface
        /// </summary>
        /// <typeparam name="T">the interface type</typeparam>
        /// <param name="typeName">type name</param>
        /// <param name="parameters">parameters</param>
        /// <returns>the object</returns>
        public static T GetInstance<T>(string typeName, params object[] parameters)
        {
            // look in the cache (for performance)
            Type type = typeCache.FirstOrDefault(d => d.Name == typeName);

            // when not found
            if (type == null)
            {
                // loop the assembies an search for the type
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assemblies)
                {
                    var types = assembly.GetTypes();
                    type = types.FirstOrDefault(d => d.Name == typeName);

                    if (type != null)
                    {
                        // when the type was found, add all types from the assembly to the cache
                        typeCache.AddRange(types);
                        break;
                    }
                }
            }

            if (type == null)
            {
                throw new Exception("The type '" + typeName + "' implementing '" + typeof(T).ToString() + "' was not found in the loaded assemblies.");
            }

            return (T)GetInstance<T>(type.Assembly, typeName, parameters);
        }
    }
}