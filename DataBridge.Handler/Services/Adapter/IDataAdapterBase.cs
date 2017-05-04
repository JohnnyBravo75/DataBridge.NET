using System;
using System.Collections.Generic;
using System.Data;
using DataBridge.ConnectionInfos;

namespace DataBridge.Common.Services.Adapter
{
    public interface IDataAdapterBase : IDisposable
    {
        //ConnectionInfoBase ConnectionInfo { get; set; }

        bool Connect();

        bool Disconnect();

        IList<DataColumn> GetAvailableColumns();

        IList<string> GetAvailableTables();

        int GetCount();

        IEnumerable<DataTable> ReadData(int? blockSize = null);

        bool WriteData(DataTable table);
    }
}