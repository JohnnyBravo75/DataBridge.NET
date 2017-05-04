using System.Text;
using System.Xml;

namespace DataBridge.Extensions
{
    public static class XmlNodeExtensions
    {        /// <summary>
             /// Gets the concatenated values of the node and
             /// all its children.
             /// </summary>
             /// <param name="node">The node.</param>
             /// <param name="separator">The separator.</param>
             /// <returns></returns>
        public static string GetInnerText(this XmlNode node, string separator = "")
        {
            XmlNode firstChild = node.FirstChild;
            if (firstChild == null)
            {
                return string.Empty;
            }
            if (firstChild.NextSibling == null)
            {
                switch (firstChild.NodeType)
                {
                    case XmlNodeType.Text:
                    case XmlNodeType.CDATA:
                    case XmlNodeType.Whitespace:
                    case XmlNodeType.SignificantWhitespace:
                        return firstChild.Value;
                }
            }

            var text = new StringBuilder();
            AppendChildText(node, text, separator);
            return text.ToString();
        }

        private static void AppendChildText(XmlNode node, StringBuilder text, string separator)
        {
            for (XmlNode child = node.FirstChild; child != null; child = child.NextSibling)
            {
                if (child.FirstChild == null)
                {
                    if (child.NodeType == XmlNodeType.Text ||
                        child.NodeType == XmlNodeType.CDATA ||
                        child.NodeType == XmlNodeType.Whitespace ||
                        child.NodeType == XmlNodeType.SignificantWhitespace)
                    {
                        text.Append(separator);
                        text.Append(child.InnerText);
                    }
                }
                else
                {
                    AppendChildText(child, text, separator);
                }
            }
        }
    }
}