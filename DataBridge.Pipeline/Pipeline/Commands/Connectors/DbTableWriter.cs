using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataBridge.ConnectionInfos;
using DataBridge.Extensions;
using DataBridge.Handler.Services.Adapter;
using DataBridge.Services;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "DbTableWriter", Title = "DbTableWriter", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class DbTableWriter : DataCommand
    {
        private DbAdapter dbAdapter = new DbAdapter();

        private DbConnectionInfoBase connectionInfo = new SqlServerConnectionInfo();

        public DbConnectionInfoBase ConnectionInfo
        {
            get { return this.connectionInfo; }
            set { this.connectionInfo = value; }
        }

        public DbTableWriter()
        {
            this.Parameters.Add(new CommandParameter() { Name = "TableName" });
            this.Parameters.Add(new CommandParameter() { Name = "DeleteBefore", Value = true });
        }

        [XmlIgnore]
        public string TableName
        {
            get { return this.Parameters.GetValue<string>("TableName"); }
            set { this.Parameters.SetOrAddValue("TableName", value); }
        }

        [XmlIgnore]
        public bool DeleteBefore
        {
            get { return this.Parameters.GetValue<bool>("DeleteBefore"); }
            set { this.Parameters.SetOrAddValue("DeleteBefore", value); }
        }

        public override void Initialize()
        {
            base.Initialize();

            if (this.dbAdapter.ConnectionInfo == null && this.ConnectionInfo != null)
            {
                this.dbAdapter.ConnectionInfo = this.ConnectionInfo;
            }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string tableName = inParameters.GetValue<string>("TableName");
            if (string.IsNullOrEmpty(tableName))
            {
                tableName = inParameters.GetValue<string>("DataName");
            }
            object data = inParameters.GetValue<object>("Data");
            bool deleteBefore = this.Parameters.GetValue<bool>("DeleteBefore");

            if (data is DataTable)
            {
                data = this.ConvertToDataSet(data);
            }

            if (data is DataSet)
            {
                foreach (DataTable table in (data as DataSet).Tables)
                {
                    this.WriteData(table, tableName, (this.IsFirstExecution && deleteBefore));

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("Data", table);
                    outParameters.SetOrAddValue("DataName", table.TableName);
                    yield return outParameters;
                }
            }
        }

        private void WriteData(DataTable table, string tableName = null, bool deleteBefore = false)
        {
            this.dbAdapter.TableName = (!string.IsNullOrEmpty(tableName)
                                                ? tableName
                                                : table.TableName);

            this.dbAdapter.Connect();

            if (deleteBefore)
            {
                this.LogDebugFormat("Delete all data from table '{0}'", this.dbAdapter.TableName);
                // this.dbAdapter.DeleteData();
                this.dbAdapter.DropTable();
            }

            this.LogDebugFormat("Write to table '{0}': Rows={1}", this.dbAdapter.TableName, table.Rows.Count);
            this.dbAdapter.WriteAllData(table);

            this.dbAdapter.Disconnect();
        }

        private object ConvertToDataSet(object data)
        {
            var dataSet = new DataSet();
            var table = data as DataTable;
            dataSet.Tables.Add(table);
            data = dataSet;
            return data;
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