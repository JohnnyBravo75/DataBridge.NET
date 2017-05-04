namespace DataBridge
{
    public class XmlNameSpace
    {
        private string prefix = "";
        private string nameSpace = "";

        public string Prefix
        {
            get { return this.prefix; }
            set { this.prefix = value; }
        }

        public string NameSpace
        {
            get { return this.nameSpace; }
            set { this.nameSpace = value; }
        }

        public override string ToString()
        {
            return this.Prefix + " " + this.NameSpace;
        }
    }
}