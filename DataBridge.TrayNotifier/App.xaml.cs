using System.ComponentModel;
using System.Windows;

namespace DataBridge.TrayNotifier
{

    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private bool isExit;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            this.MainWindow = new TrayWindow();
            this.MainWindow.Closing += this.MainWindow_Closing;

            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.DoubleClick += (s, args) => this.ShowMainWindow();
            this.notifyIcon.Icon = DataBridge.TrayNotifier.Properties.Resources.MyIcon;
            this.notifyIcon.Visible = true;

            this.CreateContextMenu();
        }

        private void CreateContextMenu()
        {
            this.notifyIcon.ContextMenuStrip = new System.Windows.Forms.ContextMenuStrip();
            this.notifyIcon.ContextMenuStrip.Items.Add("Show window").Click += (s, e) => this.ShowMainWindow();
            this.notifyIcon.ContextMenuStrip.Items.Add("Exit").Click += (s, e) => this.ExitApplication();
        }

        private void ExitApplication()
        {
            this.isExit = true;
            this.MainWindow.Close();
            this.notifyIcon.Dispose();
            this.notifyIcon = null;
        }

        private void ShowMainWindow()
        {
            if (this.MainWindow.IsVisible)
            {
                if (this.MainWindow.WindowState == WindowState.Minimized)
                {
                    this.MainWindow.WindowState = WindowState.Normal;
                }
                this.MainWindow.Activate();
            }
            else
            {
                this.MainWindow.Show();
            }
        }

        private void MainWindow_Closing(object sender, CancelEventArgs e)
        {
            if (!this.isExit)
            {
                e.Cancel = true;
                // A hidden window can be shown again, a closed one not
                this.MainWindow.Hide(); 
            }
        }
    }
}
