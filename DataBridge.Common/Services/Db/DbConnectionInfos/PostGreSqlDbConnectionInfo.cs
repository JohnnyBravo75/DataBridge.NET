using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.DbConnectionInfos
{
    [Serializable]
    public class PostgreSqlDbConnectionInfo : DbConnectionInfoBase
    {
        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                return "Provider=PostgreSQL OLE DB Provider;"
                             + "Data Source=" + this.Server + ";"
                             + "location=" + this.Database + ";"
                             + "User Id=" + this.UserName + ";"
                             + "Password=" + this.Password + ";";
            }
        }

        [XmlAttribute]
        public string Server { get; set; }

        [XmlAttribute]
        public string Password { get; set; }

        [XmlAttribute]
        public string Database { get; set; }

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