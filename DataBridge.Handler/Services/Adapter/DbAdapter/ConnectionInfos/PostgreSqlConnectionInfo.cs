using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Common.Helper;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class PostgreSqlConnectionInfo : DbConnectionInfoBase
    {
        private string server = "";
        private string password = "";
        private string database = "";

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                return "Provider=PostgreSQL OLE DB Provider;"
                             + "Data Source=" + this.Server + ";"
                             + "location=" + this.Database + ";"
                             + "User Id=" + this.UserName + ";"
                             + "Password=" + this.DecryptedPassword + ";";
            }
        }

        [XmlAttribute]
        public string Server
        {
            get { return this.server; }
            set { this.server = value; }
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
                            { "System.String", "VARCHAR()" },
                            { "System.DateTime", "DATE" },
                            { "System.Single", "NUMERIC" },
                            { "System.Int32", "INTEGER" }
                        };

                //    var mappings = new Dictionary<string, Type>()
                //{
                //    { "bigint", typeof(long) },
                //    { "bigserial", typeof(long) },
                //    { "bit", typeof(BitArray) },
                //    { "bit varying", typeof(string) },
                //    { "boolean", typeof(bool) },
                //    { "box", typeof(string) },
                //    { "bytea", typeof(byte[]) },
                //    { "character varying", typeof(string) },
                //    { "character", typeof(string) },
                //    { "cidr", typeof(string) },
                //    { "circle", typeof(string) },
                //    { "date", typeof(DateTime) },
                //    { "double precision", typeof(double) },
                //    { "inet", typeof(string) },
                //    { "integer", typeof(int) },
                //    { "interval", typeof(TimeSpan) },
                //    { "lseg", typeof(string) },
                //    { "macaddr", typeof(string) },
                //    { "money", typeof(decimal) },
                //    { "numeric", typeof(decimal) },
                //    { "path", typeof(string) },
                //    { "point", typeof(string) },
                //    { "polygon", typeof(string) },
                //    { "real", typeof(float) },
                //    { "smallint", typeof(short) },
                //    { "serial", typeof(int) },
                //    { "text", typeof(string) },
                //    { "time without time zone", typeof(TimeSpan) },
                //    { "time with time zone", typeof(TimeSpan) },
                //    { "timestamp without time zone", typeof(DateTime) },
                //    { "timestamp with time zone", typeof(DateTime) },
                //    { "tsquery", typeof(string) },
                //    { "tsvector", typeof(string) },
                //    { "uuid", typeof(Guid) },
                //    { "xml", typeof(string) }
                //};
            }
        }
    }
}