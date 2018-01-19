using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataConnectors.Adapter.FileAdapter;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "ExcelReader", Title = "ExcelReader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class ExcelReader : DataCommand
    {
        private ExcelNativeAdapter excelAdapter = new ExcelNativeAdapter();

        public ExcelReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Sheet", Direction = Directions.In, NotNull = true });
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
        public string Sheet
        {
            get { return this.Parameters.GetValue<string>("Sheet"); }
            set { this.Parameters.SetOrAddValue("Sheet", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string file = inParameters.GetValue<string>("File");
                string sheet = inParameters.GetValue<string>("Sheet");

                this.excelAdapter.FileName = file;
                this.excelAdapter.SheetName = sheet;
                this.excelAdapter.Connect();

                this.LogDebugFormat("Start reading File='{0}'", file);

                int rowIdx = 0;
                foreach (DataTable table in this.excelAdapter.ReadData(this.MaxRowsToRead))
                {
                    rowIdx += table.Rows.Count;

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("Data", table);
                    outParameters.SetOrAddValue("DataName", table.TableName);
                    yield return outParameters;
                }

                this.LogDebugFormat("End reading File='{0}': Rows={1}", file, rowIdx);
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