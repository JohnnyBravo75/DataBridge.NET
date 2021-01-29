namespace DataBridge.GUI
{
    using System;
    using System.ComponentModel;
    using DataBridge.PropertyChanged;

    /// <summary>
    /// Basisklasse für ViewModels
    /// </summary>
    public class ViewModelBase : NotifyPropertyChangedBase, IDisposable
    {
        private Guid myId = Guid.NewGuid();

        public Guid MyId
        {
            get { return this.myId; }
            private set { this.myId = value; }
        }

        public virtual void Dispose()
        {
        }
    }
}