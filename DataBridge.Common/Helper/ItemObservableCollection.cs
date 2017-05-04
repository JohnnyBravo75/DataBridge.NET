namespace DataBridge
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using DataBridge.Helper;

    /// <summary>
    /// Collection for observing propertychanged of the collection items
    /// </summary>
    /// <typeparam name="T">the type</typeparam>
    public class ItemObservableCollection<T> : BulkObservableCollection<T> where T : INotifyPropertyChanged, IDisposable
    {
        /// <summary>
        /// Occurs when a property of a item in the collection changed.
        /// </summary>
        public event EventHandler<PropertyChangedEventArgs> OnItemPropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemObservableCollection{T}" /> class.
        /// </summary>
        public ItemObservableCollection()
            : base()
        {
            this.CollectionChanged += this.ItemObservableCollection_CollectionChanged;
        }

        /// <summary>
        /// Handles the CollectionChanged event of the PropertyObservableCollection control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        private void ItemObservableCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (T item in e.NewItems)
                    {
                        if (item is INotifyPropertyChanged)
                        {
                            (item as INotifyPropertyChanged).PropertyChanged += this.Item_PropertyChanged;
                        }
                    }
                }
            }

            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (T item in e.OldItems)
                    {
                        if (item is INotifyPropertyChanged)
                        {
                            (item as INotifyPropertyChanged).PropertyChanged -= this.Item_PropertyChanged;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Handles the PropertyChanged event of a Item in the collection.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // send a custom event
            if (this.OnItemPropertyChanged != null)
            {
                this.OnItemPropertyChanged(sender, e);
            }
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public virtual void Dispose()
        {
            this.CollectionChanged -= this.ItemObservableCollection_CollectionChanged;

            foreach (T item in this.Items)
            {
                // dispose the items in the collection
                if (item is IDisposable)
                {
                    (item as IDisposable).Dispose();
                }
            }

            this.Clear();
        }

        /// <summary>
        /// Clears the items.
        /// </summary>
        protected override void ClearItems()
        {
            foreach (T item in this.Items)
            {
                if (item is INotifyPropertyChanged)
                {
                    (item as INotifyPropertyChanged).PropertyChanged -= this.Item_PropertyChanged;
                }
            }

            base.ClearItems();
        }

        /// <summary>
        /// Removes the item.
        /// </summary>
        /// <param name="index">The index.</param>
        protected override void RemoveItem(int index)
        {
            if (this.Items[index] is INotifyPropertyChanged)
            {
                (this.Items[index] as INotifyPropertyChanged).PropertyChanged -= this.Item_PropertyChanged;
            }

            base.RemoveItem(index);
        }
    }
}