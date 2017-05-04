using System;
using System.ServiceModel;
using System.Xml.Serialization;
using DataBridge.Utils;

namespace DataBridge.Formatters
{
    [Serializable]
    [ServiceKnownType("GetKnownTypes", typeof(KnownTypesProvider))]
    public abstract class FormatterBase
    {
        private FormatterOptions formatterOptions = new FormatterOptions();

        [XmlArray("Options", IsNullable = false)]
        public FormatterOptions FormatterOptions
        {
            get { return this.formatterOptions; }
            set { this.formatterOptions = value; }
        }

        public virtual object Format(object data, object existingData = null)
        {
            return data;
        }

        public override string ToString()
        {
            return this.GetType().Name;
        }
    }
}