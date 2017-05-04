using System.Data;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataBridge.PropertyChanged;

namespace DataBridge
{
    [XmlType(TypeName = "Param")]
    public class Parameter : NotifyPropertyChangedBase
    {
        protected string name = "";
        protected Directions direction = Directions.InOut;
        protected string dataType = "String";
        protected object value = "";
        private bool notNull = false;

        [XmlAttribute]
        public string Name
        {
            get { return this.name; }
            set { this.name = value; }
        }

        [XmlIgnore]
        public object Value
        {
            get { return this.value; }
            set
            {
                this.value = value;
                this.DataType = value != null
                                    ? value.GetType().Name
                                    : "";
            }
        }

        [XmlIgnore]
        public bool HasValue
        {
            get
            {
                if (this.Value == null)
                {
                    return false;
                }

                if (string.IsNullOrEmpty(this.Value.ToStringOrEmpty()))
                {
                    return false;
                }

                return true;
            }
        }

        [XmlAttribute("Value")]
        public string StringValue
        {
            get { return this.value.ToStringOrEmpty(); }
            set { this.value = value; }
        }

        [XmlAttribute]
        public Directions Direction
        {
            get { return this.direction; }
            set { this.direction = value; }
        }

        [XmlAttribute]
        public bool NotNull
        {
            get { return this.notNull; }
            set { this.notNull = value; }
        }



        [XmlAttribute]
        public string DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }

        public DbType DbType
        {
            get
            {
                switch (this.DataType)
                {
                    case "String":
                        return DbType.String;

                    case "Number":
                        return DbType.Decimal;

                    case "Date":
                        return DbType.Date;

                    case "DateTime":
                        return DbType.DateTime;
                }

                return DbType.String;
            }
        }

        public bool ShouldSerializeDataType()
        {
            return this.DataType != "String";
        }

        public bool ShouldSerializeDirection()
        {
            return this.Direction != Directions.InOut;
        }

        public bool ShouldSerializeNotNull()
        {
            return this.NotNull != false;
        }
    }

    public enum Directions
    {
        In,
        Out,
        InOut
    }

    public enum DataType
    {
        String,
        Number,
        DateTime
    }
}