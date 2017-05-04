using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace DataBridge.Commands
{
    public class ProgramStarter : DataCommand
    {
        private Services.CommandShellAction commandShellAction = new Services.CommandShellAction();

        public ProgramStarter()
        {
            this.Parameters.Add(new CommandParameter() { Name = "File" });
            this.Parameters.Add(new CommandParameter() { Name = "StartParameter" });
            this.Parameters.Add(new CommandParameter() { Name = "OutputLogFile" });
        }

        [XmlIgnore]
        public string StartParameter
        {
            get { return this.Parameters.GetValue<string>("StartParameter"); }
            set { this.Parameters.SetOrAddValue("StartParameter", value); }
        }

        [XmlIgnore]
        public string File
        {
            get { return this.Parameters.GetValue<string>("File"); }
            set { this.Parameters.SetOrAddValue("File", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string file = inParameters.GetValue<string>("File");
            string startParameter = inParameters.GetValue<string>("StartParameter");
            string logFile = inParameters.GetValue<string>("OutputLogFile");

            startParameter = TokenProcessor.ReplaceTokens(startParameter, inParameters.ToDictionary());
            var shellOutput = this.commandShellAction.RunExternalExe(file, startParameter);

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