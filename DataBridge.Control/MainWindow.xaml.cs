using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using DataBridge.Common.Helper;
using DataBridge.Extensions;
using DataBridge.Helper;
using DataBridge.Services;

namespace DataBridge.Control
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string installerExe = "InstallUtil.exe";

        private ServiceController serviceController = new ServiceController();

        private string servicePrefix = "DataBridge_";

        public string ServiceID
        {
            get
            {
                return this.txt_ServiceID.Text;
            }
        }

        public string ServiceName
        {
            get
            {
                return this.servicePrefix + this.ServiceID;
            }
        }

        public string MachineName
        {
            get
            {
                return Environment.MachineName;
            }
        }

        public string ServiceSourceDir
        {
            get
            {
                return string.Format("{0}\\service\\", this.WorkDir);
            }
        }

        public string ServiceExecutableName
        {
            get
            {
                return "DataBridge.Service.exe";
            }
        }

        public string WorkDir
        {
            get
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
        }

        public string TargetDir
        {
            get
            {
                return Path.Combine(this.txt_TargetPath.Text, this.ServiceName);
            }
        }

        private Settings settings = new Settings();

        public MainWindow()
        {
            this.InitializeComponent();

            this.txt_ServiceID.Text = "TEST";
            this.txt_TargetPath.Text = @"C:\Temp\";
            this.CheckServiceStatus();

            if (EnvironmentHelper.IsRunAsAdmin)
            {
                this.lbl_AccessRight.Text = "Administrator Access";
                this.lbl_AccessRight.Foreground = new SolidColorBrush(Colors.DarkGreen);
            }
            else
            {
                this.lbl_AccessRight.Text = "Limited Access";
                this.lbl_AccessRight.Foreground = new SolidColorBrush(Colors.DarkRed);
            }
        }

        private void btn_InstallService_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                try
                {
                    this.serviceController.ServiceName = this.ServiceName;
                    this.serviceController.MachineName = this.MachineName;

                    if (this.serviceController.Status == ServiceControllerStatus.Running ||
                        this.serviceController.Status == ServiceControllerStatus.Stopped)
                    {
                        MessageBox.Show(this, "Der Dienst existiert bereits. Bitte deinstallieren Sie ihn erst", "Info", MessageBoxButton.OK);
                        return;
                    }
                }
                catch (Exception)
                {
                }

                var response = MessageBox.Show(this, string.Format("Wollen Sie den Dienst '{0}' auf 'diesem Rechner' Installieren?", this.ServiceName) +
                                                                                          " Bitte achten Sie das darauf, dass die Konfigurationsdatei entsprechend gefüllt ist", "Info", MessageBoxButton.YesNo);
                if (response == MessageBoxResult.Yes)
                {
                    this.btn_StartService.IsEnabled = false;
                    this.btn_StopService.IsEnabled = false;
                    this.btn_UninstallService.IsEnabled = false;
                    this.btn_InstallService.IsEnabled = false;

                    //string config = asciiImport.GetValue("ConfigPath", "NULL").ToString();
                    string serviceDirectory = this.TargetDir;
                    //Settings.GetInstance().LogControlDebug(string.Format("Ziel Verzeichnis : {0}", System.Environment.CurrentDirectory));
                    //Settings.GetInstance().LogControlDebug(string.Format("Quell Verzeichnis : {0}\\service\\", this.workDir));

                    string errorMsg = this.InstallService(this.ServiceName, serviceDirectory);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        throw new Exception(errorMsg);
                    }

                    response = MessageBox.Show(this, string.Format("Der Dienst '{0}' wurde installiert. Wollen Sie den Dienst Starten? ", this.ServiceName), "Dienst Starten", MessageBoxButton.YesNo);

                    if (response == MessageBoxResult.Yes)
                    {
                        this.serviceController.ServiceName = this.ServiceName;
                        this.serviceController.MachineName = this.MachineName;
                        this.serviceController.Start();
                    }
                    else
                    {
                        this.btn_StartService.IsEnabled = true;
                    }
                }

                this.btn_StartService.IsEnabled = true;
                this.btn_StopService.IsEnabled = true;
                this.btn_UninstallService.IsEnabled = true;

                this.CheckServiceStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Fehler beim Installieren des Dienstes! \r\n{0}", ex.GetAllMessages()), "ERROR");
            }
        }

        private void btn_UninstallService_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = MessageBox.Show(this, string.Format("Der Dienst '{0}' wird runter gefahren und gelöscht! Fortfahren?", this.ServiceName), "Warnung", MessageBoxButton.YesNo);

                if (response == MessageBoxResult.Yes)
                {
                    string erroMsg = this.UnInstallService(this.ServiceName, this.serviceController.ServiceDirectory);

                    MessageBox.Show(string.Format("Fehler beim Entfernen des Dienstes! \r\n{0}", erroMsg), "ERROR");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Fehler beim Entfernen des Dienstes! \r\n{0}", ex.GetAllMessages()), "ERROR");
            }
            finally
            {
                this.CheckServiceStatus();
            }
        }

        private void btn_StartService_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = MessageBox.Show(this, string.Format("Wollen Sie den Dienst '{0}' starten?", this.ServiceName), "Warnung", MessageBoxButton.YesNo);

                if (response == MessageBoxResult.Yes)
                {
                    if (this.serviceController.Status != ServiceControllerStatus.Running)
                    {
                        this.serviceController.Start();
                    }
                    else
                    {
                        MessageBox.Show(this, string.Format("Dienst '{0}' läuft bereits", this.ServiceName), "Warnung", MessageBoxButton.OK);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Fehler beim Starten des Dienstes! \r\n{0}", ex.GetAllMessages()), "ERROR");
            }
            finally
            {
                this.CheckServiceStatus();
            }
        }

        private void btn_StopService_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                var response = MessageBox.Show(this, string.Format("Der Dienst '{0}' wird angehalten! Fortfahren?", this.ServiceName), "Warnung", MessageBoxButton.YesNo);

                if (response == MessageBoxResult.Yes)
                {
                    if (this.serviceController.Status != ServiceControllerStatus.Stopped)
                    {
                        this.serviceController.Stop();
                        this.serviceController.WaitForStatus(ServiceControllerStatus.Stopped);
                    }
                    else
                    {
                        MessageBox.Show(string.Format("Der Dienst '{0}' ist bereits angehalten", this.ServiceName), "Warnung");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Fehler beim Stoppen des Dienstes! \r\n{0}", ex.GetAllMessages()), "ERROR");
            }
            finally
            {
                this.CheckServiceStatus();
            }
        }

        private void CheckServiceStatus(int delayMilliSeconds = 50)
        {
            Thread.Sleep(delayMilliSeconds);
            try
            {
                this.serviceController.ServiceName = this.ServiceName;
                this.serviceController.MachineName = this.MachineName;

                this.serviceController.Refresh();
                var serviceStatus = this.serviceController.Status;

                //this.lbl_ServiceName.Text = ServiceName;

                if (serviceStatus == ServiceControllerStatus.Running ||
                    serviceStatus == ServiceControllerStatus.StartPending)
                {
                    this.btn_StartService.IsEnabled = false;
                    this.btn_StopService.IsEnabled = true;
                    this.btn_UninstallService.IsEnabled = false;

                    this.lbl_ServiceStatus.Text = this.serviceController.Status.ToString();
                    this.lbl_ServiceStatus.Foreground = new SolidColorBrush(Colors.DarkGreen);
                }
                else if (serviceStatus == ServiceControllerStatus.Stopped ||
                    serviceStatus == ServiceControllerStatus.StopPending)
                {
                    this.btn_StartService.IsEnabled = true;
                    this.btn_StopService.IsEnabled = false;
                    this.btn_UninstallService.IsEnabled = true;

                    this.lbl_ServiceStatus.Text = this.serviceController.Status.ToString();
                    this.lbl_ServiceStatus.Foreground = new SolidColorBrush(Colors.Red);
                }
                else
                {
                    this.lbl_ServiceStatus.Text = "Unbekannter Status";
                    this.lbl_ServiceStatus.Foreground = new SolidColorBrush(Colors.Red);

                    throw new Exception("Service Status unbekannt");
                }

                this.btn_InstallService.IsEnabled = false;
            }
            catch (Exception)
            {
                this.btn_StartService.IsEnabled = false;
                this.btn_StopService.IsEnabled = false;
                this.btn_InstallService.IsEnabled = true;
                this.btn_UninstallService.IsEnabled = false;

                this.lbl_ServiceStatus.Text = "Nicht Installiert";
                this.lbl_ServiceStatus.Foreground = new SolidColorBrush(Colors.Red);
            }
        }

        private string InstallService(string serviceName, string serviceDir, string adminUser = "", string adminPassword = "")
        {
            string installerDir = RuntimeEnvironment.GetRuntimeDirectory();
            string errorMsg = "";

            if (!File.Exists(Path.Combine(installerDir, installerExe)))
            {
                installerDir = this.WorkDir;
            }

            if (!File.Exists(Path.Combine(installerDir, installerExe)))
            {
                errorMsg = string.Format(@"'{0}' was not found in '{1}'", installerExe, installerDir);
                return errorMsg;
            }

            DirectoryUtil.CreateDirectoryIfNotExists(serviceDir);

            Environment.CurrentDirectory = serviceDir;

            FileUtil.CopyDirectory(this.ServiceSourceDir, serviceDir, true);

            var process = new ProcessHandler("cmd");
            if (!string.IsNullOrEmpty(adminUser))
            {
                process.SetExecUser(adminUser, adminPassword);
            }

            string systemUser = ".\\" + this.settings.SystemUser;
            if (!string.IsNullOrEmpty(this.settings.SystemDomain))
            {
                systemUser = ".\\" + this.settings.SystemUser + "@" + this.settings.SystemDomain;
            }

            string stmt = installerDir + string.Format(installerExe + " /ServiceAccount={0} /UserName={1} /Password={2} /ServiceName={3} \"{4}\" ", ServiceAccount.LocalSystem, systemUser, this.settings.SystemPassword, serviceName, Path.Combine(serviceDir, this.ServiceExecutableName));

            process.Execute(stmt);
            string result = process.OutputString;

            if (result.Contains("Fehler") || result.Contains("Exception") || result.Contains("Rollback"))
            {
                errorMsg = "ERROR: Beim Installieren des Dienstes, bitte Logs überprüfen" + Environment.NewLine + Environment.NewLine + result;
            }

            return errorMsg;
        }

        private string UnInstallService(string serviceName, string serviceDir, string adminUser = "", string adminPassword = "")
        {
            string errorMsg = "";

            try
            {
                var process = new ProcessHandler("cmd");
                if (!string.IsNullOrEmpty(adminUser))
                {
                    process.SetExecUser(adminUser, adminPassword);
                }

                process.Execute("sc Stop " + serviceName);

                process.Execute("sc Delete " + serviceName);
                errorMsg = process.OutputString;

                Thread.Sleep(100);
                Directory.Delete(serviceDir, true);

                return errorMsg;
            }
            catch (Exception ex)
            {
                errorMsg = ex.GetAllMessages();
            }

            return errorMsg;
        }

        private string StartService(string serviceName, string adminUser = "", string adminPassword = "")
        {
            string errorMsg = "";

            try
            {
                var process = new ProcessHandler("cmd");
                if (!string.IsNullOrEmpty(adminUser))
                {
                    process.SetExecUser(adminUser, adminPassword);
                }

                process.Execute("sc start " + serviceName);
                errorMsg = process.OutputString;

                return errorMsg;
            }
            catch (Exception ex)
            {
                errorMsg = ex.GetAllMessages();
            }

            return errorMsg;
        }

        private void txtServiceID_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            this.CheckServiceStatus();
        }

        private void btnServices_OnClick(object sender, RoutedEventArgs e)
        {
            var services = ServiceController.GetServices().Where(x => x.ServiceName.StartsWith(this.servicePrefix));

            this.lb_Services.Items.Clear();
            foreach (ServiceController service in services)
            {
                this.lb_Services.Items.Add(service.ServiceName + " " + service.Status);
            }
        }
    }

    public sealed class Settings
    {
        public string SystemUser { get; set; }
        public string SystemDomain { get; set; }
        public string SystemPassword { get; set; }
        public string ServiceName { get; set; }
    }
}