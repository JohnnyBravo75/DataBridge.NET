using System.Collections.Generic;
using DataBridge.Common.Services.Adapter;

namespace DataBridge.Handler.Services.Adapter
{
    public interface IDbAdapter : IDataAdapterBase
    {
        bool DropTable();

        //bool IsConnected { get; }

        string TableName { get; set; }

        //DataTable ExecuteSql(string sql, int? maxRows, DataTable existingTable = null);

        string TestConnection();

        IList<string> Validate();

        //void CreateTable(DataTable table, bool withContraints = true);

        bool DeleteData();

        bool ExistsTable();
    }
}