using System.Windows;
using DataBridge.GUI.Core.View.WPFControls;

namespace DataBridge.TrayNotifier
{
    /// <summary>
    /// Interaction logic for TrayWindow.xaml
    /// </summary>
    public partial class TrayWindow : WPFWindow
    {
        public TrayWindow()
        {
            this.DataContext = this;
            this.InitializeComponent();
            this.IsVisibleChanged += this.Window_IsVisibleChanged;
        }

        public bool IsAutoStartUp
        {
            get
            {
                try
                {
                    return EnvironmentHelper.IsAutoStartUp();
                }
                catch
                { return false; }
            }
            set
            {
                try
                {
                    EnvironmentHelper.SetAutoStartUp(value);
                }
                catch { }

                this.RaisePropertyChanged("IsAutoStart");
            }
        }

        private void Window_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.SetWindowPositionToCorner();
            }
        }

        private void Window_OnLoaded(object sender, RoutedEventArgs e)
        {
            this.SetWindowPositionToCorner();
        }

        private void SetWindowPositionToCorner()
        {
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width;
            this.Top = desktopWorkingArea.Bottom - this.Height;
        }
    }
}