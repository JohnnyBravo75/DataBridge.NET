using System;
using System.Collections.Generic;
using System.Data;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataBridge.Helper;

namespace DataBridge.Formatters
{
    public class XPathToDataTableFormatter : FormatterBase, IHasXmlNameSpaces
    {
        private XPathMappingList xPathMappings = new XPathMappingList();
        private List<XmlNameSpace> xmlNameSpaces = new List<XmlNameSpace>();

        public XPathMappingList XPathMappings
        {
            get { return this.xPathMappings; }
            private set { this.xPathMappings = value; }
        }

        public XPathToDataTableFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "RowXPath" });
            this.FormatterOptions.Add(new FormatterOption() { Name = "RemoveNamespaces", Value = false });
        }

        [XmlIgnore]
        public string RowXPath
        {
            get { return this.FormatterOptions.GetValue<string>("RowXPath"); }
            set { this.FormatterOptions.SetOrAddValue("RowXPath", value); }
        }

        [XmlIgnore]
        public bool RemoveNamespaces
        {
            get { return this.FormatterOptions.GetValue<bool>("RemoveNamespaces"); }
            set { this.FormatterOptions.SetOrAddValue("RemoveNamespaces", value); }
        }

        public List<XmlNameSpace> XmlNameSpaces
        {
            get { return this.xmlNameSpaces; }
            set { this.xmlNameSpaces = value; }
        }

        public override object Format(object data, object existingData = null)
        {
            DataTable table = null;
            var dataSet = existingData as DataSet;
            var xmlData = data as string;

            if (dataSet != null && dataSet.Tables.Count > 0)
            {
                table = dataSet.Tables[0];
            }

            //if (this.XPathMappings == null || this.XPathMappings.Count == 0)
            //{
            //    this.AutoDetectSettings(xmlData);
            //}

            table = this.FormatToDataTable(xmlData, table);

            //return table;
            if (dataSet == null)
            {
                dataSet = new DataSet();
            }
            else if (dataSet.Tables.Count > 0)
            {
                dataSet.Tables.RemoveAt(0);
            }
            dataSet.Tables.Add(table);

            return dataSet;
        }

        private DataTable FormatToDataTable(string xmlData, DataTable table = null)
        {
            if (table == null)
            {
                table = new DataTable();
            }

            if (string.IsNullOrEmpty(xmlData))
            {
                return table;
            }

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(xmlData);

            if (xmlDoc.DocumentElement != null)
            {
                table.TableName = xmlDoc.DocumentElement.Name;
            }

            var y = 0;

            var rowXPath = this.FormatterOptions.GetValueOrDefault<string>("RowXPath", "/");
            var removeNamespaces = this.FormatterOptions.GetValueOrDefault<bool>("RemoveNamespaces", false);

            if (removeNamespaces)
            {
                xmlDoc = XmlHelper.StripXmlNameSpaces(xmlDoc);
            }
            try
            {
                foreach (XmlNode rowNode in xmlDoc.SelectNodes(rowXPath))
                {
                    var xPathMappings = this.XPathMappings;
                    if (xPathMappings == null || xPathMappings.Count == 0)
                    {
                        xPathMappings = this.AutoDetectMapping(rowNode);
                    }

                    var rowValues = this.ReadRowValues(rowNode, xPathMappings);

                    table.AddRow(rowValues, checkForMissingColumns: true);

                    y++;
                }
            }
            catch (Exception ex)
            {
                throw new ArgumentException(string.Format("The XPath '{0}' was not found in:{1}{1}{2}", rowXPath, Environment.NewLine, xmlDoc.OuterXml), ex);
            }

            return table;
        }

        /// <summary>
        /// Reads the row values.
        /// </summary>
        /// <param name="xmlRow">The XML row.</param>
        /// <param name="xPathMappings">The x path mappings.</param>
        /// <returns>
        /// the rowdata as dictionary
        /// </returns>
        private Dictionary<string, object> ReadRowValues(XmlNode xmlRow, XPathMappingList xPathMappings)
        {
            var rowValues = new Dictionary<string, object>();

            var xdoc = new XmlDocument();
            xdoc.LoadXml(xmlRow.OuterXml);

            var nsMgr = this.CreateXmlNamespaceManager(xdoc);

            foreach (var xPathMapping in xPathMappings)
            {
                // add all values from the path to the column
                var nodes = xdoc.SelectNodes(xPathMapping.XPath, nsMgr);
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        rowValues.AddOrAppend(xPathMapping.Column, node != null
                                                                    ? node.GetInnerText("|")
                                                                    : string.Empty, "#");
                    }
                }
            }

            return rowValues;
        }

        private void AutoDetectSettings(string xmlData)
        {
            this.XPathMappings = this.AutoDetectMapping(xmlData);
        }

        private XPathMappingList AutoDetectMapping(string xmlData)
        {
            return this.AutoDetectMapping(XDocument.Parse(xmlData));
        }

        private XPathMappingList AutoDetectMapping(XmlNode xmlNode)
        {
            using (var xmlNodeReader = new XmlNodeReader(xmlNode))
            {
                return this.AutoDetectMapping(XDocument.Load(xmlNodeReader));
            }
        }

        /// <summary>
        /// Automatics the detect mapping.
        /// </summary>
        /// <param name="xDoc">The x document.</param>
        /// <returns></returns>
        private XPathMappingList AutoDetectMapping(XDocument xDoc)
        {
            var xPathMappingList = new XPathMappingList();
            if (xDoc == null)
            {
                return xPathMappingList;
            }

            var nsMgr = this.CreateXmlNamespaceManager(xDoc);

            var xpaths = XPathUtil.GetAllXPaths(xDoc, includeElementPaths: true, includeAttributePaths: true, nsMgr: nsMgr);

            foreach (var xpath in xpaths)
            {
                var xPathMapping = new XPathMapping()
                {
                    Column = this.BuildColumnNameFromXpath(xpath),
                    XPath = xpath
                };
                xPathMappingList.Add(xPathMapping);
            }


            return xPathMappingList;
        }

        /// <summary>
        /// Creates the XML namespace manager.
        /// </summary>
        /// <param name="xDoc">The x document.</param>
        /// <returns></returns>
        private XmlNamespaceManager CreateXmlNamespaceManager(XDocument xDoc)
        {
            if (xDoc == null)
            {
                return null;
            }

            var reader = xDoc.CreateReader();
            var root = xDoc.Root;
            var namespaceManager = new XmlNamespaceManager(reader.NameTable);
            var nsMgr = new XmlNamespaceManager(reader.NameTable);
            foreach (var xmlNameSpace in this.XmlNameSpaces)
            {
                nsMgr.AddNamespace(xmlNameSpace.Prefix, xmlNameSpace.NameSpace);
            }

            return nsMgr;
        }

        /// <summary>
        /// Creates the XML namespace manager.
        /// </summary>
        /// <param name="xDoc">The x document.</param>
        /// <returns></returns>
        private XmlNamespaceManager CreateXmlNamespaceManager(XmlDocument xDoc)
        {
            if (xDoc == null)
            {
                return null;
            }

            var nsMgr = new XmlNamespaceManager(xDoc.NameTable);
            foreach (var xmlNameSpace in this.XmlNameSpaces)
            {
                nsMgr.AddNamespace(xmlNameSpace.Prefix, xmlNameSpace.NameSpace);
            }
            return nsMgr;
        }

        /// <summary>
        /// Builds the column name from xpath.
        /// </summary>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        private string BuildColumnNameFromXpath(string xpath)
        {
            if (string.IsNullOrEmpty(xpath))
            {
                return string.Empty;
            }

            var columnName = xpath.Replace("/", "_")
                                  .Replace("@", "$")
                                  .TrimStart('_');

            // remove Namespaceprefix
            var index = columnName.IndexOf(":", StringComparison.Ordinal);
            if (index > 0)
            {
                columnName = columnName.Substring(index + 1);
            }

            return columnName;
        }
    }
}