using System.Data;
using Newtonsoft.Json;

namespace DataBridge.Formatters
{
    public class DataTableToJsonFormatter : FormatterBase
    {
        public override object Format(object data, object existingData = null)
        {
            var table = data as DataTable;
            var headerLine = existingData as string;

            string json = JsonConvert.SerializeObject(table, Formatting.Indented);

            return json;
        }
    }
}