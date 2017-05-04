using System;
using System.Collections.Generic;
using log4net.Appender;
using log4net.Core;

namespace DataBridge.Runtime
{
    public class RemoteLogListener : MarshalByRefObject, RemotingAppender.IRemoteLoggingSink
    {
        public class LoggingEventArgs : EventArgs
        {
            public IEnumerable<LoggingEvent> LoggingEvents { get; set; }
        }

        public EventHandler<LoggingEventArgs> MessageReceived { get; set; }

        public void LogEvents(LoggingEvent[] events)
        {
            var logMessageReceived = this.MessageReceived;
            if (logMessageReceived != null)
            {
                logMessageReceived.Invoke(this, new LoggingEventArgs { LoggingEvents = events });
            }
        }
    }
}