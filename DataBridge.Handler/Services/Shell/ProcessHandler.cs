using System.Security;

namespace DataBridge.Services
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class ProcessHandler
    {
        private readonly string processName = string.Empty;
        private readonly string processParam = string.Empty;
        private string userName = string.Empty;
        private string password = string.Empty;

        /// <summary>
        /// Sets the exec user.
        /// </summary>
        /// <param name="username">The username.</param>
        /// <param name="password">The password.</param>
        public void SetExecUser(string username, string password)
        {
            this.userName = username;
            this.password = password;
        }

        private readonly string workingDir = string.Empty;

        private readonly List<string> statementList;

        /// <summary>
        /// Gets the statement list.
        /// </summary>
        public List<string> StatementList
        {
            get
            {
                return this.statementList;
            }
        }

        private readonly List<string> outputList;

        /// <summary>
        /// Gets the output list.
        /// </summary>
        public string[] OutputList
        {
            get
            {
                string[] tmp = new string[this.outputList.Count];
                this.outputList.CopyTo(tmp);
                //this.outputList.Clear();
                return tmp;
            }
        }

        public string OutputString
        {
            get
            {
                return string.Join(Environment.NewLine, this.OutputList);
            }
        }

        private readonly List<string> errorList;

        /// <summary>
        /// Gets the error list.
        /// </summary>
        public string[] ErrorList
        {
            get
            {
                string[] tmp = new string[this.errorList.Count];
                this.errorList.CopyTo(tmp);
                //this.errorList.Clear();
                return tmp;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessHandler"/> class.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        public ProcessHandler(string processName)
        {
            this.processName = processName;

            this.statementList = new List<string>();
            this.errorList = new List<string>();
            this.outputList = new List<string>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessHandler"/> class.
        /// </summary>
        /// <param name="processName">Name of the process.</param>
        /// <param name="processParam">The process param.</param>
        public ProcessHandler(string processName, string processParam)
        {
            this.processName = processName;
            this.processParam = processParam;

            this.statementList = new List<string>();
            this.errorList = new List<string>();
            this.outputList = new List<string>();
        }

        /// <summary>
        ///     Fuehrt das Statement auf dem Process aus und schließt diesen am Ende
        /// </summary>
        public bool Execute(string statement)
        {
            this.outputList.Clear();
            this.errorList.Clear();

            bool result = false;

            // Prozess initalisieren
            using (var cmd = new Process())
            {
                try
                {
                    cmd.StartInfo.FileName = this.processName;

                    if (!String.IsNullOrEmpty(this.userName) && !String.IsNullOrEmpty(this.password))
                    {
                        cmd.StartInfo.UserName = this.userName;
                        var pw = new SecureString();

                        foreach (char c in this.password)
                        {
                            pw.AppendChar(c);
                        }

                        pw.MakeReadOnly();
                        cmd.StartInfo.Password = pw;
                        cmd.StartInfo.Verb = "runas";
                    }

                    // eventuelle Startparameter uebergeben
                    if (!String.IsNullOrEmpty(this.processName))
                    {
                        cmd.StartInfo.Arguments = this.processParam;
                    }

                    if (!string.IsNullOrEmpty(this.workingDir))
                    {
                        cmd.StartInfo.WorkingDirectory = this.workingDir;
                    }

                    // Input- / Output- und Errorstream initalisieren
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.RedirectStandardError = true;

                    // Einstellungen, damit Stream umgeleitet wird
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;

                    // asynchrone Eventhandler initalisieren
                    cmd.OutputDataReceived += this.OutputDataHandler;
                    cmd.ErrorDataReceived += this.ErrorDataHandler;

                    // Process starten
                    result = cmd.Start();

                    // Statement hinzufuegen
                    cmd.StandardInput.WriteLine(statement);

                    cmd.StandardInput.Close();

                    // Process beenden
                    //this.cmd.StandardInput.WriteLine("exit");

                    // asynchrones Aulesen der Output- und Errorstreams starten
                    cmd.BeginOutputReadLine();
                    cmd.BeginErrorReadLine();

                    // warten bis der Process ausgefuehrt wurde
                    cmd.WaitForExit();

                    if (cmd.ExitCode != 0)
                        result = false;
                    //result = true;

                    cmd.OutputDataReceived -= this.OutputDataHandler;
                    cmd.ErrorDataReceived -= this.ErrorDataHandler;
                }
                catch
                {
                    throw new InvalidOperationException(string.Format("An error occures when executing \"{0}\"", this.processName));
                }
            }

            return result;
        }

        /// <summary>
        ///     Fuehrt die Liste an Statements auf dem Process aus udn schließt duesen am Ende
        /// </summary>
        public bool Execute(List<string> statementList)
        {
            this.outputList.Clear();
            this.errorList.Clear();

            bool result = false;

            // Prozess initalisieren
            using (var cmd = new Process())
            {
                try
                {
                    cmd.StartInfo.FileName = this.processName;

                    // eventuelle Startparameter uebergeben
                    if (!String.IsNullOrEmpty(this.processName))
                    {
                        cmd.StartInfo.Arguments = this.processParam;
                    }

                    // Input- / Output- und Errorstream initalisieren
                    cmd.StartInfo.RedirectStandardInput = true;
                    cmd.StartInfo.RedirectStandardOutput = true;
                    cmd.StartInfo.RedirectStandardError = true;

                    // Einstellungen, damit Stream umgeleitet wird
                    cmd.StartInfo.CreateNoWindow = true;
                    cmd.StartInfo.UseShellExecute = false;

                    // asynchrone Eventhandler initalisieren
                    cmd.OutputDataReceived += this.OutputDataHandler;
                    cmd.ErrorDataReceived += this.ErrorDataHandler;

                    // Process starten
                    cmd.Start();

                    // Statement hinzufuegen
                    foreach (string stmt in statementList)
                    {
                        if (!String.IsNullOrEmpty(stmt))
                        {
                            cmd.StandardInput.WriteLine(stmt);
                            cmd.StandardInput.Flush();
                        }
                    }

                    // Process beenden
                    cmd.StandardInput.WriteLine("exit");

                    // asynchrones Aulesen der Output- und Errorstreams starten
                    cmd.BeginOutputReadLine();
                    cmd.BeginErrorReadLine();

                    // warten bis der Process ausgefuehrt wurde
                    cmd.WaitForExit();

                    result = true;

                    cmd.OutputDataReceived -= this.OutputDataHandler;
                    cmd.ErrorDataReceived -= this.ErrorDataHandler;
                }
                catch
                {
                    throw new InvalidOperationException(string.Format("An error occures when executing \"{0}\"", this.processName));
                }
            }

            return result;
        }

        private void OutputDataHandler(object sendingProcess, DataReceivedEventArgs outLine)
        {
            if (!String.IsNullOrEmpty(outLine.Data))
            {
                this.outputList.Add(outLine.Data);
            }
        }

        private void ErrorDataHandler(object sendingProcess, DataReceivedEventArgs errorLine)
        {
            if (!String.IsNullOrEmpty(errorLine.Data))
            {
                this.errorList.Add(errorLine.Data);
            }
        }
    }
}