namespace DataBridge.PropertyChanged
{
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;

    /// <summary>
    /// ChangeListener for objects. Listens to all propertychnages in the object hierarchy.
    /// e.g.
    ///  var personChangeListener = ChangeListener.Create(person);
    ///  personChangeListener.PropertyChanged += new PropertyChangedEventHandler(personChangeListener_PropertyChanged);
    /// </summary>
    public abstract class ChangeListener : INotifyPropertyChanged, IDisposable
    {
        protected string myPropertyName;

        ~ChangeListener()
        {
            this.Dispose(false);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public static ChangeListener Create(INotifyPropertyChanged value)
        {
            return Create(value, null);
        }

        public static ChangeListener Create(INotifyPropertyChanged value, string propertyName)
        {
            if (value is INotifyCollectionChanged)
            {
                return new CollectionChangeListener(value as INotifyCollectionChanged, propertyName);
            }
            else if (value is INotifyPropertyChanged)
            {
                return new ChildChangeListener(value as INotifyPropertyChanged, propertyName);
            }

            return null;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this.Unsubscribe();
            }
        }

        protected virtual void RaisePropertyChanged(string propertyName, object sender = null)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(sender != null ? sender : this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected abstract void Unsubscribe();
    }
}