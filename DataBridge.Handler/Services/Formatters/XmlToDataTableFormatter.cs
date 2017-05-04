using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataBridge.Helper;

namespace DataBridge.Formatters
{
    public class XmlToDataTableFormatter : FormatterBase
    {
        public XmlToDataTableFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "RowXPath" });
            this.FormatterOptions.Add(new FormatterOption() { Name = "UseAttributes", Value = false });
            this.FormatterOptions.Add(new FormatterOption() { Name = "RemoveNamespaces", Value = false });
        }

        [XmlIgnore]
        public string RowXPath
        {
            get { return this.FormatterOptions.GetValue<string>("RowXPath"); }
            set { this.FormatterOptions.SetOrAddValue("RowXPath", value); }
        }

        [XmlIgnore]
        public bool UseAttributes
        {
            get { return this.FormatterOptions.GetValue<bool>("UseAttributes"); }
            set { this.FormatterOptions.SetOrAddValue("UseAttributes", value); }
        }

        [XmlIgnore]
        public bool RemoveNamespaces
        {
            get { return this.FormatterOptions.GetValue<bool>("RemoveNamespaces"); }
            set { this.FormatterOptions.SetOrAddValue("RemoveNamespaces", value); }
        }

        //public override object Format(object data, object existingData = null)
        //{
        //    DataTable table = null;
        //    var dataSet = existingData as DataSet;
        //    var xmlData = data as string;

        //    if (dataSet != null && dataSet.Tables.Count > 0)
        //    {
        //        table = dataSet.Tables[0];
        //    }

        //    table = FormatToDataTable(xmlData, true, table);

        //    if (dataSet == null)
        //    {
        //        dataSet = new DataSet();
        //    }
        //    else if (dataSet.Tables.Count > 0)
        //    {
        //        dataSet.Tables.RemoveAt(0);
        //    }
        //    dataSet.Tables.Add(table);

        //    return dataSet;
        //}

        //private DataTable FormatToDataTable(string xmlData, bool UseAttributes, DataTable table = null)
        //{
        //    XElement rowElement = XElement.Parse(xmlData);
        //    return FormatToDataTable(rowElement, UseAttributes, table);
        //}

        //private DataTable FormatToDataTable(XElement rowElement, bool UseAttributes, DataTable table = null)
        //{
        //    if (table == null)
        //    {
        //        table = new DataTable();
        //    }
        //    var rowValues = ReadRowValues(rowElement, UseAttributes);
        //    var row = table.NewRow();
        //    foreach (var field in rowValues)
        //    {
        //        // add column when not exist
        //        if (!table.Columns.Contains(field.Key))
        //        {
        //            table.Columns.Add(field.Key);
        //        }

        //        row[field.Key] = field.Value;
        //    }
        //    table.Rows.Add(row);

        //    return table;
        //}

        //private Dictionary<string, string> ReadRowValues(XElement xmlRow, bool UseAttributes = false)
        //{
        //    var rowValues = new Dictionary<string, string>();

        //    if (UseAttributes)
        //    {
        //        // take the attribute values
        //        foreach (var attr in xmlRow.Attributes())
        //        {
        //            rowValues.Add(attr.Name.LocalName, attr.Value);
        //        }
        //    }
        //    else
        //    {
        //        // take the child nodes
        //        foreach (var child in xmlRow.Elements())
        //        {
        //            if (child.NodeType == XmlNodeType.Element)
        //            {
        //                rowValues.Add(child.Name.LocalName, child.Value);
        //            }
        //        }
        //    }

        //    return rowValues;
        //}

        public override object Format(object data, object existingData = null)
        {
            DataTable table = null;
            string xmlData = data as string;

            if (existingData is DataSet && (existingData as DataSet).Tables.Count > 0)
            {
                table = (existingData as DataSet).Tables[0];
            }
            else if (existingData is DataTable)
            {
                table = existingData as DataTable;
            }

            if (data is XmlDocument)
            {
                table = this.FormatToDataTable(data as XmlDocument, table);
            }
            else if (data is string)
            {
                table = this.FormatToDataTable(data as string, table);
            }
            else
            {
                table = this.FormatToDataTable(data.ToStringOrEmpty(), table);
            }

            if (existingData is DataSet)
            {
                var existingDataSet = existingData as DataSet;
                if (existingDataSet.Tables.Count > 0)
                {
                    existingDataSet.Tables.RemoveAt(0);
                }

                existingDataSet.Tables.Add(table);
            }

            return table;
        }

        private DataTable FormatToDataTable(string xmlData, DataTable table = null)
        {
            if (string.IsNullOrEmpty(xmlData))
            {
                return table;
            }

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            return this.FormatToDataTable(xmlDoc, table);
        }

        private DataTable FormatToDataTable(XmlDocument xmlDoc, DataTable table = null)
        {
            if (table == null)
            {
                table = new DataTable();
            }

            if (xmlDoc == null)
            {
                return table;
            }

            if (xmlDoc.DocumentElement != null)
            {
                table.TableName = xmlDoc.DocumentElement.Name;
            }

            int y = 0;

            string rowXPath = this.FormatterOptions.GetValueOrDefault<string>("RowXPath", "/*");
            bool UseAttributes = this.FormatterOptions.GetValue<bool>("UseAttributes");
            bool removeNamespaces = this.FormatterOptions.GetValue<bool>("RemoveNamespaces");

            if (removeNamespaces)
            {
                xmlDoc = XmlHelper.StripXmlNameSpaces(xmlDoc);
            }
            try
            {
                foreach (XmlNode rowNode in xmlDoc.SelectNodes(rowXPath))
                {
                    var row = table.NewRow();

                    var rowValues = this.ReadRowValues(rowNode, UseAttributes);

                    foreach (var field in rowValues)
                    {
                        // add column when not exist
                        if (!table.Columns.Contains(field.Key))
                        {
                            table.Columns.Add(field.Key);
                        }

                        row[field.Key] = field.Value;
                    }

                    table.Rows.Add(row);

                    y++;
                }

                if (y == 0)
                {
                    throw new ArgumentException(string.Format("The XPath '{0}' was not found in:{1}{1}{2}", rowXPath, Environment.NewLine, xmlDoc.OuterXml.Truncate(1, 100)));
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("The XPath '{0}' was not found in:{1}{1}{2}", rowXPath, Environment.NewLine, xmlDoc.OuterXml.Truncate(1, 100)), ex);
            }

            return table;
        }

        /// <summary>
        /// Reads the row values.
        /// </summary>
        /// <param name="xmlRow">The XML row.</param>
        /// <param name="UseAttributes">if set to <c>true</c> [take attributes].</param>
        /// <returns>the rowdata as dictionary</returns>
        private Dictionary<string, string> ReadRowValues(XmlNode xmlRow, bool UseAttributes = false)
        {
            var rowValues = new Dictionary<string, string>();

            if (UseAttributes)
            {
                if (xmlRow.Attributes != null)
                {
                    // take the attribute values
                    foreach (XmlNode attr in xmlRow.Attributes)
                    {
                        if (rowValues.ContainsKey(attr.Name))
                        {
                            rowValues[attr.Name] = attr.Value;
                        }
                        else
                        {
                            rowValues.Add(attr.Name, attr.Value);
                        }
                    }
                }
            }
            else
            {
                if (xmlRow.ChildNodes != null)
                {
                    // take the child nodes
                    foreach (XmlNode child in xmlRow.ChildNodes)
                    {
                        if (child.NodeType == XmlNodeType.Element)
                        {
                            if (rowValues.ContainsKey(child.Name))
                            {
                                rowValues[child.Name] = child.GetInnerText("|");
                            }
                            else
                            {
                                rowValues.Add(child.Name, child.GetInnerText("|"));
                            }
                        }
                    }
                }
            }

            return rowValues;
        }
    }
}