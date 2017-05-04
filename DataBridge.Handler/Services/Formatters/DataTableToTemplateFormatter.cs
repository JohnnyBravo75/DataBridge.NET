using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Extensions;

namespace DataBridge.Formatters
{
    public class DataTableToTemplateFormatter : FormatterBase
    {
        private TokenProcessor tokenProcessor = new TokenProcessor();

        public DataTableToTemplateFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "Template", Value = "" });
        }

        [XmlIgnore]
        public string Template
        {
            get { return this.FormatterOptions.GetValue<string>("Template"); }
            set { this.FormatterOptions.SetOrAddValue("Template", value); }
        }

        public override object Format(object data, object existingData = null)
        {
            string template = this.FormatterOptions.GetValue<string>("Template");

            var table = data as DataTable;
            var headerLine = existingData as string;

            var lines = new List<string>();
            if (table != null)
            {
                if (string.IsNullOrEmpty(headerLine))
                {
                    // generate header line
                    var columnNames = table.GetColumnNames().ToArray();

                    var line = template;
                    for (int i = 0; i < columnNames.Length; i++)
                    {
                        var columnName = table.Columns[i].ColumnName;
                        var value = columnNames[i].ToStringOrEmpty();

                        line = TokenProcessor.ReplaceToken(line, columnName, value);
                    }

                    lines.Add(line);
                }

                foreach (DataRow row in table.Rows)
                {
                    // generate data line
                    var fields = row.ItemArray.Select(field => field.ToString()).ToArray();

                    var line = template;
                    for (int i = 0; i < fields.Length; i++)
                    {
                        var columnName = table.Columns[i].ColumnName;
                        var value = row[i].ToStringOrEmpty();

                        line = TokenProcessor.ReplaceToken(line, columnName, value);
                    }

                    lines.Add(line);
                }
            }

            return lines;
        }
    }
}