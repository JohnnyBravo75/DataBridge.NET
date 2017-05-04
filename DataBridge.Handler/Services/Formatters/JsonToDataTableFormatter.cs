using System;
using System.Data;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace DataBridge.Formatters
{
    public class JsonToDataTableFormatter : FormatterBase
    {
        public JsonToDataTableFormatter()
        {
            this.FormatterOptions.Add(new FormatterOption() { Name = "RowXPath" });
            this.FormatterOptions.Add(new FormatterOption() { Name = "UseAttributes", Value = false });
            this.FormatterOptions.Add(new FormatterOption() { Name = "RemoveNamespaces", Value = false });
        }

        [XmlIgnore]
        public string RowXPath
        {
            get { return this.FormatterOptions.GetValue<string>("RowXPath"); }
            set { this.FormatterOptions.SetOrAddValue("RowXPath", value); }
        }

        [XmlIgnore]
        public bool UseAttributes
        {
            get { return this.FormatterOptions.GetValue<bool>("UseAttributes"); }
            set { this.FormatterOptions.SetOrAddValue("UseAttributes", value); }
        }

        [XmlIgnore]
        public bool RemoveNamespaces
        {
            get { return this.FormatterOptions.GetValue<bool>("RemoveNamespaces"); }
            set { this.FormatterOptions.SetOrAddValue("RemoveNamespaces", value); }
        }

        public override object Format(object data, object existingData = null)
        {
            var existingTable = existingData as DataTable;

            object result = null;
            string json = null;

            if (data is string)
            {
                json = data as string;
            }
            else if (data is Stream)
            {
                json = new StreamReader(data as Stream).ReadToEnd();
            }
            else if (data is object)
            {
                json = JsonConvert.SerializeObject(data);
            }

            if (!string.IsNullOrEmpty(json))
            {
                // JSON -> XML
                XmlDocument xmlDoc = null;
                bool hasDummyRoot = false;

                try
                {
                    xmlDoc = JsonConvert.DeserializeXmlNode(json);
                }
                catch (Exception)
                {
                    hasDummyRoot = true;
                    xmlDoc = JsonConvert.DeserializeXmlNode("{\"root\":" + json + "}");
                }

                // XML -> Datatable
                var xmlDataTableFormatter = new XmlToDataTableFormatter();
                xmlDataTableFormatter.RowXPath = hasDummyRoot ? ("/root" + this.RowXPath) : this.RowXPath;
                xmlDataTableFormatter.UseAttributes = this.UseAttributes;
                xmlDataTableFormatter.RemoveNamespaces = this.RemoveNamespaces;

                result = xmlDataTableFormatter.Format(xmlDoc, existingTable);
            }

            return result;
        }
    }
}