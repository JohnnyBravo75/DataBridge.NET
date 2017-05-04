using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class Access2007ConnectionInfo : DbConnectionInfoBase
    {
        private string fileName = "";

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                return "Provider=Microsoft.ACE.OLEDB.12.0;"
                     + "Data Source=" + this.FileName + ";"
                     + "Persist Security Info=False;";
            }
        }

        [XmlAttribute]
        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
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