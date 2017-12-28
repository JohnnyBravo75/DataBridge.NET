using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using DataBridge.Common.Helper;
using DataBridge.ConnectionInfos;
using DataBridge.Formatters;
using DataBridge.Handler.Services.Adapter;
using DataBridge.Helper;
using DataBridge.Models;

namespace DataBridge.Common.Services.Adapter
{
    public class XmlAdapter : DataAdapterBase
    {
        private FormatterBase readFormatter = new XPathToDataTableFormatter();
        private FormatterBase writeFormatter = new DataTableToXPathFormatter();

        private List<XmlNameSpace> xmlNameSpaces = new List<XmlNameSpace>();
        private string xPath;
        private FileConnectionInfoBase connectionInfo;
        private bool isMyDataStream = false;

        public XmlAdapter()
        {
            this.ConnectionInfo = new FlatFileConnectionInfo();
        }

        public XmlAdapter(string fileName, string xPath = null) : this()
        {
            this.FileName = fileName;
            this.XPath = xPath;
        }

        public XmlAdapter(Stream dataStream, string xPath = null) : this()
        {
            this.DataStream = dataStream;
            this.XPath = xPath;
        }

        public XmlAdapter(XDocument xDoc)
        {
            this.DataStream = new MemoryStream();
            xDoc.Save(this.DataStream);
            this.DataStream.Flush();
            this.DataStream.Position = 0;
            this.isMyDataStream = true;
        }

        public XmlAdapter(XmlDocument xmlDoc)
        {
            this.DataStream = new MemoryStream();
            xmlDoc.Save(this.DataStream);
            this.DataStream.Flush();
            this.DataStream.Position = 0;
            this.isMyDataStream = true;
        }

        [XmlElement]
        public List<XmlNameSpace> XmlNameSpaces
        {
            get { return this.xmlNameSpaces; }
            set { this.xmlNameSpaces = value; }
        }

        [XmlElement]
        public FormatterBase ReadFormatter
        {
            get { return this.readFormatter; }
            set { this.readFormatter = value; }
        }

        [XmlElement]
        public FormatterBase WriteFormatter
        {
            get { return this.writeFormatter; }
            set { this.writeFormatter = value; }
        }

        [XmlElement]
        public FileConnectionInfoBase ConnectionInfo
        {
            get { return this.connectionInfo; }
            set { this.connectionInfo = value; }
        }

        [XmlAttribute]
        public string FileName
        {
            get
            {
                if (!(this.ConnectionInfo is FlatFileConnectionInfo))
                {
                    return string.Empty;
                }
                return (this.ConnectionInfo as FlatFileConnectionInfo).FileName;
            }
            set { (this.ConnectionInfo as FlatFileConnectionInfo).FileName = value; }
        }

        [XmlAttribute]
        public string XPath
        {
            get { return this.xPath; }
            set { this.xPath = value; }
        }

        [XmlIgnore]
        public Stream DataStream { get; set; }

        [XmlAttribute]
        public bool AutoExtractNamespaces { get; set; }

        public override IList<DataColumn> GetAvailableColumns()
        {
            // not supported at the moment, table (xpath) is needed to look for the elements/attributes
            return new List<DataColumn>();
        }

        public override void Dispose()
        {
            if (this.DataStream != null)
            {
                // only destroy stream when I´m the owner/creator
                if (this.isMyDataStream)
                {
                    this.DataStream.Close();
                    this.DataStream.Dispose();
                }

                this.DataStream = null;
            }
        }

        /// <summary>
        /// Gets the available tables (xpaths) in the xml.
        /// All possible Xpaths are constructed.
        /// e.g. /books
        ///      /books/book
        ///      /books/book/pages
        /// </summary>
        /// <returns></returns>
        public override IList<string> GetAvailableTables()
        {
            this.ValidateAndThrow();

            IList<string> userTableList = new List<string>();

            XmlTextReader reader = null;
            if (!string.IsNullOrEmpty(this.FileName))
            {
                reader = new XmlTextReader(this.FileName);
            }
            else if (this.DataStream != null)
            {
                reader = new XmlTextReader(this.DataStream);
            }

            if (reader == null)
            {
                return userTableList;
            }

            var elementList = new List<string>();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.Element)
                {
                    // add opening element to the path list
                    elementList.Add(reader.Name);

                    // build the current path e.g. /main/result/address/street
                    var currentPathBuilder = new StringBuilder();
                    foreach (var element in elementList)
                    {
                        currentPathBuilder.Append("/").Append(element);
                    }
                    var currentPath = currentPathBuilder.ToString();

                    // don´t add doublets, ensure paths are distinct
                    if (!userTableList.Contains(currentPath))
                    {
                        userTableList.Add(currentPath);
                    }
                }

