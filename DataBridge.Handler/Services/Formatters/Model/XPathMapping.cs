namespace DataBridge.Formatters
{
    public class XPathMapping
    {
        private string column = "";
        private string xPath = "";

        public string Column
        {
            get { return this.column; }
            set { this.column = value; }
        }

        public string XPath
        {
            get { return this.xPath; }
            set { this.xPath = value; }
        }

        //public string Namespace { get; set; }

        //public string NamespacePrefix { get; set; }

        public override string ToString()
        {
            return this.Column + " <-> " + this.XPath;
        }
    }
}