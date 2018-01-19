using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Extensions;

namespace DataBridge.Commands
{
    public class PowerShellAction : DataCommand
    {
        private DataMappings dataMappings = new DataMappings();
        private Services.PowerShellAction powerShellAction = new Services.PowerShellAction();

        public PowerShellAction()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Script" });
            this.Parameters.Add(new CommandParameter() { Name = "OutputLogFile" });
        }

        public DataMappings DataMappings
        {
            get { return this.dataMappings; }
            set { this.dataMappings = value; }
        }

        [XmlIgnore]
        public string Script
        {
            get { return this.Parameters.GetValue<string>("Script"); }
            set { this.Parameters.SetOrAddValue("Script", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string script = inParameters.GetValue<string>("Script");
                string logFile = inParameters.GetValue<string>("OutputLogFile");

                var parameters = new Dictionary<string, object>();

                if (this.DataMappings.Any())
                {
                    // Take the values, which are defined in the mapping
                    foreach (var dataMapping in this.DataMappings)
                    {
                        string parameterValue = "";

                        if (dataMapping.Value != null)
                        {
                            parameterValue = TokenProcessor.ReplaceTokens(dataMapping.Value.ToStringOrEmpty());
                        }
                        else
                        {
                            parameterValue = inParameters.GetValue<string>(dataMapping.Name);
                        }

                        parameters.Add(dataMapping.Name, parameterValue);
                    }
                }
                else
                {
                    parameters = inParameters.ToDictionary();
                }

                var shellOutput = this.powerShellAction.Run(script, parameters);

                // log output to logfile
                if (!string.IsNullOrEmpty(logFile) &&
                    !string.IsNullOrWhiteSpace(shellOutput))
                {
                    System.IO.File.AppendAllText(logFile, shellOutput + Environment.NewLine);
                }

                var outParameters = this.GetCurrentOutParameters();
                outParameters.AddOrUpdate(new CommandParameter() { Name = "Output", Value = shellOutput });
                yield return outParameters;
            }
        }
    }
}