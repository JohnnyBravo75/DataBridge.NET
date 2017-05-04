using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using DataBridge.Extensions;
using Microsoft.Win32;

namespace DataBridge
{
    public static class EnvironmentHelper
    {
        public static bool ContainsEnvironmentVariables(string path)
        {
            // custom vars
            var envVariables = new List<string>(new string[] { "%CURRENT_DIRECTORY%", "%TIME%", "%DATE%", "%MYDOCUMENTS%", "%SYSTEM_DIRECTORY%", "%MACHINE_NAME%", "%USER_NAME%", "%USER_DOMAIN_NAME%" });

            // standard vars
            envVariables.AddRange(from DictionaryEntry e in Environment.GetEnvironmentVariables()
                                  select "%" + e.Key.ToString() + "%");

            // special folders
            var specialFolders = EnumHelper.GetValues<Environment.SpecialFolder>();
            envVariables.AddRange(specialFolders.Select(specialFolder => "%" + specialFolder.ToString().ToUpper() + "%"));

            return path.ContainsAny(envVariables.ToArray());
        }

        public static string ExpandEnvironmentVariables(string path, string machineName = "")
        {
            if (string.IsNullOrEmpty(machineName) || machineName == ".")
            {
                if (!string.IsNullOrEmpty(path))
                {
                    if (path.IndexOf('%') != -1)
                    {
                        // standard vars
                        path = Environment.ExpandEnvironmentVariables(path);

                        // special folders
                        var specialFolders = EnumHelper.GetValues<Environment.SpecialFolder>();
                        foreach (var specialFolder in specialFolders)
                        {
                            path = path.Replace("%" + specialFolder.ToString().ToUpper() + "%", Environment.GetFolderPath(specialFolder));
                        }

                        // custom vars
                        var now = DateTime.Now;

                        path = path.Replace("%TIME%", now.ToString("T") + "," + now.ToString("ff"));
                        path = path.Replace("%DATE%", now.ToString("d"));
                        path = path.Replace("%CURRENT_DIRECTORY%", Environment.CurrentDirectory);
                        path = path.Replace("%SYSTEM_DIRECTORY%", Environment.SystemDirectory);
                        path = path.Replace("%MACHINE_NAME%", Environment.MachineName);
                        path = path.Replace("%USER_NAME%", Environment.UserName);
                        path = path.Replace("%USER_DOMAIN_NAME%", Environment.UserDomainName);
                    }
                }

                return path;
            }
            else
            {
                string systemRootKey = @"Software\Microsoft\Windows NT\CurrentVersion\";

                var key = RegistryKey.OpenRemoteBaseKey(RegistryHive.LocalMachine, machineName).OpenSubKey(systemRootKey);
                if (key != null)
                {
                    string expandedSystemRoot = key.GetValue("SystemRoot").ToString();
                    key.Close();
                    key.Dispose();

                    path = path.Replace("%SystemRoot%", expandedSystemRoot);
                }

                return path;
            }
        }

        public static bool IsRunAsAdmin
        {
            get
            {
                bool isAdmin = false;
                try
                {
                    var identity = WindowsIdentity.GetCurrent();
                    if (identity == null)
                    {
                        return false;
                    }

                    var pricipal = new WindowsPrincipal(identity);

                    isAdmin = pricipal.IsInRole(WindowsBuiltInRole.Administrator);
                }
                catch (UnauthorizedAccessException ex)
                {
                    isAdmin = false;
                }
                catch (Exception ex)
                {
                    isAdmin = false;
                }

                return isAdmin;
            }
        }

        public static void RunAsAdmin(string arguments, string executable = null)
        {
            if (IsRunAsAdmin)
            {
                return;
            }

            var processStartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                WorkingDirectory = Environment.CurrentDirectory,
                FileName = executable ?? Assembly.GetEntryAssembly().Location,
                Arguments = arguments,
                Verb = "runas"
            };
            try
            {
                Process.Start(processStartInfo);
            }
            catch
            {
                return;
            }

            Environment.Exit(0);
        }

        public static DateTime? GetProcessStartTime(string processName)
        {
            Process[] processes = Process.GetProcessesByName(processName);
            if (processes.Length == 0)
            {
                return null;
            }

            DateTime retVal = DateTime.Now;

            foreach (var process in processes)
            {
                if (process.StartTime < retVal)
                {
                    retVal = process.StartTime;
                }
            }

            return retVal;
        }

        public static bool IsAutoStartUp(string appName = null, string pathToExecutable = null, RegistryKey baseRegKey = null)
        {
            if (baseRegKey == null)
            {
                baseRegKey = Registry.CurrentUser;
            }

            if (string.IsNullOrWhiteSpace(pathToExecutable))
            {
                pathToExecutable = Assembly.GetExecutingAssembly().Location;
            }

            if (string.IsNullOrWhiteSpace(appName))
            {
                appName = Path.GetFileNameWithoutExtension(pathToExecutable);
            }

            using (var regKey = baseRegKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (regKey != null)
                {
                    if (regKey.GetValue(appName) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public static void SetAutoStartUp(bool isAutoStart, string appName = null, string pathToExecutable = null, RegistryKey baseRegKey = null)
        {
            if (baseRegKey == null)
            {
                baseRegKey = Registry.CurrentUser;
            }

            if (string.IsNullOrWhiteSpace(pathToExecutable))
            {
                pathToExecutable = Assembly.GetExecutingAssembly().Location;
            }

            if (string.IsNullOrWhiteSpace(appName))
            {
                appName = Path.GetFileNameWithoutExtension(pathToExecutable);
            }

            using (var regKey = baseRegKey.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (regKey != null)
                {
                    if (isAutoStart)
                    {
                        regKey.SetValue(appName, pathToExecutable);
                    }
                    else
                    {
                        regKey.DeleteValue(appName, false);
                    }
                }
            }
        }
    }
}