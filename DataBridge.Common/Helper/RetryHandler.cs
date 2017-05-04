using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DataBridge.Helper
{
    public static class RetryHandler
    {
        /// <summary>
        /// Executes the specified action.
        /// </summary>
        /// <example>
        ///     RetryHandler.Execute(() => SomeFunctionThatCanFail(), TimeSpan.FromSeconds(1));
        ///     RetryHandler.Execute(SomeFunctionThatCanFail, TimeSpan.FromSeconds(1));
        ///     int result = RetryHandler.Execute(SomeFunctionWhichReturnsInt, TimeSpan.FromSeconds(1), 4);
        /// </example>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        public static object Execute(Action action, TimeSpan retryInterval, int retryCount = 3, Action<int> eachRetryAction = null)
        {
            return Execute<object>(() =>
            {
                action();
                return null;
            }, retryInterval, retryCount, eachRetryAction);
        }

        /// <summary>
        /// Executes the specified action.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="action">The action.</param>
        /// <param name="retryInterval">The retry interval.</param>
        /// <param name="retryCount">The retry count.</param>
        /// <param name="eachRetryAction">The each retry action.</param>
        /// <returns></returns>
        /// <exception cref="AggregateException"></exception>
        /// <example>
        /// RetryHandler.Execute(() =&gt; SomeFunctionThatCanFail(), TimeSpan.FromSeconds(1));
        /// RetryHandler.Execute(SomeFunctionThatCanFail, TimeSpan.FromSeconds(1));
        /// int result = RetryHandler.Execute(SomeFunctionWhichReturnsInt, TimeSpan.FromSeconds(1), 4);
        /// </example>
        public static T Execute<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3, Action<int> eachRetryAction = null)
        {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++)
            {
                try
                {
                    if (retry > 0)
                    {
                        if (eachRetryAction != null)
                        {
                            eachRetryAction(retry);
                        }

                        Thread.Sleep(retryInterval);
                    }

                    T result = action();

                    return result;
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}