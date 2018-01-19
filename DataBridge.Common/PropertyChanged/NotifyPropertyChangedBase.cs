using System.Xml.Serialization;

namespace DataBridge.PropertyChanged
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq.Expressions;
    using System.Reflection;

    /// <summary>
    /// Base class for items that implement property changed
    /// </summary>
    [XmlType(Namespace = "NotifyPropertyChanged")]
    public class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        //System.Collections.Generic.List<PropertyChangedEventHandler> delegates = new System.Collections.Generic.List<PropertyChangedEventHandler>();

        ///// <summary>
        ///// Occurs when [property changed].
        ///// </summary>
        //private event PropertyChangedEventHandler propertyChanged;

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        //{
        //    add
        //    {
        //        propertyChanged += value;
        //        delegates.Add(value);
        //    }

        //    remove
        //    {
        //        propertyChanged -= value;
        //        delegates.Remove(value);
        //    }
        //}

        ///// <summary>
        ///// Removes all events.
        ///// </summary>
        //public void RemoveAllPropertyChangedEvents()
        //{
        //    foreach (PropertyChangedEventHandler handler in delegates)
        //    {
        //        propertyChanged -= handler;
        //    }

        //    delegates.Clear();
        //}

        /// <summary>
        /// Raises the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <exception cref="System.Exception">Invalid property name ' + propertyName + ' in  + this.ToString()</exception>
        public void RaisePropertyChanged(string propertyName, object sender = null)
        {
            if (Debugger.IsAttached)
            {
                //if (!this.HasProperty(propertyName))
                //{
                //    throw new Exception("Invalid property name '" + propertyName + "' in " + this.ToString());
                //}
            }

            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(sender != null ? sender : this, new PropertyChangedEventArgs(propertyName));
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event for each of the  properties.
        /// </summary>
        /// <param name="propertyNames">The properties that have a new  value.</param>
        public void RaisePropertyChanged(params string[] propertyNames)
        {
            foreach (var name in propertyNames)
            {
                this.RaisePropertyChanged(name);
            }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// e.g. this.RaisePropertyChanged(() => Title);
        /// </summary>
        /// <example>this.RaisePropertyChanged(() => Title);</example>
        /// <typeparam name="T">The type of the property that has a new  value</typeparam>
        /// <param name="expression">A Lambda expression  representing the property that has a new value.</param>
        public void RaisePropertyChanged<T>(Expression<Func<T>> expression)
        {
            if (expression == null)
            {
                throw new ArgumentNullException("expression");
            }

            var propertyName = this.GetPropertyName<T>(expression);

            this.RaisePropertyChanged(propertyName);
        }

        /// <summary>
        /// Gets the property.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException">
        /// The propertyexpression is a member.;propertyExpression
        /// or
        /// The propertyexpression is a not a property.;propertyExpression
        /// </exception>
        private string GetPropertyName<T>(Expression<Func<T>> expression)
        {
            var memberExpression = expression.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException("The propertyexpression is a member.", "expression");
            }

            var property = memberExpression.Member as PropertyInfo;
            if (property == null)
            {
                throw new ArgumentException("The propertyexpression is a not a property.", "expression");
            }

            return property.Name;
        }

        /// <summary>
        /// Auf Propertychanged registrieren
        /// </summary>
        /// <param name="target">Zielobjekt, auf welches man sich registrieren möchte</param>
        /// <param name="eventhandler">Callback function  when property changes</param>
        public void RegisterPropertyChanged(object target, PropertyChangedEventHandler eventhandler)
        {
            this.DeregisterPropertyChanged(target, eventhandler);

            if (target != null && target is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)target).PropertyChanged += eventhandler;
            }
        }

        /// <summary>
        /// Von Propertychanged deregistrieren
        /// </summary>
        /// <param name="target">Zielobjekt, von welchem man sich deregistrieren möchte</param>
        /// <param name="eventhandler">Callback function when property changes</param>
        public void DeregisterPropertyChanged(object target, PropertyChangedEventHandler eventhandler)
        {
            if (target != null && target is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)target).PropertyChanged -= eventhandler;
            }
        }
    }
}