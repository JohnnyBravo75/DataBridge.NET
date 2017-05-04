using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace DataBridge.Services
{
    public class PowerShellAction
    {
        public bool IsPowerShellInstalled()
        {
            string regval = Microsoft.Win32.Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\1", "Install", null).ToString();
            if (regval.Equals("1"))
            {
                return true;
            }

            return false;
        }

        public string Run(string script, IDictionary<string, object> parameters = null)
        {
            var outputStr = new StringBuilder();

            using (Runspace runSpace = RunspaceFactory.CreateOutOfProcessRunspace(new TypeTable(new string[0])))
            {
                runSpace.Open();

                using (var pipeline = runSpace.CreatePipeline())
                {
                    // In-vars
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            runSpace.SessionStateProxy.SetVariable(param.Key, param.Value);
                        }
                    }

                    // execute
                    //pipeline.Commands.AddScript("$verbosepreference='continue'");
                    pipeline.Commands.AddScript(script);
                    pipeline.Commands.Add("Out-String");
                    Collection<PSObject> results = pipeline.Invoke();

                    foreach (PSObject outputItem in results)
                    {
                        if (outputItem != null)
                        {
                            outputStr.AppendLine(outputItem.ToString());
                        }
                    }

                    // Out-vars
                    var outparameters = new Dictionary<string, object>();
                    if (parameters != null)
                    {
                        foreach (var param in parameters)
                        {
                            var value = runSpace.SessionStateProxy.GetVariable(param.Key);
                            outparameters.Add(param.Key, value);
                        }
                    }

                    parameters = outparameters;
                }

                runSpace.Close();
            }

            return outputStr.ToString();
        }

        public string Run2(string script, IDictionary<string, object> parameters = null)
        {
            var outputStr = new StringBuilder();

            var initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.Variables.Add(new SessionStateVariableEntry("context", parameters, ""));

            using (Runspace runspace = RunspaceFactory.CreateRunspace(initialSessionState))
            {
                using (PowerShell powerShell = PowerShell.Create())
                {
                    // execute
                    runspace.Open();

                    powerShell.Runspace = runspace;
                    powerShell.AddScript(script);

                    var results = powerShell.Invoke<string>();

                    // output
                    foreach (var outputItem in results)
                    {
                        if (outputItem != null)
                        {
                            outputStr.AppendLine(outputItem.ToString());
                        }
                    }

                    // errors
                    foreach (ErrorRecord error in powerShell.Streams.Error.ReadAll())
                    {
                        outputStr.AppendLine(string.Format("{0} in line {1}, column {2}", error, error.InvocationInfo.ScriptLineNumber, error.InvocationInfo.OffsetInLine));
                    }

                    runspace.Close();
                }
            }

            return outputStr.ToString();
        }
    }
}