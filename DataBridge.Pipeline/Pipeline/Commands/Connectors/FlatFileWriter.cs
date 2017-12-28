using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataBridge.Formatters;
using DataBridge.Handler.Services.Adapter;
using DataBridge.Helper;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "FlatFileWriter", Title = "FlatFileWriter", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class FlatFileWriter : DataCommand
    {
        private FlatFileAdapter fileAdapter = new FlatFileAdapter();

        public FlatFileWriter()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "EncodingName", Direction = Directions.In });
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
        public string EncodingName
        {
            get { return this.Parameters.GetValue<string>("EncodingName"); }
            set { this.Parameters.SetOrAddValue("EncodingName", value); }
        }

        [XmlIgnore]
        public bool DeleteBefore
        {
            get { return this.Parameters.GetValue<bool>("DeleteBefore"); }
            set { this.Parameters.SetOrAddValue("DeleteBefore", value); }
        }

        public FormatterBase Formatter
        {
            get { return this.fileAdapter.WriteFormatter; }
            set { this.fileAdapter.WriteFormatter = value; }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string encodingName = inParameters.GetValue<string>("EncodingName");
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

            this.fileAdapter.FileName = file;
            this.fileAdapter.Encoding = EncodingUtil.GetEncodingOrDefault(encodingName);

            if (table != null)
            {
                this.fileAdapter.WriteAllData(table, (this.IsFirstExecution && deleteBefore));
            }
            else if (data is byte[])
            {
                this.fileAdapter.WriteBinaryData(data as byte[], (this.IsFirstExecution && deleteBefore));
            }

            var outParameters = this.GetCurrentOutParameters();
            outParameters.SetOrAddValue("File", file);
            yield return outParameters;
        }
    }
}