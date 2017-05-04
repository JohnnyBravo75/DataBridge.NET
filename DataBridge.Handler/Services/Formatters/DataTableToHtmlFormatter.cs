using System;
using System.Data;
using System.Text;
using DataBridge.Extensions;

namespace DataBridge.Formatters
{
    public class DataTableToHtmlFormatter : FormatterBase
    {
        public override object Format(object data, object existingData = null)
        {
            var table = data as DataTable;
            var html = new StringBuilder();

            html.Append("<html xmlns='http://www.w3.org/1999/xhtml'>");
            html.Append("<head>");
            html.Append("<title>");
            html.Append("Page-");
            html.Append(Guid.NewGuid());
            html.Append("</title>");
            html.Append("</head>");
            html.Append("<body>");

            if (table != null)
            {
                html.Append(this.FormatToHtml(table));
            }

            html.Append("</body>");
            html.Append("</html>");

            return html.ToString();
        }

        private StringBuilder FormatToHtml(DataTable table)
        {
            var htmlTable = new StringBuilder();

            htmlTable.Append("<table border='1px' cellpadding='5' cellspacing='0' ");
            htmlTable.Append("style='border: solid 1px Silver; font-size: x-small;'>");

            // Add the headings row.
            htmlTable.Append("<tr align='left' valign='top'>");

            foreach (DataColumn column in table.Columns)
            {
                htmlTable.Append("<td align='left' valign='top'>");
                htmlTable.Append(column.ColumnName);
                htmlTable.Append("</td>");
            }

            htmlTable.Append("</tr>");

            // Add the data rows.
            foreach (DataRow row in table.Rows)
            {
                htmlTable.Append("<tr align='left' valign='top'>");

                foreach (DataColumn column in table.Columns)
                {
                    htmlTable.Append("<td align='left' valign='top'>");
                    htmlTable.Append(row[column.ColumnName].ToStringOrEmpty());
                    htmlTable.Append("</td>");
                }

                htmlTable.Append("</tr>");
            }

            //Close tags.
            htmlTable.Append("</table>");

            return htmlTable;
        }
    }
}