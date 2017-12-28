using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.Handler.Services.Adapter;
using DataBridge.Helper;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "Excel2007Writer", Title = "Excel2007Writer", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class Excel2007Writer : DataCommand
    {
        private Excel2007NativeAdapter excelAdapter = new Excel2007NativeAdapter();

        public Excel2007Writer()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Sheet", Direction = Directions.In, NotNull = true });
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
        public string Sheet
        {
            get { return this.Parameters.GetValue<string>("Sheet"); }
            set { this.Parameters.SetOrAddValue("Sheet", value); }
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
            string sheet = inParameters.GetValue<string>("Sheet");
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

            this.WriteData(table, file, sheet, (this.IsFirstExecution && deleteBefore));

            var outParameters = this.GetCurrentOutParameters();
            outParameters.SetOrAddValue("File", file);
            yield return outParameters;
        }

        private void WriteData(DataTable table, string file, string sheet, bool deleteBefore = false)
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

            this.excelAdapter.FileName = file;
            this.excelAdapter.SheetName = !string.IsNullOrEmpty(sheet) ? sheet : "Sheet1";

            if (this.IsFirstExecution && !System.IO.File.Exists(file))
            {
                this.excelAdapter.CreateNewFile();
            }

            if (this.excelAdapter.Connect())
            {
                this.excelAdapter.WriteAllData(table);
                this.excelAdapter.Disconnect();
            }
        }

        public override void Dispose()
        {
            if (this.excelAdapter != null)
            {
                this.excelAdapter.Dispose();
            }

            base.Dispose();
        }
    }
}