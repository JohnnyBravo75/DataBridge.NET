using System;
using System.Reflection;
using System.ServiceProcess;
using System.Threading;
using System.Windows;
using System.Windows.Media.TextFormatting;
using DataBridge.Extensions;

namespace DataBridge.GUI.ViewModels
{
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using DataBridge.GUI.Core.View;
    using DataBridge.GUI.Core.View.ViewModels;
    using DataBridge.GUI.Model;
    using DataBridge.Runtime;
    using DataBridge.Services;
    using Microsoft.Practices.Prism.Commands;

    public class ServiceMonitorViewModel : ViewModelBase
    {
        #region ************************************Fields**********************************************

        private ServiceController currentService;

        private string servicePrefix = "DataBridge_";

        private SystemInfo systemInfo = new SystemInfo();

        private bool isNew;

        private bool autoStartAfterInstall = true;

        #endregion ************************************Fields**********************************************

        #region ************************************Constructor**********************************************

        public ServiceMonitorViewModel()
        {
            this.ServiceMonitorCommand = new DelegateCommand<string>(this.ExecuteServiceMonitorCommand, this.CanExecuteServiceMonitorCommand);

            this.NewServiceID = "";
            this.NewTargetPath = Environment.ExpandEnvironmentVariables(@"%TEMP%");
        }

        #endregion ************************************Constructor**********************************************

        #region ************************************Properties**********************************************

        public DelegateCommand<string> ServiceMonitorCommand { get; private set; }

