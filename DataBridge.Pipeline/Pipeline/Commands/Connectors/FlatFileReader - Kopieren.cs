using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using DataPipeline.Formatter;
using DataPipeline.Helper;
using DataPipeline.Utils;

namespace DataPipeline.Commands
{
    public class FlatFileReader : PipelineCommand
    {
        private DataTable headerTable;
        private FormatterBase formatter = new DefaultFormatter();
        private string lastFile = "";
        int maxRowsToRead = -1;
        int skipRows = 0;

        public FlatFileReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = "InOut"});
            this.Parameters.Add(new CommandParameter() { Name = "EncodingName", Direction = "In"});
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = "Out" });
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

        public FormatterBase Formatter
        {
            get { return this.formatter; }
            set { this.formatter = value; }
        }

        public override void Initialize()
        {
            DataTableHelper.DisposeTable(headerTable);
            
            base.Initialize();
        }

        public override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string encodingName = inParameters.GetValue<string>("EncodingName");
            Encoding encoding = EncodingUtil.GetEncodingOrDefault(encodingName);

            if (formatter == null)
            {
                throw new ArgumentNullException("formatter");
            }

            // new File
            if (file != lastFile)
            {
                headerTable = null;
                lastFile = file;
            }

            var lines = new List<string>(); 
            using (var reader = new StreamReader(file, encoding))
            {
                
                LogDebug(string.Format("Start reading File='{0}'", file));

                int readedRows = 0;
                int rowIdx = 0;
                DataTable table = new DataTable();
                while (!reader.EndOfStream)
                {
                    var line = reader.ReadLine();
                    lines.Add(line);
                    rowIdx++;

                    if (skipRows > 0 && rowIdx < skipRows)
                    {
                        continue;
                    }

                    // first row (header?)
                    if (readedRows == 0)
                    {
                         table = headerTable != null
                                            ? headerTable.Clone()
                                            :null;
                    }

                    table = formatter.Format(line, table) as DataTable;
                    
                    if (table != null)
                    {
                        table.TableName = Path.GetFileName(file);

                        if (headerTable == null)
                        {
                            headerTable = table.Clone();
                            continue;
                        }

                        readedRows++;

                        if (maxRowsToRead <= 0 || (maxRowsToRead > 0 && readedRows >= maxRowsToRead))
                        {                            
                            if (rowIdx % 5000 == 0)
                            {
                                LogDebug(string.Format("Readed Rows={0}", rowIdx));
                            }

                            readedRows = 0;

                            var outParameters = GetCurrentOutParameters();
                            outParameters.SetOrAddValue("Data", table);
                            outParameters.SetOrAddValue("DataName", table.TableName);
                            yield return TransferOutParameters(outParameters);
                        }
                    }                   
                }

                if (readedRows > 0 || table == null)
                {
                    var outParameters = GetCurrentOutParameters();
                    outParameters.SetOrAddValue("Data", table);
                    outParameters.SetOrAddValue("DataName", (table != null? table.TableName : ""));
                    yield return TransferOutParameters(outParameters);
                }

                LogDebug(string.Format("End reading File='{0}', Rows={1}", file, rowIdx));
                DataTableHelper.DisposeTable(table);
            }
        }
    }
}