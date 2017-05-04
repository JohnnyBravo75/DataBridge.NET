using System;

namespace DataBridge.GUI.ViewModels
{
    public class DataCommandViewModelBase : ViewModelBase
    {
        public DataCommand CurrentDataCommand { get; set; }

        public virtual void PrepareChanges()
        {
        }

        public virtual void ApplyChanges()
        {
        }

        public virtual string Validate()
        {
            if (this.CurrentDataCommand == null)
            {
                return "The DataCommand is null";
            }

            return string.Join(Environment.NewLine, this.CurrentDataCommand.Validate(null));
        }
    }
}