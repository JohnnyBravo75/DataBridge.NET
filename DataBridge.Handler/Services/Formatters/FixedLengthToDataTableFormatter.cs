using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using DataBridge.Extensions;
using DataBridge.Helper;
using DataBridge.Models;

namespace DataBridge.Formatters
{
    public class FixedLengthToDataTableFormatter : FormatterBase
    {
        private FieldDefinitionList fieldDefinitions = new FieldDefinitionList();

        public FixedLengthToDataTableFormatter()
        {
        }

        public FieldDefinitionList FieldDefinitions
        {
            get { return this.fieldDefinitions; }
        }

        public override object Format(object data, object existingData = null)
        {
            IList<string> lines = null;
            var table = existingData as DataTable;

            if (data is string)
            {
                lines = new List<string>();
                lines.Add(data as string);
            }
            else if (data is IEnumerable<string>)
            {
                lines = data as IList<string>;
            }

            if (lines != null)
            {
                if (this.FieldDefinitions == null || this.FieldDefinitions.Count == 0)
                {
                    this.AutoDetectSettings(lines);
                }

                table = this.FormatToDataTable(lines, table);
            }

            return table;
        }

        private DataTable FormatToDataTable(IList<string> lines, DataTable table)
        {
            bool isHeader = true;

            foreach (var line in lines)
            {
                List<string> values = this.SplitLine(line, this.FieldDefinitions);

                if (table == null)
                {
                    table = new DataTable();
                    DataTableHelper.CreateTableColumns(table, values, isHeader);
                }
                else
                {
                    DataTableHelper.AddTableRow(table, values);
                }
            }
            return table;
        }

        /// <summary>
        /// Splits a line based on the length in the field definitions.
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="fieldDefinitions">The field definitions with the length info.</param>
        /// <returns>The fields</returns>
        private List<string> SplitLine(string line, FieldDefinitionList fieldDefinitions)
        {
            var fieldArray = new List<string>();

            int startIndex = 0;
            foreach (var fieldDef in fieldDefinitions)
            {
                // cancel, when line shorter than the next field
                if (line.Length < startIndex)
                {
                    break;
                }

                if (fieldDef.IsActive)
                {
                    // get the unpadded field (without the spaces and tabs at the end)
                    string unpaddedField = line.Truncate(startIndex, fieldDef.DataSourceField.Length)
                                               .TrimEnd(new char[] { ' ', '\t' });

                    fieldArray.Add(unpaddedField);
                }

                startIndex += fieldDef.DataSourceField.Length;
            }

            return fieldArray;
        }

        private void AutoDetectSettings(IEnumerable<string> lines)
        {
            IEnumerable<Field> fieldList = this.AutoDetectColumns(lines);

            this.FieldDefinitions.Clear();
            int i = 0;
            foreach (Field field in fieldList)
            {
                if (this.FieldDefinitions.Count > i && this.FieldDefinitions[i] != null)
                {
                    this.FieldDefinitions[i].DataSourceField = field;
                }
                else
                {
                    this.FieldDefinitions.Add(new FieldDefinition(field));
                }

                i++;
            }
        }

        /// <summary>
        /// Autodetect columns, by looking for significant differences in the count of spaces.
        /// e.g
        /// SpaceCount for the positions
        /// 000000123000012223
        /// Charlie Harper
        /// Alan    Harper
        /// Walden  Schmidt
        ///
        /// When the spacecount changes significant, like from the 3 to 0 in the first line, the it is propably a new colum
        /// </summary>
        /// <returns></returns>
        private IEnumerable<Field> AutoDetectColumns(IEnumerable<string> lines)
        {
            var tableColumnList = new List<Field>();
            bool hasHeader = true;
            var rows = lines.ToList();

            // look in down in each position and count the spaces in the columnposition
            int lineLength = rows[0].Length;
            int[] spaceCount = new int[lineLength];

            // for each horizontal position
            for (int x = 0; x < lineLength; x++)
            {
                // look downwards in all rows
                for (int y = 0; y < rows.Count; y++)
                {
                    if (!string.IsNullOrEmpty(rows[y]))
                    {
                        // is there a whitespace?
                        if (rows[y].Length > x
                            && string.IsNullOrWhiteSpace(rows[y][x].ToStringOrEmpty()))
                        {
                            spaceCount[x]++;
                        }
                    }
                }
            }

            // analyse and try to find the columns
            int lastColumnPos = -1;
            int columnPos = 0;
            int spaceCountNoColumn = (int)Math.Round(0.7 * rows.Count, MidpointRounding.AwayFromZero);
            int spaceCountColumn = (int)Math.Round(0.05 * rows.Count, MidpointRounding.AwayFromZero);

            for (int x = 0; x < lineLength; x++)
            {
                // when a lot of spaces in the column occur and in the next column not, then this is a new column
                if ((x > 0 && spaceCount[x - 1] >= spaceCountNoColumn && spaceCount[x] <= spaceCountColumn)
                    || (x == lineLength - 1))
                {
                    lastColumnPos = columnPos;
                    columnPos = x;

                    int length = columnPos - lastColumnPos;

                    // append one at the last field (to correct the length)
                    if (x == lineLength - 1)
                    {
                        length++;
                    }

                    string fieldName = "Column" + (tableColumnList.Count + 1);
                    if (hasHeader)
                    {
                        fieldName = rows[0].Truncate(lastColumnPos, length)
                                           .TrimEnd(new char[] { ' ', '\t' });
                    }

                    tableColumnList.Add(new Field(fieldName, length));
                }
            }

            return tableColumnList;
        }
    }
}