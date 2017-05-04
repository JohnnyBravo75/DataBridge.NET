using System;
using System.Configuration.Install;
using System.Reflection;
using System.ServiceProcess;

namespace DataBridge.Service
{
    public static class Program
    {
        public const string EventLogName = "DataBridgeLog";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            if (args != null &&
                args.Length == 1 &&
                args[0].Length > 1 &&
                (args[0][0] == '-' || args[0][0] == '/'))
            {
                //if (System.Environment.UserInteractive)
                //{
                switch (args[0].Substring(1).ToLower())
                {
                    case "i":
                    case "install":
                        ManagedInstallerClass.InstallHelper(new string[] { Assembly.GetExecutingAssembly().Location });
                        break;

                    case "u":
                    case "uninstall":
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Assembly.GetExecutingAssembly().Location });
                        break;
                }
                //}
            }
            else
            {
                var servicesToRun = new ServiceBase[]
                                        {
                                                new DataBridgeService(args)
                                        };

                ServiceBase.Run(servicesToRun);
            }
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }
    }
}