namespace DataBridge.GUI.UserControls
{
    using DataBridge;
    using DataBridge.GUI.Core.View.WPFControls;

    /// <summary>
    /// Baseclass for the Editorcontrol for datacommands
    /// </summary>
    public class DataCommandEditor : WPFUserControl
    {
        public virtual DataCommand CurrentDataCommand
        {
            set;
            get;
        }

        public virtual void PrepareChanges()
        {
        }

        public virtual void ApplyChanges()
        {
        }

        public virtual string Validate()
        {
            return string.Empty;
        }
    }
}