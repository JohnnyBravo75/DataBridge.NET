using System.Data;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DataBridge.Extensions;

namespace DataBridge.Formatters
{
    public class DataTableToSimpleXmlFormatter : FormatterBase
    {
        public DataTableToSimpleXmlFormatter()
        {
        }

        public override object Format(object data, object existingData = null)
        {
            var table = data as DataTable;
            var headerLine = existingData as string;
            var xml = new StringBuilder();

            if (table != null)
            {
                if (string.IsNullOrEmpty(headerLine))
                {
                    // generate header line
                    XmlDocument xDoc = new XmlDocument();
                    XmlDeclaration declaration = xDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xml.AppendLine(declaration.Value);
                }

                int rowIdx = 0;
                foreach (DataRow row in table.Rows)
                {
                    xml.AppendLine("<" + table.TableName + ">");
                    foreach (DataColumn column in row.Table.Columns)
                    {
                        // generate data line
                        var line = new XElement(column.ToString(), row[column].ToString()).ToString();
                        xml.AppendLine(line);
                    }

                    xml.AppendLine("</" + table.TableName + ">");

                    //var fields = row.ItemArray.Select(field => field.ToString()).ToArray();

                    rowIdx++;
                }
            }

            return xml.ToStringOrEmpty();
        }
    }
}