using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataBridge.Handler.Services.Adapter;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "AccessReader", Title = "AccessReader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class AccessReader : DataCommand
    {
        private AccessAdapter accessAdapter = new AccessAdapter();

        public AccessReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Table", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
        }

        [XmlIgnore]
        protected int MaxRowsToRead
        {
            get { return this.UseStreaming ? this.StreamingBlockSize : -1; }
        }

        [XmlIgnore]
        [System.ComponentModel.Editor(typeof(Wpf.Toolkit.PropertyEditors.OpenFilePropertyEditor), typeof(Wpf.Toolkit.PropertyEditors.OpenFilePropertyEditor))]
        public string File
        {
            get { return this.Parameters.GetValue<string>("File"); }
            set { this.Parameters.SetOrAddValue("File", value); }
        }

        [XmlIgnore]
        public string Table
        {
            get { return this.Parameters.GetValue<string>("Table"); }
            set { this.Parameters.SetOrAddValue("Table", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string tableName = inParameters.GetValue<string>("Table");

            this.accessAdapter.FileName = file;
            this.accessAdapter.TableName = tableName;
            this.accessAdapter.Connect();

            this.LogDebugFormat("Start reading File='{0}'", file);

            int rowIdx = 0;
            foreach (DataTable table in this.accessAdapter.ReadData(this.MaxRowsToRead))
            {
                rowIdx += table.Rows.Count;

                var outParameters = this.GetCurrentOutParameters();
                outParameters.SetOrAddValue("Data", table);
                outParameters.SetOrAddValue("DataName", table.TableName);
                yield return outParameters;
            }

            this.LogDebugFormat("End reading File='{0}': Rows={1}", file, rowIdx);
            this.accessAdapter.Disconnect();
        }

        public override void Dispose()
        {
            if (this.accessAdapter != null)
            {
                this.accessAdapter.Dispose();
            }

            base.Dispose();
        }
    }
}