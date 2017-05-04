namespace DataBridge.GUI
{
    using Core.View.WPFControls;

    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class MainWindow : WPFWindow
    {
        public MainWindow()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }
    }
}