using System.Xml.Serialization;
using DataBridge.Common.Helper;

namespace DataBridge.Runtime
{
    public class SystemInfo
    {
        private string systemDomain = "";
        private string systemUser = "";
        private string host = "";
        private string systemPassword = "";

        [XmlAttribute]
        public string SystemDomain
        {
            get { return this.systemDomain; }
            set { this.systemDomain = value; }
        }

        [XmlAttribute]
        public string SystemUser
        {
            get { return this.systemUser; }
            set { this.systemUser = value; }
        }

        [XmlAttribute]
        public string Host
        {
            get { return this.host; }
            set { this.host = value; }
        }

        [XmlAttribute]
        public string SystemPassword
        {
            get { return this.systemPassword; }
            set { this.systemPassword = EncryptionHelper.GetEncryptedString(value); }
        }
    }
}