using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.DbConnectionInfos;
using Oracle.DataAccess.Client;

namespace DataBridge.Services
{
    [Serializable]
    public class OracleNativeDbConnectionInfo : DbConnectionInfoBase
    {
        public OracleNativeDbConnectionInfo()
        {
            this.DbProvider = "Oracle.DataAccess.Client";  // Does not work sometimes, therefor set the factory by hand
            this.DbProviderFactory = new OracleClientFactory();
        }

        private int port = 1521;

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                return "Data Source=(DESCRIPTION =(ADDRESS =(PROTOCOL=TCP)(HOST=" + this.Host + ")(PORT=" + this.Port + "))(CONNECT_DATA=(SID=" + this.Database + ")));pooling=true;enlist=false;statement cache size=50;min pool size=1;incr pool size=5;decr pool size=2;"
                        + "User Id=" + this.UserName + ";"
                        + "Password=" + this.Password + ";";
            }
        }

        [XmlAttribute]
        public string Password { get; set; }

        [XmlAttribute]
        public string Database { get; set; }

        [XmlAttribute]
        public string Host { get; set; }

        [XmlAttribute]
        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        [XmlIgnore]
        public override Dictionary<string, string> DataTypeMappings
        {
            get
            {
                return new Dictionary<string, string>()
                        {
                            { "System.String", "VARCHAR2()" },
                            { "System.DateTime", "DATE" },
                            { "System.Single", "NUMBER" },
                            { "System.Int32", "NUMBER" }
                        };
            }
        }
    }
}