using System.Collections.Generic;
using System.Data;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using DataBridge.Helper;

namespace DataBridge.Formatters
{
    public class RegExToDataTableFormatter : FormatterBase
    {
        public RegExToDataTableFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "Pattern", Value = "" });
        }

        [XmlIgnore]
        public string Pattern
        {
            get { return this.FormatterOptions.GetValue<string>("Pattern"); }
            set { this.FormatterOptions.SetOrAddValue("Pattern", value); }
        }

        public override object Format(object data, object existingData = null)
        {
            string pattern = this.FormatterOptions.GetValue<string>("Pattern");
            bool hasHeader = true;

            var table = existingData as DataTable;
            var line = data as string;

            if (line != null)
            {
                var values = new List<string>();
                var regex = new Regex(pattern);
                foreach (Match match in regex.Matches(line))
                {
                    values.Add(match.Value);
                }

                if (table == null)
                {
                    table = new DataTable();
                    DataTableHelper.CreateTableColumns(table, values, hasHeader);
                }
                else
                {
                    DataTableHelper.AddTableRow(table, values);
                }
            }

            return table;
        }
    }
}