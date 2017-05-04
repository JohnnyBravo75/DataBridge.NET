using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using Microsoft.Win32;

namespace DataBridge.GUI
{
    public class ServiceController : System.ServiceProcess.ServiceController
    {
        private string imagePath;
        private ServiceController[] dependentServices;
        private ServiceController[] servicesDependedOn;

        public ServiceController()
            : base()
        {
        }

        public ServiceController(string name)
            : base(name)
        {
        }

        public ServiceController(string name, string machineName)
            : base(name, machineName)
        {
        }

        public string ImagePath
        {
            get
            {
                if (this.imagePath == null)
                {
                    this.imagePath = this.GetImagePath();
                }
                return this.imagePath;
            }
        }

        public new ServiceControllerStatus Status
        {
            get
            {
                return base.Status;
            }
        }

        public string ServiceDirectory
        {
            get
            {
                string path = this.ImagePath.Trim().Trim('"');

                if (string.IsNullOrEmpty(path))
                {
                    return string.Empty;
                }

                return Path.GetDirectoryName(path);
            }
        }

        public string Description
        {
            get
            {
                var path = new ManagementPath("Win32_Service.Name='" + this.ServiceName + "'");
                var managementObj = new ManagementObject(path);
                if (managementObj["Description"] != null)
                {
                    return managementObj["Description"].ToString();
                }

                return string.Empty;
            }
        }

        public string StartupType
        {
            get
            {
                if (this.ServiceName != null)
                {
                    var path = new ManagementPath("Win32_Service.Name='" + this.ServiceName + "'");
                    var managementObj = new ManagementObject(path);
                    return managementObj["StartMode"].ToString();
                }

                return string.Empty;
            }
            set
            {
                if (value != "Automatic" && value != "Manual" && value != "Disabled")
                    throw new Exception("The valid values are Automatic, Manual or Disabled");

                if (this.ServiceName != null)
                {
                    var path = new ManagementPath("Win32_Service.Name='" + this.ServiceName + "'");
                    var managementObj = new ManagementObject(path);

                    managementObj.InvokeMethod("ChangeStartMode", new object[] { value });
                }
            }
        }

        public bool IsInstalled()
        {
            return IsInstalled(this.ServiceName, this.MachineName);
        }

        public static bool IsInstalled(string serviceName, string machineName = "")
        {
            var serviceControllers = GetServices(!string.IsNullOrEmpty(machineName)
                                            ? machineName
                                            : ".");

            try
            {
                using (var sc = serviceControllers.FirstOrDefault(s => s.ServiceName == serviceName &&
                                                             s.MachineName == machineName))
                {
                    return sc != null;
                }
            }
            catch
            {
                return false;
            }
            finally
            {
                foreach (var serviceController in serviceControllers)
                {
                    serviceController.Dispose();
                }
            }
        }

        public bool IsInstalledWithStatus(ServiceControllerStatus status)
        {
            bool installed = (this.IsInstalled() && this.Status == status)
                                ? true
                                : false;
            return installed;
        }

        public new ServiceController[] DependentServices
        {
            get
            {
                if (this.dependentServices == null)
                {
                    this.dependentServices = GetServices(base.DependentServices);
                }
                return this.dependentServices;
            }
        }

        public new ServiceController[] ServicesDependedOn
        {
            get
            {
                if (this.servicesDependedOn == null)
                {
                    this.servicesDependedOn = GetServices(base.ServicesDependedOn);
                }
                return this.servicesDependedOn;
            }
        }

        public static new ServiceController[] GetServices()
        {
            return GetServices(".");
        }

        public static new ServiceController[] GetServices(string machineName)
        {
            return GetServices(System.ServiceProcess.ServiceController.GetServices
                (machineName));
        }

        private string GetImagePath()
        {
            string registryPath = @"SYSTEM\CurrentControlSet\Services\" + this.ServiceName;

            RegistryKey key;
            if (this.MachineName != "" && this.MachineName != ".")
            {
                key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, this.MachineName).OpenSubKey(registryPath);
            }
            else
            {
                key = Registry.LocalMachine.OpenSubKey(registryPath);
            }

            string value = "";
            if (key != null)
            {
                value = key.GetValue("ImagePath").ToString().Trim();
                key.Close();
                key.Dispose();
            }

            return EnvironmentHelper.ExpandEnvironmentVariables(value);
        }

        private static ServiceController[] GetServices(System.ServiceProcess.ServiceController[] systemServices)
        {
            var services = new List<ServiceController>(systemServices.Length);

            foreach (System.ServiceProcess.ServiceController service in systemServices)
            {
                services.Add(new ServiceController(service.ServiceName, service.MachineName));
            }

            return services.ToArray();
        }

        public ServiceController Self
        {
            get
            {
                return this;
            }
        }
    }
}