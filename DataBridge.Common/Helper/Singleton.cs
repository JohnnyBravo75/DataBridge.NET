namespace DataBridge
{
    using System;
    using System.Reflection;

    public abstract class Singleton<T>
           where T : class
    {
        private static volatile T instance;
        private static readonly object lockObject = new object();

        static Singleton()
        {
        }

        public static T Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (lockObject)
                    {
                        if (instance == null)
                        {
                            instance = CreateInstance();
                        }
                    }
                }

                return instance;
            }
        }

        private static T CreateInstance()
        {
            ConstructorInfo constructor = null;

            try
            {
                // Binding flags exclude public constructors.
                constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
            }
            catch (Exception exception)
            {
                throw new ArgumentException("", exception);
            }

            if (constructor == null || constructor.IsAssembly)
            {
                constructor = typeof(T).GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new Type[0], null);

                //throw new ArgumentException(string.Format("A private or " + "protected constructor is missing for '{0}'.", typeof(T).Name));
            }

            return (T)constructor.Invoke(null);
        }
    }
}