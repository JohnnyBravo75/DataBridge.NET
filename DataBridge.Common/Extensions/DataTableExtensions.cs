namespace DataBridge.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    public static class DataTableExtensions
    {
        public static bool AddWhenNotExist(this DataColumnCollection columns, string columnName, Type dataType = null)
        {
            if (columnName == null)
            {
                return false;
            }

            if (!columns.Contains(columnName))
            {
                if (dataType == null)
                {
                    dataType = typeof(string);
                }

                var column = columns.Add(columnName, dataType);

                return true;
            }

            return false;
        }

        public static bool AddWhenNotExist(this DataColumnCollection columns, DataColumn column)
        {
            if (column == null)
            {
                return false;
            }

            if (!columns.Contains(column.ColumnName))
            {
                columns.Add(column);
                return true;
            }

            return false;
        }

        public static void AddWhenNotExist(this DataColumnCollection columns, DataColumnCollection columnsToAdd)
        {
            if (columnsToAdd == null)
            {
                return;
            }

            foreach (DataColumn column in columns)
            {
                AddWhenNotExist(columns, column);
            }
        }

        public static DataTable ClearColumn(this DataTable table, DataColumn column, string whereExpression = "")
        {
            DataTable filteredTable = !string.IsNullOrEmpty(whereExpression) ? table.SelectRows(whereExpression, "")
                                                                             : table;

            foreach (DataRow row in filteredTable.Rows)
            {
                row[column] = null;
            }

            return filteredTable;
        }

        public static DataTable Delete(this DataTable table, string whereExpression = "")
        {
            DataTable filteredTable = !string.IsNullOrEmpty(whereExpression) ? table.SelectRows(whereExpression, "")
                                                                             : table;
            foreach (DataRow row in filteredTable.Rows)
            {
                row.Delete();
            }

            return filteredTable;
        }

        /// <summary>
        /// Lists the details of Column names and their types in a datatable
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        public static Dictionary<string, object> GetColumnNamesAndDataType(this DataTable dt)
        {
            Dictionary<string, object> dictColumnNameAndType = new Dictionary<string, object>();
            Enumerable.Range(0, dt.Columns.Count)
                        .ToList()
                        .ForEach(i => dictColumnNameAndType.Add(dt.Columns[i].ColumnName, dt.Columns[i].DataType));

            return dictColumnNameAndType;
        }

        public static IEnumerable<string> GetColumnNames(this DataTable table)
        {
            if (table == null)
            {
                return Enumerable.Empty<string>();
            }

            return table.Columns.Cast<DataColumn>()
                                .Select(column => column.ColumnName);
        }

        public static T GetField<T>(this DataRow row, string name, CultureInfo culture = null)
        {
            if (!row.Table.Columns.Contains(name))
            {
                return default(T);
            }

            if (culture == null)
            {
                culture = CultureInfo.InvariantCulture;
            }

            return row[name].ConvertTo<T>(culture);
        }

        /// <summary>
        /// Checks if a datatable has rows
        /// </summary>
        /// <param name="dt">the DataTable</param>
        /// <returns></returns>
        public static bool HasRows(this DataTable dt)
        {
            return dt.Rows.Count > 0 ? true : false;
        }

        public static bool ImportExternalRow(this DataTable table, DataRow importRow)
        {
            bool wasFound = false;
            bool allFound = true;
            DataRow row = table.NewRow();
            for (int i = 0; i < importRow.Table.Columns.Count; i++)
            {
                wasFound = false;
                for (int j = 0; j < row.Table.Columns.Count; j++)
                {
                    if (importRow.Table.Columns[i].ColumnName == row.Table.Columns[j].ColumnName)
                    {
                        row[j] = importRow[i];
                        wasFound = true;
                        break;
                    }
                }

                if (!wasFound)
                {
                    allFound = false;
                }
            }

            table.Rows.Add(row);

            return allFound;
        }

        /// <summary>
        /// Prints a DataTable to the console
        /// </summary>
        /// <param name="table">the DataTable</param>
        /// <param name="colLength">the max length of the printed columns</param>
        public static void PrintToConsole(this DataTable table, int colLength)
        {
            // print the header with the coluzmn names.
            foreach (var item in table.Columns)
            {
                string itemStr = StringExtensions.Truncate(item.ToString(), colLength);
                Console.Write(itemStr.PadRight(colLength) + " | ");
            }
            Console.WriteLine(""); // Print separator.
            Console.WriteLine("".PadRight((colLength + 3) * table.Columns.Count, '-'));

            // Loop over the rows.
            foreach (DataRow row in table.Rows)
            {
                // Loop over the column values.
                foreach (var item in row.ItemArray)
                {
                    string itemStr = StringExtensions.Truncate(item.ToString(), colLength);
                    Console.Write(itemStr.PadRight(colLength) + " | ");
                }
                Console.WriteLine(""); // Print separator.
            }
        }

        /// <summary>
        /// Filters a DataTable
        /// </summary>
        /// <param name="table">the DataTable</param>
        /// <param name="whereExpression"></param>
        /// <param name="orderByExpression"></param>
        /// <returns>the filtered DataTable</returns>
        public static DataTable SelectRows(this DataTable table, string whereExpression, string orderByExpression)
        {
            //dt.DefaultView.RowFilter = whereExpression;
            //dt.DefaultView.Sort = orderByExpression;
            //return dt.DefaultView.ToTable();

            DataView view = new DataView(table, whereExpression, orderByExpression, DataViewRowState.CurrentRows);
            return view.ToTable();
        }

        public static void SetField<T>(this DataRow row, string name, T value)
        {
            row[name] = value;
        }

        public static Dictionary<string, object> ToDictionary(this DataRow row)
        {
            return ToDictionary<object>(row);
        }

        public static Dictionary<string, T> ToDictionary<T>(this DataRow row)
        {
            var rowValues = row.Table.Columns
                                    .Cast<DataColumn>()
                                    .ToDictionary(col => col.ColumnName, col => row.Field<T>(col.ColumnName));
            return rowValues;
        }

        public static DataRow AddRow(this DataTable table, IDictionary<string, object> row, bool checkForMissingColumns = false)
        {
            // check for an empty row
            if (row == null || row.Count == 0)
            {
                return null;
            }

            var dataRow = table.NewRow();
            foreach (var col in row)
            {
                if (checkForMissingColumns)
                {
                    var colType = (col.Value == DBNull.Value || col.Value == null)
                                    ? typeof(string)
                                    : col.Value.GetType();

                    table.Columns.AddWhenNotExist(col.Key, colType);
                }

                dataRow[col.Key] = col.Value;
            }
            table.Rows.Add(dataRow);

            return dataRow;
        }

        //public static void SetColumnsOrder(this DataTable table, params String[] columnNames)
        //{
        //    for (int columnIndex = 0; columnIndex < columnNames.Length; columnIndex++)
        //    {
        //        table.Columns[columnNames[columnIndex]].SetOrdinal(columnIndex);
        //    }
        //}

        /// <summary>
        /// SetOrdinal of DataTable columns based on the index of the columnNames array. Removes invalid column names first.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="columnNames"></param>
        /// <remarks> http://stackoverflow.com/questions/3757997/how-to-change-datatable-colums-order</remarks>
        public static void SetColumnsOrder(this DataTable table, params String[] columnNames)
        {
            List<string> listColNames = columnNames.ToList();

            //Remove invalid column names.
            foreach (string colName in columnNames)
            {
                if (!table.Columns.Contains(colName))
                {
                    listColNames.Remove(colName);
                }
            }

            foreach (string colName in listColNames)
            {
                table.Columns[colName].SetOrdinal(listColNames.IndexOf(colName));
            }
        }
    }
}