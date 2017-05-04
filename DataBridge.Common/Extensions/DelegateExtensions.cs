using System;
using DataBridge.Helper;

namespace DataBridge.Extensions
{
    public static class DelegateExtensions
    {
        public static void Raise(this EventHandler handler, object sender, EventArgs args)
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        public static void Raise<T>(this EventHandler handler, object sender, EventArgs<T> args)
        {
            if (handler != null)
            {
                handler(sender, args);
            }
        }

        public static void InvokeSafe<T>(this Action<T> action, T param)
        {
            if (action != null)
            {
                action(param);
            }
        }

        public static void InvokeSafe<T1, T2>(this Action<T1, T2> action, T1 param1, T2 param2)
        {
            if (action != null)
            {
                action(param1, param2);
            }
        }

        public static void InvokeSafe<T1, T2, T3>(this Action<T1, T2, T3> action, T1 param1, T2 param2, T3 param3)
        {
            if (action != null)
            {
                action(param1, param2, param3);
            }
        }
    }
}