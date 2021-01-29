namespace DataBridge.GUI.Core.View.WPFControls
{
    using System.Windows.Controls;

    public class WPFTextBox : TextBox
    {
        public WPFTextBox()
        {
            this.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            this.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Stretch;
            this.VerticalContentAlignment = System.Windows.VerticalAlignment.Stretch;
        }
    }
}