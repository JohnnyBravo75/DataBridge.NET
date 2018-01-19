using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataConnectors.Adapter.DbAdapter;
using DataConnectors.Adapter.DbAdapter.ConnectionInfos;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "SqlDataReader", Title = "SqlDataReader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class SqlDataReader : DataCommand
    {
        private DataMappings dataMappings = new DataMappings();

        private DbAdapter dbAdapter = new DbAdapter();

        public DbConnectionInfoBase ConnectionInfo { get; set; }

        public SqlDataReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
        }

        [XmlIgnore]
        protected int MaxRowsToRead
        {
            get { return this.UseStreaming ? this.StreamingBlockSize : -1; }
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

        public override void Initialize()
        {
            base.Initialize();

            this.dbAdapter.ConnectionInfo = this.ConnectionInfo;
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                this.dbAdapter.Connect();

                //inParameters = GetCurrentInParameters();
                string sql = this.SqlTemplate;

                sql = this.ApplyDataMappings(sql, inParameters.ToDictionary());

                this.LogDebugFormat("Start reading Sql='{0}'", sql);

                int rowIdx = 0;
                foreach (DataTable table in this.dbAdapter.ExecuteSql(sql, this.MaxRowsToRead))
                {
                    rowIdx += table.Rows.Count;

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("Data", table);
                    outParameters.SetOrAddValue("DataName", table.TableName);
                    yield return outParameters;
                }

                this.LogDebugFormat("End reading Sql='{0}': Rows={1}", sql, rowIdx);

                this.dbAdapter.Disconnect();
            }
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