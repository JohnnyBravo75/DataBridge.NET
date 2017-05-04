using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using DataBridge.Extensions;

namespace DataBridge.Formatters
{
    public class EventLogToDataTableFormatter : FormatterBase
    {
        public override object Format(object data, object existingData = null)
        {
            IList<EventLogEntry> eventLogs = null;

            var table = existingData as DataTable;

            if (data is EventLogEntry)
            {
                eventLogs = new List<EventLogEntry>();
                eventLogs.Add(data as EventLogEntry);
            }
            else if (data is IEnumerable<EventLogEntry>)
            {
                eventLogs = data as IList<EventLogEntry>;
            }

            if (eventLogs != null)
            {
                if (table == null)
                {
                    table = new DataTable();
                }

                table.Columns.AddWhenNotExist("Category", typeof(string));
                table.Columns.AddWhenNotExist("Data", typeof(byte[]));
                table.Columns.AddWhenNotExist("EntryType", typeof(EventLogEntryType));
                table.Columns.AddWhenNotExist("InstanceId", typeof(long));
                table.Columns.AddWhenNotExist("MachineName", typeof(string));
                table.Columns.AddWhenNotExist("Message", typeof(string));
                table.Columns.AddWhenNotExist("Source", typeof(string));
                table.Columns.AddWhenNotExist("TimeWritten", typeof(DateTime));
                table.Columns.AddWhenNotExist("UserName", typeof(string));

                table = this.FormatToDataTable(eventLogs, table);
            }

            return table;
        }

        private DataTable FormatToDataTable(IList<EventLogEntry> eventLogs, DataTable table)
        {
            foreach (var eventLog in eventLogs)
            {
                var dataRow = table.NewRow();
                dataRow["Category"] = eventLog.Category;
                dataRow["Data"] = eventLog.Data;
                dataRow["EntryType"] = eventLog.EntryType;
                dataRow["InstanceId"] = eventLog.InstanceId;
                dataRow["MachineName"] = eventLog.MachineName;
                dataRow["Message"] = eventLog.Message;
                dataRow["Source"] = eventLog.Source;
                dataRow["TimeWritten"] = eventLog.TimeWritten;
                dataRow["UserName"] = eventLog.UserName;
                table.Rows.Add(dataRow);
            }

            return table;
        }
    }
}