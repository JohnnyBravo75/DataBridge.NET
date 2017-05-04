using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Extensions;

namespace DataBridge.Formatters
{
    public class DataTableToCustomPatternFormatter : FormatterBase
    {
        private TokenProcessor tokenProcessor = new TokenProcessor();

        public DataTableToCustomPatternFormatter()
        {
        }

        [XmlElement]
        public string Pattern
        {
            get { return this.FormatterOptions.GetValue<string>("Pattern"); }
            set { this.FormatterOptions.SetOrAddValue("Pattern", value); }
        }

        public override object Format(object data, object existingData = null)
        {
            string pattern = this.FormatterOptions.GetValue<string>("Pattern");
            bool hasHeader = true;

            var table = data as DataTable;
            var headerLine = existingData as string;

            var lines = new List<string>();
            if (table != null)
            {
                if (string.IsNullOrEmpty(headerLine))
                {
                    // generate header line
                    headerLine = pattern;
                    var columnNames = table.GetColumnNames().ToArray();
                    foreach (var columnName in columnNames)
                    {
                        headerLine = TokenProcessor.ReplaceToken(headerLine, columnName, columnName);
                    }
                    lines.Add(headerLine);
                }

                foreach (DataRow row in table.Rows)
                {
                    // generate data line
                    var line = pattern;
                    var columnNames = table.GetColumnNames().ToArray();
                    foreach (var columnName in columnNames)
                    {
                        var columnValue = row[columnName];
                        line = TokenProcessor.ReplaceToken(line, columnName, columnValue);
                    }
                    var fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}