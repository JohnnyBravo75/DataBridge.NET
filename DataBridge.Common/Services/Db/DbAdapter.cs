using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Linq;
using System.Reflection;
using DataBridge.DbConnectionInfos;
using DataBridge.Extensions;

namespace DataBridge.Services
{
    public class DbAdapter : IDbAdapter
    {
        private DbConnection connection;

        private string tableName = "";

        private DbDataAdapter sqlDataAdapter;

        public DbConnectionInfoBase DbConnectionInfo { get; set; }

        public string TableName
        {
            get { return this.tableName; }
            set { this.tableName = value; }
        }

        public DbConnection Connection
        {
            get { return this.connection; }
            set { this.connection = value; }
        }

        public DbProviderFactory DbProviderFactory
        {
            get
            {
                if (this.DbConnectionInfo == null)
                {
                    return null;
                }
                return this.DbConnectionInfo.DbProviderFactory;
            }
        }

        public void SaveDataSet(DataSet dataSet)
        {
            foreach (DataTable table in dataSet.Tables)
            {
                this.LoadDataTable(dataSet, table, "SELECT * FROM " + this.QuoteIdentifier(table.TableName) + " WHERE 1 = 2 ");
                this.SaveDataTable(dataSet, table);
            }
        }

        private void SaveDataTable(DataSet dataSet, DataTable table)
        {
            DbCommandBuilder sqlBuilder = this.DbProviderFactory.CreateCommandBuilder();
            sqlBuilder.DataAdapter = this.sqlDataAdapter;

            this.sqlDataAdapter.InsertCommand = sqlBuilder.GetInsertCommand(true);
            //sqlDataAdapter.DeleteCommand = sqlBuilder.GetDeleteCommand(true);
            if (table.PrimaryKey.Any())
            {
                this.sqlDataAdapter.UpdateCommand = sqlBuilder.GetUpdateCommand(true);
            }
            //sqlDataAdapter.UpdateCommand = BuildUpdateCommand(table);
            this.sqlDataAdapter.Update(dataSet, table.TableName);
        }

        private DbCommand BuildUpdateCommand(DataTable table)
        {
            DbCommand cmd = this.DbProviderFactory.CreateCommand();

            string columns = "";
            foreach (DataColumn column in table.Columns)
            {
                if (table.PrimaryKey.Contains(column))
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(columns))
                {
                    columns += Environment.NewLine + ",";
                }

                columns += column.ColumnName + "=?";

                var param = cmd.CreateParameter();
                param.DbType = this.MapToDbType(column.DataType);
                param.ParameterName = column.ColumnName;
                param.SourceColumn = column.ColumnName;
                cmd.Parameters.Add(cmd.CreateParameter());
            }

            string where = "";
            int i = 0;
            foreach (DataColumn column in table.PrimaryKey)
            {
                if (!string.IsNullOrEmpty(columns) && i > 1)
                {
                    where += Environment.NewLine + " AND ";
                }

                where += column.ColumnName + "=?";

                var param = cmd.CreateParameter();
                param.DbType = this.MapToDbType(column.DataType);
                param.ParameterName = column.ColumnName;
                param.SourceColumn = column.ColumnName;
                cmd.Parameters.Add(cmd.CreateParameter());
                i++;
            }

            cmd.CommandText = "UPDATE " + this.QuoteIdentifier(table.TableName) + Environment.NewLine +
                             " SET " + columns + Environment.NewLine +
                             " WHERE " + where;

            return cmd;
        }

