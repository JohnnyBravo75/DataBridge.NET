using System.Collections.Generic;

namespace DataBridge
{
    public interface IHasXmlNameSpaces
    {
        List<XmlNameSpace> XmlNameSpaces { get; set; }
    }
}