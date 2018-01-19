using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DataBridge.Helper;
using Ionic.Zip;

namespace DataBridge.Commands
{
    public class DirectoryArchiver : DataCommand
    {
        public DirectoryArchiver()
        {
            this.Parameters.Add(new CommandParameter() { Name = "TargetDirectory", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "SourceDirectory", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "ZipName", NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "FileFilter", NotNull = false, Value = "*.*" });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "RemoveSourceFiles", Value = true });
        }

        [XmlIgnore]
        public string TargetDirectory
        {
            get { return this.Parameters.GetValue<string>("TargetDirectory"); }
            set { this.Parameters.SetOrAddValue("TargetDirectory", value); }
        }

        [XmlIgnore]
        public string SourceDirectory
        {
            get { return this.Parameters.GetValue<string>("SourceDirectory"); }
            set { this.Parameters.SetOrAddValue("SourceDirectory", value); }
        }

        [XmlIgnore]
        public string ZipName
        {
            get { return this.Parameters.GetValue<string>("ZipName"); }
            set { this.Parameters.SetOrAddValue("ZipName", value); }
        }

        [XmlIgnore]
        public string FileFilter
        {
            get { return this.Parameters.GetValue<string>("FileFilter"); }
            set { this.Parameters.SetOrAddValue("FileFilter", value); }
        }

        [XmlIgnore]
        public string Password
        {
            get { return this.Parameters.GetValue<string>("Password"); }
            set { this.Parameters.SetOrAddValue("Password", value); }
        }

        [XmlIgnore]
        public bool RemoveSourceFiles
        {
            get { return this.Parameters.GetValue<bool>("RemoveSourceFiles"); }
            set { this.Parameters.SetOrAddValue("RemoveSourceFiles", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string sourceDirectory = inParameters.GetValue<string>("SourceDirectory");
                string targetDirectory = inParameters.GetValueOrDefault<string>("TargetDirectory",
                    Path.Combine(sourceDirectory, @"\{yyyy}\{MM}\"));
                string zipName = inParameters.GetValueOrDefault<string>("ZipName", "data" + ".zip");
                string password = inParameters.GetValue<string>("Password");
                bool removeSourceFiles = inParameters.GetValueOrDefault<bool>("RemoveSourceFiles", true);
                string fileFiler = inParameters.GetValueOrDefault<string>("FileFilter", "*.*");

                DirectoryUtil.CreateDirectoryIfNotExists(targetDirectory);

                var targetFile = Path.Combine(targetDirectory, zipName);

                this.ZipDirectory(sourceDirectory, password, targetFile, fileFiler);

                if (removeSourceFiles)
                {
                    this.DeleteFilesInDirectory(sourceDirectory, fileFiler);
                }

                this.LogDebug(string.Format("Zipping archive='{0}'", targetFile));

                var outParameters = this.GetCurrentOutParameters();
                outParameters.SetOrAddValue("File", targetFile);
                yield return outParameters;
            }
        }

        private void DeleteFilesInDirectory(string sourceDirectory, string fileFilter = "*.*")
        {
            var dirInfo = new DirectoryInfo(sourceDirectory);

            foreach (var file in dirInfo.GetFiles(fileFilter, SearchOption.TopDirectoryOnly))
            {
                file.Delete();
            }
        }

        private void ZipDirectory(string sourceDirectory, string password, string targetFile, string fileFilter = "*.*")
        {
            using (var zipFile = new ZipFile(targetFile))
            {
                var filenames = Directory.GetFiles(sourceDirectory, fileFilter, SearchOption.TopDirectoryOnly);
                foreach (var filename in filenames)
                {
                    zipFile.AddFile(filename);
                }
                zipFile.Password = password;
                zipFile.Save(targetFile);
            }
        }
    }
}