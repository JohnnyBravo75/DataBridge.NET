using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Common.Helper;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class MySqlConnectionInfo : DbConnectionInfoBase
    {
        private string password = "";
        private string server = "";

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                return "Provider=MySqlProv;"
                     + "Data Source=" + this.Server + ";"
                     + "User Id=" + this.UserName + ";"
                     + "Password=" + this.DecryptedPassword + ";";
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
        public string Server
        {
            get { return this.server; }
            set { this.server = value; }
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