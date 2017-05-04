using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Timers;
using System.Windows;
using DataBridge.Extensions;
using DataBridge.GUI.Core.View;
using DataBridge.Runtime;
using Microsoft.Practices.Prism.Commands;

namespace DataBridge.GUI.ViewModels
{
    public class ServiceControllerViewModel : ViewModelBase
    {
        private bool canStartService;
        private bool canStopService;
        private ServiceController currentService;
        private DataBridgeInfo currentDataBridgeInfo;

        // ************************************Constructor**********************************************

        public ServiceControllerViewModel()
        {
            this.ServiceControllerCommand = new DelegateCommand<string>(this.ExecuteServiceControllerCommand, this.CanExecuteServiceControllerCommand);

            this.CheckServiceStatus();
        }

        // ************************************Delegate**********************************************

        public DelegateCommand<string> ServiceControllerCommand { get; private set; }

        // ************************************Properties**********************************************


        public ServiceController CurrentService
        {
            get { return this.currentService; }
            set
            {
                if (this.currentService != value)
                {
                    this.currentService = value;
                    this.RaisePropertyChanged("CurrentService");

                    this.CheckServiceStatus();

                    if (this.currentService != null)
                    {
                        this.CurrentDataBridgeInfo = DataBridgeManager.Instance.LoadDataBridgeinDirectory(this.ConfigDirectory);
                    }
                }
            }
        }

        public PipelineInfo CurrentPipelineInfo { get; set; }

        public ObservableCollection<PipelineInfo> CurrentPipelineInfos
        {
            get
            {
                if (this.CurrentDataBridgeInfo == null)
                {
                    return null;
                }

                foreach (var item in this.CurrentDataBridgeInfo.PipelineInfos)
                {
                    string logPathComplete = Path.Combine(this.CurrentService.ServiceDirectory, item.LogFile);
                    item.Properties.AddOrUpdate("LogPathComplete", logPathComplete);

                    item.Properties.AddOrUpdate("LastLogWriteTime", File.Exists(logPathComplete)
                                                                              ? (DateTime?)File.GetLastWriteTime(logPathComplete)
                                                                              : null);
                }

                return this.CurrentDataBridgeInfo.PipelineInfos;
            }
        }

        public DataBridgeInfo CurrentDataBridgeInfo
        {
            get { return this.currentDataBridgeInfo; }
            set
            {
                if (this.currentDataBridgeInfo != value)
                {
                    this.currentDataBridgeInfo = value;
                    this.RaisePropertyChanged("CurrentDataBridgeInfo");
                    this.RaisePropertyChanged("CurrentPipelineInfos");
                }
            }
        }

        public string CurrentServiceStatus
        {
            get
            {
                if (this.CurrentService == null)
                {
                    return "No service defined";
                }

                if (!this.CurrentService.IsInstalled())
                {
                    return "Not installed";
                }

                return this.CurrentService.Status.ToString();
            }
        }

        public bool CanStartService
        {
            get { return this.canStartService; }
            private set
            {
                if (this.canStartService != value)
                {
                    this.canStartService = value;
                    this.RaisePropertyChanged("CanStartService");
                }
            }
        }

        public bool CanStopService
        {
            get { return this.canStopService; }
            private set
            {
                if (this.canStopService != value)
                {
                    this.canStopService = value;
                    this.RaisePropertyChanged("CanStopService");
                }
            }
        }

        public void ExecuteServiceControllerCommand(string action)
        {
            if (!this.CanExecuteServiceControllerCommand(action))
            {
                return;
            }

            switch (action)
            {
                case "Start":
                    this.HandleStart();
                    break;

                case "Stop":
                    this.HandleStop();
                    break;

                case "OpenFolder":
                    Process.Start("explorer.exe", this.CurrentService.ServiceDirectory);
                    break;

                case "OpenLogFile":
                    Process.Start("notepad.exe", this.CurrentPipelineInfo.Properties["LogPathComplete"].ToStringOrEmpty());
                    break;

                default:
                    throw new Exception(string.Format("The action '{0}' is not supported", action));
            }
        }

