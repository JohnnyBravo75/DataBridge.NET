using System;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataConnectors.Adapter;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class FtpConnectionInfo : ConnectionInfoBase
    {
        private string userName = "";
        private string password = "";
        private string server = "";
        private int port = 21;

        [XmlAttribute]
        public string FtpServer
        {
            get { return this.server; }
            set { this.server = value; }
        }

        [XmlAttribute]
        public int FtpPort
        {
            get { return this.port; }
            set { this.port = value; }
        }

        [XmlAttribute]
        public string UserName
        {
            get { return this.userName; }
            set { this.userName = value; }
        }

        [XmlIgnore]
        public string DecryptedPassword
        {
            get
            {
                return EncryptionHelper.GetDecrptedString(this.Password);
            }
        }

        [XmlAttribute]
        public string Password
        {
            get { return this.password; }
            set { this.password = EncryptionHelper.GetEncryptedString(value); }
        }
    }
}