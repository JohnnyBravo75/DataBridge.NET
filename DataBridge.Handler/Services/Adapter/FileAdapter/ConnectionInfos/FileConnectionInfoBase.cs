using System.Xml.Serialization;

namespace DataBridge.ConnectionInfos
{
    using System;

    [Serializable]
    public class FileConnectionInfoBase : ConnectionInfoBase
    {
        private string fileName = "";

        [XmlAttribute]
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }
    }
}