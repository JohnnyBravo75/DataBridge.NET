using System;
using System.Collections.Generic;
using System.Data;

namespace DataBridge.Common.Services.Adapter
{
    public interface IDataAdapterBase : IDisposable
    {
        bool Connect();

        bool Disconnect();

        IList<DataColumn> GetAvailableColumns();

        IList<string> GetAvailableTables();

        int GetCount();

        DataTable ReadData();

        DataTable ReadData(int? maxRows, DataTable existingTable = null);

        bool WriteData(DataTable table);
    }
}
