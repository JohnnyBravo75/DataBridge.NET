namespace DataBridge.PropertyChanged
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Reflection;

    public class ChildChangeListener : ChangeListener
    {
        protected static readonly Type iNotifyType = typeof(INotifyPropertyChanged);

        private readonly Dictionary<string, ChangeListener> childListeners = new Dictionary<string, ChangeListener>();
        private readonly Type myType;
        private readonly INotifyPropertyChanged myValue;

        public ChildChangeListener(INotifyPropertyChanged instance)
        {
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            this.myValue = instance;
            this.myType = this.myValue.GetType();

            this.Subscribe();
        }

        public ChildChangeListener(INotifyPropertyChanged instance, string propertyName)
            : this(instance)
        {
            this.myPropertyName = propertyName;
        }

        /// <summary>
        /// Raises the property changed event
        /// </summary>
        /// <param name="propertyName">Name of the property.</param>
        /// <param name="sender">The sender.</param>
        protected override void RaisePropertyChanged(string propertyName, object sender = null)
        {
            // Special Formatting of the property path
            base.RaisePropertyChanged(string.Format("{0}{1}{2}", this.myPropertyName, this.myPropertyName != null ? "." : null, propertyName),
                                      sender != null ? sender : this);
        }

        /// <summary>
        /// Release all child handlers and self handler
        /// </summary>
        protected override void Unsubscribe()
        {
            this.myValue.PropertyChanged -= this.Value_PropertyChanged;

            if (this.childListeners.Count > 0)
            {
                foreach (string propertyName in this.childListeners.Keys)
                {
                    if (this.childListeners[propertyName] != null)
                    {
                        this.childListeners[propertyName].Dispose();
                    }
                }

                this.childListeners.Clear();
            }

            if (!string.IsNullOrEmpty(this.myPropertyName))
            {
                System.Diagnostics.Debug.WriteLine(string.Format("ChildChangeListener '{0}' unsubscribed", this.myPropertyName));
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Child property.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        private void Child_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            this.RaisePropertyChanged(e.PropertyName, sender);
        }

        /// <summary>
        /// Resets known (must exist in children collection) child event handlers
        /// </summary>
        /// <param name="propertyName">Name of known child property</param>
        private void ResetChildListener(string propertyName)
        {
            if (this.childListeners.ContainsKey(propertyName))
            {
                // Unsubscribe if existing
                if (this.childListeners[propertyName] != null)
                {
                    if (this.childListeners[propertyName] is INotifyCollectionChanged)
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("CollectionChangeListener '{0}' unsubscribed", propertyName));
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine(string.Format("ChildChangeListener '{0}' unsubscribed", propertyName));
                    }

                    this.childListeners[propertyName].PropertyChanged -= this.Child_PropertyChanged;

                    // Should unsubscribe all events
                    this.childListeners[propertyName].Dispose();
                    this.childListeners[propertyName] = null;
                }

                var property = this.myType.GetProperty(propertyName);
                if (property == null)
                {
                    throw new InvalidOperationException(string.Format("Was unable to get '{0}' property information from Type '{1}'", propertyName, this.myType.Name));
                }

                object newValue = property.GetValue(this.myValue, null);

                // Only recreate if there is a new value
                if (newValue != null)
                {
                    if (newValue is INotifyCollectionChanged)
                    {
                        this.childListeners[propertyName] = new CollectionChangeListener(newValue as INotifyCollectionChanged, propertyName);
                        System.Diagnostics.Debug.WriteLine(string.Format("CollectionChangeListener '{0}' subscribed", property.Name));
                    }
                    else if (newValue is INotifyPropertyChanged)
                    {
                        this.childListeners[propertyName] = new ChildChangeListener(newValue as INotifyPropertyChanged, propertyName);
                        System.Diagnostics.Debug.WriteLine(string.Format("ChildChangeListener '{0}' subscribed", property.Name));
                    }

                    if (this.childListeners[propertyName] != null)
                    {
                        this.childListeners[propertyName].PropertyChanged += this.Child_PropertyChanged;
                    }
                }
            }
        }

        /// <summary>
        /// Subscribes this instance and all child properties to PropertyChanged
        /// </summary>
        private void Subscribe()
        {
            this.myValue.PropertyChanged += this.Value_PropertyChanged;

            var query =
                from property
                in this.myType.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                where iNotifyType.IsAssignableFrom(property.PropertyType)
                select property;

            foreach (var property in query)
            {
                // Declare property as known "Child", then register it
                this.childListeners.Add(property.Name, null);
                this.ResetChildListener(property.Name);
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the current Value.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        private void Value_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // First, reset child on change, if required...
            this.ResetChildListener(e.PropertyName);

            // ...then, notify about it
            this.RaisePropertyChanged(e.PropertyName, sender);
        }
    }
}