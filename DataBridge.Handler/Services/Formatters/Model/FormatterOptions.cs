using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DataBridge.Extensions;

namespace DataBridge
{
    public class FormatterOptions : ObservableCollection<FormatterOption>
    {
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

        public FormatterOption this[string name]
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

        public FormatterOption Get(string name)
        {
            var parameter = this.FirstOrDefault(x => x.Name == name);
            if (parameter == null)
            {
                throw new KeyNotFoundException(string.Format("The parameter '{0}' was not found", name));
            }

            return parameter;
        }

        public void Set(FormatterOption value)
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

        public new void Add(FormatterOption item)
        {
            this.AddOrUpdate(item);
        }

        /// <summary>
        /// Adds the or update.
        /// </summary>
        /// <param name="item">The item.</param>
        public void AddOrUpdate(FormatterOption item)
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
                var newParam = new FormatterOption() { Name = name, Value = value };
                this.Add(newParam);
            }
        }
    }
}