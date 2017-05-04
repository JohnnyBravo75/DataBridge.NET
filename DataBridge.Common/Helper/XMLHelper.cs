using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Schema;
using System.Xml.Xsl;

namespace DataBridge.Helper
{
    using System.Linq;
    using System.Security;
    using System.Text.RegularExpressions;
    using System.Xml;
    using System.Xml.Linq;

    public static class XmlHelper
    {
        //public static string XmlEscape(string unescaped)
        //{
        //    XmlDocument xmlDoc = new XmlDocument();
        //    XmlNode node = xmlDoc.CreateElement("root");
        //    node.InnerText = unescaped;
        //    return node.InnerXml;
        //}

        //public static string XmlUnescape(string escaped)
        //{
        //    XmlDocument xmlDoc = new XmlDocument();
        //    XmlNode node = xmlDoc.CreateElement("root");
        //    node.InnerXml = escaped;
        //    return node.InnerText;
        //}

        //    public static string XmlEncode(this string value)
        //    {
        //        var output = new StringBuilder();
        //        var text = new XText(value);

        //        using (var writer = XmlWriter.Create(output, new XmlWriterSettings { ConformanceLevel = ConformanceLevel.Fragment }))
        //        {
        //            text.WriteTo(writer);
        //            writer.Flush();
        //            return output.ToString();
        //        }
        //    }

        //    public static string XmlDecode(this string value)
        //    {
        //        if (value.Length == 0)
        //            return value;

        //        var output = new StringBuilder();
        //        var reader = XmlReader.Create(new StringReader(value), new XmlReaderSettings { ConformanceLevel = ConformanceLevel.Fragment });
        //        reader.MoveToContent();
        //        var text = (XText)XText.ReadFrom(reader);

        //        return text.Value;
        //    }

        public static string EscapeXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return xml;
            }

