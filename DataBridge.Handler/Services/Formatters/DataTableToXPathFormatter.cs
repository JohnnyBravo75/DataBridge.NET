using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using DataBridge.Extensions;
using DataBridge.Helper;

namespace DataBridge.Formatters
{
    public class DataTableToXPathFormatter : FormatterBase
    {
        private XPathMappingList xPathMappings = new XPathMappingList();

        public XPathMappingList XPathMappings
        {
            get { return this.xPathMappings; }
            private set { this.xPathMappings = value; }
        }

        public DataTableToXPathFormatter()
        {
        }

        public override object Format(object data, object existingData = null)
        {
            var table = data as DataTable;
            var headerLine = existingData as string;

            var xmlLines = new List<string>();
            if (table != null)
            {
                if (this.XPathMappings == null || this.XPathMappings.Count == 0)
                {
                    this.AutoDetectSettings(table);
                }

                int rowIdx = 0;
                foreach (DataRow row in table.Rows)
                {
                    XmlDocument xmlDoc = new XmlDocument();

                    foreach (var xPathMapping in this.XPathMappings)
                    {
                        var value = row.GetField<string>(xPathMapping.Column);
                        if (value != null)
                        {
                            var subValues = value.Split(new string[] { "#" }, StringSplitOptions.None);
                            if (subValues.Length > 1)
                            {
                                var index = 1;
                                foreach (var subValue in subValues)
                                {
                                    string indexedXpath = xPathMapping.XPath.Replace("/@", "[" + index + "]/@");
                                    XPathUtil.CreateXPath(xmlDoc, indexedXpath, subValue);
                                    index++;
                                }
                            }
                            else
                            {
                                XPathUtil.CreateXPath(xmlDoc, xPathMapping.XPath, value);
                            }
                        }
                        else
                        {
                            XPathUtil.CreateXPath(xmlDoc, xPathMapping.XPath, value);
                        }
                    }

                    xmlLines.Add(xmlDoc.OuterXml);

                    rowIdx++;
                }
            }

            return xmlLines;
        }

        private void AutoDetectSettings(DataTable table)
        {
            this.XPathMappings = this.AutoDetectMapping(table);
        }

        private XPathMappingList AutoDetectMapping(DataTable table)
        {
            var xPathMappingList = new XPathMappingList();
            foreach (DataColumn column in table.Columns)
            {
                var xPathMapping = new XPathMapping()
                {
                    Column = column.ColumnName,
                    XPath = this.BuildXpathFromColumnName(column.ColumnName)
                };
                xPathMappingList.Add(xPathMapping);
            }

            return xPathMappingList;
        }

        private string BuildXpathFromColumnName(string columnName)
        {
            string leftPart = columnName;
            string rightPart = "";
            var index = columnName.IndexOf("$", StringComparison.Ordinal);
            if (index > -1)
            {
                leftPart = columnName.Substring(0, index);
                rightPart = columnName.Substring(index);
            }
            leftPart = leftPart.Replace("_", "/");
            rightPart = rightPart.Replace("$", "@");
            string xpath = "/" + leftPart + rightPart;

            xpath = this.AddRowGrouperWhenNotNested(xpath);

            return xpath;
        }

        private string AddRowGrouperWhenNotNested(string xpath)
        {
            if (!xpath.TrimStart('/').Contains("/"))
            {
                xpath = "/row/" + xpath;
            }
            return xpath;
        }
    }
}