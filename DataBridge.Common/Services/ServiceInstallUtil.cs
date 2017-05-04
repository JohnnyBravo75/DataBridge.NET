using System;
using System.IO;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Threading;
using DataBridge.Extensions;
using DataBridge.Helper;

namespace DataBridge.Services
{
    public class ServiceInstallUtil
    {
        private static readonly ServiceInstallUtil instance = new ServiceInstallUtil();

        private ServiceInstallUtil()
        {
        }

        public static ServiceInstallUtil Instance
        {
            get { return instance; }
        }

        public string InstallService(string serviceName, string physicalLocation, string userName, string password, string serviceSourceDirectory)
        {
            if (string.IsNullOrEmpty(userName))
            {
                userName = "LocalSystem";
            }

            if (userName.IndexOf('\\') < 0)
            {
                userName = ".\\" + userName;
            }

            string installerDir = RuntimeEnvironment.GetRuntimeDirectory();
            string errorMsg = "";

            if (!File.Exists(Path.Combine(installerDir, "InstallUtil.exe")))
            {
                installerDir = Environment.CurrentDirectory;
            }

            if (!File.Exists(Path.Combine(installerDir, "InstallUtil.exe")))
            {
                errorMsg = string.Format("'InstallUtil.exe' was not found in '{0}'", installerDir);
                return errorMsg;
            }

            string serviceTargetDir = Path.GetDirectoryName(physicalLocation);

            DirectoryUtil.CreateDirectoryIfNotExists(serviceTargetDir);

            Environment.CurrentDirectory = serviceTargetDir;

            FileUtil.CopyDirectory(serviceSourceDirectory, serviceTargetDir, true);

            var process = new ProcessHandler("cmd");

            string stmt = installerDir + string.Format("installutil.exe /ServiceAccount={0} /UserName={1} /Password={2} /ServiceName={3} \"{4}\" ", ServiceAccount.LocalSystem, userName, password, serviceName, physicalLocation);

            var resultCode = process.Execute(stmt);
            string result = process.OutputString;

            if (result.Contains("Fehler") || result.Contains("Exception") || result.Contains("Rollback"))
            {
                errorMsg = "ERROR: Beim Installieren des Dienstes, bitte Logs überprüfen" + Environment.NewLine + Environment.NewLine + result;
            }

            return errorMsg;
        }

        public string UnInstallService(string serviceName, string serviceDir, bool clearServiceDir = true)
        {
            string errorMsg = "";

            try
            {
                var process = new ProcessHandler("cmd");

                process.Execute("sc Stop " + serviceName);

                process.Execute("sc Delete " + serviceName);
                errorMsg = process.OutputString;

                if (clearServiceDir)
                {
                    Thread.Sleep(200);
                    Directory.Delete(serviceDir, true);
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