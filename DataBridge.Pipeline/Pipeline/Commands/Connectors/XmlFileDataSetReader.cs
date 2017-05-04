using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DataPipeline.Extensions;

namespace DataPipeline.Commands
{
    public class XmlFileDataSetReader : PipelineCommand
    {
        private int rowIndex = 0;
        private DataTable headerTable;
        private string lastFile = "";

        public XmlFileDataSetReader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File" });
            this.Parameters.Add(new CommandParameter() { Name = "RowGrouper" });
        }

        public override void Initialize()
        {
            rowIndex = 0;
            headerTable = null;
            base.Initialize();
        }

        public override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string rowGrouper = inParameters.GetValue<string>("RowGrouper");

            // new File
            if (file != lastFile)
            {
                headerTable = null;
                lastFile = file;
            }

            DataTable table = new DataTable();
            DataSet dataSet = new DataSet();
            bool UseAttributes = true;

            using (XmlReader xmlReader = XmlReader.Create(file, new XmlReaderSettings { ProhibitDtd = false }))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.Name.Equals(rowGrouper) && (xmlReader.NodeType == XmlNodeType.Element))
                    {
                        var rowElement = (XElement)XElement.ReadFrom(xmlReader);

                        dataSet.ReadXml(new StringReader(rowElement.ToStringOrEmpty()), XmlReadMode.InferSchema);

                        //var rowValues = ReadRowValues(rowElement, UseAttributes);

                        //var row = table.NewRow();
                        //foreach (var field in rowValues)
                        //{
                        //    // add column when not exist
                        //    if (!table.Columns.Contains(field.Key))
                        //    {
                        //        table.Columns.Add(field.Key);
                        //    }

                        //    row[field.Key] = field.Value;
                        //}

                        //table.Rows.Add(row);
                    }

                    //this.Table.Value = table;

                    //var outParameters = new ConnectionParameters();
                    //outParameters.Add(new ConnectionParameter() { Name = "Table", Value = table });
                    //yield return outParameters;

                    foreach (DataTable dsTable in dataSet.Tables)
                    {
                        var outParameters = GetCurrentOutParameters();
                        outParameters.Add(new CommandParameter() { Name = "Data", Value = dsTable });
                        yield return TransferOutParameters(outParameters);
                    }
                }
            }
        }

        private Dictionary<string, string> ReadRowValues(XElement xmlRow, bool UseAttributes = false)
        {
            var rowValues = new Dictionary<string, string>();

            if (UseAttributes)
            {
                // take the attribute values
                foreach (var attr in xmlRow.Attributes())
                {
                    rowValues.Add(attr.Name.LocalName, attr.Value);
                }
            }
            else
            {
                // take the child nodes
                foreach (var child in xmlRow.Elements())
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        rowValues.Add(child.Name.LocalName, child.Value);
                    }
                }
            }

            return rowValues;
        }
    }
}