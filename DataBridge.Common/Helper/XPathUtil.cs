using System.Collections.Generic;
using System.Xml.Linq;
using DataBridge.Extensions;

namespace DataBridge.Helper
{
    using System;
    using System.Linq;
    using System.Xml;

    public static class XPathUtil
    {
        /// <summary>
        /// Splits a XPath.
        /// </summary>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        public static string[] SplitXPath(string xpath)
        {
            return xpath.Trim('/').Split('/');
        }

        /// <summary>
        /// Appends a nodename at the end of the XPath.
        /// e.g "/Documents" + "Symbols" -> "/Documents/Symbols"
        /// </summary>
        /// <param name="xPath">The x path.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        public static string AppendToXPath(string xPath, string nodeName)
        {
            if (!xPath.EndsWith("/"))
            {
                xPath += "/";
            }

            xPath += nodeName;

            return xPath;
        }

        /// <summary>
        /// Builds a XPath in a xml document.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        public static XmlNode MakeXPath(XmlDocument doc, string xpath)
        {
            return MakeXPath(doc, doc as XmlNode, xpath);
        }

        /// <summary>
        /// Builds a XPath in a xml document.
        /// </summary>
        /// <param name="doc">The doc.</param>
        /// <param name="parent">The parent.</param>
        /// <param name="xpath">The xpath.</param>
        /// <returns></returns>
        public static XmlNode MakeXPath(XmlDocument doc, XmlNode parent, string xpath)
        {
            // grab the next node name in the xpath; or return parent if empty
            string[] partsOfXPath = xpath.Trim('/').Split('/');
            string nextNodeInXPath = partsOfXPath.First();
            if (string.IsNullOrEmpty(nextNodeInXPath))
            {
                return parent;
            }

            // get or create the node from the name
            XmlNode node = parent.SelectSingleNode(nextNodeInXPath);
            if (node == null)
            {
                node = parent.AppendChild(doc.CreateElement(nextNodeInXPath));
            }

            // rejoin the remainder of the array as an xpath expression and recurse
            string rest = string.Join("/", partsOfXPath.Skip(1).ToArray());
            return MakeXPath(doc, node, rest);
        }

        public static void SplitOnce(this string value, string separator, out string part1, out string part2)
        {
            if (value != null)
            {
                int idx = value.IndexOf(separator, StringComparison.Ordinal);
                if (idx >= 0)
                {
                    part1 = value.Substring(0, idx);
                    part2 = value.Substring(idx + separator.Length);
                }
                else
                {
                    part1 = value;
                    part2 = null;
                }
            }
            else
            {
                part1 = "";
                part2 = null;
            }
        }

        public static XmlNode CreateXPath(XmlDocument doc, string xpath)
        {
            if (doc == null)
            {
                throw new NullReferenceException("doc");
            }

            XmlNode node = doc;
            foreach (string xpathPart in xpath.Substring(1).Split('/'))
            {
                XmlNodeList nodes = node.SelectNodes(xpathPart);
                if (nodes.Count > 1)
                {
                    throw new ApplicationException("Xpath '" + xpath + "' was not found multiple times!");
                }
                else if (nodes.Count == 1)
                {
                    node = nodes[0];
                    continue;
                }

                if (xpathPart.StartsWith("@"))
                {
                    // Attribute
                    var attributeNode = doc.CreateAttribute(xpathPart.Substring(1));
                    node.Attributes.Append(attributeNode);
                    node = attributeNode;
                }
                else
                {
                    string elementName, attrib = null;
                    if (xpathPart.Contains("["))
                    {
                        xpathPart.SplitOnce("[", out elementName, out attrib);
                        if (!attrib.EndsWith("]"))
                        {
                            throw new ApplicationException("Unsupported XPath (missing ]): " + xpathPart);
                        }
                        attrib = attrib.Substring(0, attrib.Length - 1);
                    }
                    else
                    {
                        elementName = xpathPart;
                    }

                    XmlNode next = doc.CreateElement(elementName);
                    node.AppendChild(next);
                    node = next;

                    if (attrib != null)
                    {
                        if (!attrib.StartsWith("@"))
                        {
                            attrib = " Id='" + attrib + "'";
                        }
                        string name, value;
                        attrib.Substring(1).SplitOnce("='", out name, out value);
                        if (string.IsNullOrEmpty(value) || !value.EndsWith("'"))
                        {
                            throw new ApplicationException("Unsupported XPath attrib: " + xpathPart);
                        }
                        value = value.Substring(0, value.Length - 1);
                        var attributeNode = doc.CreateAttribute(name);
                        attributeNode.Value = value;
                        node.Attributes.Append(attributeNode);
                    }
                }
            }
            return node;
        }

        public static XmlNode CreateXPath(XmlDocument doc, string xpath, string value)
        {
            XmlNode node = null;
            if (doc == null)
            {
                throw new ArgumentNullException("doc");
            }

            if (string.IsNullOrEmpty(xpath))
            {
                throw new ArgumentNullException("xpath");
            }

            XmlNodeList nodes = doc.SelectNodes(xpath);
            if (nodes.Count > 1)
            {
                throw new Exception("Xpath '" + xpath + "' was not found multiple times!");
            }
            else if (nodes.Count == 0)
            {
                node = CreateXPath(doc, xpath);
                if (!string.IsNullOrEmpty(value))
                {
                    node.InnerText = value;
                }
            }
            else
            {
                node = nodes[0];
                if (!string.IsNullOrEmpty(value))
                {
                    node.InnerText = value;
                }
            }

            return node;
        }

        public static IEnumerable<string> GetAllXPaths(XDocument xDoc, bool includeElementPaths = true, bool includeAttributePaths = false, XmlNamespaceManager nsMgr = null)
        {
            var xpaths = new List<string>();

            if (xDoc == null)
            {
                return xpaths;
            }

            var nodes = xDoc.Descendants()
                            .Where(e => (!e.HasElements) || (e.HasAttributes && includeAttributePaths));

            foreach (var node in nodes)
            {
                var xpath = node.GetPath(nsMgr);

                if (!node.Descendants().Any())
                {
                    if (includeElementPaths && !xpaths.Contains(xpath))
                    {
                        xpaths.Add(xpath);
                    }
                }
                if (includeAttributePaths && node.HasAttributes)
                {
                    foreach (var attr in node.Attributes())
                    {
                        if (!attr.IsNamespaceDeclaration)
                        {
                            var attrPath = xpath + "/@" + attr.Name;
                            if (!xpaths.Contains(attrPath))
                            {
                                xpaths.Add(attrPath);
                            }
                        }
                    }
                }
            }

            return xpaths;
        }
    }
}