using System;
using System.Xml.Serialization;
using DataBridge.Extensions;

namespace DataBridge
{
    [XmlType(TypeName = "Parameter")]
    [Serializable]
    public class CommandParameter : Parameter
    {
        public CommandParameter()
        {
        }

        public CommandParameter(Parameter parameter = null)
        {
            if (parameter != null)
            {
                this.Name = parameter.Name;
                this.Direction = parameter.Direction;
                this.DataType = parameter.DataType;
                this.NotNull = parameter.NotNull;
                this.Value = parameter.Value;
            }
        }

        protected bool Equals(CommandParameter other)
        {
            return string.Equals(this.name, other.name) &&
                    string.Equals(this.direction, other.direction) &&
                    string.Equals(this.dataType, other.dataType) &&
                    Equals(this.value, other.value) &&
                    string.Equals(this.token, other.token);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (this.name != null ? this.name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.direction != null ? this.direction.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.dataType != null ? this.dataType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.value != null ? this.value.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (this.token != null ? this.token.GetHashCode() : 0);
                return hashCode;
            }
        }

        private string token = "";

        [XmlAttribute]
        public string Token
        {
            get
            {
                if (string.IsNullOrEmpty(this.token))
                {
                    return this.Name;
                }
                return this.token;
            }
            set { this.token = value; }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", this.Name, this.Value.ToStringOrEmpty());
        }

        public CommandParameter Clone()
        {
            var clone = new CommandParameter();
            clone.Name = this.name;
            clone.DataType = this.dataType;
            clone.Direction = this.direction;
            clone.Token = this.token;
            clone.Value = this.value;
            clone.UseEncryption = this.UseEncryption;

            return clone;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return this.Equals((CommandParameter)obj);
        }

        public bool ShouldSerializeToken()
        {
            return this.Token != this.Name;
        }
    }
}