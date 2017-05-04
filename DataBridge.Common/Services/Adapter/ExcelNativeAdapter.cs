using DataBridge.Common.Services.Adapter;

namespace DMF.Data.DataAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.IO;
    using System.Xml.Serialization;
    using DataBridge.Extensions;
    using NPOI.HPSF;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;

    public class ExcelNativeAdapter : IDataAdapterBase
    {
        // ***********************Fields***********************

        private string userName = "";
        private HSSFWorkbook workbook;
        private Sheet sheet;
        private const int MAX_SHEET_ROWS = 65535;
        private const int MAX_SHEET_COLUMNS = 255;

        protected int importRowIndex = 0;
        protected StreamReader importReader;

        protected int exportRowIndex = 0;
        protected StreamWriter exportWriter;
        private string fileName = "";
        // ***********************Constructors***********************

        public ExcelNativeAdapter()
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

        public bool Connect()
        {
            this.Disconnect();

            this.workbook = this.OpenExcelFile(this.fileName);

            if (this.workbook != null)
            {
                this.IsConnected = true;
            }

            return this.IsConnected;
        }

        // ***********************Functions***********************

        public bool Disconnect()
        {
            if (this.workbook != null)
            {
                this.workbook.Dispose();
                this.workbook = null;
            }

            if (this.workbook == null)
            {
                this.IsConnected = false;
            }

            return (this.workbook == null);
        }

        public void Dispose()
        {
            this.Disconnect();
        }

        public IList<DataColumn> GetAvailableColumns()
        {
            var tableColumnList = new List<DataColumn>();

            if (this.workbook == null)
            {
                return tableColumnList;
            }

            Row excelRow = null;
            Cell excelCell = null;

            this.sheet = this.workbook.GetSheet(this.SheetName);

            if (this.sheet == null)
            {
                return tableColumnList;
            }

            // get first row
            excelRow = this.sheet.GetRow(0);

            // read the headers
            for (int x = 0; x < MAX_SHEET_COLUMNS; x++)
            {
                excelCell = excelRow.GetCell(x);

                if (excelCell == null)
                {
                    break;
                }

                var field = new DataColumn(excelCell.StringCellValue);

                tableColumnList.Add(field);
            }

            return tableColumnList;
        }

        public IList<string> GetAvailableTables()
        {
            IList<string> userTableList = new List<string>();

            if (this.workbook == null)
            {
                return userTableList;
            }

            for (int i = 0; i < this.workbook.NumberOfSheets; i++)
            {
                userTableList.Add(this.workbook.GetSheetName(i));
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

            this.sheet = this.workbook.GetSheet(this.SheetName);
            Row excelRow = null;

            for (int y = 0; y < MAX_SHEET_ROWS; y++)
            {
                excelRow = this.sheet.GetRow(y);

                // when no row found exit
                if (excelRow == null)
                {
                    break;
                }

                count++;
            }

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

        public DataTable ReadData()
        {
            return this.ReadData(null);
        }

        public DataTable ReadData(int? maxRows, DataTable existingTable = null)
        {
            this.ResetToStart();

            bool hasHeader = true;
            Row excelRow = null;
            Cell excelCell = null;
            DataRow tableRow = null;
            DataTable table = null;
            string tableName = this.SheetName;

            if (!this.IsConnected)
            {
                return table;
            }

            try
            {
                if (existingTable != null)
                {
                    // use existing datatable
                    table = existingTable;

                    if (string.IsNullOrEmpty(table.TableName))
                    {
                        table.TableName = tableName;
                    }
                }
                else
                {
                    // create a new datatable
                    table = new DataTable(tableName);
                }

                if (this.workbook == null)
                {
                    return table;
                }

                this.sheet = this.workbook.GetSheet(tableName);

                if (this.sheet == null)
                {
                    return table;
                }

                int rowsRead = 0;
                int y = 0;

                // loop the rows
                while (maxRows == null || rowsRead < maxRows)
                //for (int y = 0; y < 65536; y++)
                {
                    y = this.importRowIndex;
                    excelRow = this.sheet.GetRow(y);

                    // when no row found exit
                    if (excelRow == null)
                    {
                        break;
                    }

                    // first row?
                    if (y == 0)
                    {
                        // read the headers and create the columns
                        for (int x = 0; x < MAX_SHEET_COLUMNS; x++)
                        {
                            excelCell = excelRow.GetCell(x);

                            if (excelCell == null)
                            {
                                break;
                            }

                            table.Columns.Add(excelCell.StringCellValue, typeof(string));
                        }
                    }

                    if (!(y == 0 && hasHeader))
                    {
                        tableRow = table.NewRow();

                        // loop the columns
                        for (int x = 0; x < table.Columns.Count; x++)
                        {
                            // get the cell
                            excelCell = excelRow.GetCell(x);

                            if (excelCell != null)
                            {
                                tableRow[x] = this.GetCellValue(excelCell);
                            }
                        }

                        table.Rows.Add(tableRow);
                    }

                    rowsRead++;
                    this.importRowIndex++;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error in " + this.GetType().FullName + " Method: [" + System.Reflection.MethodBase.GetCurrentMethod() + "] File: " + this.fileName, ex);
            }

            return table;
        }

        private object GetCellValue(Cell excelCell)
        {
            object result = null;

            if (excelCell == null)
            {
                return result;
            }

            // add to the current row
            switch (excelCell.CellType)
            {
                case CellType.BOOLEAN:
                    result = excelCell.BooleanCellValue;
                    break;

                case CellType.FORMULA:
                    // force to string
                    result = excelCell.StringCellValue;
                    // result = "=" + excelCell.CellFormula;
                    break;

                case CellType.STRING:
                    result = excelCell.StringCellValue;
                    break;

                case CellType.NUMERIC:
                    result = excelCell.ToString();

                    //if (IsCellDateFormatted(excelCell))
                    //{
                    //    result = excelCell.DateCellValue;
                    //}
                    //else
                    //{
                    //    result = excelCell.NumericCellValue;
                    //}

                    break;

                case CellType.Unknown:
                    result = excelCell.StringCellValue;
                    break;

                default:
                    result = excelCell.StringCellValue;
                    break;
            }

            return result;
        }

        public bool WriteData(DataTable table)
        {
            if (!this.IsConnected)
            {
                return false;
            }

            string tableName = this.SheetName;
            CellStyle headerCellStyle = this.GetHeaderCellStyle();

            if (this.workbook == null)
            {
                this.workbook = this.CreateNewWorkbook();
            }

            this.sheet = this.workbook.GetSheet(tableName);

            if (this.sheet == null)
            {
                // make a new sheet
                this.sheet = this.workbook.CreateSheet(tableName);
            }

            // create header row
            var row1 = this.sheet.CreateRow(0);

            for (int x = 0; x < table.Columns.Count; x++)
            {
                Cell cell = row1.CreateCell(x);
                string columnName = table.Columns[x].ToString();
                cell.SetCellValue(columnName);
                if (headerCellStyle != null)
                {
                    cell.CellStyle = headerCellStyle;
                }
            }

            // loop through data
            for (int y = 0; y < table.Rows.Count; y++)
            {
                var row = this.sheet.CreateRow(y + 1);
                for (int x = 0; x < table.Columns.Count; x++)
                {
                    Cell cell = row.CreateCell(x);
                    string columnName = table.Columns[x].ToString();
                    var value = table.Rows[y][columnName];

                    this.SetCellValue(cell, value);

                    string dataFormat = this.GetDefaultDataFormat(value);

                    // find/create cell style
                    cell.CellStyle = this.GetCellStyleForFormat(this.sheet.Workbook, dataFormat);
                }
            }

            this.WriteExcelFile(this.fileName, this.workbook);

            return true;
        }

        private void SetCellValue(Cell cell, object value)
        {
            if (value != null && value != DBNull.Value)
            {
                var type = value.GetType();
                if (type == typeof(bool))
                {
                    cell.SetCellValue(Convert.ToBoolean(value));
                }
                else if (type == typeof(double))
                {
                    cell.SetCellValue(Convert.ToDouble(value));
                }
                else if (type == typeof(int))
                {
                    cell.SetCellValue(Convert.ToDouble(value));
                }
                else if (type == typeof(string))
                {
                    cell.SetCellValue(value as string);
                }
                else if (type == typeof(DateTime))
                {
                    cell.SetCellValue(Convert.ToDateTime(value));
                }
                else
                {
                    cell.SetCellValue(value.ToString());
                }
            }
        }

        private HSSFWorkbook CreateNewWorkbook()
        {
            HSSFWorkbook hssfworkbook = new HSSFWorkbook();

            // create a entry of DocumentSummaryInformation
            DocumentSummaryInformation documentInfo = PropertySetFactory.CreateDocumentSummaryInformation();
            documentInfo.Company = "";
            hssfworkbook.DocumentSummaryInformation = documentInfo;

            // create a entry of SummaryInformation
            SummaryInformation summaryInfo = PropertySetFactory.CreateSummaryInformation();
            summaryInfo.Subject = string.Empty;
            summaryInfo.ApplicationName = string.Empty;
            summaryInfo.Author = string.Empty;
            summaryInfo.Title = string.Empty;
            hssfworkbook.SummaryInformation = summaryInfo;

            return hssfworkbook;
        }

        private CellStyle GetHeaderCellStyle()
        {
            var headerLabelCellStyle = this.workbook.CreateCellStyle();
            // headerLabelCellStyle.BorderBottom = CellBorderType.THIN;
            var headerLabelFont = this.workbook.CreateFont();
            headerLabelFont.Boldweight = (short)FontBoldWeight.BOLD;
            headerLabelCellStyle.SetFont(headerLabelFont);
            return headerLabelCellStyle;
        }

        private HSSFWorkbook OpenExcelFile(string filePath)
        {
            HSSFWorkbook hssfworkbookLocal = null;
            try
            {
                FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                hssfworkbookLocal = new HSSFWorkbook(fileStream);
                fileStream.Close();
            }
            catch
            {
            }

            return hssfworkbookLocal;
        }

        private void WriteExcelFile(string filePath, HSSFWorkbook hssfworkbook)
        {
            FileStream file = new FileStream(filePath, FileMode.Create);
            hssfworkbook.Write(file);
            file.Close();
        }

        // **************************************************************

        private readonly Dictionary<string, CellStyle> cellStyleCache = new Dictionary<string, CellStyle>();

        private CellStyle GetCellStyleForFormat(Workbook workbook, string dataFormat)
        {
            if (!this.cellStyleCache.ContainsKey(dataFormat))
            {
                var style = workbook.CreateCellStyle();

                // check if this is a built-in format
                var builtinFormatId = HSSFDataFormat.GetBuiltinFormat(dataFormat);

                if (builtinFormatId != -1)
                {
                    style.DataFormat = builtinFormatId;
                }
                else
                {
                    // not a built-in format, so create a new one
                    var newDataFormat = workbook.CreateDataFormat();
                    style.DataFormat = newDataFormat.GetFormat(dataFormat);
                }

                this.cellStyleCache[dataFormat] = style;
            }

            return this.cellStyleCache[dataFormat];
        }

        private bool IsCellDateFormatted(Cell cell)
        {
            if (cell == null)
            {
                return false;
            }

            bool isDateFormatted = false;

            int dataFormat = cell.CellStyle.DataFormat;
            string dataFormatString = cell.CellStyle.GetDataFormatString();

            if (dataFormatString.ContainsAny(new[] { "mm", "dd", "yy", "hh", "ss", "tt", "Date" }))
            {
                isDateFormatted = true;
            }

            return isDateFormatted;
        }

        /// <summary>
        /// Returns a default format string based on the object type of value.
        ///
        /// http://poi.apache.org/apidocs/org/apache/poi/ss/usermodel/BuiltinFormats.html
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        private string GetDefaultDataFormat(object value)
        {
            if (value == null)
            {
                return "General";
            }

            if (value is DateTime)
            {
                return "m/d";
            }

            if (value is bool)
            {
                return "[=0]\"Yes\";[=1]\"No\"";
            }

            if (value is byte || value is ushort || value is short ||
                 value is uint || value is int || value is ulong || value is long)
            {
                return "0";
            }

            if (value is float || value is double)
            {
                return "0.00";
            }

            // strings and anything else should be text
            return "text";
        }

        ///// <summary>
        ///// Writes to file data.
        ///// </summary>
        ///// <param name="filePath">The file path.</param>
        ///// <param name="hssfworkbook">The hssfworkbook.</param>
        ///// <returns>das Memory Fileobjekt</returns>
        //private FileData WriteToFileData(string filePath, HSSFWorkbook hssfworkbook)
        //{
        //    FileData result = new FileData();

        //    result.FileName = Path.GetFileName(filePath);

        //    if (hssfworkbook.DocumentSummaryInformation == null || hssfworkbook.SummaryInformation == null)
        //    {
        //        this.CreateSummaryInformation(hssfworkbook);
        //    }

        //    MemoryStream memStream = new MemoryStream();
        //    hssfworkbook.Write(memStream);
        //    result.Data = memStream.ToArray();
        //    memStream.Close();

        //    return result;
        //}

        //using System.IO;
        //using NPOI.HSSF.UserModel;
        //using NPOI.HPSF;
        //using NPOI.POIFS.FileSystem;
        //using NPOI.HSSF.Util;
        //using System.Data; //For DataTable

        //static MemoryStream WriteToStream(HSSFWorkbook hssfworkbook)
        //{
        //    //Write the stream data of workbook to the root directory
        //    MemoryStream file = new MemoryStream();
        //    hssfworkbook.Write(file);
        //    return file;
        //}

        //public void ExportDataTableToWorkbook(DataTable exportData, string sheetName)
        //{
        //    // Create the header row cell style
        //    var headerLabelCellStyle = this.Workbook.CreateCellStyle();
        //    headerLabelCellStyle.BorderBottom = CellBorderType.THIN;
        //    var headerLabelFont = this.Workbook.CreateFont();
        //    headerLabelFont.Boldweight = (short)FontBoldWeight.BOLD;
        //    headerLabelCellStyle.SetFont(headerLabelFont);

        //    var sheet = CreateExportDataTableSheetAndHeaderRow(exportData, sheetName, headerLabelCellStyle);
        //    var currentNPOIRowIndex = 1;
        //    var sheetCount = 1;

        //    for (var rowIndex = 0; rowIndex < exportData.Rows.Count; rowIndex++)
        //    {
        //        if (currentNPOIRowIndex >= MaximumNumberOfRowsPerSheet)
        //        {
        //            sheetCount++;
        //            currentNPOIRowIndex = 1;

        //            sheet = CreateExportDataTableSheetAndHeaderRow(exportData,
        //                                                            sheetName + " - " + sheetCount,
        //                                                            headerLabelCellStyle);
        //        }

        //        var row = sheet.CreateRow(currentNPOIRowIndex++);

        //        for (var colIndex = 0; colIndex < exportData.Columns.Count; colIndex++)
        //        {
        //            var cell = row.CreateCell(colIndex);
        //            cell.SetCellValue(exportData.Rows[rowIndex][colIndex].ToString());
        //        }
        //    }
        //}

        //protected Sheet CreateExportDataTableSheetAndHeaderRow(DataTable exportData, string sheetName, CellStyle headerRowStyle)
        //{
        //    var sheet = this.Workbook.CreateSheet(EscapeSheetName(sheetName));

        //    // Create the header row
        //    var row = sheet.CreateRow(0);

        //    for (var colIndex = 0; colIndex < exportData.Columns.Count; colIndex++)
        //    {
        //        var cell = row.CreateCell(colIndex);
        //        cell.SetCellValue(exportData.Columns[colIndex].ColumnName);

        //        if (headerRowStyle != null)
        //            cell.CellStyle = headerRowStyle;
        //    }

        //    return sheet;
        //}
    }
}