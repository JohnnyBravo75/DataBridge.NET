using System;
using System.Xml.Serialization;
using DataBridge.Common.Helper;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class SmtpConnectionInfo : ConnectionInfoBase
    {
        private string userName = "";
        private string password = "";
        private string server = "";
        private int port = 25;
        private bool enableSecure = false;

        [XmlAttribute]
        public string SmtpServer
        {
            get { return this.server; }
            set { this.server = value; }
        }

        [XmlAttribute]
        public int SmtpPort
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

        [XmlAttribute]
        public bool EnableSecure
        {
            get { return this.enableSecure; }
            set
            {
                this.enableSecure = value;

                // When default port, set to standard smtp port with ssl
                if (this.enableSecure && this.port == 25)
                {
                    this.port = 587;
                }
            }
        }
    }
}