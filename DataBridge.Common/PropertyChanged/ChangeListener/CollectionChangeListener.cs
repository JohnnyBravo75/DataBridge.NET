namespace DataBridge.PropertyChanged
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;

    public class CollectionChangeListener : ChangeListener
    {
        private readonly INotifyCollectionChanged myValue;
        private readonly Dictionary<INotifyPropertyChanged, ChangeListener> collectionListeners = new Dictionary<INotifyPropertyChanged, ChangeListener>();

        public CollectionChangeListener(INotifyCollectionChanged collection, string propertyName)
        {
            this.myValue = collection;
            this.myPropertyName = propertyName;

            this.Subscribe();
        }

        /// <summary>
        /// Subscribes this instance and all items in the collection
        /// </summary>
        private void Subscribe()
        {
            this.myValue.CollectionChanged += new NotifyCollectionChangedEventHandler(this.Value_CollectionChanged);

            foreach (INotifyPropertyChanged item in (IEnumerable)this.myValue)
            {
                this.ResetChildListener(item);
            }
        }

        private void ResetChildListener(INotifyPropertyChanged item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            this.RemoveItem(item);

            ChangeListener listener = null;

            // Add new
            if (item is INotifyCollectionChanged)
            {
                listener = new CollectionChangeListener(item as INotifyCollectionChanged, this.myPropertyName);
                System.Diagnostics.Debug.WriteLine(string.Format("CollectionChangeListener '{0}' subscribed", this.myPropertyName));
            }
            else
            {
                listener = new ChildChangeListener(item as INotifyPropertyChanged);
            }

            listener.PropertyChanged += new PropertyChangedEventHandler(this.Listener_PropertyChanged);
            this.collectionListeners.Add(item, listener);
        }

        private void RemoveItem(INotifyPropertyChanged item)
        {
            // Remove old
            if (this.collectionListeners.ContainsKey(item))
            {
                this.collectionListeners[item].PropertyChanged -= new PropertyChangedEventHandler(this.Listener_PropertyChanged);

                System.Diagnostics.Debug.WriteLine(string.Format("ChangeListener '{0}' unsubscribed", this.myPropertyName));

                this.collectionListeners[item].Dispose();
                this.collectionListeners.Remove(item);
            }
        }

        private void ClearCollection()
        {
            foreach (var key in this.collectionListeners.Keys)
            {
                this.collectionListeners[key].PropertyChanged -= new PropertyChangedEventHandler(this.Listener_PropertyChanged);
                this.collectionListeners[key].Dispose();
            }

            this.collectionListeners.Clear();
        }

        /// <summary>
        /// Handles the CollectionChanged event of the Value property.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        private void Value_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.ClearCollection();
            }
            else
            {
                // Don't care about e.Action, if there are old items, Remove them...
                if (e.OldItems != null)
                {
                    foreach (INotifyPropertyChanged item in (IEnumerable)e.OldItems)
                    {
                        this.RemoveItem(item);
                        this.RaisePropertyChanged(string.Format("{0}", this.myPropertyName), sender);
                    }
                }

                // ...add new items as well
                if (e.NewItems != null)
                {
                    foreach (INotifyPropertyChanged item in (IEnumerable)e.NewItems)
                    {
                        this.ResetChildListener(item);
                        this.RaisePropertyChanged(string.Format("{0}", this.myPropertyName), sender);
                    }
                }
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of the Listener.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        private void Listener_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // ...then, notify about it
            this.RaisePropertyChanged(string.Format("{0}{1}{2}", this.myPropertyName, this.myPropertyName != null ? "." : null, e.PropertyName), sender);
        }

        /// <summary>
        /// Releases all collection item handlers and self handler
        /// </summary>
        protected override void Unsubscribe()
        {
            this.ClearCollection();

            this.myValue.CollectionChanged -= new NotifyCollectionChangedEventHandler(this.Value_CollectionChanged);

            System.Diagnostics.Debug.WriteLine(string.Format("CollectionChangeListener '{0}' unsubscribed", this.myPropertyName));
        }
    }
}