            return !SecurityElement.IsValidText(xml)
                   ? SecurityElement.Escape(xml) : xml;
        }

        public static string UnescapeXml(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return xml;
            }

            xml = xml.Replace("&apos;", "'");
            xml = xml.Replace("&quot;", "\"");
            xml = xml.Replace("&gt;", ">");
            xml = xml.Replace("&lt;", "<");
            xml = xml.Replace("&amp;", "&");

            return xml;
        }

        /// <summary>
        /// Strips/removes the XML name spaces.
        /// </summary>
        /// <param name="xDoc">The x xmlDoc.</param>
        /// <returns>a XmlDocument without namespace declarations</returns>
        public static XmlDocument StripXmlNameSpaces(XmlDocument xDoc)
        {
            string xml = xDoc.InnerXml;

            if (string.IsNullOrEmpty(xml))
            {
                return xDoc;
            }

            string strippedXml = StripXmlNameSpaces(xml);
            var strippedXDoc = new XmlDocument();
            strippedXDoc.LoadXml(strippedXml);
            return strippedXDoc;
        }

        /// <summary>
        /// Strips/removes all XML namespaces.
        /// </summary>
        /// <param name="xml">The XML.</param>
        /// <returns></returns>
        public static string StripXmlNameSpaces(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return xml;
            }

            string strXMLPattern = @"xmlns(:\w+)?=\""([^\""]*)\""";
            string strippedXml = System.Text.RegularExpressions.Regex.Replace(xml, strXMLPattern, "", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            return strippedXml;
        }

        public static XmlDocument RemoveXmlns(XmlDocument xmlDoc)
        {
            XDocument xDoc;
            using (var nodeReader = new XmlNodeReader(xmlDoc))
            {
                xDoc = XDocument.Load(nodeReader);
            }

            return RemoveXmlns(xDoc);
        }

        public static XmlDocument RemoveXmlns(string xml)
        {
            XDocument xDoc = XDocument.Parse(xml);
            return RemoveXmlns(xDoc);
        }

        public static XmlDocument RemoveXmlns(XDocument xDoc)
        {
            if (xDoc == null)
            {
                return null;
            }

            xDoc.Root.Attributes().Where(x => x.IsNamespaceDeclaration).Remove();
            xDoc.Root.Descendants().Attributes().Where(x => x.IsNamespaceDeclaration).Remove();

            foreach (var elem in xDoc.Descendants())
            {
                elem.Name = elem.Name.LocalName;
            }

            var xmlDocument = new XmlDocument();
            using (var xmlReader = xDoc.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }

            return xmlDocument;
        }

        public static string GetXPathToNode(XmlNode node)
        {
            if (node.NodeType == XmlNodeType.Attribute)
            {
                // attributes have an OwnerElement, not a ParentNode; also they have
                // to be matched by name, not found by position
                return String.Format(
                    "{0}/@{1}",
                    GetXPathToNode(((XmlAttribute)node).OwnerElement),
                    node.Name
                    );
            }
            if (node.ParentNode == null)
            {
                // the only node with no parent is the root node, which has no path
                return "";
            }
            // the path to a node is the path to its parent, plus "/node()[n]", where
            // n is its position among its siblings.
            return String.Format(
                "{0}/node()[{1}]",
                GetXPathToNode(node.ParentNode),
                GetNodePosition(node)
                );
        }

        private static int GetNodePosition(XmlNode node)
        {
            if (node == null)
            {
                return -1;
            }

            for (int i = 0; i < node.ParentNode.ChildNodes.Count; i++)
            {
                if (node.ParentNode.ChildNodes[i] == node)
                {
                    // in XPath the positions is not starting at 0, like normal
                    return i + 1;
                }
            }

            throw new InvalidOperationException("Child node not found in its parent'xml ChildNodes property.");
        }

        public static XmlNamespaceManager CreateNamespaceManagerForDocument(XmlDocument xmlDoc, string defaultNsPrefix = "ns")
        {
            if (xmlDoc == null)
            {
                return null;
            }

            var nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);

            // Find and remember each xmlns attribute, assigning the 'ns' prefix to default namespaces.
            var nameSpaces = new Dictionary<string, string>();

            foreach (Match match in new Regex(@"xmlns:?(.*?)=([\x22\x27])(.+?)\2").Matches(xmlDoc.OuterXml))
            {
                var prefix = match.Groups[1].Value == ""
                                ? defaultNsPrefix
                                : match.Groups[1].Value;

                nameSpaces[match.Groups[1].Value + ":" + match.Groups[3].Value] = prefix;
            }

            // Go through the dictionary, and number non-unique prefixes before adding them to the namespace manager.
            var prefixCounts = new Dictionary<string, int>();

            foreach (var namespaceItem in nameSpaces)
            {
                var prefix = namespaceItem.Value;
                if (prefix != null)
                {
                    var namespaceURI = namespaceItem.Key.Split(':')[1];
                    if (prefix != null && prefixCounts.ContainsKey(prefix))
                    {
                        prefixCounts[prefix]++;
                    }
                    else
                    {
                        prefixCounts[prefix] = 0;
                    }

                    nsMgr.AddNamespace(prefix + prefixCounts[prefix].ToString("#;;"), namespaceURI);
                }
            }

            return nsMgr;
        }

        /// <summary>
        /// Creates an XmlNamespaceManager based on a source XmlDocument'xml name table, and prepopulates its namespaces with any 'xmlns:' attributes of the root node.
        /// </summary>
        /// <param name="xmlDoc">The source XML xmlDoc to create the XmlNamespaceManager for.</param>
        /// <returns>The created XmlNamespaceManager.</returns>
        public static XmlNamespaceManager CreateNsMgrForDocument(XmlDocument xmlDoc)
        {
            if (xmlDoc == null)
            {
                return null;
            }

            var nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);

            foreach (XmlAttribute attr in xmlDoc.SelectSingleNode("/*").Attributes)
            {
                if (attr.Prefix == "xmlns")
                {
                    nsMgr.AddNamespace(attr.LocalName, attr.Value);
                }
            }

            return nsMgr;
        }

        public static string XslTransform(string xml, string xslPath)
        {
            var settings = new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document };

            using (var stringReader = new StringReader(xml))
            {
                var xDoc = XmlReader.Create(stringReader, settings);

                var xslTransfrom = new XslCompiledTransform();
                xslTransfrom.Load(xslPath);

                var result = new StringBuilder();
                using (var stringWriter = new StringWriter(result))
                {
                    xslTransfrom.Transform(xDoc, null, stringWriter);
                }

                return result.ToString();
            }
        }

        public static string BuildXmlSchema(string xmlFileName)
        {
            string xmlSchema = "";

            using (XmlReader reader = XmlReader.Create(xmlFileName))
            {
                var schemaSet = new XmlSchemaSet();
                var schemaInference = new XmlSchemaInference();
                schemaSet = schemaInference.InferSchema(reader);

                foreach (XmlSchema schema in schemaSet.Schemas())
                {
                    using (var stringWriter = new StringWriter())
                    {
                        using (var writer = XmlWriter.Create(stringWriter))
                        {
                            schema.Write(writer);
                        }

                        xmlSchema = stringWriter.ToString();
                    }
                }

                return xmlSchema;
            }
        }
    }
}