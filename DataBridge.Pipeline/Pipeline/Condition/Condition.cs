using System.Xml.Serialization;

namespace DataBridge
{
    public class Condition
    {
        private ConditionOperators @operator = ConditionOperators.Equals;

        private string value = "";
        private string valueToken = "";
        private string token = "";

        [XmlAttribute]
        public string Token
        {
            get { return this.token; }
            set { this.token = value; }
        }

        [XmlAttribute]
        public ConditionOperators Operator
        {
            get
            {
                return this.@operator;
            }
            set
            {
                this.@operator = value;
            }
        }

        [XmlAttribute]
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }

        [XmlAttribute]
        public string ValueToken
        {
            get { return this.valueToken; }
            set { this.valueToken = value; }
        }
    }
}