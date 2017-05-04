using System.Collections.Generic;

namespace DataBridge.Extensions
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Xml;
    using System.Xml.Linq;

    /// <summary>
    /// XExtensions für Xml Linq
    /// </summary>
    public static class XExtensions
    {
        /// <summary>
        /// Writes the XML to string.
        /// </summary>
        /// <param name="xdoc">The xdoc.</param>
        /// <returns></returns>
        public static string WriteXmlToString(this XDocument xdoc)
        {
            string result;

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.GetEncoding(xdoc.Declaration.Encoding);
            settings.ConformanceLevel = ConformanceLevel.Document;
            settings.Indent = true;

            using (Stream ms = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(ms, settings))
                {
                    xdoc.Save(writer);
                    writer.Flush();

                    using (StreamReader sr = new StreamReader(ms, Encoding.GetEncoding(xdoc.Declaration.Encoding)))
                    {
                        ms.Seek(0, SeekOrigin.Begin);

                        result = sr.ReadToEnd();
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Strips the XML namespaces.
        /// </summary>
        /// <param name="xDoc">The XML.</param>
        /// <returns></returns>
        public static XDocument StripXmlNameSpaces(this XDocument xDoc)
        {
            string xml = xDoc.ToString();
            // const string strXMLPattern = @"xmlns(:\w+)?="".+?"""; // 29.04.2014 - tl - Scheint nicht richtig zu funktionieren
            const string strXmlPattern = @"xmlns(:\w+)?=\""([^\""]*)\""";
            string strippedXml = System.Text.RegularExpressions.Regex.Replace(xml, strXmlPattern, "", System.Text.RegularExpressions.RegexOptions.Multiline);
            var strippedXDoc = XDocument.Parse(strippedXml);
            return strippedXDoc;
        }

        /// <summary>
        /// Strips the XML name spaces.
        /// </summary>
        /// <param name="root">The root.</param>
        /// <returns></returns>
        public static XElement StripXmlNameSpaces(XElement root)
        {
            return new XElement(
                root.Name.LocalName,
                root.HasElements ? root.Elements().Select(el => StripXmlNameSpaces(el))
                                 : (object)root.Value
                               );
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default Value.</param>
        /// <returns></returns>
        public static string GetAttributeValue(this XElement element, string name, string defaultValue = "")
        {
            return element.GetAttributeValue<string>(name, defaultValue);
        }

        /// <summary>
        /// Gets the attribute value.
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="name">The name.</param>
        /// <param name="defaultValue">The default Value.</param>
        /// <returns></returns>
        public static T GetAttributeValue<T>(this XElement element, string name, T defaultValue = default(T))
        {
            XAttribute attribute = element.Attribute(XName.Get(name));

            if (attribute == null)
            {
                return defaultValue;
            }

            return ConvertExtensions.ConvertTo<T>(attribute.Value);
        }

        /// <summary>
        /// Gets the path to a XElement e.g. Common/Products/Product/Newspaper
        /// </summary>
        /// <param name="element">The element.</param>
        /// <param name="nsMgr">The XmlNamespaceManager.</param>
        public static string GetPath(this XElement element, XmlNamespaceManager nsMgr = null)
        {
            const string separator = "/";
            var nodes = new List<string>();
            var node = element;
            while (node != null)
            {
                string nodeName = node.Name.ToString();
                if (nsMgr != null)
                {
                    string prefix = nsMgr.LookupPrefix(node.Name.NamespaceName);
                    nodeName = (!string.IsNullOrEmpty(prefix)
                                    ? prefix + ":"
                                    : "")
                             + node.Name.LocalName;
                }

                nodes.Add(nodeName);
                node = node.Parent;
            }

            return separator + string.Join(separator, Enumerable.Reverse(nodes));
        }

        public static IEnumerable<XAttribute> Attributes(this IEnumerable<XElement> elements, XName name, bool ignoreNamespace)
        {
            if (ignoreNamespace)
                return elements.Attributes().Where(a => a.Name.LocalName == name.LocalName);
            else
                return elements.Attributes(name);
        }

        /// <summary>
        /// Get the absolute XPath to a given XElement
        /// (e.g. "/people/person[6]/name[1]/last[1]").
        /// </summary>
        /// <param name="element">
        /// The element to get the index of.
        /// </param>
        public static string AbsoluteXPath(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            Func<XElement, string> relativeXPath = e =>
            {
                int index = e.IndexPosition();
                string name = e.Name.LocalName;

                // If the element is the root, no index is required

                return (index == -1) ? "/" + name : string.Format
                (
                    "/{0}[{1}]",
                    name,
                    index.ToString()
                );
            };

            var ancestors = from e in element.Ancestors()
                            select relativeXPath(e);

            return string.Concat(ancestors.Reverse().ToArray()) + relativeXPath(element);
        }

        /// <summary>
        /// Get the index of the given XElement relative to its
        /// siblings with identical names. If the given element is
        /// the root, -1 is returned.
        /// </summary>
        /// <param name="element">
        /// The element to get the index of.
        /// </param>
        public static int IndexPosition(this XElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (element.Parent == null)
            {
                return -1;
            }

            int i = 1; // Indexes for nodes start at 1, not 0

            foreach (var sibling in element.Parent.Elements(element.Name))
            {
                if (sibling == element)
                {
                    return i;
                }

                i++;
            }

            throw new InvalidOperationException("element has been removed from its parent.");
        }

        /// <summary>
        /// Converts a XDocument to a XmlDocument.
        /// </summary>
        /// <param name="xDocument">The x document.</param>
        /// <returns></returns>
        public static XmlDocument ToXmlDocument(this XDocument xDocument)
        {
            var xmlDocument = new XmlDocument();
            xmlDocument.Load(xDocument.CreateReader());
            return xmlDocument;
        }

        public static XDocument ToXDocument(XmlDocument xmlDocument)
        {
            return XDocument.Parse(xmlDocument.OuterXml);
        }
    }
}