using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataBridge.Extensions;

namespace DataBridge
{
    public class CommandParameters : ObservableCollection<CommandParameter>
    {
        public CommandParameter this[string name]
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

        public CommandParameter Get(string name)
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);
            if (parameter == null)
            {
                throw new KeyNotFoundException(string.Format("The parameter '{0}' was not found", name));
            }

            return parameter;
        }

        public void Set(CommandParameter value)
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

        public new void Add(CommandParameter item)
        {
            this.AddOrUpdate(item);
        }

        /// <summary>
        /// Adds the or update.
        /// </summary>
        /// <param name="item">The item.</param>
        public void AddOrUpdate(CommandParameter item)
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

        public void SetOrAddValue(string name, object value)
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);

            if (parameter != null)
            {
                parameter.Value = value;
            }
            else
            {
                var newParam = new CommandParameter() { Name = name, Value = value };
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

            return this.GetParameterValueOrDefault(parameter, default(TResult));
        }

        public TResult GetValueOrDefault<TResult>(string name, TResult defaultValue = default(TResult))
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);

            return this.GetParameterValueOrDefault(parameter, defaultValue);
        }

        public TResult GetValue<TResult>(int index)
        {
            var parameter = this[index];

            return this.GetParameterValueOrDefault(parameter, default(TResult));
        }

        private TResult GetParameterValueOrDefault<TResult>(CommandParameter parameter, TResult defaultValue = default(TResult))
        {
            if (parameter == null)
            {
                return defaultValue;
            }

            var value = parameter.GetEvaluatedValue(this.ToDictionary());

            if (value == null)
            {
                return defaultValue;
            }

            if (string.IsNullOrEmpty(value.ToStringOrEmpty()))
            {
                return defaultValue;
            }

            if (value is IConvertible)
            {
                return (TResult)ConvertExtensions.ChangeType(value, typeof(TResult));
            }

            return (TResult)value;
        }

        public override string ToString()
        {
            string str = "";
            foreach (var item in this.Items)
            {
                str += "(" + item.ToStringOrEmpty() + ") ";
            }

            return str;
        }

        public Dictionary<string, object> ToDictionary()
        {
            var dictionary = new Dictionary<string, object>();
            foreach (var item in this)
            {
                dictionary.Add(item.Name, item.Value);
            }

            return dictionary;
        }
    }
}