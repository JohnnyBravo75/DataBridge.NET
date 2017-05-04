using System;
using System.Collections.Generic;
using System.Data;

namespace DMF.Data.DataAdapters
{
    public interface IImportAdapter : IDisposable
    {
        bool Connect();

        bool Disconnect();

        IList<DataColumn> GetAvailableColumns();

        IList<string> GetAvailableTables();

        int GetCount();

        DataTable ReadData();

        DataTable ReadData(int? maxRows, DataTable existingTable = null);
    }
}