using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Text;

namespace DataBridge.Services
{
    public class PowerShellRunner
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
    }
}