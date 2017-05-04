using System.Diagnostics;

namespace DataBridge
{
    public static class EventLogHelper
    {
        public static bool WriteEntry(string source, string message, EventLogEntryType entryType = EventLogEntryType.Information)
        {
            bool succesfull = true;
            try
            {
                if (!EventLog.SourceExists(source))
                {
                    EventLog.CreateEventSource(source, "Application");
                }

                EventLog.WriteEntry(source, message, entryType);
            }
            catch
            {
                succesfull = false;
            }
            return succesfull;
        }
    }
}