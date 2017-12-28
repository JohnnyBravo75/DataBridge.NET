using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataBridge.Common.Services.Adapter;
using DataBridge.Formatters;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "XmlFileWriter", Title = "XmlFileWriter", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class XmlFileWriter : DataCommand
    {
        private XmlAdapter xmlAdapter = new XmlAdapter();

        public XmlFileWriter()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File" });
            this.Parameters.Add(new CommandParameter() { Name = "RowXPath" });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "DeleteBefore", Value = true, Direction = Directions.In });

            this.Formatter = new DataTableToXPathFormatter();
        }

        [XmlIgnore]
        [System.ComponentModel.Editor(typeof(Wpf.Toolkit.PropertyEditors.OpenFilePropertyEditor), typeof(Wpf.Toolkit.PropertyEditors.OpenFilePropertyEditor))]
        public string File
        {
            get { return this.Parameters.GetValue<string>("File"); }
            set { this.Parameters.SetOrAddValue("File", value); }
        }

        [XmlIgnore]
        public string RowXPath
        {
            get { return this.Parameters.GetValue<string>("RowXPath"); }
            set { this.Parameters.SetOrAddValue("RowXPath", value); }
        }

        [XmlIgnore]
        public bool DeleteBefore
        {
            get { return this.Parameters.GetValue<bool>("DeleteBefore"); }
            set { this.Parameters.SetOrAddValue("DeleteBefore", value); }
        }

        public FormatterBase Formatter
        {
            get { return this.xmlAdapter.WriteFormatter; }
            set { this.xmlAdapter.WriteFormatter = value; }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string rowXPath = inParameters.GetValueOrDefault<string>("RowXPath", "/table[@name='{DataName}']");
            var data = inParameters.GetValue<object>("Data");
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

            this.xmlAdapter.FileName = file;
            this.xmlAdapter.XPath = rowXPath;

            this.xmlAdapter.WriteAllData(table, (this.IsFirstExecution && deleteBefore));

            this.LogDebug(string.Format("Writing file={0}, Nodes={1}", file, table.Rows.Count));

            var outParameters = this.GetCurrentOutParameters();
            outParameters.SetOrAddValue("File", file);

            yield return outParameters;
        }
    }
}