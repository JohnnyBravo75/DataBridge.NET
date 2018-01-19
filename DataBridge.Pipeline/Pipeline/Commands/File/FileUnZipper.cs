using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DataBridge.Helper;
using Ionic.Zip;

namespace DataBridge.Commands
{
    public class FileUnZipper : DataCommand
    {
        public FileUnZipper()
        {
            this.Parameters.Add(new CommandParameter() { Name = "TargetDirectory", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "SourceFile", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
        }

        [XmlIgnore]
        public string TargetDirectory
        {
            get { return this.Parameters.GetValue<string>("TargetDirectory"); }
            set { this.Parameters.SetOrAddValue("TargetDirectory", value); }
        }

        [XmlIgnore]
        public string SourceFile
        {
            get { return this.Parameters.GetValue<string>("SourceFile"); }
            set { this.Parameters.SetOrAddValue("SourceFile", value); }
        }

        [XmlIgnore]
        public string Password
        {
            get { return this.Parameters.GetValue<string>("Password"); }
            set { this.Parameters.SetOrAddValue("Password", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string file = inParameters.GetValue<string>("SourceFile");
                string targetDirectory = inParameters.GetValue<string>("TargetDirectory");
                string password = inParameters.GetValue<string>("Password");

                DirectoryUtil.CreateDirectoryIfNotExists(targetDirectory);

                int numUnzipped = 0;
                using (var zipFile = ZipFile.Read(file))
                {
                    foreach (ZipEntry entry in zipFile)
                    {
                        entry.ExtractWithPassword(targetDirectory, password);
                        string unzipFile = Path.Combine(targetDirectory, entry.FileName);

                        numUnzipped++;

                        var outParameters = this.GetCurrentOutParameters();
                        outParameters.SetOrAddValue("File", unzipFile);
                        yield return outParameters;
                    }
                }

                if (numUnzipped > 0)
                {
                    yield break;
                }

                var defaultOutParameters = this.GetCurrentOutParameters();
                yield return this.TransferOutParameters(defaultOutParameters);
            }
        }
    }
}