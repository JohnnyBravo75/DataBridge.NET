using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataBridge.Helper;

namespace DataBridge.Formatters
{
    public class FlatFileToDataTableFormatter : FormatterBase
    {
        public FlatFileToDataTableFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "ColumnName", Value = "Data" });
        }

        [XmlIgnore]
        public string ColumnName
        {
            get { return this.FormatterOptions.GetValue<string>("ColumnName"); }
            set { this.FormatterOptions.SetOrAddValue("ColumnName", value); }
        }

        public override object Format(object data, object existingData = null)
        {
            IList<string> lines = null;
            var table = existingData as DataTable;

            if (data is string)
            {
                lines = new List<string>();
                lines.Add(data as string);
            }
            else if (data is IEnumerable<string>)
            {
                lines = data as IList<string>;
            }

            if (lines != null)
            {
                table = this.FormatToDataTable(lines, table);
            }

            return table;
        }

        private DataTable FormatToDataTable(IList<string> lines, DataTable table)
        {
            foreach (var line in lines)
            {
                if (line != null)
                {
                    var values = new List<string>();

                    if (table == null)
                    {
                        string columnName = this.FormatterOptions.GetValue<string>("ColumnName") ?? "Data";
                        values.Add(columnName);

                        table = new DataTable();
                        DataTableHelper.CreateTableColumns(table, values, true);
                    }
                    else
                    {
                        values.Add(line);
                        DataTableHelper.AddTableRow(table, values);
                    }
                }
            }

            return table;
        }
    }
}