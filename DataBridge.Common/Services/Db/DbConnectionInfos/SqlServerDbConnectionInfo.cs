using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.DbConnectionInfos
{
    [Serializable]
    public class SqlServerDbConnectionInfo : DbConnectionInfoBase
    {
        private int timeOutSeconds = 15;

        [XmlIgnore]
        public override string ConnectionString
        {
            get
            {
                if (this.Authentication == AuthenticationMode.Windows)
                {
                    return "Provider=SQLOLEDB;"
                                     + "Data Source=" + this.Server + ";"
                                     + "Initial Catalog=" + this.Database + ";"
                                     + "Connect Timeout=" + this.timeOutSeconds + ";"
                                     + "Integrated Security=SSPI;"
                                     + "Persist Security Info=false;";
                }
                else
                {
                    return "Provider=SQLOLEDB;"
                                    + "Data Source=" + this.Server + ";"
                                    + "Initial Catalog=" + this.Database + ";"
                                    + "User ID=" + this.UserName + ";"
                                    + "Password=" + this.Password + ";"
                                    + "Connect Timeout=" + this.TimeOutSeconds + ";"
                                    + "Persist Security Info=True;";
                }
            }
        }

        [XmlAttribute]
        public int TimeOutSeconds
        {
            get { return this.timeOutSeconds; }
            set { this.timeOutSeconds = value; }
        }

        [XmlAttribute]
        public AuthenticationMode Authentication { get; set; }

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
                            { "System.String", "NVARCHAR()" },
                            { "System.DateTime", "DATETIME" },
                            { "System.Single", "NUMERIC" },
                            { "System.Int32", "INT" }
                        };

                //    var mappins = new Dictionary<string, Type>()
                //{
                //    { "bigint", typeof(long) },
                //    { "binary", typeof(byte[]) },
                //    { "bit", typeof(bool) },
                //    { "char", typeof(string) },
                //    { "date", typeof(Date) },
                //    { "datetime", typeof(DateTime) },
                //    { "datetime2", typeof(DateTime) },
                //    { "datetimeoffset", typeof(DateTimeOffset) },
                //    { "decimal", typeof(decimal) },
                //    { "float", typeof(double) },
                //    { "geography", typeof(SqlGeography) },
                //    { "geometry", typeof(SqlGeometry) },
                //    { "hierarchyid", typeof(SqlHierarchyId) },
                //    { "image", typeof(byte[]) },
                //    { "int", typeof(int) },
                //    { "money", typeof(Currency) },
                //    { "nchar", typeof(string) },
                //    { "ntext", typeof(string) },
                //    { "numeric", typeof(decimal) },
                //    { "nvarchar", typeof(string) },
                //    { "real", typeof(float) },
                //    { "smalldatetime", typeof(DateTime) },
                //    { "smallint", typeof(short) },
                //    { "smallmoney", typeof(Currency) },
                //    { "sql_variant", typeof(object) },
                //    { "text", typeof(string) },
                //    { "time", typeof(Time) },
                //    { "timestamp", typeof(byte[]) },
                //    { "tinyint", typeof(byte) },
                //    { "uniqueidentifier", typeof(Guid) },
                //    { "varbinary", typeof(byte[]) },
                //    { "varchar", typeof(string) },
                //    { "xml", typeof(string) }
                //};
            }
        }
    }

    public enum AuthenticationMode
    {
        Windows,
        UserAndPassword
    }
}