using System.Collections.Generic;
using System.Xml;
using System.Xml.Linq;

namespace DataBridge.Extensions
{
    public static class XmlReaderExtensions
    {
        /// <summary>
        /// Returns a sequence of <see cref="XElement">XElements</see> corresponding to the currently
        /// positioned element and all following sibling elements which match the specified name.
        /// </summary>
        /// <param name="reader">The xml reader positioned at the desired hierarchy level.</param>
        /// <param name="elementName">An <see cref="XName"/> representing the name of the desired element.</param>
        /// <returns>A sequence of <see cref="XElement">XElements</see>.</returns>
        /// <remarks>At the end of the sequence, the reader will be positioned on the end tag of the parent element.</remarks>
        public static IEnumerable<XElement> ReadElements(this XmlReader reader, XName elementName)
        {
            if (reader.Name == elementName.LocalName && reader.NamespaceURI == elementName.NamespaceName)
                yield return (XElement)XNode.ReadFrom(reader);

            while (reader.ReadToNextSibling(elementName.LocalName, elementName.NamespaceName))
                yield return (XElement)XNode.ReadFrom(reader);
        }

        public static bool SkipToElement(this XmlReader xmlReader, string elementName)
        {
            if (!xmlReader.Read())
                return false;

            while (!xmlReader.EOF)
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == elementName)
                    return true;

                xmlReader.Skip();
            }

            return false;
        }

        public static IEnumerable<XElement> ReadElements(this XmlReader xmlReader, string elementName)
        {
            while (xmlReader.ReadToFollowing(elementName))
            {
                var element = (XElement)XNode.ReadFrom(xmlReader);
                yield return element;
            }
        }
    }
}