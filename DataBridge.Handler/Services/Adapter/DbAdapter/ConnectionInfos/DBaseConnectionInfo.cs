using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Common.Helper;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class DBaseConnectionInfo : DbConnectionInfoBase
    {
        private string password = "";
        private string directory = "";

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                // for dBASE, just the path is needed, the file is a table
                string connectionString = "Provider=Microsoft.Jet.OLEDB.4.0;"
                         + "Data Source=" + this.Directory + ";"
                         + "Extended Properties=DBASE IV;";

                if (!string.IsNullOrEmpty(this.UserName))
                {
                    connectionString += "User Id=" + this.UserName + ";";
                    connectionString += "Password=" + this.DecryptedPassword + ";";
                }

                return connectionString;
            }
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
        public string Directory
        {
            get { return this.directory; }
            set { this.directory = value; }
        }

        [XmlIgnore]
        public override Dictionary<string, string> DataTypeMappings
        {
            get
            {
                return new Dictionary<string, string>()
                        {
                            { "System.String", "VARCHAR()" },
                            { "System.DateTime", "DATE" },
                            { "System.Single", "FLOAT" },
                            { "System.Int32", "INTEGER" }
                        };
            }
        }
    }
}