using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.Helper;

namespace DataBridge.Commands
{
    public class FileMover : DataCommand
    {
        public FileMover()
        {
            this.Parameters.Add(new CommandParameter() { Name = "TargetDirectory" });
            this.Parameters.Add(new CommandParameter() { Name = "SourceFile" });
            this.Parameters.Add(new CommandParameter() { Name = "Mode", Value = FileMoveModes.Copy, Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut });
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
        public FileMoveModes Mode
        {
            get { return this.Parameters.GetValue<FileMoveModes>("Mode"); }
            set { this.Parameters.SetOrAddValue("Mode", value); }
        }

        public enum FileMoveModes
        {
            Copy,
            Move
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                var sourceFile = inParameters.GetValue<string>("SourceFile");
                var targetDirectory = inParameters.GetValue<string>("TargetDirectory");
                var mode = inParameters.GetValue<FileMoveModes>("Mode");

                var targetFilename = Path.GetFileName(sourceFile);
                var targetFile = Path.Combine(targetDirectory, targetFilename);

                DirectoryUtil.CreateDirectoryIfNotExists(targetDirectory);

                var retryInterval = TimeSpan.FromSeconds(1);
                var maxRetries = 3;

                switch (mode)
                {
                    case FileMoveModes.Copy:
                        RetryHandler.Execute(() => File.Copy(sourceFile, targetFile, true), retryInterval, maxRetries,
                            (retry) =>
                            {
                                this.LogDebugFormat("Next try ({0}) to copy SourceFile='{0}', TargetFile='{1}'",
                                    sourceFile, targetFile);
                            });

                        this.LogDebugFormat("Copying SourceFile='{0}', TargetFile='{1}'", sourceFile, targetFile);
                        break;

                    case FileMoveModes.Move:

                        RetryHandler.Execute(() => FileUtil.DeleteFileIfExists(targetFile), retryInterval, maxRetries);

                        RetryHandler.Execute(() => File.Move(sourceFile, targetFile), retryInterval, maxRetries,
                            (retry) =>
                            {
                                this.LogDebugFormat("Next try ({0}) to move SourceFile='{0}', TargetFile='{1}'",
                                    sourceFile, targetFile);
                            });

                        this.LogDebugFormat("Moving SourceFile='{0}', TargetFile='{1}'", sourceFile, targetFile);
                        break;
                }

                var outParameters = this.GetCurrentOutParameters();
                outParameters.AddOrUpdate(new CommandParameter() { Name = "File", Value = targetFile });
                yield return outParameters;
            }
        }
    }
}