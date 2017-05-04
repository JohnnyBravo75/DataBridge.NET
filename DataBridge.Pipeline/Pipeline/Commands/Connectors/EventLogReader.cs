using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Formatters;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "EventLogReader", Title = "EventLogReader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class EventLogReader : DataCommand
    {
        private FormatterBase formatter = new EventLogToDataTableFormatter();

        public EventLogReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "LogGroup", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Source", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "MachineName", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "EntryType", Direction = Directions.In, Value = EventLogEntryType.Error });
            this.Parameters.Add(new CommandParameter() { Name = "MinTimeGenerated", Direction = Directions.In, DataType = DataTypes.DateTime });
        }

        [XmlIgnore]
        public string LogGroup
        {
            get { return this.Parameters.GetValue<string>("LogGroup"); }
            set { this.Parameters.SetOrAddValue("LogGroup", value); }
        }

        [XmlIgnore]
        public string Source
        {
            get { return this.Parameters.GetValue<string>("Source"); }
            set { this.Parameters.SetOrAddValue("Source", value); }
        }

        [XmlIgnore]
        public string MachineName
        {
            get { return this.Parameters.GetValue<string>("MachineName"); }
            set { this.Parameters.SetOrAddValue("MachineName", value); }
        }

        [XmlIgnore]
        public EventLogEntryType EntryType
        {
            get { return this.Parameters.GetValue<EventLogEntryType>("EntryType"); }
            set { this.Parameters.SetOrAddValue("EntryType", value); }
        }

        [XmlIgnore]
        public DateTime? MinTimeGenerated
        {
            get { return this.Parameters.GetValue<DateTime?>("MinTimeGenerated"); }
            set { this.Parameters.SetOrAddValue("MinTimeGenerated", value); }
        }

        public FormatterBase Formatter
        {
            get { return this.formatter; }
            set { this.formatter = value; }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string logGroup = inParameters.GetValue<string>("LogGroup");
            string source = inParameters.GetValue<string>("Source");
            string machineName = inParameters.GetValue<string>("MachineName");
            EventLogEntryType entryType = inParameters.GetValue<EventLogEntryType>("EntryType");
            DateTime? minTimeGenerated = inParameters.GetValue<DateTime?>("MinTimeGenerated");

            var eventLog = new EventLog()
            {
                Log = logGroup,
                Source = source,
                MachineName = machineName
            };

            this.LogDebugFormat("Start reading events from Eventlog='{0}', Source='{1}'", logGroup, source);

            var eventLogs = eventLog.Entries.Cast<EventLogEntry>()
                                        .Where(x => (x.Source == source || string.IsNullOrEmpty(source)) &&
                                                    x.EntryType == entryType &&
                                                    ((minTimeGenerated.HasValue && x.TimeGenerated >= minTimeGenerated.Value) || !minTimeGenerated.HasValue)
                                              );

            var table = new DataTable();

            table = this.formatter.Format(eventLogs.ToList(), table) as DataTable;

            var outParameters = this.GetCurrentOutParameters();
            outParameters.SetOrAddValue("Data", table);
            outParameters.SetOrAddValue("DataName", table.TableName);
            yield return outParameters;

            this.LogDebugFormat("End reading events from Eventlog='{0}', Source='{1}': Readed={2}", logGroup, source, table.Rows.Count);
        }
    }
}