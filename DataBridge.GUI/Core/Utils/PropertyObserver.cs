namespace DataBridge.GUI.Core
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Windows;

    /// <summary>
    /// Monitors the PropertyChanged event of an object that implements INotifyPropertyChanged,
    /// and executes callback methods (i.e. handlers) registered for properties of that object.
    ///
    /// private PropertyObserver&lt;NumberViewModel&gt; observer;
    ///
    /// observer = new PropertyObserver&lt;NumberViewModel&gt;(this.Number)
    //                          .RegisterPropertyChanged(n => n.Value, n => Log("Value: " + n.Value))
    //                          .RegisterPropertyChanged(n => n.IsNegative, this.AppendIsNegative)
    //                          .RegisterPropertyChanged(n => n.IsEven, this.AppendIsEven);
    /// </summary>
    /// <typeparam name="TPropertySource">The type of object to monitor for property changes.</typeparam>
    public class PropertyObserver<TPropertySource> : IWeakEventListener
        where TPropertySource : INotifyPropertyChanged
    {
        #region Constructor

        /// <summary>
        /// Initializes a new instance of PropertyObserver, which
        /// observes the 'propertySource' object for property changes.
        /// </summary>
        /// <param name="propertySource">The object to monitor for property changes.</param>
        public PropertyObserver(TPropertySource propertySource)
        {
            //if (propertySource == null)
            //    throw new ArgumentNullException("propertySource");

            if (propertySource != null)
            {
                this._propertySourceRef = new WeakReference(propertySource);
                this._propertyNameToHandlerMap = new Dictionary<string, Action<string, TPropertySource>>();
            }
        }

        #endregion Constructor

        #region Public Methods

        #region RegisterHandler

        /// <summary>
        /// Registers a callback to be invoked when the PropertyChanged event has been raised for the specified property.
        /// </summary>
        /// <param name="expression">A lambda expression like 'n => n.PropertyName'.</param>
        /// <param name="handler">The callback to invoke when the property has changed.</param>
        /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
        public PropertyObserver<TPropertySource> RegisterPropertyChanged(Expression<Func<TPropertySource, object>> expression,
                                                                         Action<string, TPropertySource> handler)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            string propertyName = this.GetPropertyName(expression);
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException("'expression' did not provide a property name.");

            return this.RegisterPropertyChanged(propertyName, handler);
        }

        public PropertyObserver<TPropertySource> RegisterPropertyChanged(string propertyName,
                                                                         Action<string, TPropertySource> handler)
        {
            if (handler == null)
                throw new ArgumentNullException("handler");

            TPropertySource propertySource = this.GetPropertySource();
            if (propertySource != null)
            {
                this._propertyNameToHandlerMap[propertyName] = handler;
                PropertyChangedEventManager.AddListener(propertySource, this, propertyName);
            }

            return this;
        }

        #endregion RegisterHandler

        #region UnregisterHandler

        /// <summary>
        /// Removes the callback associated with the specified property.
        /// </summary>
        /// <param name="propertyName">A lambda expression like 'n => n.PropertyName'.</param>
        /// <returns>The object on which this method was invoked, to allow for multiple invocations chained together.</returns>
        public PropertyObserver<TPropertySource> UnregisterHandler(Expression<Func<TPropertySource, object>> expression)
        {
            if (expression == null)
                throw new ArgumentNullException("expression");

            string propertyName = this.GetPropertyName(expression);
            if (String.IsNullOrEmpty(propertyName))
                throw new ArgumentException("'expression' did not provide a property name.");

            TPropertySource propertySource = this.GetPropertySource();
            if (propertySource != null)
            {
                if (this._propertyNameToHandlerMap.ContainsKey(propertyName))
                {
                    this._propertyNameToHandlerMap.Remove(propertyName);
                    PropertyChangedEventManager.RemoveListener(propertySource, this, propertyName);
                }
            }

            return this;
        }

        #endregion UnregisterHandler

        #endregion Public Methods

        #region Private Helpers

        #region GetPropertyName

        private string GetPropertyName(Expression<Func<TPropertySource, object>> expression)
        {
            var lambda = expression as LambdaExpression;
            MemberExpression memberExpression;
            if (lambda.Body is UnaryExpression)
            {
                var unaryExpression = lambda.Body as UnaryExpression;
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else
            {
                memberExpression = lambda.Body as MemberExpression;
            }

            Debug.Assert(memberExpression != null, "Please provide a lambda expression like 'n => n.PropertyName'");

            if (memberExpression != null)
            {
                var propertyInfo = memberExpression.Member as PropertyInfo;

                return propertyInfo.Name;
            }

            return null;
        }

        #endregion GetPropertyName

        #region GetPropertySource

        private TPropertySource GetPropertySource()
        {
            try
            {
                return (TPropertySource)this._propertySourceRef.Target;
            }
            catch
            {
                return default(TPropertySource);
            }
        }

        #endregion GetPropertySource

        #endregion Private Helpers

        #region Fields

        private readonly Dictionary<string, Action<string, TPropertySource>> _propertyNameToHandlerMap;
        private readonly WeakReference _propertySourceRef;

        #endregion Fields

        #region IWeakEventListener Members

        bool IWeakEventListener.ReceiveWeakEvent(Type managerType, object sender, EventArgs e)
        {
            if (managerType == typeof(PropertyChangedEventManager))
            {
                string propertyName = ((PropertyChangedEventArgs)e).PropertyName;
                TPropertySource propertySource = (TPropertySource)sender;

                if (String.IsNullOrEmpty(propertyName))
                {
                    // When the property name is empty, all properties are considered to be invalidated.
                    // Iterate over a copy of the list of handlers, in case a handler is registered by a callback.
                    foreach (Action<string, TPropertySource> handler in this._propertyNameToHandlerMap.Values.ToArray())
                        handler(propertyName, propertySource);

                    return true;
                }
                else
                {
                    Action<string, TPropertySource> handler;
                    if (this._propertyNameToHandlerMap.TryGetValue(propertyName, out handler))
                    {
                        handler(propertyName, propertySource);
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion IWeakEventListener Members
    }
}