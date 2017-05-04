using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Common.Helper;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class OracleConnectionInfo : DbConnectionInfoBase
    {
        private string password = "";
        private string database = "";

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                return "Provider=OraOLEDB.Oracle;"
                        + "Data Source=" + this.Database + ";"
                        + "User Id=" + this.UserName + ";"
                        + "Password=" + this.DecryptedPassword + ";"
                        + "OLEDB.NET=true;";
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
        public string Database
        {
            get { return this.database; }
            set { this.database = value; }
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

                //var mappings = new Dictionary<string, Type>()
                //{
                //    { "INTEGER", typeof(decimal) },
                //    { "LONG RAW", typeof(byte[]) },
                //    { "ROWID", typeof(string) },
                //    { "UNSIGNED INTEGER", typeof(decimal) },
                //    { "BINARY_DOUBLE", typeof(double) },
                //    { "BINARY_FLOAT", typeof(float) },
                //    { "BLOB", typeof(byte[]) },
                //    { "BFILE", typeof(byte[]) },
                //    { "CHAR", typeof(string) },
                //    { "CLOB", typeof(string) },
                //    { "DATE", typeof(DateTime) },
                //    { "FLOAT", typeof(decimal) },
                //    { "INTERVAL DAY TO SECOND", typeof(TimeSpan) },
                //    { "INTERVAL YEAR TO MONTH", typeof(int) },
                //    { "LONG", typeof(string) },
                //    { "NCHAR", typeof(string) },
                //    { "NCLOB", typeof(string) },
                //    { "NUMBER", typeof(double) },
                //    { "NVARCHAR2", typeof(string) },
                //    { "RAW", typeof(byte[]) },
                //    { "TIMESTAMP", typeof(DateTime) },
                //    { "TIMESTAMP WITH LOCAL TIME ZONE", typeof(DateTime) },
                //    { "TIMESTAMP WITH TIME ZONE", typeof(DateTime) },
                //    { "VARCHAR2", typeof(string) }
                //};
            }
        }
    }
}