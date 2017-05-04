using System;
using System.Xml.Serialization;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class SqLiteConnectionInfo : DbConnectionInfoBase
    {
        private string database = "";

        public SqLiteConnectionInfo()
        {
            this.DbProvider = "System.Data.SQLite";
        }

        [XmlAttribute]
        public string Database
        {
            get { return this.database; }
            set { this.database = value; }
        }

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