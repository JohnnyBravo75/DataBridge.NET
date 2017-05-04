using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Formatters;
using DataBridge.Helper;

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

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string encodingName = inParameters.GetValue<string>("EncodingName");

            if (string.IsNullOrEmpty(encodingName))
            {
                this.AutoDetectSettings();
                encodingName = this.EncodingName;
            }

            this.LogDebugFormat("Start reading File='{0}'", file);

            this.fileAdapter.FileName = file;
            this.fileAdapter.Encoding = EncodingUtil.GetEncodingOrDefault(encodingName);

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