using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataBridge.Common;
using DataBridge.Extensions;
using DataBridge.Formatters;
using DataBridge.Handler.Services.Adapter;
using DataBridge.Helper;

namespace DataBridge.Commands
{
    public class FileImporterReader : DataCommand, IPlugIn
    {
        private FlatFileAdapter fileAdapter = new FlatFileAdapter();

        public FileImporterReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "EncodingName", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "TableName", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "ColumnPrefix", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
        }

        [XmlIgnore]
        protected int MaxRowsToRead
        {
            get { return this.UseStreaming ? this.StreamingBlockSize : -1; }
        }

        [XmlIgnore]
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
        public string TableName
        {
            get { return this.Parameters.GetValue<string>("TableName"); }
            set { this.Parameters.SetOrAddValue("TableName", value); }
        }

        [XmlIgnore]
        public string ColumnPrefix
        {
            get { return this.Parameters.GetValue<string>("ColumnPrefix"); }
            set { this.Parameters.SetOrAddValue("ColumnPrefix", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string encodingName = inParameters.GetValue<string>("EncodingName");
            string tableName = inParameters.GetValue<string>("TableName");
            string columnPrefix = inParameters.GetValue<string>("ColumnPrefix");

            if (string.IsNullOrEmpty(encodingName))
            {
                this.AutoDetectSettings();
                encodingName = this.EncodingName;
            }

            this.LogDebugFormat("Start reading File='{0}'", file);

            this.fileAdapter.FileName = file;
            this.fileAdapter.Encoding = EncodingUtil.GetEncodingOrDefault(encodingName);

            var formatter = new FlatFileToDataTableFormatter();
            formatter.ColumnName = columnPrefix + "_ROW";
            this.fileAdapter.ReadFormatter = formatter;

            int rowCount = 0;
            foreach (var table in this.fileAdapter.ReadData(this.MaxRowsToRead))
            {
                table.TableName = tableName;

                table.Columns.AddWhenNotExist(columnPrefix + "_ID", typeof(long));
                table.Columns.AddWhenNotExist(columnPrefix + "_FILE_NAME", typeof(string));
                table.Columns.AddWhenNotExist(columnPrefix + "_STATUS", typeof(string));
                table.Columns.AddWhenNotExist(columnPrefix + "_TRANSFARE_DATE", typeof(DateTime));
                table.Columns.AddWhenNotExist(columnPrefix + "_INSERT_DATE", typeof(DateTime));
                table.Columns.AddWhenNotExist(columnPrefix + "_INSERT_USER", typeof(string));
                table.Columns.AddWhenNotExist(columnPrefix + "_UPDATE_DATE", typeof(DateTime));
                table.Columns.AddWhenNotExist(columnPrefix + "_UPDATE_USER", typeof(string));

                foreach (DataRow row in table.Rows)
                {
                    row[columnPrefix + "_FILE_NAME"] = this.fileAdapter.FileName;
                    row[columnPrefix + "_STATUS"] = "pending";
                }

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

            string file = parameters.GetValue<string>("File");
            if (!System.IO.File.Exists(file))
            {
                messages.Add(string.Format("The file '{0}' does not exist", file));
            }

            return messages;
        }

        public void AutoDetectSettings()
        {
            this.EncodingName = this.fileAdapter.AutoDetectEncoding(this.File).WebName;
        }
    }
}