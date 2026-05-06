namespace DataBridge.GUI.Model
{
    using System;

    public enum DebugLogLevel
    {
        Debug,
        Info,
        Warn,
        Error,
        Fatal
    }

    public class DebugLogEntry
    {
        public DateTime Timestamp { get; set; }

        public DebugLogLevel Level { get; set; }

        public string Source { get; set; }

        public string Message { get; set; }

        public string ExceptionText { get; set; }
    }
}
