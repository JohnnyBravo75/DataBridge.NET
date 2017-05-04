using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.Handler.Services.Adapter;
using DataBridge.Helper;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "AccessWriter", Title = "AccessWriter", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class AccessWriter : DataCommand
    {
        private AccessAdapter accessAdapter = new AccessAdapter();

        public AccessWriter()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Table", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "DeleteBefore", Value = true, Direction = Directions.In });
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

        [XmlIgnore]
        public bool DeleteBefore
        {
            get { return this.Parameters.GetValue<bool>("DeleteBefore"); }
            set { this.Parameters.SetOrAddValue("DeleteBefore", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string tableName = inParameters.GetValue<string>("Table");
            object data = inParameters.GetValue<object>("Data");
            bool deleteBefore = this.Parameters.GetValue<bool>("DeleteBefore");

            DataTable table = null;
            if (data is DataTable)
            {
                table = data as DataTable;
            }
            else if (data is DataSet)
            {
                table = (data as DataSet).Tables[0];
            }

            this.WriteData(table, file, tableName, (this.IsFirstExecution && deleteBefore));

            var outParameters = this.GetCurrentOutParameters();
            outParameters.SetOrAddValue("File", file);
            yield return outParameters;
        }

        private void WriteData(DataTable table, string file, string tableName, bool deleteBefore = false)
        {
            if (string.IsNullOrEmpty(file))
            {
                file = table.TableName;
            }

            DirectoryUtil.CreateDirectoryIfNotExists(Path.GetDirectoryName(file));

            if (deleteBefore)
            {
                FileUtil.DeleteFileIfExists(file);
            }

            this.accessAdapter.FileName = file;
            this.accessAdapter.TableName = !string.IsNullOrEmpty(tableName) ? tableName : "Table1";

            if (this.IsFirstExecution && !System.IO.File.Exists(file))
            {
                this.accessAdapter.CreateNewFile();
            }

            if (this.accessAdapter.Connect())
            {
                this.accessAdapter.WriteData(table);
                this.accessAdapter.Disconnect();
            }
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