        public bool CanExecuteServiceControllerCommand(string action)
        {
            switch (action)
            {
                case "Start":
                    if (!EnvironmentHelper.IsRunAsAdmin)
                    {
                        return false;
                    }

                    if (this.CurrentService == null)
                    {
                        return false;
                    }

                    if (!this.CanStartService)
                    {
                        return false;
                    }
                    break;

                case "Stop":
                    if (!EnvironmentHelper.IsRunAsAdmin)
                    {
                        return false;
                    }

                    if (this.CurrentService == null)
                    {
                        return false;
                    }

                    if (!this.CanStopService)
                    {
                        return false;
                    }
                    break;

                case "OpenFolder":
                    if (this.CurrentService == null)
                    {
                        return false;
                    }
                    break;

                case "OpenLogFile":
                    if (this.CurrentService == null)
                    {
                        return false;
                    }

                    if (this.CurrentPipelineInfo == null)
                    {
                        return false;
                    }
                    break;
            }

            return true;
        }

        public string ConfigDirectory
        {
            get
            {
                if (this.CurrentService == null)
                {
                    return string.Empty;
                }
                return Path.Combine(this.CurrentService.ServiceDirectory, "Configs");
            }
        }

        // ************************************Functions**********************************************

        public DataBridgeInfo LoadDataBridgeinDirectory(string configDirectory)
        {
            if (this.CurrentService == null)
            {
                return null;
            }
            return DataBridgeManager.Instance.LoadDataBridgeinDirectory(configDirectory);
        }

        private void HandleStart()
        {
            try
            {
                if (this.CurrentService.Status != ServiceControllerStatus.Running)
                {
                    ViewManager.Instance.ShowWaitCursor();
                    this.CurrentService.Start();
                    this.CurrentService.WaitForStatus(ServiceControllerStatus.Running, new TimeSpan(0, 0, 15));
                    ViewManager.Instance.HideWaitCursor();
                }
                else
                {
                    MessageBox.Show(string.Format("Service '{0}' is already running", this.CurrentService.ServiceName), "Warning", MessageBoxButton.OK);
                }
            }
            catch (Exception ex)
            {
                ViewManager.Instance.HideWaitCursor();
                MessageBox.Show(string.Format("Error when starting service '{0}'! \r\n{1}", this.CurrentService.ServiceName, ex.GetAllMessages()), "Error");
            }
            finally
            {
                this.CheckServiceStatus();
                this.RaisePropertyChanged("Services");
                this.RaisePropertyChanged("CurrentService");
            }
        }

        private void HandleStop()
        {
            try
            {
                if (this.CurrentService.Status != ServiceControllerStatus.Stopped)
                {
                    ViewManager.Instance.ShowWaitCursor();
                    this.CurrentService.Stop();
                    this.CurrentService.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
                    ViewManager.Instance.HideWaitCursor();
                }
                else
                {
                    MessageBox.Show(string.Format("Service '{0}' is already stopped", this.CurrentService.ServiceName), "Warning");
                }
            }
            catch (Exception ex)
            {
                ViewManager.Instance.HideWaitCursor();
                MessageBox.Show(string.Format("Error when stopping service '{0}'! \r\n{1}", this.CurrentService.ServiceName, ex.GetAllMessages()), "Error");
            }
            finally
            {
                this.CheckServiceStatus();
                this.RaisePropertyChanged("Services");
                this.RaisePropertyChanged("CurrentService");
            }
        }

        public void CheckServiceStatus(int delayMilliSeconds = 50)
        {
            this.CanStartService = false;
            this.CanStopService = false;

            if (this.CurrentService == null)
            {
                this.RaisePropertyChanged("CurrentServiceStatus");
                return;
            }

            System.Threading.Thread.Sleep(delayMilliSeconds);
            try
            {
                this.CurrentService.Refresh();

                if (!this.CurrentService.IsInstalled())
                {
                    this.RaisePropertyChanged("CurrentServiceStatus");
                    return;
                }

                var serviceStatus = this.CurrentService.Status;

                if (serviceStatus == ServiceControllerStatus.Running ||
                    serviceStatus == ServiceControllerStatus.StartPending)
                {
                    this.CanStopService = true;
                }
                else if (serviceStatus == ServiceControllerStatus.Stopped ||
                         serviceStatus == ServiceControllerStatus.StopPending)
                {
                    this.CanStartService = true;
                }
                else
                {
                    throw new Exception("Service Status is unknown");
                }
            }
            catch (Exception)
            {
                //this.CurrentServiceStatus = "Not installed";
            }

            this.ServiceControllerCommand.RaiseCanExecuteChanged();

            this.RaisePropertyChanged("CurrentServiceStatus");
        }
    }
}