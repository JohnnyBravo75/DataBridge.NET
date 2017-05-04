namespace DataBridge.Helper
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;

    /// <summary>
    /// Collection with an AddRange method for faster adding items (the change notifictaions for every items are suppressed)
    /// </summary>
    /// <typeparam name="T">the type</typeparam>
    public class BulkObservableCollection<T> : ObservableCollection<T>
    {
        /// <summary>
        /// Flag um das CollectionChanged-Event zu unterdrücken
        /// </summary>
        private bool suppressNotification = false;

        /// <summary>
        /// Adds the range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <exception cref="System.ArgumentNullException">range</exception>
        public void AddRange(IEnumerable<T> range)
        {
            if (range == null)
            {
                throw new ArgumentNullException("range");
            }

            this.suppressNotification = true;

            foreach (T item in range)
            {
                this.Add(item);
            }

            this.suppressNotification = false;

            // this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, range.ToList<T>()));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Removes the range.
        /// </summary>
        /// <param name="range">The range.</param>
        /// <exception cref="System.ArgumentNullException">range</exception>
        public void RemoveRange(IEnumerable<T> range)
        {
            if (range == null)
            {
                throw new ArgumentNullException("range");
            }

            this.suppressNotification = true;

            foreach (T item in new List<T>(range))
            {
                this.Remove(item);
            }

            this.suppressNotification = false;

            // this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, range.ToList<T>()));
            this.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        /// Raises the <see cref="E:CollectionChanged" /> event.
        /// </summary>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs" /> instance containing the event data.</param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!this.suppressNotification)
            {
                base.OnCollectionChanged(e);
            }
        }
    }
}