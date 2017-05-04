using System;
using System.Configuration.Install;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using DataBridge.Common.Helper;
using DataBridge.Extensions;
using DataBridge.Helper;

namespace DataBridge.Services
{
    public class LocalServiceInstaller
    {
        private static readonly LocalServiceInstaller instance = new LocalServiceInstaller();

        private const string INSTALLEREXE = "InstallUtil.exe";

        private LocalServiceInstaller()
        {
        }

        public static LocalServiceInstaller Instance
        {
            get { return instance; }
        }

        public string InstallService(string serviceName, string physicalLocation, string userName, string password, string serviceSourceDirectory, bool autoRestartOption = true)
        {
            if (string.IsNullOrEmpty(userName))
            {
                userName = "LocalSystem";
            }

            if (userName.IndexOf('\\') < 0)
            {
                userName = ".\\" + userName;
            }

            string errorMsg;
            string installerDir = this.GetInstallerDirectory(out errorMsg);
            if (string.IsNullOrEmpty(installerDir))
            {
                return errorMsg;
            }

            string serviceTargetDir = Path.GetDirectoryName(physicalLocation);

            DirectoryUtil.CreateDirectoryIfNotExists(serviceTargetDir);

            Environment.CurrentDirectory = serviceTargetDir;

            FileUtil.CopyDirectory(serviceSourceDirectory, serviceTargetDir, true);

            //            var installParams = new[]
            //{
            //                "/ServiceAccount=" + ServiceAccount.LocalSystem,
            //                "/UserName=" + userName,
            //                "/Password=" + password,
            //                "/ServiceName=\"" + serviceName + "\"",
            //                "\""+ physicalLocation + "\""
            //            };

            //            // ManagedInstallerClass.InstallHelper(installParams);

            //            string installStmt = installerDir + INSTALLEREXE + " " + string.Join(" ", installParams);



            string installStmt = installerDir + string.Format(INSTALLEREXE + " /ServiceAccount={0} /UserName={1} /Password={2} /ServiceName=\"{3}\" \"{4}\" ", ServiceAccount.LocalSystem, userName, password, serviceName, physicalLocation);

            var process = new ProcessHandler("cmd");
            var isOk = process.Execute(installStmt);
            string result = process.OutputString;

            if (!isOk || result.ContainsAny("Error", "Exception", "Rollback"))
            {
                errorMsg = "Error: An error occured during installing the service." + Environment.NewLine + Environment.NewLine + result;
            }
            else if (autoRestartOption)
            {
                this.SetServiceRecovery(serviceName);
            }

            return errorMsg;
        }

        private string GetInstallerDirectory(out string errorMsg)
        {
            string installerDir = RuntimeEnvironment.GetRuntimeDirectory();
            errorMsg = "";

            if (!File.Exists(Path.Combine(installerDir, INSTALLEREXE)))
            {
                installerDir = Environment.CurrentDirectory;
            }

            if (!File.Exists(Path.Combine(installerDir, INSTALLEREXE)))
            {
                errorMsg = string.Format("'{0}' was not found in '{1}'", INSTALLEREXE, installerDir);

            }
            return installerDir;
        }

        private void SetServiceRecovery(string serviceName)
        {
            var process = new ProcessHandler("cmd");
            process.Execute("sc failure \"" + serviceName + "\" reset= 0 actions= restart/60000");
        }

        public string UnInstallService(string serviceName, string serviceDir, bool clearServiceDir = true)
        {
            Thread.Sleep(100);

            string errorMsg = "";

            try
            {
                //  ManagedInstallerClass.InstallHelper(new [] { "/u", serviceDir });
                var process = new ProcessHandler("cmd");

                process.Execute("sc Stop \"" + serviceName + "\"");
                process.Execute("sc Delete \"" + serviceName + "\"");

                errorMsg = process.OutputString;

                if (clearServiceDir)
                {
                    Thread.Sleep(5000);
                    DirectoryUtil.DeleteDirectory(serviceDir);
                }

                return errorMsg;
            }
            catch (Exception ex)
            {
                errorMsg = ex.GetAllMessages();
            }

            return errorMsg;
        }
    }
}