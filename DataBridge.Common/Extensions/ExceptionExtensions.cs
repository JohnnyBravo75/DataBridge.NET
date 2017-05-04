using System;
using System.Text;

namespace DataBridge.Extensions
{
    public static class ExceptionExtensions
    {
        public static string GetAllMessages(this Exception ex)
        {
            var message = new StringBuilder();
            while (ex != null)
            {
                if (message.Length > 0)
                {
                    message.AppendFormat("{0}", Environment.NewLine);
                }
                message.Append(ex.Message);
                ex = ex.InnerException;
            }

            return message.ToString();
        }

        public static string GetAllStackTraces(this Exception ex)
        {
            var message = new StringBuilder();
            while (ex != null)
            {
                message = new StringBuilder(
                    string.Format("Stack trace for {0} in module {1}, {2} ({3}):{6}{4}{6}{6}{5}",
                    ex.GetType().Name, ex.Source, ex.TargetSite.Name, ex.Message, ex.StackTrace,
                    message.ToString(), Environment.NewLine));
                ex = ex.InnerException;
            }

            return message.ToString();
        }
    }
}