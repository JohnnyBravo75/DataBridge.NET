using System;
using System.Collections.Generic;
using System.Data;

namespace DMF.Data.DataAdapters
{
    public interface IExportAdapter : IDisposable
    {
        bool Connect();

        bool Disconnect();

        IList<DataColumn> GetAvailableColumns();

        IList<string> GetAvailableTables();

        int GetCount();

        bool WriteData(DataTable table);
    }
}