using DataBridge.PropertyChanged;

namespace DataBridge.GUI.Core.View.ViewModels
{
    using System;

    /// <summary>
    /// Baseclass for ViewModels
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