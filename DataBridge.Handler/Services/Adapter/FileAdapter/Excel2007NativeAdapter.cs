namespace DataBridge.Handler.Services.Adapter
{
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Xml.Serialization;
    using DataBridge.Common.Services.Adapter;
    using DataBridge.Helper;
    using OfficeOpenXml;
    using System;
    using DataBridge.ConnectionInfos;

    public class Excel2007NativeAdapter : DataAdapterBase, IDataAdapterBase
    {
        // ***********************Fields***********************

        private ExcelPackage excelPackage;

        private ConnectionInfoBase connectionInfo;

        private int rowIndex;

        // ***********************Constructors***********************

        public Excel2007NativeAdapter()
        {
            this.ConnectionInfo = new ExcelConnectionInfo();
        }

        public Excel2007NativeAdapter(string filenName, string sheetName = null) : this()
        {
            this.FileName = filenName;
            this.SheetName = sheetName;
        }

        [XmlIgnore]
        public Stream DataStream { get; set; }

        [XmlElement]
        public ConnectionInfoBase ConnectionInfo
        {
            get
            {
                return this.connectionInfo;
            }
            set
            {
                this.connectionInfo = value;
            }
        }

        [XmlAttribute]
        public string FileName
        {
            get
            {
                if (!(this.ConnectionInfo is ExcelConnectionInfo))
                {
                    return string.Empty;
                }
                return (this.ConnectionInfo as ExcelConnectionInfo).FileName;
            }
            set { (this.ConnectionInfo as ExcelConnectionInfo).FileName = value; }
        }

        [XmlAttribute]
        public string SheetName
        {
            get
            {
                if (!(this.ConnectionInfo is ExcelConnectionInfo))
                {
                    return string.Empty;
                }
                return (this.ConnectionInfo as ExcelConnectionInfo).SheetName;
            }
            set { (this.ConnectionInfo as ExcelConnectionInfo).SheetName = value; }
        }

        [XmlIgnore]
        public bool IsConnected { get; protected set; }

        // ***********************Functions***********************

        public bool Connect()
        {
            this.Disconnect();

            if (string.IsNullOrEmpty(this.FileName) && this.DataStream == null)
            {
                throw new ArgumentNullException("Please provide a DataStream or a FileName.");
            }

            this.excelPackage = this.OpenExcelFile();

            if (this.excelPackage == null)
            {
                throw new ArgumentNullException("Workbook", "The workbook was not found or could not be opened.");
            }

            this.IsConnected = true;

            return this.IsConnected;
        }

        private ExcelPackage OpenExcelFile()
        {
            ExcelPackage excelPackage = null;

            if (!string.IsNullOrEmpty(this.FileName))
            {
                excelPackage = new ExcelPackage(new FileInfo(this.FileName));
            }
            else if (this.DataStream != null)
            {
                excelPackage = new ExcelPackage(this.DataStream);
            }

            return excelPackage;
        }

        public void CreateNewFile()
        {
            this.excelPackage = new ExcelPackage(new FileInfo(this.FileName));

            ExcelWorksheet sheet = this.excelPackage.Workbook.Worksheets[this.SheetName];

            if (sheet == null)
            {
                sheet = this.excelPackage.Workbook.Worksheets.Add(this.SheetName);
            }

            this.excelPackage.Save();
        }

        public bool Disconnect()
        {
            if (this.excelPackage != null)
            {
                this.excelPackage.Dispose();
                this.excelPackage = null;
            }

            if (this.excelPackage == null)
            {
                this.IsConnected = false;
            }

            this.startX = 0;
            this.startY = 1;

            return (this.excelPackage == null);
        }

        public override void Dispose()
        {
            this.Disconnect();

            if (this.DataStream != null)
            {
                // do not destroy stream I´m not the owner/creator
                //this.DataStream.Close();
                //this.DataStream.Dispose();
                this.DataStream = null;
            }
        }

        public override IList<DataColumn> GetAvailableColumns()
        {
            var tableColumnList = new List<DataColumn>();

            if (this.excelPackage == null)
            {
                return tableColumnList;
            }

            ExcelWorksheet sheet = this.excelPackage.Workbook.Worksheets[this.SheetName];

            if (sheet == null)
            {
                return tableColumnList;
            }

            // read the headers
            int colCnt = sheet.Dimension.End.Column;
            for (int x = 0; x < colCnt; x++)
            {
                string cellValue = sheet.Cells[1, 1 + x].Value.ToString();

                if (string.IsNullOrEmpty(cellValue))
                {
                    break;
                }

                var field = new DataColumn(cellValue);
                tableColumnList.Add(field);
            }

            return tableColumnList;
        }

        public override IList<string> GetAvailableTables()
        {
            IList<string> userTableList = new List<string>();

            if (this.excelPackage == null)
            {
                return userTableList;
            }

            foreach (var sheet in this.excelPackage.Workbook.Worksheets)
            {
                userTableList.Add(sheet.Name);
            }

            return userTableList;
        }

        public override int GetCount()
        {
            int count = 0;
            if (!this.IsConnected)
            {
                return count;
            }

            ExcelWorksheet sheet = this.excelPackage.Workbook.Worksheets[this.SheetName];

            if (sheet.Dimension == null)
            {
                return 0;
            }

            count = sheet.Dimension.End.Row;
            return count;
        }

        public void ResetToStart()
        {
            this.rowIndex = 0;
            if (this.DataStream != null)
            {
                this.DataStream.Position = 0;
            }
        }

        public override IEnumerable<DataTable> ReadData(int? blockSize = null)
        {
            if (!this.IsConnected)
            {
                this.Connect();
            }

            this.ResetToStart();

            bool hasHeader = true;
            DataRow tableRow = null;
            DataTable table = null;
            string tableName = this.SheetName;

            if (string.IsNullOrEmpty(this.SheetName))
            {
                throw new ArgumentNullException("SheetName", "Please provide a sheet name");
            }

            if (!this.IsConnected)
            {
                yield return table;
            }

            // create a new datatable
            table = new DataTable(tableName);
            table.TableName = tableName;

            if (this.excelPackage == null)
            {
                yield break;
            }

            ExcelWorksheet sheet = this.excelPackage.Workbook.Worksheets[tableName];

            if (sheet == null)
            {
                throw new Exception(string.Format("The sheet '{0}' was not found.", this.SheetName));
            }

            int rowCnt = sheet.Dimension.End.Row;
            int colCnt = sheet.Dimension.End.Column;
            int rowsRead = 0;
            //int y = 0;

            // loop the rows
            for (int y = 0; y < rowCnt; y++)
            {
                y = this.rowIndex;

                // first row?
                if (y == 0)
                {
                    // read the headers and create the columns
                    for (int x = 0; x < colCnt; x++)
                    {
                        object cellValue = sheet.Cells[1, 1 + x].Value;

                        if (cellValue == null)
                        {
                            break;
                        }

                        if (string.IsNullOrEmpty(cellValue.ToString()))
                        {
                            break;
                        }

                        table.Columns.Add(cellValue.ToString(), typeof(string));
                    }
                }

                if (!(y == 0 && hasHeader))
                {
                    tableRow = table.NewRow();

                    // loop the columns
                    for (int x = 0; x < table.Columns.Count; x++)
                    {
                        tableRow[x] = sheet.Cells[1 + y, 1 + x].Value;
                    }

                    table.Rows.Add(tableRow);
                }

                rowsRead++;
                this.rowIndex++;

                if (blockSize.HasValue && y % blockSize == 0)
                {
                    yield return table;

                    // create new table with the columns and destroy the old table
                    var headerTable = table.Clone();
                    DataTableHelper.DisposeTable(table);
                    table = headerTable;
                }
            }

            yield return table;
        }

        public override bool WriteData(IEnumerable<DataTable> tables, bool deleteBefore = false)
        {
            if (!this.IsConnected)
            {
                this.Connect();
            }

            string tableName = this.SheetName;

            ExcelWorksheet sheet = this.excelPackage.Workbook.Worksheets[tableName];

            if (sheet == null)
            {
                sheet = this.excelPackage.Workbook.Worksheets.Add(tableName);
            }

            int i = 0;
            foreach (DataTable table in tables)
            {
                this.startY = deleteBefore && i == 0 ? 0 : this.GetCount();

                bool hasCreatedHeader = false;

                if (this.startY == 0)
                {
                    // create header row
                    for (int x = 0; x < table.Columns.Count; x++)
                    {
                        string columnName = table.Columns[x].ToString();
                        sheet.Cells[1 + this.startY, 1 + this.startX + x].Value = columnName;
                    }
                    hasCreatedHeader = true;
                }

                // loops through data
                for (int y = 0; y < table.Rows.Count; y++)
                {
                    for (int x = 0; x < table.Columns.Count; x++)
                    {
                        string columnName = table.Columns[x].ToString();
                        sheet.Cells[1 + this.startY + y + (hasCreatedHeader ? 1 : 0), 1 + this.startX + x].Value = table.Rows[y][columnName].ToString();
                    }
                }

                i++;
            }

            // Save ExcelDocument
            this.excelPackage.Save();

            if (this.excelPackage.Stream != null)
            {
                this.excelPackage.Stream.Flush();
            }

            return true;
        }

        private int startY = 0;
        private int startX = 0;
    }
}