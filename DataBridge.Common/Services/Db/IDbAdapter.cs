using System;
using System.Collections.Generic;
using System.Data;
using DataBridge.DbConnectionInfos;

namespace DataBridge.Services
{
    public interface IDbAdapter : IDisposable
    {
        bool Connect();

        bool Disconnect();

        IList<DataColumn> GetAvailableColumns();

        IList<string> GetAvailableTables();

        int GetCount();

        DataTable ReadData();

        DataTable ReadData(int? maxRows, DataTable existingTable = null);

        bool WriteData(DataTable table);

        DbConnectionInfoBase DbConnectionInfo { get; set; }

        //bool IsConnected { get; }

        string TableName { get; set; }

        //DataTable ExecuteSql(string sql, int? maxRows, DataTable existingTable = null);

        string TestConnection();

        IList<string> Validate();

        //void CreateTable(DataTable table, bool withContraints = true);

        bool DeleteData();

        bool ExistsTable(string tableName);

    }
}