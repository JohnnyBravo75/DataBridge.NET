using System.Data;
using System.IO;
using System.Xml.Linq;
using DataBridge.Extensions;

namespace DataBridge.Formatters
{
    public class XmlToDataSetFormatter : FormatterBase
    {
        private bool autoRenameWhenConflict = true;

        public override object Format(object data, object existingData = null)
        {
            var xmlData = data as string;
            return this.FormatToDataSet(xmlData, existingData as DataSet);
        }

        private DataSet FormatToDataSet(XElement rowElement, DataSet dataSet = null)
        {
            return this.FormatToDataSet(rowElement.ToStringOrEmpty(), dataSet);
        }

        private DataSet FormatToDataSet(string xmlData, DataSet dataSet = null)
        {
            if (dataSet == null)
            {
                dataSet = new DataSet();
            }

            try
            {
                dataSet.ReadXml(new StringReader(xmlData), XmlReadMode.InferSchema);
            }
            catch (DuplicateNameException)
            {
                if (this.autoRenameWhenConflict)
                {
                    dataSet.ReadXml(new StringReader(xmlData), XmlReadMode.IgnoreSchema);

                    this.RenameDuplicateColumn(dataSet);
                }
                else
                {
                    throw;
                }
            }

            return dataSet;
        }

        private void RenameDuplicateColumn(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                bool hasDuplicate = dataSet.Tables[0].Columns.Contains(table.TableName);

                if (hasDuplicate)
                {
                    table.TableName = table.TableName + "_Renamed";
                }
            }
        }
    }
}