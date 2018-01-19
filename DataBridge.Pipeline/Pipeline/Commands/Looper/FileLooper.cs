using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace DataBridge.Commands
{
    public class FileLooper : DataCommand
    {
        public FileLooper()
        {
            this.Parameters.Add(new CommandParameter() { Name = "SourceDirectory", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "FileFilter", Direction = Directions.In, Value = "*.*" });
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut });
        }

        [XmlIgnore]
        public string SourceDirectory
        {
            get { return this.Parameters.GetValue<string>("SourceDirectory"); }
            set { this.Parameters.SetOrAddValue("SourceDirectory", value); }
        }

        [XmlIgnore]
        public string FileFilter
        {
            get { return this.Parameters.GetValue<string>("FileFilter"); }
            set { this.Parameters.SetOrAddValue("FileFilter", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string fileFilter = inParameters.GetValueOrDefault<string>("FileFilter", "*.*");
                string sourceDirectory = inParameters.GetValue<string>("SourceDirectory");

                sourceDirectory = EnvironmentHelper.ExpandEnvironmentVariables(sourceDirectory);

                string[] files = Directory.GetFiles(sourceDirectory, fileFilter, SearchOption.TopDirectoryOnly);
                this.LogDebugFormat("{0} files found in '{1}' matching '{2}'", files.Length, sourceDirectory, fileFilter);

                int idx = 0;
                foreach (string file in files)
                {
                    idx++;
                    this.LogDebugFormat("Looping ({0}/{1}) file='{2}'", idx, files.Length, file);

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.AddOrUpdate(new CommandParameter() { Name = "File", Value = file });
                    outParameters.AddOrUpdate(new CommandParameter() { Name = "FileName", Value = Path.GetFileName(file) });
                    yield return outParameters;
                }
            }
        }
    }
}