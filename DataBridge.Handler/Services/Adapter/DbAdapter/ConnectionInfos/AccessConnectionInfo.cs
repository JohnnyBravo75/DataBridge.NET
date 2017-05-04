using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Common.Helper;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class AccessConnectionInfo : DbConnectionInfoBase
    {
        private string fileName = "";
        private string password = "";

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;"
                                        + "Data Source=" + this.FileName + ";";

                if (!string.IsNullOrEmpty(this.Password))
                {
                    connectionString += "Persist Security Info=true;";
                    connectionString += "Jet OLEDB:Database Password=" + this.DecryptedPassword + ";";
                }
                return connectionString;
            }
        }

        [XmlAttribute]
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
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

        [XmlIgnore]
        public override Dictionary<string, string> DataTypeMappings
        {
            get
            {
                return new Dictionary<string, string>()
                        {
                            { "System.String", "TEXT" },
                            { "System.DateTime", "DATE" },
                            { "System.Single", "FLOAT" },
                            { "System.Int32", "INTEGER" }
                        };
            }
        }
    }
}