        public string ServiceSourceDir
        {
            get { return string.Format(@"{0}\Service\", this.WorkDir); }
        }

        public string ServiceExecutableName
        {
            get { return "DataBridge.Service.exe"; }
        }

        public string WorkDir
        {
            get { return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location); }
        }

        public string NewServiceName
        {
            get { return this.servicePrefix + this.NewServiceID; }
        }

        public string NewTargetPath { get; set; }

        public string NewMachineName { get; set; }

        public string TargetDir
        {
            get { return Path.Combine(this.NewTargetPath, this.NewServiceName); }
        }

        public bool IsRunAsAdmin
        {
            get { return EnvironmentHelper.IsRunAsAdmin; }
        }

        public ObservableCollection<ServiceController> Services
        {
            get
            {
                var oldService = this.CurrentService;

                var services = ServiceController.GetServices()
                                                .Where(x => x.ServiceName.StartsWith(this.servicePrefix))
                                                .ToObservableCollection();

                if (oldService != null)
                {
                    this.CurrentService = services.FirstOrDefault(x => x.MachineName == oldService.MachineName &&
                                                                       x.ServiceName == oldService.ServiceName);
                }
                return services;
            }
        }

        public ServiceController CurrentService
        {
            get { return this.currentService; }
            set
            {
                if (this.currentService != value)
                {
                    this.currentService = value;
                    this.RaisePropertyChanged("CurrentService");
                    this.IsNew = false;
                    this.ServiceMonitorCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public string NewServiceID { get; set; }

        public bool IsNew
        {
            get { return this.isNew; }
            set
            {
                if (this.isNew != value)
                {
                    this.isNew = value;
                    this.RaisePropertyChanged("IsNew");
                }
            }
        }

        public bool AutoStartAfterInstall
        {
            get { return this.autoStartAfterInstall; }
            set { this.autoStartAfterInstall = value; }
        }

        #endregion ************************************Properties**********************************************

        #region ************************************Functions**********************************************

        public void ExecuteServiceMonitorCommand(string action)
        {
            if (!this.CanExecuteServiceMonitorCommand(action))
            {
                return;
            }

            switch (action)
            {
                case "Install":
                    this.HandleInstall();
                    break;

                case "Uninstall":
                    this.HandleUninstall();
                    break;

                case "New":
                    this.HandleNew();
                    break;

                case "Cancel":
                    this.IsNew = false;
                    this.CurrentService = this.Services.FirstOrDefault();
                    break;

                case "RunAsAdmin":
                    EnvironmentHelper.RunAsAdmin(string.Empty);
                    break;

                default:
                    throw new Exception(string.Format("The action '{0}' is not supported", action));
            }

            this.ServiceMonitorCommand.RaiseCanExecuteChanged();
        }

        public bool CanExecuteServiceMonitorCommand(string action)
        {
            switch (action)
            {
                case "Install":
                    if (!this.IsRunAsAdmin)
                    {
                        return false;
                    }

                    if (this.currentService == null)
                    {
                        return false;
                    }

                    if (this.currentService.IsInstalled())
                    {
                        return false;
                    }
                    break;

                case "Uninstall":
                    if (!this.IsRunAsAdmin)
                    {
                        return false;
                    }

                    if (this.currentService == null)
                    {
                        return false;
                    }

                    if (!this.currentService.IsInstalled())
                    {
                        return false;
                    }
                    break;

                case "New":
                    if (!this.IsRunAsAdmin)
                    {
                        return false;
                    }

                    if (this.IsNew)
                    {
                        return false;
                    }
                    break;

                case "Cancel":
                    if (!this.IsNew)
                    {
                        return false;
                    }
                    break;

                case "RunAsAdmin":
                    if (this.IsRunAsAdmin)
                    {
                        return false;
                    }
                    break;
            }

            return true;
        }

        private ServiceController CreateNewService()
        {
            return new ServiceController()
            {
                ServiceName = this.NewServiceName,
                MachineName = Environment.MachineName
            };
        }

        private void HandleNew()
        {
            var newService = this.CreateNewService();
            this.CurrentService = newService;
            this.IsNew = true;
            this.ServiceMonitorCommand.RaiseCanExecuteChanged();
        }

        private string ValidateInstall()
        {
            if (string.IsNullOrEmpty(this.NewServiceID))
            {
                return "The ServiceID must not be empty";
            }

            if (this.CurrentService.IsInstalled())
            {
                return string.Format("The service '{0}' exists already.", this.CurrentService.ServiceName);
            }

            return "";
        }

        private void HandleInstall()
        {
            this.CurrentService.ServiceName = this.NewServiceName;
            this.CurrentService.MachineName = Environment.MachineName;

            var msg = this.ValidateInstall();
            if (!string.IsNullOrEmpty(msg))
            {
                MessageBox.Show(msg, "Validation error");
                return;
            }

            try
            {
                string serviceDirectory = this.TargetDir;

                ViewManager.Instance.ShowWaitCursor();
                string errorMsg = LocalServiceInstaller.Instance.InstallService(this.NewServiceName, Path.Combine(serviceDirectory, this.ServiceExecutableName), this.systemInfo.SystemUser, this.systemInfo.SystemPassword, this.ServiceSourceDir);
                ViewManager.Instance.HideWaitCursor();

                if (!string.IsNullOrEmpty(errorMsg))
                {
                    throw new Exception(errorMsg);
                }

                Thread.Sleep(100);

                if (this.AutoStartAfterInstall)
                {
                    this.CurrentService.Start();
                    this.CurrentService.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 15));
                }
                this.RaisePropertyChanged("CurrentService");
            }
            catch (Exception ex)
            {
                ViewManager.Instance.HideWaitCursor();
                MessageBox.Show(string.Format("Error during installing service! \r\n{0}", ex.GetAllMessages()), "Error");
            }
            finally
            {
                this.RaisePropertyChanged("Services");
            }
        }

        private void HandleUninstall()
        {
            try
            {
                var response = MessageBox.Show(string.Format("The service '{0}' will be shutdown and deleted! Continue?", this.CurrentService.ServiceName), "Warnung", MessageBoxButton.YesNo);

                if (response == MessageBoxResult.Yes)
                {
                    ViewManager.Instance.ShowWaitCursor();
                    string errorMsg = LocalServiceInstaller.Instance.UnInstallService(this.CurrentService.ServiceName, this.CurrentService.ServiceDirectory);
                    ViewManager.Instance.HideWaitCursor();

                    MessageBox.Show(string.Format("Error during removing service! \r\n{0}", errorMsg), "ERROR");
                }
            }
            catch (Exception ex)
            {
                ViewManager.Instance.HideWaitCursor();
                MessageBox.Show(string.Format("Error during removing service! \r\n{0}", ex.GetAllMessages()), "ERROR");
            }
            finally
            {
                this.RaisePropertyChanged("Services");
            }
        }

        #endregion ************************************Functions**********************************************
    }
}