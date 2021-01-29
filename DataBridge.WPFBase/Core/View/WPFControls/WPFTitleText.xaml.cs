using System.Windows.Controls;

namespace DataBridge.GUI.Core.View.WPFControls
{
    /// <summary>
    /// Interaction logic for WPFTitleText.xaml
    /// </summary>
    public partial class WPFTitleText : UserControl
    {
        private string text;

        public WPFTitleText()
        {
            this.InitializeComponent();
        }

        public string Text
        {
            get { return this.Label.Content as string; }
            set { this.Label.Content = value; }
        }
    }
}