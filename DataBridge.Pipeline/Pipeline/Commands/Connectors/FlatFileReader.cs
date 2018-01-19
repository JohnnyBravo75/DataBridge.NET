using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Helper;
using DataConnectors.Adapter.FileAdapter;
using DataConnectors.Formatters;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "FlatFileReader", Title = "FlatFileReader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class FlatFileReader : DataCommand
    {
        private FlatFileAdapter fileAdapter = new FlatFileAdapter();

        public FlatFileReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "EncodingName", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
            this.Parameters.Add(new CommandParameter() { Name = "FileTemplate", Direction = Directions.In });
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
        public string FileTemplate
        {
            get { return this.Parameters.GetValue<string>("FileTemplate"); }
            set { this.Parameters.SetOrAddValue("FileTemplate", value); }
        }

        [XmlIgnore]
        public string EncodingName
        {
            get { return this.Parameters.GetValue<string>("EncodingName"); }
            set { this.Parameters.SetOrAddValue("EncodingName", value); }
        }

        public FormatterBase Formatter
        {
            get { return this.fileAdapter.ReadFormatter; }
            set { this.fileAdapter.ReadFormatter = value; }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string file = inParameters.GetValue<string>("File");
                string encodingName = inParameters.GetValue<string>("EncodingName");
                string fileTemplate = inParameters.GetValue<string>("FileTemplate");

                if (!string.IsNullOrEmpty(fileTemplate))
                {
                    var tokenValues = TokenProcessor.ParseTokenValues(file, fileTemplate);
                    this.SetTokens(tokenValues);
                }

                this.fileAdapter.FileName = file;

                if (string.IsNullOrEmpty(encodingName))
                {
                    this.AutoDetectSettings();
                    encodingName = this.EncodingName;
                }
                this.fileAdapter.Encoding = EncodingUtil.GetEncodingOrDefault(encodingName);

                this.LogDebugFormat("Start reading File='{0}'", file);

                int rowCount = 0;
                foreach (var table in this.fileAdapter.ReadData(this.MaxRowsToRead))
                {
                    rowCount += table.Rows.Count;

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("Data", table);
                    outParameters.SetOrAddValue("DataName", table.TableName);
                    yield return outParameters;
                }

                this.LogDebugFormat("End reading File='{0}': Rows={1}", file, rowCount);
            }
        }

        public override IList<string> Validate(CommandParameters parameters, ValidationContext context)
        {
            var messages = base.Validate(parameters, context);
            if (parameters != null)
            {
                string file = parameters.GetValue<string>("File");
                if (!System.IO.File.Exists(file))
                {
                    messages.Add(string.Format("The file '{0}' does not exist", file));
                }
            }
            return messages;
        }

        public void AutoDetectSettings()
        {
            this.EncodingName = this.fileAdapter.AutoDetectEncoding(this.File).WebName;
        }
    }
}