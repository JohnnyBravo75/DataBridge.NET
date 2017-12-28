namespace DataBridge.Handler.Services.Adapter
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Xml.Serialization;
    using DataBridge.Common.Services.Adapter;
    using DataBridge.Extensions;
    using DataBridge.Helper;
    using NPOI.HPSF;
    using NPOI.HSSF.UserModel;
    using NPOI.SS.UserModel;
    using DataBridge.ConnectionInfos;

    public class ExcelNativeAdapter : DataAdapterBase, IDataAdapterBase
    {
        // ***********************Fields***********************

        private HSSFWorkbook workbook;
        private Sheet sheet;
        private const int MAX_SHEET_ROWS = 65535;
        private const int MAX_SHEET_COLUMNS = 255;

        protected int importRowIndex = 0;
        protected StreamReader importReader;

        protected int exportRowIndex = 0;
        protected StreamWriter exportWriter;

        private bool applyCellStyles = true;

        private bool applyDataType = false;

        private FileConnectionInfoBase connectionInfo;

        // ***********************Constructors***********************

        public ExcelNativeAdapter()
        {
            this.ConnectionInfo = new ExcelConnectionInfo();
        }

        public ExcelNativeAdapter(string filenName, string sheetName = null) : this()
        {
            this.FileName = filenName;
            this.SheetName = sheetName;
        }

        [XmlElement]
        public FileConnectionInfoBase ConnectionInfo
        {
            get { return this.connectionInfo; }
            set { this.connectionInfo = value; }
        }

        [XmlIgnore]
        public Stream DataStream { get; set; }

        [XmlAttribute]
        public bool ApplyCellStyles
        {
            get { return this.applyCellStyles; }
            set { this.applyCellStyles = value; }
        }

        [XmlAttribute]
        public bool ApplyDataType
        {
            get { return this.applyDataType; }
            set { this.applyDataType = value; }
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

            this.workbook = this.OpenExcelFile();

            if (this.workbook == null)
            {
                throw new ArgumentNullException("Workbook", "The workbook was not found or could not be opened.");
            }

            this.IsConnected = true;

            return this.IsConnected;
        }

        public bool Disconnect()
        {
            if (this.workbook != null)
            {
                this.workbook.Dispose();
                this.workbook = null;
            }

            if (this.cellStyleCache != null)
            {
                this.cellStyleCache.Clear();
            }

            if (this.workbook == null)
            {
                this.IsConnected = false;
            }

            this.startX = 0;
            this.startY = 0;

            return (this.workbook == null);
        }

        public override void Dispose()
        {
            this.Disconnect();
        }

        public override IList<DataColumn> GetAvailableColumns()
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

        public override IList<string> GetAvailableTables()
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

        public override int GetCount()
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

                //if (excelRow.Cells.All(d => d.CellType == CellType.BLANK))
                //{
                //}
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

        public override IEnumerable<DataTable> ReadData(int? blockSize = null)
        {
            if (!this.IsConnected)
            {
                this.Connect();
            }

            this.ResetToStart();

            bool hasHeader = true;
            Row excelRow = null;
            Cell excelCell = null;
            DataRow tableRow = null;
            DataTable table = null;
            string tableName = this.SheetName;

            if (string.IsNullOrEmpty(this.SheetName))
            {
                throw new ArgumentNullException("SheetName", "Please provide a sheet name");
            }

            if (!this.IsConnected)
            {
                yield break;
            }

            // create a new datatable
            table = new DataTable(tableName);
            table.TableName = tableName;

            if (this.workbook == null)
            {
                yield break;
            }

            this.sheet = this.workbook.GetSheet(tableName);

            if (this.sheet == null)
            {
                throw new Exception(string.Format("The sheet '{0}' was not found.", this.SheetName));
            }

            int rowsRead = 0;
            // int y = 0;

            // loop the rows
            //while (maxRows == null || rowsRead < maxRows)
            for (int y = 0; y < 65536; y++)
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

        public void CreateNewFile()
        {
            this.Disconnect();

            this.workbook = this.CreateNewWorkbook();

            string tableName = this.SheetName;

            // make a new sheet
            tableName = string.IsNullOrEmpty(tableName) ? "Sheet1" : tableName;
            this.sheet = this.workbook.CreateSheet(tableName);

            this.WriteExcelFile(this.workbook);
        }

        public override bool WriteData(IEnumerable<DataTable> tables, bool deleteBefore = false)
        {
            if (!this.IsConnected)
            {
                this.Connect();
            }

            string tableName = this.SheetName;

            if (this.workbook == null)
            {
                this.workbook = this.CreateNewWorkbook();
            }

            if (!string.IsNullOrEmpty(tableName))
            {
                this.sheet = this.workbook.GetSheet(tableName);
            }

            if (this.sheet == null)
            {
                // make a new sheet
                this.sheet = this.workbook.CreateSheet(tableName);
            }

            int i = 0;
            foreach (DataTable table in tables)
            {
                this.startY = deleteBefore && i == 0 ? 0 : this.GetCount();

                bool hasCreatedHeader = false;

                if (this.startY == 0)
                {
                    this.CreateHeaderRow(table, this.startY);
                    hasCreatedHeader = true;
                }

                // loop through data
                for (int y = 0; y < table.Rows.Count; y++)
                {
                    var excelRow = this.sheet.GetRow(y + this.startY + (hasCreatedHeader ? 1 : 0));

                    if (excelRow == null)
                    {
                        excelRow = this.sheet.CreateRow(y + this.startY + (hasCreatedHeader ? 1 : 0));
                    }

                    for (int x = 0; x < table.Columns.Count; x++)
                    {
                        var excelCell = excelRow.GetCell(x + this.startX);

                        if (excelCell == null)
                        {
                            excelCell = excelRow.CreateCell(x + this.startX);
                        }

                        string columnName = table.Columns[x].ToString();
                        var value = table.Rows[y][columnName];

                        this.SetCellValue(excelCell, value, this.applyDataType);

                        string dataFormat = "text";
                        if (this.applyCellStyles)
                        {
                            dataFormat = this.GetDefaultDataFormat(value);
                        }

                        // find/create cell style
                        excelCell.CellStyle = this.GetCellStyleForFormat(this.sheet.Workbook, dataFormat);
                    }
                }
            }

            // Forcing formula recalculation
            this.sheet.ForceFormulaRecalculation = true;

            this.WriteExcelFile(this.workbook);

            return true;
        }

        private int startX = 0;
        private int startY = 0;

        private void CreateHeaderRow(DataTable table, int y = 0)
        {
            CellStyle headerCellStyle = this.applyCellStyles
                                                ? this.GetHeaderCellStyle()
                                                : (CellStyle)null;

            // get or create header row
            var headerRow = this.sheet.GetRow(y + this.startY);

            if (headerRow == null)
            {
                headerRow = this.sheet.CreateRow(y + this.startY);
            }

            // write all columns
            for (int x = 0; x < table.Columns.Count; x++)
            {
                Cell cell = headerRow.GetCell(x + this.startX);
                if (cell == null)
                {
                    cell = headerRow.CreateCell(x + this.startX);
                }

                string columnName = table.Columns[x].ToString();
                cell.SetCellValue(columnName);
                if (headerCellStyle != null)
                {
                    cell.CellStyle = headerCellStyle;
                }
            }
        }

        private void SetCellValue(Cell cell, object value, bool setCellType = false)
        {
            if (value != null && value != DBNull.Value)
            {
                if (setCellType)
                {
                    var type = value.GetType();
                    if (type == typeof(bool))
                    {
                        cell.SetCellValue(Convert.ToBoolean(value));
                        cell.SetCellType(CellType.BOOLEAN);
                    }
                    else if (type == typeof(double) || type == typeof(int) || type == typeof(float) || type == typeof(decimal))
                    {
                        cell.SetCellValue(Convert.ToDouble(value));
                        cell.SetCellType(CellType.NUMERIC);
                    }
                    else if (type == typeof(string))
                    {
                        cell.SetCellValue(value as string);
                        cell.SetCellType(CellType.STRING);
                    }
                    else if (type == typeof(DateTime))
                    {
                        cell.SetCellValue(Convert.ToDateTime(value));
                        cell.SetCellType(CellType.Unknown);
                    }
                    else
                    {
                        cell.SetCellValue(value.ToString());
                        cell.SetCellType(CellType.STRING);
                    }
                }
                else
                {
                    cell.SetCellValue(value.ToString());
                    cell.SetCellType(CellType.STRING);
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

        private HSSFWorkbook OpenExcelFile()
        {
            HSSFWorkbook workBook = null;

            if (!string.IsNullOrEmpty(this.FileName))
            {
                using (var fileStream = new FileStream(this.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    workBook = new HSSFWorkbook(fileStream);
                    fileStream.Close();
                }
            }
            else
            {
                workBook = new HSSFWorkbook(this.DataStream);
            }

            return workBook;
        }

        private void WriteExcelFile(HSSFWorkbook workBook)
        {
            if (!string.IsNullOrEmpty(this.FileName))
            {
                using (var fileStream = new FileStream(this.FileName, FileMode.Create))
                {
                    workBook.Write(fileStream);
                    fileStream.Close();
                }
            }
            else
            {
                workBook.Write(this.DataStream);
            }
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
                return CultureInfo.CurrentCulture.DateTimeFormat.FullDateTimePattern;
                // return "yyyyMMdd HH:mm:ss";
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

            if (value is float || value is double || value is decimal)
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