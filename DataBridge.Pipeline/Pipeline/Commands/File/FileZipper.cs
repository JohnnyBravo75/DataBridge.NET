using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Helper;
using Ionic.Zip;

namespace DataBridge.Commands
{
    public class FileZipper : DataCommand
    {
        public FileZipper()
        {
            this.Parameters.Add(new CommandParameter() { Name = "TargetDirectory", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "SourceFile", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "ZipName", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "RemoveSourceFile", Value = true });
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
        public string ZipName
        {
            get { return this.Parameters.GetValue<string>("ZipName"); }
            set { this.Parameters.SetOrAddValue("ZipName", value); }
        }

        [XmlIgnore]
        public string Password
        {
            get { return this.Parameters.GetValue<string>("Password"); }
            set { this.Parameters.SetOrAddValue("Password", value); }
        }

        [XmlIgnore]
        public bool RemoveSourceFile
        {
            get { return this.Parameters.GetValue<bool>("RemoveSourceFile"); }
            set { this.Parameters.SetOrAddValue("RemoveSourceFile", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string sourceFile = inParameters.GetValue<string>("SourceFile");
            string targetDirectory = inParameters.GetValueOrDefault<string>("TargetDirectory", Path.Combine(Path.GetDirectoryName(sourceFile), @"\{yyyy}\{MM}\"));
            string zipName = inParameters.GetValueOrDefault<string>("ZipName", Path.GetFileNameWithoutExtension(sourceFile) + ".zip");
            string password = inParameters.GetValue<string>("Password");
            bool removeSourceFile = inParameters.GetValueOrDefault<bool>("RemoveSourceFile", true);

            zipName = TokenProcessor.ReplaceToken(zipName, "SourceFileName", Path.GetFileName(sourceFile));

            DirectoryUtil.CreateDirectoryIfNotExists(targetDirectory);

            var targetFile = Path.Combine(targetDirectory, zipName);
            this.ZipFile(sourceFile, targetFile, password);

            if (removeSourceFile)
            {
                File.Delete(sourceFile);
            }

            this.LogDebug(string.Format("Zipping archive='{0}'", targetFile));

            var outParameters = this.GetCurrentOutParameters();
            outParameters.SetOrAddValue("File", targetFile);
            yield return outParameters;
        }

        private void ZipFile(string sourceFile, string targetFile, string password)
        {
            using (var zipFile = new ZipFile(targetFile))
            {
                var existingZipEntry = zipFile.FirstOrDefault(x => x.FileName == Path.GetFileName(sourceFile));
                if (existingZipEntry != null)
                {
                    zipFile.RemoveEntry(existingZipEntry);
                }

                zipFile.AddItem(sourceFile, "");
                zipFile.Password = password;
                zipFile.Save(targetFile);
            }
        }
    }
}