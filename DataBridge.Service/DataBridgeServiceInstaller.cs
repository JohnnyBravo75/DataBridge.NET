namespace DataBridge.Service
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Configuration.Install;
    using System.ServiceProcess;
    using Microsoft.Win32;

    [RunInstaller(true)]
    public class DataBridgeServiceInstaller : Installer
    {
        private readonly string serviceName;

        private ServiceInstaller serviceInstaller = new ServiceInstaller();

        public DataBridgeServiceInstaller()
        {
            // Debugger.Launch();

            string[] args = Environment.GetCommandLineArgs();

            var serviceContext = new InstallContext(null, args);
            var parameters = serviceContext.Parameters;

            this.serviceName = parameters["ServiceName"];

            var processInstaller = new ServiceProcessInstaller();

            switch (parameters["ServiceAccount"])
            {
                case "LocalService":
                    processInstaller.Account = ServiceAccount.LocalService;
                    break;

                case "User":
                    processInstaller.Account = ServiceAccount.User;
                    processInstaller.Username = parameters["UserName"];
                    processInstaller.Password = parameters["Password"];
                    break;

                case "LocalSystem":
                    processInstaller.Account = ServiceAccount.LocalSystem;
                    break;
            }

            this.Installers.Add(processInstaller);

            this.serviceInstaller.Context = serviceContext;
            this.serviceInstaller.DisplayName = parameters["ServiceName"];
            this.serviceInstaller.ServiceName = parameters["ServiceName"];
            this.serviceInstaller.StartType = ServiceStartMode.Automatic;
            this.serviceInstaller.DelayedAutoStart = true;

            this.Installers.Add(this.serviceInstaller);
        }

        public override void Install(IDictionary stateServer)
        {
            // System.Diagnostics.Debugger.Launch();

            try
            {
                base.Install(stateServer);

                this.ModifyServicePathEntry();
            }
            catch (Exception ex)
            {
                Console.WriteLine("An exception was thrown during service installation:\n" + ex.ToString());
                throw;
            }
        }

        protected override void OnCommitted(IDictionary savedState)
        {
            base.OnCommitted(savedState);

            //this.StartService();
        }

        protected override void OnAfterInstall(IDictionary savedState)
        {
            base.OnAfterInstall(savedState);

            //this.StartService();
        }

        private void StartService()
        {
            // System.Diagnostics.Debugger.Launch();

            using (var serviceController = new ServiceController(this.serviceInstaller.ServiceName, Environment.MachineName))
            {
                if (serviceController.Status == ServiceControllerStatus.Stopped)
                {
                    serviceController.Start();
                }
            }
        }

        private bool ModifyServicePathEntry()
        {
            var serviceKey = Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services")
                                                  .OpenSubKey(this.serviceName, true);

            if (serviceKey != null)
            {
                var configKey = serviceKey.CreateSubKey("Parameters");
                configKey.SetValue("ServiceName", this.serviceName);

                string path = serviceKey.GetValue("ImagePath").ToString();
                path = path.Trim();
                path = path.Trim('"');
                path = path + " " + this.serviceName;
                serviceKey.SetValue("ImagePath", path);

                return true;
            }

            return false;
        }
    }
}