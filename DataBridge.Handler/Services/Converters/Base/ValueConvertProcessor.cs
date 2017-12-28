using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.Extensions;

namespace DataBridge.Handler.Services.Converters
{
    public class ValueConvertProcessor
    {
        private ObservableCollection<ValueConverterDefinition> converterDefinitions = new ObservableCollection<ValueConverterDefinition>();

        private CultureInfo defaultCulture = null;

        private readonly ConvertDirections convertDirection;

        public ValueConvertProcessor()
        {
        }


        public ValueConvertProcessor(ConvertDirections convertDirection)
        {
            this.convertDirection = convertDirection;
        }

        public enum ConvertDirections
        {
            Read,
            Write
        }

        public ObservableCollection<ValueConverterDefinition> ConverterDefinitions
        {
            get { return this.converterDefinitions; }
            set { this.converterDefinitions = value; }
        }

        public string CountryColumnName { get; set; }

        public string LanguageColumnName { get; set; }

        [XmlAttribute]
        public string Culture
        {
            get
            {
                return this.DefaultCulture.ToStringOrEmpty();
            }
            set
            {
                this.DefaultCulture = new CultureInfo(value);
            }

        }

        [XmlIgnore]
        public CultureInfo DefaultCulture
        {
            get
            {
                if (this.defaultCulture == null)
                {
                    return CultureInfo.InvariantCulture;
                }
                return this.defaultCulture;
            }
            set { this.defaultCulture = value; }
        }

        public void ExecuteConverters(DataTable table)
        {
            if (table == null)
            {
                return;
            }

            if (this.converterDefinitions == null || !this.converterDefinitions.Any())
            {
                return;
            }

            foreach (DataRow row in table.Rows)
            {
                this.ExecuteConverters(row);
            }
        }

        public void ExecuteConverters(object obj)
        {
            if (obj is DataTable)
            {
                this.ExecuteConverters(obj as DataTable);
            }
            else if (obj is DataSet)
            {
                var dataSet = obj as DataSet;
                foreach (DataTable table in dataSet.Tables)
                {
                    this.ExecuteConverters(table);
                }
            }
        }

        protected void ExecuteConverters(DataRow row)
        {
            if (row == null)
            {
                return;
            }

            if (this.converterDefinitions == null || !this.converterDefinitions.Any())
            {
                return;
            }

            var culture = this.GetCulture(row);

            // when converters exists, convert the value
            foreach (var converterDef in this.converterDefinitions)
            {
                // Converter for specific field?
                if (!string.IsNullOrEmpty(converterDef.FieldName))
                {
                    switch (this.convertDirection)
                    {
                        case ConvertDirections.Read:
                            row[converterDef.FieldName] = converterDef.Converter.Convert(row[converterDef.FieldName], null, converterDef.ConverterParameter, culture);
                            break;

                        case ConvertDirections.Write:
                            row[converterDef.FieldName] = converterDef.Converter.ConvertBack(row[converterDef.FieldName], null, converterDef.ConverterParameter, culture);
                            break;
                    }
                }
            }
        }

        private CultureInfo GetCulture(DataRow row)
        {
            string cultureString = "";

            if (!string.IsNullOrEmpty(this.CountryColumnName) &&
                this.CountryColumnName == this.LanguageColumnName)
            {
                cultureString = row[this.CountryColumnName].ToStringOrEmpty();
            }
            else
            {
                // Is the country "US" in a column?
                string countryString = "";
                if (!string.IsNullOrEmpty(this.CountryColumnName))
                {
                    countryString = row[this.CountryColumnName].ToStringOrEmpty();
                }

                // Is the language "en" in a column?
                string languageString = "";
                if (!string.IsNullOrEmpty(this.LanguageColumnName))
                {
                    languageString = row[this.LanguageColumnName].ToStringOrEmpty();
                }

                // language an country? -> build "en-US"
                if (!string.IsNullOrEmpty(languageString) && !string.IsNullOrEmpty(countryString))
                {
                    cultureString = languageString.ToLower() + "-" + countryString;
                }
                else
                {
                    // otherwise just take "US"
                    cultureString = countryString;
                }
            }

            var culture = CultureUtil.GetCultureFromString(cultureString);
            if (culture == null)
            {
                culture = this.DefaultCulture;
            }

            return culture;
        }
    }
}