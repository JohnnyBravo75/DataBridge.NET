using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataConnectors.Adapter.FileAdapter;
using DataConnectors.Formatters;
using DataConnectors.Formatters.Model;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "XmlFileReader", Title = "XmlFileReader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class XmlFileReader : DataCommand, IHasXmlNameSpaces
    {
        private XmlAdapter xmlAdapter = new XmlAdapter();

        public XmlFileReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "RowXPath", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });

            this.Formatter = new XPathToDataTableFormatter();
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
        public string RowXPath
        {
            get { return this.Parameters.GetValue<string>("RowXPath"); }
            set { this.Parameters.SetOrAddValue("RowXPath", value); }
        }

        public FormatterBase Formatter
        {
            get { return this.xmlAdapter.ReadFormatter; }
            set { this.xmlAdapter.ReadFormatter = value; }
        }

        public List<XmlNameSpace> XmlNameSpaces
        {
            get { return this.xmlAdapter.XmlNameSpaces; }
            set { this.xmlAdapter.XmlNameSpaces = value; }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string file = inParameters.GetValue<string>("File");
                string rowXPath = inParameters.GetValue<string>("RowXPath");

                this.LogDebugFormat("Start reading File='{0}'", file);

                this.xmlAdapter.FileName = file;
                this.xmlAdapter.XPath = rowXPath;

                int rowCount = 0;
                foreach (var data in this.xmlAdapter.ReadDataObjects<object>(this.MaxRowsToRead))
                {
                    if (data is DataSet)
                    {
                        foreach (DataTable dsTable in (data as DataSet).Tables)
                        {
                            rowCount += dsTable.Rows.Count;

                            var outParameters = this.GetCurrentOutParameters();
                            outParameters.Add(new CommandParameter() { Name = "Data", Value = dsTable });
                            outParameters.Add(new CommandParameter() { Name = "DataName", Value = dsTable.TableName });
                            yield return outParameters;
                        }
                    }
                    else if (data is DataTable)
                    {
                        rowCount += (data as DataTable).Rows.Count;

                        var outParameters = this.GetCurrentOutParameters();
                        outParameters.Add(new CommandParameter() { Name = "Data", Value = data });
                        outParameters.Add(new CommandParameter()
                        {
                            Name = "DataName",
                            Value = (data as DataTable).TableName
                        });
                        yield return outParameters;
                    }
                }

                this.LogDebugFormat("End reading File='{0}': Rows={1}", file, rowCount);
            }
        }
    }
}