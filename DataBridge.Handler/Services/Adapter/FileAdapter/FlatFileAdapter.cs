using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.ConnectionInfos;
using DataBridge.Formatters;
using DataBridge.Helper;

namespace DataBridge.Handler.Services.Adapter
{
    public class FlatFileAdapter : DataAdapterBase
    {
        private FormatterBase readFormatter = new FlatFileToDataTableFormatter();
        private FormatterBase writeFormatter = new DefaultFormatter();
        private string recordSeperator = Environment.NewLine;
        private FileConnectionInfoBase connectionInfo;

        public FileConnectionInfoBase ConnectionInfo
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

        public FlatFileAdapter()
        {
            this.ConnectionInfo = new FlatFileConnectionInfo();
        }

        public FlatFileAdapter(string filenName) : this()
        {
            this.FileName = filenName;
        }

        [XmlAttribute]
        public string RecordSeperator
        {
            get { return this.recordSeperator; }
            set { this.recordSeperator = value; }
        }

        [XmlElement]
        public FormatterBase ReadFormatter
        {
            get { return this.readFormatter; }
            set { this.readFormatter = value; }
        }

        [XmlElement]
        public FormatterBase WriteFormatter
        {
            get { return this.writeFormatter; }
            set { this.writeFormatter = value; }
        }

        [XmlAttribute]
        public string FileName
        {
            get
            {
                if (!(this.ConnectionInfo is FlatFileConnectionInfo))
                {
                    return string.Empty;
                }
                return (this.ConnectionInfo as FlatFileConnectionInfo).FileName;
            }
            set { (this.ConnectionInfo as FlatFileConnectionInfo).FileName = value; }
        }

        [XmlIgnore]
        public Encoding Encoding
        {
            get { return (this.ConnectionInfo as FlatFileConnectionInfo).Encoding; }
            set { (this.ConnectionInfo as FlatFileConnectionInfo).Encoding = value; }
        }

        [XmlIgnore]
        public Stream DataStream { get; set; }

        private bool IsNewFile(string fileName)
        {
            // new File?
            if (!string.IsNullOrEmpty(fileName) && !File.Exists(fileName))
            {
                return true;
            }

            return false;
        }

        public override void Dispose()
        {
            if (this.DataStream != null)
            {
                // do not destroy stream I´m not the owner/creator
                //this.DataStream.Close();
                //this.DataStream.Dispose();
                this.DataStream = null;
            }
        }

        public override IEnumerable<DataTable> ReadData(int? blockSize = null)
        {
            this.ValidateAndThrow();

            StreamReader reader = null;
            if (!string.IsNullOrEmpty(this.FileName))
            {
                reader = new StreamReader(this.FileName, this.Encoding);
            }
            else if (this.DataStream != null)
            {
                reader = new StreamReader(this.DataStream);
            }

            DataTable headerTable = null;
            var lines = new List<string>();
            int readedRows = 0;
            int rowIdx = 0;
            DataTable table = new DataTable();
            while (!reader.EndOfStream)
            {
                var line = reader.ReadLine();
                lines.Add(line);
                rowIdx++;

                //if (this.skipRows > 0 && rowIdx < this.skipRows)
                //{
                //    continue;
                //}

                // first row (header?)
                if (readedRows == 0)
                {
                    DataTableHelper.DisposeTable(table);

                    table = headerTable != null
                        ? headerTable.Clone()
                        : null;
                }

                readedRows++;

                if (blockSize.HasValue && blockSize > 0 && readedRows >= blockSize)
                {
                    table = this.ReadFormatter.Format(lines, table) as DataTable;
                    if (table != null)
                    {
                        this.ReadConverter.ExecuteConverters(table);

                        table.TableName = Path.GetFileNameWithoutExtension(this.FileName);

                        if (headerTable == null)
                        {
                            headerTable = table.Clone();
                            lines.Clear();
                            continue;
                        }
                    }
                    else
                    {
                        table = new DataTable();
                    }

                    lines.Clear();
                    readedRows = 0;

                    yield return table;
                }
            }

            if (readedRows > 0 || table == null)
            {
                table = this.ReadFormatter.Format(lines, table) as DataTable;
                if (table != null)
                {
                    this.ReadConverter.ExecuteConverters(table);

                    table.TableName = Path.GetFileNameWithoutExtension(this.FileName);
                }
                else
                {
                    table = new DataTable();
                }

                yield return table;
            }

            if (reader != null)
            {
                reader.Close();
                reader.Dispose();
            }
        }

        public override IList<DataColumn> GetAvailableColumns()
        {
            this.ValidateAndThrow();

            IList<DataColumn> tableColumnList = new List<DataColumn>();

            var header = this.ReadData(1).FirstOrDefault();

            if (header != null)
            {
                foreach (DataColumn column in header.Columns)
                {
                    var field = new DataColumn(column.ColumnName);
                    tableColumnList.Add(field);
                }
            }

            return tableColumnList;
        }

        public override IList<string> GetAvailableTables()
        {
            this.ValidateAndThrow();

            IList<string> userTableList = new List<string>();

            if (!string.IsNullOrEmpty(this.FileName))
            {
                if (File.Exists(this.FileName))
                {
                    userTableList.Add(this.FileName);
                }
            }
            else if (this.DataStream is FileStream)
            {
                userTableList.Add((this.DataStream as FileStream).Name);
            }

            return userTableList;
        }

