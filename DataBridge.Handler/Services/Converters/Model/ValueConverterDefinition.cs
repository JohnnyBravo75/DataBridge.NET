using System;
using System.Xml.Serialization;
using DataBridge.Helper;
using DataBridge.PropertyChanged;

namespace DataBridge.Handler.Services.Converters
{
    [Serializable]
    public class ValueConverterDefinition : NotifyPropertyChangedBase
    {
        private string converterName = "";
        private string fieldName = "";
        private ValueConverterBase converter;

        // ***********************Constructors***********************

        public ValueConverterDefinition()
        {
        }

        public ValueConverterDefinition(string fieldName, Type converterType, string converterParameter) : this(fieldName, converterType.Name, converterParameter)
        {
        }

        public ValueConverterDefinition(string fieldName, string converterName, string converterParameter)
        {
            this.FieldName = fieldName;
            this.ConverterName = converterName;
            this.converter = GenericFactory.GetInstance<ValueConverterBase>(this.ConverterName);
            this.ConverterParameter = converterParameter;
        }

        public ValueConverterDefinition(string fieldName, ValueConverterBase converter, string converterParameter)
        {
            this.FieldName = fieldName;
            this.Converter = converter;
            this.ConverterParameter = converterParameter;
        }

        // ***********************Properties***********************

        [XmlAttribute]
        public string FieldName
        {
            get { return this.fieldName; }

            set
            {
                if (this.fieldName != value)
                {
                    this.fieldName = value;
                    this.RaisePropertyChanged("FieldName");
                }
            }
        }

        [XmlAttribute]
        public string ConverterName
        {
            get { return this.converterName; }

            set
            {
                if (this.converterName != value)
                {
                    this.converterName = value;
                    this.RaisePropertyChanged("ConverterName");
                }
            }
        }

        [XmlIgnore]
        public ValueConverterBase Converter
        {
            get
            {
                if (this.converter == null && !string.IsNullOrEmpty(this.ConverterName))
                {
                    this.converter = GenericFactory.GetInstance<ValueConverterBase>(this.ConverterName);
                }

                return this.converter;
            }

            set
            {
                if (this.converter != value)
                {
                    this.converter = value;
                    this.RaisePropertyChanged("Converter");
                }
            }
        }

        [XmlAttribute]
        public string ConverterParameter { get; set; }
    }
}