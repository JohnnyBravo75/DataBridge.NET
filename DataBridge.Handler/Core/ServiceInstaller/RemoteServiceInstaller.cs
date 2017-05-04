using System;

namespace DataBridge.Services
{
    public class RemoteServiceInstaller
    {
        private static readonly RemoteServiceInstaller instance = new RemoteServiceInstaller(new WmiAccess());

        static RemoteServiceInstaller()
        {
        }

        private RemoteServiceInstaller()
        {
        }

        public static RemoteServiceInstaller Instance
        {
            get { return instance; }
        }

        private readonly IWmiAccess wmi;

        /// <summary>
        /// Creates a new RemoteServiceInstaller for use to access Windows Services
        /// </summary>
        /// <param name="wmi">The WMI access object - the tool that does the low level work</param>
        public RemoteServiceInstaller(IWmiAccess wmi)
        {
            this.wmi = wmi;
        }

        public ServiceReturnCode Install(string name, string displayName, string physicalLocation, ServiceStartMode startMode, string userName, string password, string[] dependencies)
        {
            return this.Install(Environment.MachineName, name, displayName, physicalLocation, startMode, userName, password, dependencies, false);
        }

        public ServiceReturnCode Install(string machineName, string name, string displayName, string physicalLocation, ServiceStartMode startMode, string userName, string password, string[] dependencies)
        {
            return this.Install(machineName, name, displayName, physicalLocation, startMode, userName, password, dependencies, false);
        }

        /// <summary>
        /// Installs a service on any machine
        /// </summary>
        /// <param name="machineName">Name of the computer to perform the operation on</param>
        /// <param name="serviceName">The name of the service in the registry</param>
        /// <param name="displayName">The display name of the service in the service manager</param>
        /// <param name="physicalLocation">The physical disk location of the executable</param>
        /// <param name="startMode">How the service starts - usually Automatic</param>
        /// <param name="userName">The user for the service to run under</param>
        /// <param name="password">The password fo the user</param>
        /// <param name="dependencies">Other dependencies the service may have based on the name of the service in the registry</param>
        /// <param name="interactWithDesktop">Should the service interact with the desktop?</param>
        /// <returns>A service return code that defines whether it was successful or not</returns>
        public ServiceReturnCode Install(string machineName, string serviceName, string displayName, string physicalLocation, ServiceStartMode startMode, string userName, string password, string[] dependencies, bool interactWithDesktop)
        {
            if (string.IsNullOrEmpty(userName))
            {
                userName = "LocalSystem";
            }

            if (userName.IndexOf('\\') < 0)
            {
                userName = ".\\" + userName;
            }

            try
            {
                object[] parameters = new object[]
                                      {
                                          serviceName,                                       // Name
                                          displayName,                                       // Display Name
                                          physicalLocation,                                  // Path Name | The Location "E:\somewhere\something"
                                          Convert.ToInt32(ServiceType.OwnProcess),           // ServiceType
                                          Convert.ToInt32(ServiceErrorControl.UserNotified), // Error Control
                                          startMode.ToString(),                              // Start Mode
                                          interactWithDesktop,                               // Desktop Interaction
                                          userName,                                          // StartName | Username
                                          password,                                          // StartPassword |Password
                                          null,                                              // LoadOrderGroup | Service Order Group
                                          null,                                              // LoadOrderGroupDependencies | Load Order Dependencies
                                          dependencies                                       // ServiceDependencies
                                      };
                return (ServiceReturnCode)this.wmi.InvokeStaticMethod(machineName, "Win32_Service", "Create", parameters);
            }
            catch
            {
                return ServiceReturnCode.UnknownFailure;
            }
        }

        public ServiceReturnCode Uninstall(string serviceName)
        {
            return this.Uninstall(Environment.MachineName, serviceName);
        }

        /// <summary>
        /// Uninstalls a service on any machine
        /// </summary>
        /// <param name="machineName">Name of the computer to perform the operation on</param>
        /// <param name="serviceName">The name of the service in the registry</param>
        /// <returns>A service return code that defines whether it was successful or not</returns>
        public ServiceReturnCode Uninstall(string machineName, string serviceName)
        {
            try
            {
                return (ServiceReturnCode)this.wmi.InvokeInstanceMethod(machineName, "Win32_Service", serviceName, "Delete");
            }
            catch
            {
                return ServiceReturnCode.UnknownFailure;
            }
        }
    }
}