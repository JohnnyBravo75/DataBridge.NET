using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.DbConnectionInfos
{
    [Serializable]
    public class DBaseDbConnectionInfo : DbConnectionInfoBase
    {
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
                    connectionString += "Password=" + this.Password + ";";
                }

                return connectionString;
            }
        }

        [XmlAttribute]
        public string Password { get; set; }

        [XmlAttribute]
        public string Directory { get; set; }

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