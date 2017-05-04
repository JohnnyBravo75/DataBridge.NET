namespace DMF.Data.DataAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Xml.Serialization;
    using OfficeOpenXml;
    using DataBridge.Common.Services.Adapter;

    public class Excel2007NativeExportAdapter : IExportAdapter
    {
        // ***********************Fields***********************

        private ExcelPackage excelPackage;

        protected int importRowIndex = 0;
        protected StreamReader importReader;

        protected int exportRowIndex = 0;
        protected StreamWriter exportWriter;
        private string fileName = "";

        // ***********************Constructors***********************

        public Excel2007NativeExportAdapter()
        {
        }

        public string FileName
        {
            get { return this.fileName; }
            set { this.fileName = value; }
        }

        public string SheetName { get; set; }

        [XmlIgnore]
        public bool IsConnected { get; protected set; }

        // ***********************Functions***********************

        public bool Connect()
        {
            this.Disconnect();

            if (string.IsNullOrEmpty(this.fileName))
            {
                return false;
            }

            if (!File.Exists(this.fileName))
            {
                return false;
            }

            this.excelPackage = new ExcelPackage(new FileInfo(this.fileName));

            if (this.excelPackage != null)
            {
                this.IsConnected = true;
            }

            return this.IsConnected;
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

            return (this.excelPackage == null);
        }

        public void Dispose()
        {
            this.Disconnect();
        }

        public IList<DataColumn> GetAvailableColumns()
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

        public IList<string> GetAvailableTables()
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

        public int GetCount()
        {
            int count = 0;
            if (!this.IsConnected)
            {
                return count;
            }

            ExcelWorksheet sheet = this.excelPackage.Workbook.Worksheets[this.SheetName];
            count = sheet.Dimension.End.Row;
            return count;
        }

        public void ResetToStart()
        {
            this.importRowIndex = 0;
            if (this.importReader != null)
            {
                this.importReader.BaseStream.Position = 0;
                this.importReader.DiscardBufferedData();
            }

            this.exportRowIndex = 0;
            if (this.exportWriter != null)
            {
                this.exportWriter.BaseStream.Position = 0;
            }
        }

        public bool WriteData(DataTable table)
        {
            string tableName = this.SheetName;
            ExcelWorksheet sheet = this.excelPackage.Workbook.Worksheets[tableName];

            if (sheet == null)
            {
                sheet = this.excelPackage.Workbook.Worksheets.Add(tableName);
            }

            // create header row
            for (int x = 0; x < table.Columns.Count; x++)
            {
                string columnName = table.Columns[x].ToString();
                sheet.Cells[1, 1 + x].Value = columnName;
            }

            int offSetY = 1;

            // loops through data
            for (int y = 0; y < table.Rows.Count; y++)
            {
                for (int x = 0; x < table.Columns.Count; x++)
                {
                    string columnName = table.Columns[x].ToString();
                    sheet.Cells[offSetY + 1 + y, 1 + x].Value = table.Rows[y][columnName].ToString();
                }
            }

            // Save ExcelDocument
            this.excelPackage.Save();
            if (this.excelPackage.Stream != null)
            {
                this.excelPackage.Stream.Flush();
            }

            return true;
        }
    }
}