        public DbType MapToDbType(Type type)
        {
            var typeMap = new Dictionary<Type, DbType>();
            typeMap[typeof(byte)] = DbType.Byte;
            typeMap[typeof(sbyte)] = DbType.SByte;
            typeMap[typeof(short)] = DbType.Int16;
            typeMap[typeof(ushort)] = DbType.UInt16;
            typeMap[typeof(int)] = DbType.Int32;
            typeMap[typeof(uint)] = DbType.UInt32;
            typeMap[typeof(long)] = DbType.Int64;
            typeMap[typeof(ulong)] = DbType.UInt64;
            typeMap[typeof(float)] = DbType.Single;
            typeMap[typeof(double)] = DbType.Double;
            typeMap[typeof(decimal)] = DbType.Decimal;
            typeMap[typeof(bool)] = DbType.Boolean;
            typeMap[typeof(string)] = DbType.String;
            typeMap[typeof(char)] = DbType.StringFixedLength;
            typeMap[typeof(Guid)] = DbType.Guid;
            typeMap[typeof(DateTime)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset)] = DbType.DateTimeOffset;
            typeMap[typeof(byte[])] = DbType.Binary;
            typeMap[typeof(byte?)] = DbType.Byte;
            typeMap[typeof(sbyte?)] = DbType.SByte;
            typeMap[typeof(short?)] = DbType.Int16;
            typeMap[typeof(ushort?)] = DbType.UInt16;
            typeMap[typeof(int?)] = DbType.Int32;
            typeMap[typeof(uint?)] = DbType.UInt32;
            typeMap[typeof(long?)] = DbType.Int64;
            typeMap[typeof(ulong?)] = DbType.UInt64;
            typeMap[typeof(float?)] = DbType.Single;
            typeMap[typeof(double?)] = DbType.Double;
            typeMap[typeof(decimal?)] = DbType.Decimal;
            typeMap[typeof(bool?)] = DbType.Boolean;
            typeMap[typeof(char?)] = DbType.StringFixedLength;
            typeMap[typeof(Guid?)] = DbType.Guid;
            typeMap[typeof(DateTime?)] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset?)] = DbType.DateTimeOffset;
            //typeMap[typeof(System.Data.Linq.Binary)] = DbType.Binary;

            return typeMap[type];
        }

        private void PrintTableErrors(DataTable table)
        {
            DataRow[] rowErrors = table.GetErrors();

            System.Diagnostics.Debug.WriteLine(table.TableName + " Errors:"
                + rowErrors.Length);

            for (int i = 0; i < rowErrors.Length; i++)
            {
                System.Diagnostics.Debug.WriteLine(rowErrors[i].RowError);

                foreach (DataColumn col in rowErrors[i].GetColumnsInError())
                {
                    System.Diagnostics.Debug.WriteLine(col.ColumnName
                        + ":" + rowErrors[i].GetColumnError(col));
                }
            }
        }

        public bool IsConnected
        {
            get
            {
                if (this.Connection == null)
                {
                    return false;
                }

                if (this.Connection.State == ConnectionState.Closed ||
                    this.Connection.State == ConnectionState.Broken)
                {
                    return false;
                }

                return true;
            }
        }

        public string TestConnection()
        {
            try
            {
                using (var connection = this.DbProviderFactory.CreateConnection())
                {
                    connection.ConnectionString = this.DbConnectionInfo.ConnectionString;

                    connection.Open();
                    connection.Close();
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }

        public bool Connect()
        {
            this.Disconnect();

            try
            {
                this.Connection = this.DbProviderFactory.CreateConnection();
                this.Connection.ConnectionString = this.DbConnectionInfo.ConnectionString;

                this.Connection.Open();
            }
            catch (Exception ex)
            {
                // Set the connection to null if it was created.
                this.Connection = null;

                throw;
            }

            return this.Connection != null;
        }

        public bool Disconnect()
        {
            if (this.Connection != null && this.Connection.State != ConnectionState.Closed)
            {
                this.Connection.Close();
                this.Connection = null;
            }

            return (this.Connection == null);
        }

        public void Dispose()
        {
            if (this.Connection != null)
            {
                this.Connection.Close();
                this.Connection.Dispose();
                this.Connection = null;
            }

            this.DbConnectionInfo = null;

            if (this.sqlDataAdapter != null)
            {
                this.sqlDataAdapter.Dispose();
                this.sqlDataAdapter = null;
            }
        }

        public string GetConnectionValue(string key)
        {
            object value = "";

            var builder = this.DbProviderFactory.CreateConnectionStringBuilder();
            builder.ConnectionString = this.DbConnectionInfo.ConnectionString;
            builder.TryGetValue(key, out value);

            return value.ToStringOrEmpty();
        }

        public IList<string> GetAvailableTables()
        {
            if (string.IsNullOrEmpty(this.DbConnectionInfo.UserName))
            {
                this.DbConnectionInfo.UserName = this.GetConnectionValue("User ID");
            }

            // restrict to user
            var restrictions = new string[1];
            restrictions[0] = this.DbConnectionInfo.UserName.ToUpper();

            // Get list of tables (for user)
            DataTable userTables = this.Connection.GetSchema("Tables", restrictions);

            // copy to a list
            var userTableList = new List<string>();
            for (int i = 0; i < userTables.Rows.Count; i++)
            {
                userTableList.Add(userTables.Rows[i]["TABLE_NAME"].ToString());
            }

            return userTableList;
        }

        public IList<DataColumn> GetAvailableColumns()
        {
            // restrict to tables of the user
            string[] restrictions = new string[4];
            restrictions[1] = this.DbConnectionInfo.UserName.ToUpper();
            restrictions[2] = this.TableName.ToUpper();

            // Get list of user tables
            DataTable tableColumns = this.Connection.GetSchema("Columns", restrictions);

            // copy to a list
            var tableColumnList = new List<DataColumn>();
            for (int i = 0; i < tableColumns.Rows.Count; i++)
            {
                string columnName = tableColumns.Rows[i]["COLUMN_NAME"].ToString();

                var column = new DataColumn(columnName);
                tableColumnList.Add(column);
            }

            return tableColumnList;
        }

        public string QuoteIdentifier(string unquotedIdentifier)
        {
            using (var commandBuilder = this.DbProviderFactory.CreateCommandBuilder())
            {
                var identifiers = unquotedIdentifier.Split(new string[] { commandBuilder.SchemaSeparator }, StringSplitOptions.RemoveEmptyEntries);
                return string.Join(commandBuilder.SchemaSeparator, identifiers.ForEach(str => commandBuilder.QuoteIdentifier(str)));
            }
        }

        public bool IsIdentifierQuoted(string unquotedIdentifier)
        {
            using (var commandBuilder = this.DbProviderFactory.CreateCommandBuilder())
            {
                if (unquotedIdentifier.StartsWith(commandBuilder.QuotePrefix) && unquotedIdentifier.EndsWith(commandBuilder.QuoteSuffix))
                {
                    return true;
                }

                return false;
            }
        }

        public void CreateDataModel(DataSet dataSet, bool withContraints = true)
        {
            var exisitingTables = this.GetAvailableTables();

            foreach (DataTable table in dataSet.Tables)
            {
                if (!exisitingTables.Contains(table.TableName.ToUpper()))
                {
                    this.CreateTable(table, withContraints);
                }
                else
                {
                    this.ModifyTable(table);
                }
            }
        }

        public void CreateTable(DataTable table, bool withContraints = true)
        {
            string commandText = "";
            try
            {
                using (DbCommand cmd = this.Connection.CreateCommand())
                {
                    string columns = "";
                    foreach (DataColumn column in table.Columns)
                    {
                        if (!string.IsNullOrEmpty(columns))
                        {
                            columns += Environment.NewLine + ",";
                        }

                        // maps the internal dataType (e.g. "System.String") to a database type of the adapter (e.g. VARCHAR2(100))
                        string internalDataType = column.DataType.ToStringOrEmpty();
                        string dbDataType = this.MapToDbDataType(internalDataType);

                        // is a string, the length must be mounted
                        if (dbDataType.Contains("()"))
                        {
                            int length = column.MaxLength > 0 ? column.MaxLength
                                                              : 254;

                            // put in the length
                            dbDataType = dbDataType.Replace("()", "(" + length + ")");
                        }

                        columns = columns.Append(column.ColumnName + " " + dbDataType + " ");
                    }

                    commandText = "CREATE TABLE " + this.QuoteIdentifier(table.TableName) + " (" + columns + ")";
                    cmd.CommandText = commandText;
                    cmd.ExecuteNonQuery();
                }

                if (withContraints)
                {
                    this.CreateTableConstraints(table);
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Table '{0}' could not be created. Command='{1}'", this.QuoteIdentifier(table.TableName), commandText), ex);
            }
        }

        public void ModifyTable(DataTable table)
        {
            using (DbCommand cmd = this.Connection.CreateCommand())
            {
                var exisitingColumns = this.GetAvailableColumns();

                string columns = "";
                foreach (DataColumn column in table.Columns)
                {
                    if (!string.IsNullOrEmpty(columns))
                    {
                        columns += Environment.NewLine + ",";
                    }

                    // maps the internal dataType (e.g. "System.String") to a database type of the adapter (e.g. VARCHAR2(100))
                    string internalDataType = column.DataType.ToStringOrEmpty();
                    string dbDataType = this.MapToDbDataType(internalDataType);

                    // is a string, the length must be mounted
                    if (dbDataType.Contains("()"))
                    {
                        int length = column.MaxLength > 0 ? column.MaxLength
                                                          : 254;

                        // put in the length
                        dbDataType = dbDataType.Replace("()", "(" + length + ")");
                    }

                    columns = columns.Append(column.ColumnName + " " + dbDataType + " ");
                }

                cmd.CommandText = "ALTER TABLE " + this.QuoteIdentifier(table.TableName) + " ADD (" + columns + ")"; ;
                cmd.ExecuteNonQuery();
            }
        }

        /// <summary>
        /// maps the internal dataType (e.g. "System.String") to a database type of the adapter (e.g. VARCHAR2() )
        /// </summary>
        /// <param name="internalDataType">the internal dataType from C#</param>
        /// <returns>the dbDataType</returns>
        private string MapToDbDataType(string internalDataType)
        {
            if (!this.DbConnectionInfo.DataTypeMappings.ContainsKey(internalDataType))
            {
                // When no Datatype found, fallback to string
                internalDataType = "System.String";
            }

            string dbDataType = this.DbConnectionInfo.DataTypeMappings[internalDataType];
            return dbDataType;
        }

        public bool ExistsTable(string tableName)
        {
            var tables = this.GetAvailableTables().ForEach(x => x.ToUpper());
            return tables.Contains(tableName.ToUpper());
        }

        private void CreateTableConstraints(DataTable table)
        {
            // Primary Key
            using (DbCommand cmd = this.Connection.CreateCommand())
            {
                var pkeyColumns = table.PrimaryKey
                                       .Select(col => col.ColumnName)
                                       .ToArray();
                if (pkeyColumns.Any())
                {
                    cmd.CommandText = " ALTER TABLE " + this.QuoteIdentifier(table.TableName) +
                                      " ADD CONSTRAINT PK_" + table.TableName +
                                      " PRIMARY KEY (" + string.Join(",", pkeyColumns) + ")";
                    cmd.ExecuteNonQuery();
                }

                // Foreign Key
                foreach (DataRelation relation in table.ParentRelations)
                {
                    var fkeyColumns = relation.ParentColumns
                                              .Select(col => col.ColumnName)
                                              .ToArray();
                    if (fkeyColumns.Any())
                    {
                        cmd.CommandText = " ALTER TABLE " + this.QuoteIdentifier(table.TableName) +
                                          " ADD CONSTRAINT FK_" + table.TableName.TrySubstring(11) + "_TO_" + relation.ParentTable.TableName.TrySubstring(11) +
                                          " FOREIGN KEY (" + string.Join(",", fkeyColumns) + ") REFERENCES " + relation.ParentTable.TableName;
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;

                default:
                    return false;
            }
        }

        private void LoadDataTable(DataSet dataSet, DataTable table, string sql)
        {
            //sqlDataAdapter.MissingSchemaAction = MissingSchemaAction.AddWithKey;
            using (var cmd = this.Connection.CreateCommand())
            {
                cmd.CommandText = sql;
                cmd.Connection = this.connection;
                this.sqlDataAdapter.SelectCommand = cmd;
                try
                {
                    dataSet.EnforceConstraints = false;
                    this.sqlDataAdapter.Fill(dataSet, table.TableName);
                    dataSet.EnforceConstraints = true;
                }
                catch (ConstraintException)
                {
                    this.PrintTableErrors(table);
                    throw;
                }
            }
        }

        public DataTable ReadData()
        {
            return this.ReadData(null);
        }

        public DataTable ReadData(int? maxRows, DataTable existingTable = null)
        {
            // no fielddefinitions, take all columns
            string columnList = "*";

            string sql = string.Format("SELECT {0} FROM {1} ", columnList, this.QuoteIdentifier(this.TableName));
            return this.ReadTableData(sql, maxRows, existingTable);
        }

        private DataTable ReadTableData(string sql, int? maxRows, DataTable existingTable = null)
        {
            DataTable table = null;
            if (existingTable != null)
            {
                table = existingTable;
            }
            else
            {
                table = new DataTable();
            }

            table.TableName = this.TableName;

            if (string.IsNullOrEmpty(table.TableName))
            {
                return table;
            }

            try
            {
                using (var cmd = this.connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandText = sql;

                    using (var reader = cmd.ExecuteReader())
                    {
                        DataTable schemaTable = reader.GetSchemaTable();

                        var listCols = new List<DataColumn>();

                        if (schemaTable != null)
                        {
                            // create the columns in the datatable
                            string columnName = "";
                            foreach (DataRow row in schemaTable.Rows)
                            {
                                // take the original column name
                                columnName = (string)row["ColumnName"];

                                DataColumn column = new DataColumn(columnName);
                                column.DataType = (Type)(row["DataType"]);
                                column.Unique = (bool)row["IsUnique"];
                                column.AllowDBNull = (bool)row["AllowDBNull"];
                                column.AutoIncrement = (bool)row["IsAutoIncrement"];
                                listCols.Add(column);
                                table.Columns.Add(column);
                            }
                        }

                        // Read rows from DataReader and populate the DataTable
                        int rowIndex = 0;
                        while (reader.Read() && (maxRows == null || rowIndex < maxRows))
                        {
                            DataRow dataRow = table.NewRow();
                            for (int i = 0; i < listCols.Count; i++)
                            {
                                dataRow[((DataColumn)listCols[i])] = reader[i];
                            }

                            table.Rows.Add(dataRow);

                            rowIndex++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler in " + this.GetType().FullName + " Method: [" + MethodBase.GetCurrentMethod() + "] Table: " + this.TableName, ex);
            }

            return table;
        }

        public DataTable ExecuteSql(string sql, int? maxRows = null, DataTable existingTable = null)
        {
            DataTable table = null;
            if (existingTable != null)
            {
                table = existingTable;
            }
            else
            {
                table = new DataTable();
            }

            using (var cmd = this.Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;
                cmd.CommandText = sql;

                using (var reader = cmd.ExecuteReader())
                {
                    int rowIndex = 0;
                    while (reader.Read() && (maxRows == null || rowIndex < maxRows))
                    {
                        var rowValues = new Dictionary<string, object>();
                        for (int i = 0; i < reader.FieldCount; i++)
                        {
                            rowValues.Add(reader.GetName(i), reader.GetValue(i));
                        }

                        table.AddRow(rowValues);

                        rowIndex++;
                    }
                }
            }

            return table;
        }

        public bool DeleteData()
        {
            try
            {
                using (DbCommand cmd = this.connection.CreateCommand())
                {
                    // delete the data
                    string sql = string.Format("DELETE FROM {0} ", this.QuoteIdentifier(this.TableName));
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return true;
        }

        public bool WriteData(DataTable table)
        {
            table.TableName = this.TableName;

            if (!this.ExistsTable(table.TableName))
            {
                this.CreateTable(table, withContraints: false);
            }

            this.DeleteData();

            try
            {
                using (DbCommand cmd = this.connection.CreateCommand())
                {
                    // build the insert
                    cmd.Parameters.Clear();
                    string sqlColumns = "";
                    string sqlValues = "";
                    int colIndex = 0;
                    object cellValue = "";

                    foreach (DataColumn column in table.Columns)
                    {
                        string columnName = "";

                        columnName = column.ColumnName;

                        sqlColumns += this.QuoteIdentifier(columnName);

                        switch (this.DbProviderFactory.GetType().Name)
                        {
                            case "SqlClientFactory":
                                sqlValues += "@" + column.ColumnName;
                                break;

                            case "OracleClientFactory":
                                sqlValues += ":" + column.ColumnName;
                                break;

                            case "OleDbFactory":
                            case "OdbcFactory":
                                sqlValues += "?" + column.ColumnName;
                                break;

                            default:
                                sqlValues += "?" + column.ColumnName;
                                break;
                        }

                        if (colIndex < table.Columns.Count - 1)
                        {
                            sqlColumns += ",";
                            sqlValues += ",";
                        }

                        DbParameter parameter = cmd.CreateParameter();
                        parameter.DbType = this.MapToDbType(table.Columns[colIndex].DataType);
                        parameter.ParameterName = table.Columns[colIndex].ColumnName;
                        cmd.Parameters.Add(parameter);

                        colIndex++;
                    }

                    string sql = string.Format("insert into {0} ", this.QuoteIdentifier(table.TableName)) + Environment.NewLine
                                        + @" (" + sqlColumns + ") " + Environment.NewLine
                                        + @" values (" + sqlValues + ") ";

                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;

                    foreach (DataRow row in table.Rows)
                    {
                        // set parameter vales for one row
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            cellValue = row[i].ToString();

                            if (string.IsNullOrEmpty(cellValue as string))
                            {
                                cellValue = DBNull.Value;
                            }

                            cmd.Parameters[table.Columns[i].ColumnName].Value = cellValue;
                        }

                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }

            return true;
        }

        public int GetCount()
        {
            try
            {
                string sql = string.Format("SELECT COUNT(*) FROM {0} ", this.QuoteIdentifier(this.TableName));

                using (DbCommand cmd = this.connection.CreateCommand())
                {
                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    int count = (int)cmd.ExecuteScalar();

                    return count;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Fehler in " + this.GetType().FullName + " Method: [" + MethodBase.GetCurrentMethod() + "] Table: " + this.TableName, ex);
            }
        }

        protected virtual string EscapeParameterName(string parameterName)
        {
            return parameterName.Replace(" ", "_");
        }

        public IList<string> Validate()
        {
            var messages = new List<string>();

            if (this.DbConnectionInfo == null)
            {
                messages.Add("ConnectionInfo must not be empty");
            }

            if (this.DbConnectionInfo != null)
            {
                var errorMsg = this.TestConnection();
                if (!string.IsNullOrEmpty(errorMsg))
                {
                    messages.Add("Cannot connect to database: " + errorMsg);
                }
            }

            return messages;
        }

        public void RunStoredProc(string procName, IEnumerable<DbParameter> parameters)
        {
            try
            {
                using (DbCommand cmd = this.connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = procName;

                    // map input params => command params
                    cmd.Parameters.Clear();

                    // return value needs to be the first!
                    var retValue = parameters.FirstOrDefault(x => x.Direction == ParameterDirection.ReturnValue);
                    if (retValue != null)
                    {
                        cmd.Parameters.Add(retValue);
                    }

                    foreach (var parameter in parameters.Where(x => x.Direction != ParameterDirection.ReturnValue))
                    {
                        cmd.Parameters.Add(parameter);
                    }

                    cmd.ExecuteNonQuery();

                    // map  command params => input params
                    foreach (DbParameter parameter in cmd.Parameters)
                    {
                        if (parameter.Direction != ParameterDirection.Input)
                        {
                            var inOutParam = parameters.FirstOrDefault(x => x.ParameterName == parameter.ParameterName);
                            if (inOutParam != null)
                            {
                                inOutParam.Value = parameter.Value;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //****************************************************************************************************
        //****************************************************************************************************
        //****************************************************************************************************

        //private DataTable ReadData(string tableName)
        //{
        //    DataTable table = new DataTable();
        //    table.TableName = TableName;

        //    try
        //    {
        //        string sql = "SELECT * FROM " + QuoteIdentifier(table.TableName) + " ";

        //        using (DbCommand cmd = connection.CreateCommand())
        //        {
        //            cmd.CommandText = sql;
        //            cmd.CommandType = CommandType.Text;
        //            DbDataReader reader = cmd.ExecuteReader();
        //            DataTable schemaTable = reader.GetSchemaTable();

        //            List<DataColumn> listCols = new List<DataColumn>();

        //            if (schemaTable != null)
        //            {
        //                foreach (DataRow row in schemaTable.Rows)
        //                {
        //                    string columnName = System.Convert.ToString(row["ColumnName"]);
        //                    DataColumn column = new DataColumn(columnName, (Type)(row["DataType"]));
        //                    column.Unique = (bool)row["IsUnique"];
        //                    column.AllowDBNull = (bool)row["AllowDBNull"];
        //                    column.AutoIncrement = (bool)row["IsAutoIncrement"];
        //                    listCols.Add(column);
        //                    table.Columns.Add(column);
        //                }
        //            }

        //            // Read rows from DataReader and populate the DataTable
        //            while (reader.Read())
        //            {
        //                DataRow dataRow = table.NewRow();
        //                for (int i = 0; i < listCols.Count; i++)
        //                {
        //                    dataRow[((DataColumn)listCols[i])] = reader[i];
        //                }
        //                table.Rows.Add(dataRow);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        throw;
        //    }

        //    return table;
        //}

        //public void CreateTable(DataTable table, bool withContraints = true)
        //{
        //    using (DbCommand cmd = this.connection.CreateCommand())
        //    {
        //        cmd.Connection = connection;

        //        string columns = "";
        //        foreach (DataColumn column in table.Columns)
        //        {
        //            if (!string.IsNullOrEmpty(columns))
        //            {
        //                columns += Environment.NewLine + ",";
        //            }

        //            string dataType = "VARCHAR(254)";
        //            if (IsNumericType(column.DataType))
        //            {
        //                dataType = "NUMBER";
        //            }
        //            else if (column.DataType == typeof(string))
        //            {
        //                dataType = "VARCHAR(" + (column.MaxLength > 0 ? column.MaxLength : 254) + ")";
        //            }

        //            columns += QuoteIdentifier(column.ColumnName) + " " + dataType;
        //        }

        //        cmd.CommandText = "CREATE TABLE " + QuoteIdentifier(table.TableName) + " (" + columns + ")";
        //        cmd.ExecuteNonQuery();

        //        if (withContraints)
        //        {
        //            this.CreateTableConstraints(table);
        //        }
        //    }
        //}

        //public string GetProviderVersion(string providerName)
        //{
        //    try
        //    {
        //        string clsid = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Classes\\" + providerName + "\\clsid", "");

        //        if (string.IsNullOrEmpty(clsid.Trim()))
        //            return "Not Installed";

        //        string path = GetRegistryValue(Registry.LocalMachine, "SOFTWARE\\Classes\\CLSID\\" + clsid + "\\InprocServer32", "");

        //        FileVersionInfo fileInfo = default(FileVersionInfo);
        //        fileInfo = FileVersionInfo.GetVersionInfo(path);
        //        return fileInfo.ProductVersion.ToString();
        //    }
        //    catch (Exception ex)
        //    {
        //        return "Unable to get Version";
        //    }
        //}

        //public string GetRegistryValue(RegistryKey regKey, string subKey, string valueName)
        //{
        //    string value = "";
        //    RegistryKey registryKey = regKey;
        //    RegistryKey registrySubKey = default(RegistryKey);
        //    registrySubKey = registryKey.OpenSubKey(subKey);
        //    if (registrySubKey != null)
        //    {
        //        try
        //        {
        //            value = registrySubKey.GetValue(valueName).ToString();
        //        }
        //        catch (Exception ex)
        //        {
        //            value = "";
        //        }
        //        registrySubKey.Close();
        //    }
        //    return value;
        //}

        //private DbConnection GetConnection(string connStr)
        //{
        //    string providerName = null;
        //    var csb = new DbConnectionStringBuilder { ConnectionString = connStr };

        //    if (csb.ContainsKey("provider"))
        //    {
        //        providerName = csb["provider"].ToString();
        //    }
        //    else
        //    {
        //        var css = ConfigurationManager
        //                          .ConnectionStrings
        //                          .Cast<ConnectionStringSettings>()
        //                          .FirstOrDefault(x => x.ConnectionString == connStr);
        //        if (css != null)
        //            providerName = css.ProviderName;
        //    }

        //    if (providerName != null)
        //    {
        //        var providerExists = DbProviderFactories
        //                                    .GetFactoryClasses()
        //                                    .Rows.Cast<DataRow>()
        //                                    .Any(r => r[2].Equals(providerName));
        //        if (providerExists)
        //        {
        //            var factory = DbProviderFactories.GetFactory(providerName);
        //            return factory.CreateConnection();
        //        }
        //    }

        //    return null;
        //}

        // Given a provider name and connection string,
        // create the DbProviderFactory and DbConnection.
        // Returns a DbConnection on success; null on failure.
        //private static DbConnection CreateDbConnection(string providerName, string connectionString)
        //{
        //    // Assume failure.
        //    DbConnection connection = null;

        //    // Create the DbProviderFactory and DbConnection.
        //    if (connectionString != null)
        //    {
        //        try
        //        {
        //            DbProviderFactory factory =
        //                DbProviderFactories.GetFactory(providerName);

        //            connection = factory.CreateConnection();
        //            connection.ConnectionString = connectionString;
        //        }
        //        catch (Exception ex)
        //        {
        //            // Set the connection to null if it was created.

        //            connection = null;

        //            Console.WriteLine(ex.Message);
        //        }
        //    }
        //    // Return the connection.
        //    return connection;
        //}

        //public string FullQualifiedTableName
        //{
        //    get
        //    {
        //        return !string.IsNullOrEmpty(this.DbConnectionInfo.UserName)
        //                   ? this.DbConnectionInfo.UserName + "." + TableName
        //                   : TableName;
        //    }
        //}
    }
}