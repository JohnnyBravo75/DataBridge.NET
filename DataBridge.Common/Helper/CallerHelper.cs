using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace DataBridge.Helper
{
    public static class CallerHelper
    {
        public static string GetCallerName([CallerMemberName] string memberName = "")
        {
            return memberName;
        }

        public static MethodBase GetCallingMethod()
        {
            return new StackFrame(2, false).GetMethod();
        }

        public static Type GetCallingType()
        {
            return new StackFrame(2, false).GetMethod().DeclaringType;
        }
    }
}