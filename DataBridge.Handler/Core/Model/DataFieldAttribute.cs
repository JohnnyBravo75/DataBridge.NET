using System;

namespace DataBridge.Models
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class DataFieldAttribute : Attribute
    {
        public DataFieldAttribute(string name = "", string xPath = "", bool isRequired = false)
        {
            this.Name = name;
            this.XPath = xPath;
            this.IsRequired = isRequired;
        }

        public string Name { get; set; }

        public string XPath { get; set; }

        public bool IsRequired { get; set; }
    }
}