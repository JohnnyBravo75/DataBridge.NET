using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataBridge.Extensions;
using DataBridge.Models;

namespace DataBridge.Formatters
{
    public class DataTableToFixedLengthFormatter : FormatterBase
    {
        private FieldDefinitionList fieldDefinitions = new FieldDefinitionList();

        public DataTableToFixedLengthFormatter()
        {
        }

        public FieldDefinitionList FieldDefinitions
        {
            get
            {
                return this.fieldDefinitions;
            }
        }

        public override object Format(object data, object existingData = null)
        {
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
                    var line = this.BuildLine(columnNames, this.FieldDefinitions);
                    lines.Add(line);
                }

                foreach (DataRow row in table.Rows)
                {
                    // generate data line
                    var fields = row.ItemArray.Select(field => field.ToString()).ToArray();
                    var line = this.BuildLine(fields, this.FieldDefinitions);
                    lines.Add(line);
                }
            }

            return lines;
        }

        private string BuildLine(IList<object> fields, FieldDefinitionList fieldDefinitions)
        {
            string line = "";
            int fieldIndex = 0;
            string paddedField = "";

            // loop through all defintions of import/exportfields and build the line of fixed text
            foreach (var fieldDef in fieldDefinitions)
            {
                if (fieldDef.IsActive)
                {
                    // build a padded field with the right length (take the field, pad to the max. length, and then cut of at the given position)
                    paddedField = fields[fieldIndex].ToStringOrEmpty()
                                                    .PadRight(fieldDef.DataSourceField.Length)
                                                    .Truncate(fieldDef.DataSourceField.Length);

                    // add to the line
                    line += paddedField;
                }

                fieldIndex++;
            }

            return line;
        }
    }
}