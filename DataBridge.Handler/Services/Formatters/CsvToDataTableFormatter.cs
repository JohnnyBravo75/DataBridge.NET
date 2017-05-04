using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using DataBridge.Helper;
using DataBridge.Models;

namespace DataBridge.Formatters
{
    public class CsvToDataTableFormatter : FormatterBase
    {
        private FieldDefinitionList fieldDefinitions = new FieldDefinitionList();

        private List<string> errorData = new List<string>();

        [Browsable(false)]
        public List<string> ErrorData
        {
            get { return this.errorData; }
            private set { this.errorData = value; }
        }

        public CsvToDataTableFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "Separator", Value = ";" });
            this.FormatterOptions.Add(new FormatterOption() { Name = "Enclosure", Value = "" });
        }

        public FieldDefinitionList FieldDefinitions
        {
            get { return this.fieldDefinitions; }
            set { this.fieldDefinitions = value; }
        }


        [XmlIgnore]
        public string Separator
        {
            get { return this.FormatterOptions.GetValue<string>("Separator"); }
            set { this.FormatterOptions.SetOrAddValue("Separator", value); }
        }

        [XmlIgnore]
        public string Enclosure
        {
            get { return this.FormatterOptions.GetValue<string>("Enclosure"); }
            set { this.FormatterOptions.SetOrAddValue("Enclosure", value); }
        }

        public override object Format(object data, object existingData = null)
        {
            string separator = this.FormatterOptions.GetValue<string>("Separator");
            IList<string> lines = null;
            this.errorData.Clear();

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
                if (string.IsNullOrEmpty(separator))
                {
                    this.AutoDetectSettings(lines);
                }

                table = this.FormatToDataTable(lines, table);
            }

            return table;
        }

        /// <summary>
        /// Validates the header, when FieldDefinitions exist
        /// </summary>
        /// <param name="headers">The header fields.</param>
        /// <returns></returns>
        private bool ValidateHeader(List<string> headers)
        {
            if (this.FieldDefinitions.Any())
            {
                int i = 0;
                foreach (var fieldDef in this.FieldDefinitions)
                {
                    if (fieldDef.DataSourceField.Name.ToUpper() != headers[i].ToUpper())
                    {
                        return false;
                    }

                    i++;
                }

                return true;
            }

            return true;
        }

        private bool ValidateData(List<string> values, DataColumnCollection columns)
        {
            // Check field<->header count
            if (columns.Count > 0 &&
                columns.Count != values.Count())
            {
                return false;
            }

            if (this.FieldDefinitions.Any())
            {
                // Check field<->definition count
                if (this.FieldDefinitions.Count != values.Count())
                {
                    return false;
                }

                //int i = 0;
                //foreach (var fieldDef in this.FieldDefinitions)
                //{
                //    if (fieldDef.DataSourceField.Datatype == typeof(int))
                //    {
                //        if (ConvertExtensions.ChangeType(values[i], fieldDef.DataSourceField.Datatype) == null)
                //        {
                //            return false;
                //        }
                //    }
                //    else if (fieldDef.DataSourceField.Datatype == typeof(DateTime))
                //    {
                //        if (ConvertExtensions.ChangeType(values[i], fieldDef.DataSourceField.Datatype) == null)
                //        {
                //            return false;
                //        }
                //    }
                //    else
                //    {
                //        // ok
                //    }

                //    i++;
                //}
            }

            return true;
        }

        private DataTable FormatToDataTable(IList<string> lines, DataTable table)
        {
            string separator = this.FormatterOptions.GetValue<string>("Separator");
            string enclosure = this.FormatterOptions.GetValue<string>("Enclosure");
            bool isHeader = true;

            foreach (var line in lines)
            {
                List<string> values = this.SplitLine(line, separator[0], enclosure);

                if (table == null)
                {
                    table = new DataTable();

                    if (!this.ValidateHeader(values))
                    {
                        throw new Exception(string.Format("Fielddefinitions do not match. Data={0}", line));
                    }
                    else
                    {
                        DataTableHelper.CreateTableColumns(table, values, isHeader);
                    }
                }
                else
                {
                    if (!this.ValidateData(values, table.Columns))
                    {
                        this.ErrorData.Add(line);
                    }
                    else
                    {
                        DataTableHelper.AddTableRow(table, values);
                    }
                }
            }

            return table;
        }

        /// <summary>
        /// Splits a line by an given separator (recognizes quoting,...)
        /// </summary>
        /// <param name="line">The line.</param>
        /// <param name="separator">The separator.</param>
        /// <param name="enclosure">The enclosure.</param>
        /// <returns>
        /// the array of fields
        /// </returns>
        private List<string> SplitLine(string line, char separator, string enclosure)
        {
            var fieldArray = new List<string>();
            var field = new StringBuilder();
            Quotestatus status = Quotestatus.None;
            char lastChar = char.MinValue;
            char? quoteChar = null;
            if (!string.IsNullOrEmpty(enclosure))
            {
                quoteChar = enclosure[0];
            }
            bool quoted = quoteChar.HasValue;

            foreach (char currentChar in line)
            {
                string charType = "char";
                if (currentChar == separator)
                {
                    if (status == Quotestatus.Firstquote && quoted)
                    {
                        charType = "char";
                    }
                    else
                    {
                        charType = "separator";
                    }
                }

                if (currentChar == quoteChar && quoted)
                {
                    charType = "char";

                    if (status == Quotestatus.Firstquote)
                    {
                        charType = "quotes";
                        status = Quotestatus.Secondquote;
                    }

                    if (lastChar == separator || lastChar == char.MinValue)
                    {
                        if (status == Quotestatus.None)
                        {
                            status = Quotestatus.Firstquote;
                        }

                        charType = "quotes";
                    }
                }

                switch (charType)
                {
                    case "char":
                        field.Append(currentChar);
                        break;

                    case "separator":
                        fieldArray.Add(field.ToString());
                        field.Clear();
                        break;

                    case "quotes":
                        if (status == Quotestatus.Firstquote)
                        {
                            field.Clear();
                        }

                        if (status == Quotestatus.Secondquote)
                        {
                            status = Quotestatus.None;
                        }

                        break;
                }

                lastChar = currentChar;
            }

            if (field.Length != 0)
            {
                fieldArray.Add(field.ToString());
            }

            return fieldArray;
        }

        private enum Quotestatus
        {
            None,
            Firstquote,
            Secondquote
        }

        private void AutoDetectSettings(IEnumerable<string> lines)
        {
            this.Separator = this.AutoDetectSeparator(lines);
            this.Enclosure = this.AutoDetectEnclosure(lines);
        }

        private string AutoDetectSeparator(IEnumerable<string> lines)
        {
            string detectedSeparator = "";

            if (lines == null)
            {
                return detectedSeparator;
            }

            // init the list with the separators to check
            var detectionList = new Dictionary<string, int>()
            {
                {",", 0},
                {";", 0},
                {"\t",0},
                {"|", 0}
            };

            try
            {
                var lineCount = 0;
                foreach (var line in lines)
                {
                    if (lineCount > 100)
                    {
                        break;
                    }

                    var separators = new List<string>(detectionList.Keys);

                    // loop through all item chars (e.g. "," ";" "\t" ...)
                    foreach (var separator in separators)
                    {
                        // the count e.g. "12"
                        int separatorCount = StringUtil.CountCharacter(line, separator[0]);

                        // count all characters in the string (curent row) and add them to the existing count
                        detectionList[separator] = detectionList[separator] + separatorCount;
                    }

                    lineCount++;
                }

                // get the separator how has the maximum count
                int maxValue = detectionList.Values.Max(x => x);
                var maxItem = detectionList.FirstOrDefault(x => x.Value == maxValue);
                detectedSeparator = maxItem.Key;
            }
            catch (Exception ex)
            {
                detectedSeparator = "";
            }

            return detectedSeparator;
        }

        private string AutoDetectEnclosure(IEnumerable<string> lines)
        {
            string detectedEnclosure = "";

            if (lines == null)
            {
                return detectedEnclosure;
            }

            string separator = this.Separator;

            // init the list with the separators to check
            var detectionList = new Dictionary<string, int>()
            {
                {"\"", 0},
                {"'", 0}
            };

            try
            {
                var lineCount = 0;
                foreach (var line in lines)
                {
                    if (lineCount > 100)
                    {
                        break;
                    }
                    var quotingChars = new List<string>(detectionList.Keys);

                    // loop through all item chars (e.g. "," ";" "\t" ...)
                    foreach (var quotingChar in quotingChars)
                    {
                        // detect quoting by following rules (one must match):
                        // 1. starts or ends with a quoting e.g. "id";"Name1";"Name2"
                        // 2. the row contains ";"
                        // 3. there are more than 6 quoting chars in the line
                        if (line.StartsWith(quotingChar) && line.EndsWith(quotingChar)
                        || (line.Contains(quotingChar + separator + quotingChar))
                        || (StringUtil.CountCharacter(line, quotingChar[0]) > 6)
                            )
                        {
                            detectionList[quotingChar] = detectionList[quotingChar] + 1;
                        }
                    }
                    lineCount++;
                }

                // get the separator how has the maximum count
                int maxValue = detectionList.Values.Max(x => x);
                var maxItem = detectionList.FirstOrDefault(x => x.Value == maxValue);
                detectedEnclosure = maxItem.Key;
            }
            catch (Exception ex)
            {
                detectedEnclosure = "";
            }

            return detectedEnclosure;
        }
    }
}