using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.DbConnectionInfos
{
    [Serializable]
    public class MySqlDbConnectionInfo : DbConnectionInfoBase
    {
        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                return "Provider=MySqlProv;"
                     + "Data Source=" + this.Server + ";"
                     + "User Id=" + this.UserName + ";"
                     + "Password=" + this.Password + ";";
            }
        }

        [XmlAttribute]
        public string Password { get; set; }

        [XmlAttribute]
        public string Server { get; set; }

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

                //    var mappings = new Dictionary<string, Type>()
                //{
                //    { "bigint", typeof(long) },
                //    { "binary", typeof(byte[]) },
                //    { "bit", typeof(bool) },
                //    { "blob", typeof(byte[]) },
                //    { "char", typeof(string) },
                //    { "date", typeof(DateTime) },
                //    { "datetime", typeof(DateTime) },
                //    { "decimal", typeof(decimal) },
                //    { "double", typeof(double) },
                //    { "enum", typeof(string) },
                //    { "float", typeof(float) },
                //    { "int", typeof(int) },
                //    { "longblob", typeof(byte[]) },
                //    { "longtext", typeof(string) },
                //    { "mediumblob", typeof(byte[]) },
                //    { "mediumint", typeof(int) },
                //    { "mediumtext", typeof(string) },
                //    { "set", typeof(string) },
                //    { "smallint", typeof(short) },
                //    { "text", typeof(string) },
                //    { "time", typeof(TimeSpan) },
                //    { "timestamp", typeof(DateTime) },
                //    { "tinyblob", typeof(byte[]) },
                //    { "tinyint", typeof(short) },
                //    { "tinytext", typeof(string) },
                //    { "varbinary", typeof(byte[]) },
                //    { "varchar", typeof(string) },
                //    { "year", typeof(short) }
                //};
            }
        }
    }
}