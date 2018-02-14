using System;
using System.Collections.Generic;
using System.Data;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.Extensions;
using DataBridge.PropertyChanged;

namespace DataBridge
{
    [XmlType(TypeName = "Param")]
    public class Parameter : NotifyPropertyChangedBase
    {
        protected string name = "";
        protected Directions direction = Directions.InOut;
        protected DataTypes dataType = DataTypes.String;
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

                if (this.UseEncryption)
                {
                    this.value = EncryptionHelper.GetEncryptedString(this.value.ToStringOrEmpty());
                }

                if (value is string)
                {
                    this.DataType = DataTypes.String;
                }
                else if (value is byte || value is ushort || value is short ||
                    value is uint || value is int || value is ulong || value is long)
                {
                    this.DataType = DataTypes.Number;
                }
                else if (value is float || value is double || value is decimal)
                {
                    this.DataType = DataTypes.Number;
                }
                else if (value is DateTime)
                {
                    this.DataType = DataTypes.DateTime;
                }
                else
                {
                    this.DataType = DataTypes.Object;
                }
            }
        }

        public object GetEvaluatedValue(IDictionary<string, object> contextTokens = null)
        {
            var value = this.Value;

            if (this.UseEncryption && this.DataType == DataTypes.String)
            {
                value = EncryptionHelper.GetDecrptedString(value.ToStringOrEmpty());
            }

            if (this.IsValueExpression)
            {
                value = TokenProcessor.EvaluateExpression(value.ToStringOrEmpty(), contextTokens);
            }

            return value;
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

        [XmlAttribute]
        public bool UseEncryption { get; set; }

        [XmlIgnore]
        public string DecryptedStringValue
        {
            get { return EncryptionHelper.GetDecrptedString(this.StringValue); }
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
        public DataTypes DataType
        {
            get { return this.dataType; }
            set { this.dataType = value; }
        }

        [XmlIgnore]
        public DbType DbType
        {
            get
            {
                switch (this.DataType)
                {
                    case DataTypes.String:
                        return DbType.String;

                    case DataTypes.Number:
                        return DbType.Decimal;

                    case DataTypes.Boolean:
                        return DbType.Boolean;

                    case DataTypes.Object:
                        return DbType.Binary;

                    case DataTypes.DateTime:
                        return DbType.DateTime;

                    default:
                        return DbType.String;
                }
            }
        }

        [XmlAttribute]
        public bool IsValueExpression { get; set; }

        public bool ShouldSerializeDataType()
        {
            return this.DataType != DataTypes.String;
        }

        public bool ShouldSerializeDirection()
        {
            return true;
            // return this.Direction != Directions.InOut;
        }

        public bool ShouldSerializeNotNull()
        {
            return true;
            // return this.NotNull;
        }

        public bool ShouldSerializeUseEncryption()
        {
            return this.UseEncryption;
        }

        public bool ShouldSerializeIsValueExpression()
        {
            return this.IsValueExpression;
        }
    }

    public enum Directions
    {
        In,
        Out,
        InOut
    }

    public enum DataTypes
    {
        String,
        Number,
        DateTime,
        Object,
        Boolean
    }
}