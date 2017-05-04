using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DataBridge.Extensions;

namespace DataBridge.Formatters
{
    public class DataTableToCsvFormatter : FormatterBase
    {
        public DataTableToCsvFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "Separator", Value = ";" });
            this.FormatterOptions.Add(new FormatterOption() { Name = "Enclosure", Value = "" });
        }

        [XmlIgnore]
        public string Separator
        {
            get { return this.FormatterOptions.GetValue<string>("Separator"); }
            set { this.FormatterOptions.SetOrAddValue("Separator", value); }
        }

        [XmlIgnore]
        public string Enclosure
        {
            get { return this.FormatterOptions.GetValue<string>("Enclosure"); }
            set { this.FormatterOptions.SetOrAddValue("Enclosure", value); }
        }

        public override object Format(object data, object existingData = null)
        {
            string separator = this.FormatterOptions.GetValue<string>("Separator");
            string enclosure = this.FormatterOptions.GetValue<string>("Enclosure");
            bool hasHeader = true;

            var table = data as DataTable;
            var headerLine = existingData as string;

            var lines = new List<string>();
            if (table != null)
            {
                if (string.IsNullOrEmpty(headerLine))
                {
                    // generate header line
                    var columnNames = table.GetColumnNames().ToArray();
                    var line = this.BuildLine(columnNames, separator[0], enclosure);
                    lines.Add(line);
                }

                foreach (DataRow row in table.Rows)
                {
                    // generate data line
                    var fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                    var line = this.BuildLine(fields, separator[0], enclosure);
                    lines.Add(line);
                }
            }

            return lines;
        }

        /// <summary>
        /// Builds a line out of the given fields
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="enclosure">if set to <c>true</c> [quoted].</param>
        /// <returns>a line as string</returns>
        private string BuildLine(IList<string> fields, char separator, string enclosure)
        {
            var line = new StringBuilder();
            bool quoted = !string.IsNullOrEmpty(enclosure);

            for (int i = 0; i < fields.Count; i++)
            {
                if (quoted)
                {
                    line.Append(enclosure);
                }

                line.Append(fields[i].ToString());

                if (quoted)
                {
                    line.Append(enclosure);
                }

                if (i < fields.Count - 1)
                {
                    line.Append(separator);
                }
            }

            return line.ToString();
        }
    }
}