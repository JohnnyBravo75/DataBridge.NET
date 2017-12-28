using System;
using System.Collections.Generic;
using System.Data;
using DataBridge.ConnectionInfos;

namespace DataBridge.Handler.Services.Adapter
{
    public interface IDataAdapterBase : IDisposable
    {
        //ConnectionInfoBase ConnectionInfo { get; set; }

        //bool Connect();

        //bool Disconnect();

        //bool IsConnected { get; }

        //ValueConvertProcessor ReadConverter { get; set; }

        //ValueConvertProcessor WriteConverter { get; set; }

        IList<DataColumn> GetAvailableColumns();

        IList<string> GetAvailableTables();

        int GetCount();

        DataTable ReadAllData();

        IEnumerable<DataTable> ReadData(int? blockSize = default(int?));

        void WriteAllData(DataTable table, bool deleteBefore = false);

        bool WriteData(IEnumerable<DataTable> tables, bool deleteBefore = false);

    }
}