using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.DbConnectionInfos
{
    [Serializable]
    public class SQLiteDbConnectionInfo : DbConnectionInfoBase
    {
        public SQLiteDbConnectionInfo()
        {
            this.DbProvider = "System.Data.SQLite";
        }

        [XmlAttribute]
        public string Database { get; set; }

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                return "Data Source=" + this.Database + ";Version=3;";
            }
        }
    }
}