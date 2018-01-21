using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataBridge.Extensions;

namespace DataBridge
{
    public class DataMappings : ObservableCollection<DataMapping>
    {
        public DataMapping this[string name]
        {
            get
            {
                return this.Get(name);
            }

            set
            {
                value.Name = name;
                this.Set(value);
            }
        }

        public bool Exist(string name)
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);
            return (parameter != null);
        }

        public DataMapping Get(string name)
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);
            if (parameter == null)
            {
                throw new KeyNotFoundException(string.Format("The parameter '{0}' was not found", name));
            }

            return parameter;
        }

        public void Set(DataMapping value)
        {
            var item = this.FirstOrDefault(x => x.Name == value.Name);

            if (item != null)
            {
                int index = this.IndexOf(item);
                this[index] = value;
            }
            else
            {
                this.Add(value);
            }
        }

        public new void Add(DataMapping item)
        {
            this.AddOrUpdate(item);
        }

        public void Add(string name, object value)
        {
            var newParam = new DataMapping() { Name = name, Value = value };
            this.Add(newParam);
        }

        /// <summary>
        /// Adds the or update.
        /// </summary>
        /// <param name="item">The item.</param>
        public void AddOrUpdate(DataMapping item)
        {
            var parameter = this.FirstOrDefault(x => x.Name == item.Name);

            if (parameter != null)
            {
                // replace the parameter with the new one
                this[this.IndexOf(parameter)] = item;
            }
            else
            {
                // add the parameter to the list
                base.Add(item);
            }
        }

        public void AddOrUpdate(string name, object value)
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);

            if (parameter != null)
            {
                parameter.Value = value;
            }
            else
            {
                var newParam = new DataMapping() { Name = name, Value = value };
                this.Add(newParam);
            }
        }

        public void SetValue(string name, object value)
        {
            var parameter = this.First(x => x.Name == name);

            if (parameter == null)
            {
                return;
            }

            parameter.Value = value;
        }

        public object GetValueOrDefault(string name, object defaultValue = null)
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);

            if (parameter == null)
            {
                return defaultValue;
            }

            return parameter.Value;
        }

        public TResult GetValue<TResult>(string name)
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);

            if (parameter == null)
            {
                throw new KeyNotFoundException(string.Format("The parameter '{0}' was not found", name));
                //return default(TResult);
            }

            if (parameter.Value == null)
            {
                return default(TResult);
            }

            return (TResult)Convert.ChangeType(parameter.Value, typeof(TResult));
        }

        public TResult GetValueOrDefault<TResult>(string name, TResult defaultValue = default(TResult))
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);

            if (parameter == null)
            {
                return defaultValue;
            }

            if (parameter.Value == null)
            {
                return defaultValue;
            }

            if (string.IsNullOrEmpty(parameter.Value.ToStringOrEmpty()))
            {
                return defaultValue;
            }

            return (TResult)Convert.ChangeType(parameter.Value, typeof(TResult));
        }

        public TResult GetValue<TResult>(int index)
        {
            var parameter = this[index];

            if (parameter == null)
            {
                return default(TResult);
            }

            return (TResult)Convert.ChangeType(parameter.Value, typeof(TResult));
        }

        public override string ToString()
        {
            string str = "";
            foreach (var connectionParameter in this.Items)
            {
                str += "(" + connectionParameter.ToStringOrEmpty() + ") ";
            }

            return str;
        }
    }
}