                // EndElement=</element>
                // EmptyElement = the end of an empty open/closing element with no value <element/>
                if (reader.NodeType == XmlNodeType.EndElement || reader.IsEmptyElement)
                {
                    // when end element, remove from the path list
                    var lastElement = elementList.LastOrDefault();
                    if (lastElement == null || lastElement == reader.Name)
                    {
                        elementList.RemoveAt(elementList.Count - 1);
                    }
                }
            }

            reader.Close();
            reader.Dispose();

            return userTableList;
        }

        public override int GetCount()
        {
            int count = 0;

            var xPathIterator = this.CreateXPathIterator();
            if (xPathIterator == null)
            {
                return count;
            }

            // loop through all paths and count them
            while (xPathIterator.MoveNext())
            {
                count++;
            }

            return count;
        }

        public override IEnumerable<DataTable> ReadData(int? blockSize = null)
        {
            this.ValidateAndThrow();

            foreach (object dataObj in this.ReadDataObjects<object>(blockSize))
            {
                if (dataObj is DataSet)
                {
                    foreach (DataTable table in (dataObj as DataSet).Tables)
                    {
                        yield return table;
                    }
                }
                else if (dataObj is DataTable)
                {
                    yield return dataObj as DataTable;
                }
                else
                {
                    yield return new DataTable();
                }
            }
        }

        public IEnumerable<TObj> ReadDataObjects<TObj>(int? blockSize = null) where TObj : class
        {
            DataSet dataSet = new DataSet();
            TObj result = default(TObj);
            int readedRows = 0;

            var xPathIterator = this.CreateXPathIterator();
            if (xPathIterator == null)
            {
                yield return result;
            }

            // when formatter supports namespaces, and has no, add to them
            if (this.ReadFormatter is IHasXmlNameSpaces && (this.ReadFormatter as IHasXmlNameSpaces).XmlNameSpaces.Count == 0)
            {
                (this.ReadFormatter as IHasXmlNameSpaces).XmlNameSpaces = this.XmlNameSpaces;
            }

            while (xPathIterator.MoveNext())
            {
                string xml = xPathIterator.Current.OuterXml;

                if (readedRows == 0)
                {
                    dataSet = new DataSet();
                }

                object tmpResult = this.ReadFormatter.Format(xml, dataSet);

                result = tmpResult as TObj;
                this.ReadConverter.ExecuteConverters(result);

                readedRows++;

                if (blockSize.HasValue && (blockSize > 0 && readedRows >= blockSize))
                {
                    readedRows = 0;
                    yield return result;
                }
            }

            if (readedRows > 0)
            {
                yield return result;
            }
        }

        public override bool WriteData(IEnumerable<DataTable> tables, bool deleteBefore = false)
        {
            this.ValidateAndThrow();

            var xmlDoc = new XmlDocument();
            var namespaceMgr = new XmlNamespaceManager(xmlDoc.NameTable);

            if (!string.IsNullOrEmpty(this.FileName))
            {
                DirectoryUtil.CreateDirectoryIfNotExists(Path.GetDirectoryName(this.FileName));

                if (deleteBefore)
                {
                    FileUtil.DeleteFileIfExists(this.FileName);
                }

                if (this.IsNewFile(this.FileName))
                {
                    // add Declaration
                    XmlNode docNode = xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null);
                    xmlDoc.AppendChild(docNode);

                    // create base Path
                    XPathUtil.CreateXPath(xmlDoc, this.XPath);
                }
                else
                {
                    xmlDoc.Load(this.FileName);
                }
            }

            foreach (DataTable table in tables)
            {
                var xmlLines = this.WriteFormatter.Format(table, null) as IEnumerable<string>;

                int writtenRows = 0;
                int rowIdx = 0;

                if (xmlLines != null)
                {
                    foreach (var xmlLine in xmlLines)
                    {
                        var lastNode = xmlDoc.SelectSingleNode(this.XPath + "[last()]", namespaceMgr);

                        if (lastNode != null)
                        {
                            // Append xml to the last node
                            var xmlDocFragment = xmlDoc.CreateDocumentFragment();
                            xmlDocFragment.InnerXml = xmlLine;
                            lastNode.AppendChild(xmlDocFragment);

                            writtenRows++;
                        }

                        rowIdx++;
                    }
                }
            }

            var settings = new XmlWriterSettings { Indent = true };
            XmlWriter writer = null;

            if (!string.IsNullOrEmpty(this.FileName))
            {
                writer = XmlWriter.Create(this.FileName, settings);
            }
            else if (this.DataStream != null)
            {
                writer = XmlWriter.Create(this.DataStream, settings);
            }
            else
            {
                return false;
            }

            xmlDoc.Save(writer);

            writer.Close();
            writer.Dispose();

            return true;
        }

        private XPathNodeIterator CreateXPathIterator()
        {
            string xPath = this.XPath;

            // create xpath navigator
            XPathNavigator xPathNavigator = null;

            if (!string.IsNullOrEmpty(this.FileName))
            {
                xPathNavigator = new XPathDocument(this.FileName).CreateNavigator();
            }
            else if (this.DataStream != null)
            {
                xPathNavigator = new XPathDocument(this.DataStream).CreateNavigator();
            }
            else
            {
                return null;
            }

            var nsMgr = new XmlNamespaceManager(new NameTable());

            // extract and add namespaces
            if (this.AutoExtractNamespaces)
            {
                xPathNavigator.MoveToFollowing(XPathNodeType.Element);
                var extractedNamespaces = xPathNavigator.GetNamespacesInScope(XmlNamespaceScope.All);
                if (extractedNamespaces != null)
                {
                    foreach (var xmlNameSpace in extractedNamespaces)
                    {
                        string prefix = xmlNameSpace.Key;
                        // empty default namespace does not work (Xpath 1.0!), so generate default prefix "ns"
                        if (string.IsNullOrEmpty(prefix))
                        {
                            nsMgr.AddNamespace(prefix, xmlNameSpace.Value);
                            prefix = "ns";
                        }

                        nsMgr.AddNamespace(prefix, xmlNameSpace.Value);
                    }
                }
            }

            // add defined namespaces
            if (this.XmlNameSpaces != null)
            {
                foreach (var xmlNameSpace in this.XmlNameSpaces)
                {
                    nsMgr.AddNamespace(xmlNameSpace.Prefix, xmlNameSpace.NameSpace);
                }
            }

            if (!string.IsNullOrEmpty(nsMgr.DefaultNamespace))

            {
                xPath = this.AddNamespacePrefixToXPath(xPath, "ns");
            }

            // compile and create iterator
            var xPathExpr = xPathNavigator.Compile(xPath);
            xPathExpr.SetContext(nsMgr);

            var xPathIterator = xPathNavigator.Select(xPathExpr);
            return xPathIterator;
        }

        private bool IsNewFile(string fileName)
        {
            // new File?
            if (!string.IsNullOrEmpty(fileName) && !File.Exists(fileName))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Adds a prefix to each part of the xpath
        ///  e.g. "/books/book/pages" -> "/ns:books/ns:book/ns:pages"
        /// </summary>
        /// <param name="xPath">The xpath.</param>
        /// <param name="namespacePrefix">The namespace prefix.</param>
        /// <returns></returns>
        private string AddNamespacePrefixToXPath(string xPath, string namespacePrefix)
        {
            var pieces = xPath.Split(new string[] { @"/" }, StringSplitOptions.RemoveEmptyEntries);

            string prefixedXPath = string.Empty;
            foreach (var piece in pieces)
            {
                if (!string.IsNullOrWhiteSpace(piece))
                {
                    string prefixedPiece = piece;
                    if (!prefixedPiece.StartsWith(namespacePrefix))
                    {
                        prefixedPiece = namespacePrefix + ":" + prefixedPiece;
                    }

                    prefixedXPath += @"/" + prefixedPiece;
                }
            }
            return prefixedXPath;
        }

        private void ExtractXPathMappingFromAttributes<TObj>(XPathMappingList xPathMappings) where TObj : class
        {
            var dataFieldsProps = typeof(TObj).GetProperties()
                                .Where(p => Attribute.IsDefined(p, typeof(DataFieldAttribute)))
                                .ToList();

            foreach (var dataFieldProp in dataFieldsProps)
            {
                var attr = dataFieldProp.GetCustomAttribute<DataFieldAttribute>(false);
                var xPathMapping = new XPathMapping()
                {
                    XPath = attr.XPath,
                    Column = !string.IsNullOrEmpty(attr.Name)
                                        ? attr.Name
                                        : dataFieldProp.Name
                };

                xPathMappings.Add(xPathMapping);
            }
        }

        public IList<string> Validate()
        {
            var messages = new List<string>();

            if (string.IsNullOrEmpty(this.FileName) && this.DataStream == null)
            {
                messages.Add("FileName or DataStream must not be null");
            }

            return messages;
        }

        private void ValidateAndThrow()
        {
            var messages = this.Validate();
            if (messages.Any())
            {
                throw new Exception(messages.First());
            }
        }
    }
}