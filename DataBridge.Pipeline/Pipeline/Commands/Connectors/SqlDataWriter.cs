using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataConnectors.Adapter.DbAdapter;
using DataConnectors.Adapter.DbAdapter.ConnectionInfos;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "SqlDataWriter", Title = "SqlDataWriter", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class SqlDataWriter : DataCommand
    {
        private DataMappings dataMappings = new DataMappings();

        private DbAdapter dbAdapter = new DbAdapter();

        public DbConnectionInfoBase ConnectionInfo { get; set; }

        public SqlDataWriter()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
            this.Parameters.Add(new CommandParameter() { Name = "UseStoredProc", Direction = Directions.In, Value = false });

            this.dbAdapter.ConnectionInfo = this.ConnectionInfo;
        }

        public DataMappings DataMappings
        {
            get { return this.dataMappings; }
            set { this.dataMappings = value; }
        }

        [XmlElement]
        public string SqlTemplate
        {
            get;
            set;
        }

        [XmlIgnore]
        public bool UseStoredProc
        {
            get { return this.Parameters.GetValue<bool>("UseStoredProc"); }
            set { this.Parameters.SetOrAddValue("UseStoredProc", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                var data = inParameters.GetValue<object>("Data");
                var useStoredProc = inParameters.GetValue<bool>("UseStoredProc");

                if (data is DataTable)
                {
                    var table = data as DataTable;
                    this.WriteData(table, this.SqlTemplate, inParameters.ToDictionary(), useStoredProc);

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("Data", table);
                    outParameters.SetOrAddValue("DataName", table.TableName);
                    yield return outParameters;
                }
                else if (data != null)
                {
                    throw new NotSupportedException(
                        string.Format("Parameter 'Data' is not a 'DataTable' - Type '{0}' is not supported.",
                            data.GetType()));
                }
            }
        }

        private void WriteData(DataTable table, string sqlTemplate, IDictionary<string, object> parameters, bool useStoredProc)
        {
            this.dbAdapter.Connect();

            foreach (var row in table.Rows)
            {
                string sql = sqlTemplate;
                sql = this.ApplyDataMappings(sql, parameters);

                if (useStoredProc)
                {
                    var dbParameters = this.BuildDbParameters(parameters);
                    this.dbAdapter.RunStoredProc(sql, dbParameters);
                }
                else
                {
                    this.dbAdapter.ExecuteSql(sql);
                }
            }

            this.dbAdapter.Disconnect();
        }

        private string ApplyDataMappings(string sql, IDictionary<string, object> parameters)
        {
            // prepare the sql and set the values
            if (this.DataMappings.Any())
            {
                foreach (var dataMapping in this.DataMappings)
                {
                    if (dataMapping.Value != null)
                    {
                        // replace token in the datamapping value e.g. ["Filename"]="{Filename}" -> ["Filename"]="C:\Temp\Test.txt"
                        string value = TokenProcessor.ReplaceTokens(dataMapping.Value.ToStringOrEmpty(), parameters);
                        // Replace the tokens in the sql template e.g. "SELECT * FROM tb_test WHERE ID = {Id}"
                        sql = TokenProcessor.ReplaceToken(sql, dataMapping.Name, value);
                    }
                }
            }
            else
            {
                sql = TokenProcessor.ReplaceTokens(sql, parameters);
            }

            return sql;
        }

        private List<DbParameter> BuildDbParameters(IDictionary<string, object> parameters)
        {
            var dbParameters = new List<DbParameter>();
            foreach (var dataMapping in this.DataMappings)
            {
                var dbParameter = this.dbAdapter.DbProviderFactory.CreateParameter();
                dbParameter.ParameterName = dataMapping.Name;

                // replace token in the datamapping value e.g. ["Filename"]="{Filename}" -> ["Filename"]="C:\Temp\Test.txt"
                dbParameter.Value = TokenProcessor.ReplaceTokens(dataMapping.Value.ToStringOrEmpty(), parameters);

                dbParameter.DbType = dataMapping.DbType;

                switch (dataMapping.Direction)
                {
                    case Directions.In:
                        dbParameter.Direction = ParameterDirection.Input;
                        break;

                    case Directions.Out:
                        dbParameter.Direction = ParameterDirection.Output;
                        break;

                    case Directions.InOut:
                        dbParameter.Direction = ParameterDirection.InputOutput;
                        break;
                }
            }
            return dbParameters;
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