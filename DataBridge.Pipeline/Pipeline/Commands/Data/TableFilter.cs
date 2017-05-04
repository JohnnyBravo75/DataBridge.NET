using System.Collections.Generic;
using System.Data;
using DataBridge.Extensions;

namespace DataBridge.Commands
{
    public class TableFilter : DataCommand
    {
        private Conditions filterConditions = new Conditions();

        public Conditions FilterConditions
        {
            get { return this.filterConditions; }
            set { this.filterConditions = value; }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            DataTable srcTable = inParameters.GetValue<DataTable>("Data");
            var tgtTable = this.FilterTable(srcTable);

            var outParameters = this.GetCurrentOutParameters();
            outParameters.Add(new CommandParameter() { Name = "Data", Value = tgtTable });
            yield return outParameters;
        }

        private DataTable FilterTable(DataTable srcTable)
        {
            // DataTable tgtTable = null;

            if (srcTable != null)
            {
                int rowCountBefore = srcTable.Rows.Count;

                //tgtTable = srcTable.Clone();
                int filtered = 0;

                for (int i = srcTable.Rows.Count - 1; i >= 0; i--)
                {
                    DataRow row = srcTable.Rows[i];

                    if (this.FilterConditions.IsNullOrEmpty() || ConditionEvaluator.CheckMatchingConditions(this.FilterConditions, row.ToDictionary()))
                    {
                        // keep the row
                    }
                    else
                    {
                        srcTable.Rows.Remove(row);
                        filtered++;
                    }
                }

                srcTable.AcceptChanges();

                this.LogDebug(string.Format("RowCount before={0}, RowCount={1}, Rows filtered={2}", rowCountBefore, srcTable.Rows.Count, filtered));
            }

            return srcTable;
        }
    }
}