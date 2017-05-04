using System;
using System.Data;
using System.Xml.Serialization;
using DataBridge.Helper;

namespace DataBridge.Formatters
{
    public class XslToDataTableFormatter : FormatterBase
    {
        public XslToDataTableFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "XslPath" });
            this.FormatterOptions.Add(new FormatterOption() { Name = "FieldSeperator" });
        }

        [XmlIgnore]
        public string XslPath
        {
            get { return this.FormatterOptions.GetValue<string>("XslPath"); }
            set { this.FormatterOptions.SetOrAddValue("XslPath", value); }
        }

        [XmlIgnore]
        public string FieldSeperator
        {
            get { return this.FormatterOptions.GetValue<string>("FieldSeperator"); }
            set { this.FormatterOptions.SetOrAddValue("FieldSeperator", value); }
        }

        public override object Format(object data, object existingData = null)
        {
            var xmlData = data as string;
            var table = existingData as DataTable;

            var result = XmlHelper.XslTransform(xmlData, this.XslPath);

            string[] lines = result.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                var values = line.Split(new string[] { this.FieldSeperator }, StringSplitOptions.RemoveEmptyEntries);

                if (table == null)
                {
                    table = new DataTable();
                    DataTableHelper.CreateTableColumns(table, values, isHeader: true);
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