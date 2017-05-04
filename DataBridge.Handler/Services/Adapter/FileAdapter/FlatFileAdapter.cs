using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.ConnectionInfos;
using DataBridge.Formatters;
using DataBridge.Helper;

namespace DataBridge
{
    public class FlatFileAdapter
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

        public string RecordSeperator
        {
            get { return this.recordSeperator; }
            set { this.recordSeperator = value; }
        }

        public FormatterBase ReadFormatter
        {
            get { return this.readFormatter; }
            set { this.readFormatter = value; }
        }

        public FormatterBase WriteFormatter
        {
            get { return this.writeFormatter; }
            set { this.writeFormatter = value; }
        }

        [XmlAttribute]
        public string FileName
        {
            get { return (this.ConnectionInfo as FlatFileConnectionInfo).FileName; }
            set { (this.ConnectionInfo as FlatFileConnectionInfo).FileName = value; }
        }

        [XmlAttribute]
        public Encoding Encoding
        {
            get { return (this.ConnectionInfo as FlatFileConnectionInfo).Encoding; }
            set { (this.ConnectionInfo as FlatFileConnectionInfo).Encoding = value; }
        }

        private bool IsNewFile(string fileName)
        {
            // new File?
            if (!string.IsNullOrEmpty(fileName) && !File.Exists(fileName))
            {
                return true;
            }

            return false;
        }

        public IEnumerable<DataTable> ReadData(int? blockSize = null)
        {
            DataTable headerTable = null;
            var lines = new List<string>();
            using (var reader = new StreamReader(this.FileName, this.Encoding))
            {
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
                        //if (rowIdx % Math.Min(this.StreamingBlockSize, 5000) == 0)
                        //{
                        //    this.LogDebug(string.Format("Read from fileName '{0}': Rows={1}", fileName, rowIdx));
                        //}

                        table = this.ReadFormatter.Format(lines, table) as DataTable;
                        if (table != null)
                        {
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
                        table.TableName = Path.GetFileNameWithoutExtension(this.FileName);
                    }
                    else
                    {
                        table = new DataTable();
                    }

                    yield return table;
                }

                DataTableHelper.DisposeTable(table);
            }
        }

        public void WriteBinaryData(object data, bool deleteBefore = false)
        {
            var fileName = this.FileName;

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            DirectoryUtil.CreateDirectoryIfNotExists(Path.GetDirectoryName(fileName));

            if (deleteBefore)
            {
                FileUtil.DeleteFileIfExists(fileName);
            }

            if (data is byte[])
            {
                using (FileStream stream = new FileStream(fileName, FileMode.Create))
                {
                    using (BinaryWriter writer = new BinaryWriter(stream))
                    {
                        writer.Write(data as byte[]);
                        writer.Close();
                    }
                }
            }
            else
            {
                using (var writer = new StreamWriter(fileName, true, this.Encoding))
                {
                    writer.Write(data);
                    writer.Close();
                }
            }
        }

        public void WriteData(DataTable table, bool deleteBefore = false)
        {
            var fileName = this.FileName;

            if (string.IsNullOrEmpty(fileName) && table != null)
            {
                fileName = table.TableName;
            }

            if (string.IsNullOrEmpty(fileName))
            {
                return;
            }

            DirectoryUtil.CreateDirectoryIfNotExists(Path.GetDirectoryName(fileName));

            if (deleteBefore)
            {
                FileUtil.DeleteFileIfExists(fileName);
            }

            var isNewFile = this.IsNewFile(fileName);

            using (var writer = new StreamWriter(fileName, !isNewFile, this.Encoding))
            {
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
            }
        }

        public Encoding AutoDetectEncoding(string fileName)
        {
            Encoding encoding = Encoding.Default;

            try
            {
                using (Stream reader = System.IO.File.OpenRead(fileName))
                {
                    encoding = EncodingUtil.DetectEncoding(reader);
                }
            }
            catch (Exception ex)
            {
                encoding = Encoding.Default;
            }

            return encoding;
        }
    }
}