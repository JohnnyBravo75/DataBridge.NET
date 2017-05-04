using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataBridge.Extensions;

namespace DataBridge.Helper
{
    public static class DataTableHelper
    {
        public static void CreateTableColumns(DataTable table, IList<string> values, bool isHeader)
        {
            for (int i = 0; i < values.Count; i++)
            {
                // default column name, when file has no header line
                string columnName = string.Format("Column{0}", (i + 1));

                if (isHeader && !string.IsNullOrEmpty(values[i]))
                {
                    columnName = values[i].Truncate(254);
                }

                // add column, when not exists
                if (!string.IsNullOrEmpty(columnName) && !table.Columns.Contains(columnName))
                {
                    table.Columns.Add(new DataColumn(columnName, typeof(string)));
                }
            }
        }

        public static void AddTableRow(DataTable table, IList<string> values)
        {
            // check for an empty row
            if (values == null || values.Count == 0)
            {
                return;
            }

            // enough columns existing? (can happen, when there arw rows with differnt number of separators)
            if (table.Columns.Count < values.Count())
            {
                // no, add the missing
                CreateTableColumns(table, values, true);
            }

            // put the whole row into the table
            table.Rows.Add(values.ToArray());
        }

        public static void DisposeTable(DataTable table)
        {
            if (table != null)
            {
                table.Clear();
                table.Dispose();
                table = null;
            }
        }
    }
}