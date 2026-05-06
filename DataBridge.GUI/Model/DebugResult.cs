namespace DataBridge.GUI.Model
{
    using System;
    using System.Collections.Generic;

    public class DebugResult
    {
        public bool Success { get; set; }

        public bool WasCanceled { get; set; }

        public int? ExitCode { get; set; }

        public TimeSpan Duration { get; set; }

        public string SummaryMessage { get; set; }


        public IList<DebugLogEntry> Logs { get; set; }

        public static DebugResult CreateDefault()
        {
            return new DebugResult
            {
                Logs = new List<DebugLogEntry>()
            };
        }
    }
}
