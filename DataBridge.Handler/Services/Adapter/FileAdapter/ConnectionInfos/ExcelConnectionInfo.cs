using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.ConnectionInfos
{
    [Serializable]
    public class ExcelConnectionInfo : FileConnectionInfoBase
    {
        private string sheetName = "";

        [XmlAttribute]
        public string SheetName
        {
            get { return this.sheetName; }
            set { this.sheetName = value; }
        }
    }
}