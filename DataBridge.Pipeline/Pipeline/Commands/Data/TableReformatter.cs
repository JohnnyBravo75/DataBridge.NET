using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;

namespace DataBridge.Commands
{
    public class TableReformatter : DataCommand
    {
        private List<ColumnMapping> columnMappings = new List<ColumnMapping>();

        public TableReformatter()
        {
            this.Parameters.Add(new CommandParameter() { Name = "TableName" });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
        }

        public List<ColumnMapping> ColumnMappings
        {
            get { return this.columnMappings; }
            set { this.columnMappings = value; }
        }

        [XmlIgnore]
        public string TableName
        {
            get { return this.Parameters.GetValue<string>("TableName"); }
            set { this.Parameters.SetOrAddValue("TableName", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            DataTable table = inParameters.GetValue<DataTable>("Data");
            string tableName = inParameters.GetValue<string>("TableName");

            if (table != null && table.TableName == tableName)
            {
                this.ReFormatTable(table);
            }

            var outParameters = this.GetCurrentOutParameters();
            outParameters.AddOrUpdate(new CommandParameter() { Name = "Data", Value = table });
            yield return outParameters;
        }

        private void ReFormatTable(DataTable table)
        {
            // Remove columns not in the mapping list
            for (int i = table.Columns.Count - 1; i >= 0; i--)
            {
                string columnName = table.Columns[i].ColumnName.ToString();

                if (!this.ColumnMappings.Exists(x => x.SourceColumn == columnName))
                {
                    table.Columns.RemoveAt(i);
                }
            }

            // Reorder
            foreach (var columnMapping in this.ColumnMappings)
            {
                if (!string.IsNullOrEmpty(columnMapping.SourceColumn))
                {
                    if (table.Columns.Contains(columnMapping.SourceColumn))
                    {
                        table.Columns[columnMapping.SourceColumn].SetOrdinal(this.ColumnMappings.IndexOf(columnMapping));
                    }
                }
            }

            // Rename
            foreach (var columnMapping in this.ColumnMappings)
            {
                if (!string.IsNullOrEmpty(columnMapping.SourceColumn))
                {
                    if (table.Columns.Contains(columnMapping.SourceColumn))
                    {
                        table.Columns[columnMapping.SourceColumn].ColumnName = columnMapping.TargetColumn;
                    }
                }
            }

            table.AcceptChanges();
        }
    }

    public class ColumnMapping
    {
        private string targetColumn;

        public string TargetColumn
        {
            get
            {
                if (string.IsNullOrEmpty(this.targetColumn))
                {
                    return this.SourceColumn;
                }
                return this.targetColumn;
            }
            set { this.targetColumn = value; }
        }

        public string SourceColumn { get; set; }
    }
}