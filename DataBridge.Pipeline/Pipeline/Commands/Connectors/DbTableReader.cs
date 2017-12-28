using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataBridge.ConnectionInfos;
using DataBridge.Extensions;
using DataBridge.Handler.Services.Adapter;
using DataBridge.Services;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "DbTableReader", Title = "DbTableReader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class DbTableReader : DataCommand
    {
        private DbAdapter dbAdapter = new DbAdapter();
        private DbConnectionInfoBase connectionInfo = new SqlServerConnectionInfo();

        [ExpandableObject]
        public DbConnectionInfoBase ConnectionInfo
        {
            get { return this.connectionInfo; }
            set { this.connectionInfo = value; }
        }

        public DbTableReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "TableName", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
        }

        [XmlIgnore]
        protected int MaxRowsToRead
        {
            get { return this.UseStreaming ? this.StreamingBlockSize : -1; }
        }

        [XmlIgnore]
        public string TableName
        {
            get { return this.Parameters.GetValue<string>("TableName"); }
            set { this.Parameters.SetOrAddValue("TableName", value); }
        }

        public override void Initialize()
        {
            base.Initialize();

            this.dbAdapter.ConnectionInfo = this.ConnectionInfo;
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string tableName = inParameters.GetValue<string>("TableName");

            this.dbAdapter.TableName = tableName;
            this.dbAdapter.Connect();

            foreach (DataTable table in this.dbAdapter.ReadData(this.MaxRowsToRead))
            {
                var outParameters = this.GetCurrentOutParameters();
                outParameters.SetOrAddValue("Data", table);
                outParameters.SetOrAddValue("DataName", table.TableName);
                yield return outParameters;
            }

            this.dbAdapter.Disconnect();
        }

        public override IList<string> Validate(CommandParameters parameters, ValidationContext context)
        {
            var messages = base.Validate(parameters, context);

            messages.AddRange(this.dbAdapter.Validate());

            return messages;
        }

        public override void Dispose()
        {
            if (this.dbAdapter != null)
            {
                this.dbAdapter.Dispose();
            }

            base.Dispose();
        }
    }
}