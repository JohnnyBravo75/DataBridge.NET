namespace DataBridge.GUI.Core.EventBinding
{
    using System.Windows;
    using System.Windows.Input;

    public class EventCommand : DependencyObject
    {
        public string CommandName { get; set; }
        public string EventName { get; set; }

        public static readonly DependencyProperty CommandProperty =
        DependencyProperty.Register("Command", typeof(ICommand), typeof(EventCommand), new PropertyMetadata(null));

        public static readonly DependencyProperty ParameterProperty =
        DependencyProperty.Register("Parameter", typeof(object), typeof(EventCommand), new PropertyMetadata(null));

        public ICommand Command
        {
            get
            {
                return (ICommand)this.GetValue(CommandProperty);
            }
            set
            {
                this.SetValue(CommandProperty, value);
            }
        }

        public object Parameter
        {
            get
            {
                return (object)this.GetValue(ParameterProperty);
            }
            set
            {
                this.SetValue(ParameterProperty, value);
            }
        }
    }
}