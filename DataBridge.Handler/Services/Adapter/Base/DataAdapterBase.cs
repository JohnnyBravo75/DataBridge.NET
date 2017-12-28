using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System;
using System.Globalization;
using System.Xml.Serialization;
using DataBridge;
using DataBridge.Common.Services.Adapter;
using DataBridge.Handler.Services.Converters;
using DataBridge.Helper;

namespace DataBridge.Handler.Services.Adapter
{
    [XmlInclude(typeof(XmlAdapter))]
    [XmlInclude(typeof(FlatFileAdapter))]
    [XmlInclude(typeof(ExcelNativeAdapter))]
    [XmlInclude(typeof(Excel2007NativeAdapter))]
    [Serializable]
    public abstract class DataAdapterBase : IDataAdapterBase
    {
        private ValueConvertProcessor readConverter = new ValueConvertProcessor(ValueConvertProcessor.ConvertDirections.Read);

        private ValueConvertProcessor writeConverter = new ValueConvertProcessor(ValueConvertProcessor.ConvertDirections.Write);

        public ValueConvertProcessor ReadConverter
        {
            get { return this.readConverter; }
            set { this.readConverter = value; }
        }

        public ValueConvertProcessor WriteConverter
        {
            get { return this.writeConverter; }
            set { this.writeConverter = value; }
        }

        public abstract IList<DataColumn> GetAvailableColumns();

        public abstract IList<string> GetAvailableTables();

        public abstract int GetCount();

        public abstract IEnumerable<DataTable> ReadData(int? blockSize = null);

        public abstract bool WriteData(IEnumerable<DataTable> tables, bool deleteBefore = false);

        protected IEnumerable<Dictionary<string, object>> ConvertTablesToDictionaries(IEnumerable<DataTable> tables)
        {
            foreach (DataTable table in tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    Dictionary<string, object> dict = row.Table.Columns
                                                                .Cast<DataColumn>()
                                                                .ToDictionary(c => c.ColumnName, c => row[c]);

                    yield return dict;
                }
            }
        }

        

        public abstract void Dispose();

        public virtual DataTable ReadAllData()
        {
            return this.ReadData().FirstOrDefault();
        }

        public virtual IEnumerable<Dictionary<string, object>> ReadAllDataAs()
        {
            return this.ReadDataAs().ToList();
        }

        public virtual IEnumerable<Dictionary<string, object>> ReadDataAs(int? blockSize = null)
        {
            return this.ConvertTablesToDictionaries(this.ReadData(blockSize));
        }

        public virtual void WriteAllData(DataTable table, bool deleteBefore = false)
        {
            var list = new List<DataTable>();
            list.Add(table);
            this.WriteData(list, deleteBefore);
        }
    }
}