        public override int GetCount()
        {
            this.ValidateAndThrow();

            int count = 0;

            TextReader reader = null;

            if (!string.IsNullOrEmpty(this.FileName))
            {
                reader = new StreamReader(this.FileName);
            }
            else if (this.DataStream != null)
            {
                reader = new StreamReader(this.DataStream);
            }

            if (reader == null)
            {
                return count;
            }

            while (reader.ReadLine() != null)
            {
                count++;
            }

            reader.Close();
            reader.Dispose();

            return count;
        }

        public byte[] ReadBinaryData()
        {
            byte[] data = null;

            this.ValidateAndThrow();

            var fileName = this.FileName;

            BinaryReader reader = null;

            if (!string.IsNullOrEmpty(fileName))
            {
                reader = new BinaryReader(File.Open(fileName, FileMode.Open));
            }
            else if (this.DataStream != null)
            {
                reader = new BinaryReader(this.DataStream);
            }

            if (reader != null)
            {
                int length = (int)reader.BaseStream.Length;

                data = reader.ReadBytes(length);
                reader.Close();
                reader.Dispose();
            }

            return data;
        }

        public void WriteBinaryData(byte[] data, bool deleteBefore = false)
        {
            this.ValidateAndThrow();

            var fileName = this.FileName;

            if (!string.IsNullOrEmpty(fileName))
            {
                DirectoryUtil.CreateDirectoryIfNotExists(Path.GetDirectoryName(fileName));

                if (deleteBefore)
                {
                    FileUtil.DeleteFileIfExists(fileName);
                }
            }

            BinaryWriter writer = null;

            if (!string.IsNullOrEmpty(fileName))
            {
                writer = new BinaryWriter(File.Open(fileName, FileMode.Create));
            }
            else if (this.DataStream != null)
            {
                writer = new BinaryWriter(this.DataStream);
            }

            if (writer != null)
            {
                writer.Write(data);
                writer.Close();
                writer.Dispose();
            }
        }

        public override bool WriteData(IEnumerable<DataTable> tables, bool deleteBefore = false)
        {
            this.ValidateAndThrow();

            string lastFileName = "";
            string fileName = "";
            bool isNewFile = true;
            StreamWriter writer = null;

            foreach (DataTable table in tables)
            {
                if (writer == null || lastFileName != this.FileName)
                {
                    fileName = this.FileName;

                    if (string.IsNullOrEmpty(fileName))
                    {
                        fileName = table.TableName;
                    }

                    if (string.IsNullOrEmpty(fileName))
                    {
                        return false;
                    }

                    DirectoryUtil.CreateDirectoryIfNotExists(Path.GetDirectoryName(fileName));

                    if (deleteBefore)
                    {
                        FileUtil.DeleteFileIfExists(fileName);
                    }

                    isNewFile = this.IsNewFile(fileName);

                    if (writer != null)
                    {
                        writer.Flush();
                        writer.Close();
                        writer.Dispose();
                    }

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        writer = new StreamWriter(fileName, !isNewFile, this.Encoding);
                    }
                    else if (this.DataStream != null)
                    {
                        writer = new StreamWriter(this.DataStream, this.Encoding);
                    }

                    lastFileName = fileName;
                }

                writer.NewLine = this.recordSeperator;

                var lines = this.WriteFormatter.Format(table) as IEnumerable<string>;

                int writtenRows = 0;
                int rowIdx = 0;

                if (lines != null)
                {
                    foreach (var line in lines)
                    {
                        if (!isNewFile && rowIdx == 0)
                        {
                            // skip header when it is no new fileName
                            rowIdx++;
                            continue;
                        }

                        writer.WriteLine(line);

                        if (writtenRows % 100 == 0)
                        {
                            writer.Flush();
                        }

                        writtenRows++;
                        rowIdx++;
                    }

                    writer.Flush();
                }

                isNewFile = this.IsNewFile(fileName);
            }

            if (writer != null)
            {
                writer.Close();
                writer.Dispose();
            }

            return true;
        }

        public void AutoDetectEncoding()
        {
            this.Encoding = this.AutoDetectEncoding(this.FileName);
        }

        public Encoding AutoDetectEncoding(string fileName)
        {
            this.ValidateAndThrow();

            Encoding encoding = Encoding.Default;

            try
            {
                if (!string.IsNullOrEmpty(fileName))
                {
                    using (Stream reader = File.OpenRead(fileName))
                    {
                        encoding = EncodingUtil.DetectEncoding(reader);
                    }
                }
                else if (this.DataStream != null)
                {
                    encoding = EncodingUtil.DetectEncoding(this.DataStream);
                }
            }
            catch (Exception ex)
            {
                encoding = Encoding.Default;
            }

            return encoding;
        }

        public IList<string> Validate()
        {
            var messages = new List<string>();

            if (string.IsNullOrEmpty(this.FileName) && this.DataStream == null)
            {
                messages.Add("FileName or DataStream must not be null");
            }

            return messages;
        }

        private void ValidateAndThrow()
        {
            var messages = this.Validate();
            if (messages.Any())
            {
                throw new Exception(messages.First());
            }
        }
    }
}