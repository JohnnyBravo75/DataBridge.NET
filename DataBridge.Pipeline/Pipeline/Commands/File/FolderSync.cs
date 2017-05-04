using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace DataBridge.Commands
{
    public class FolderSync : DataCommand
    {
        public FolderSync()
        {
            this.Parameters.Add(new CommandParameter() { Name = "SourceDirectory", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "TargetDirectory", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "FileFilter", Direction = Directions.In, Value = "*.*" });
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
        public string FileFilter
        {
            get { return this.Parameters.GetValue<string>("FileFilter"); }
            set { this.Parameters.SetOrAddValue("FileFilter", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string sourceDirectory = inParameters.GetValue<string>("SourceDirectory");
            string targetDirectory = inParameters.GetValue<string>("TargetDirectory");
            string fileFilter = inParameters.GetValue<string>("FileFilter");

            var folderSyncer = new Services.FileFolderSynchronizer();
            if (Directory.Exists(sourceDirectory))
            {
                this.LogDebugFormat("Start synchronizing from '{0}' to '{1}'...", sourceDirectory, targetDirectory);

                folderSyncer.SourceDirectory = sourceDirectory;
                folderSyncer.TargetDirectory = targetDirectory;
                folderSyncer.FileFilter = fileFilter;
                folderSyncer.Sync();

                this.LogDebugFormat("Ended synchronizing. SyncNew={0}, SyncChanged={1}, SyncDeleted={2}, SyncChecked={3}", folderSyncer.Added.Count(), folderSyncer.Changed.Count(), folderSyncer.Deleted.Count(), folderSyncer.Checked.Count());

                this.SetToken("SyncNew", folderSyncer.Added.Count());
                this.SetToken("SyncChanged", folderSyncer.Changed.Count());
                this.SetToken("SyncDeleted", folderSyncer.Deleted.Count());
                this.SetToken("SyncChecked", folderSyncer.Checked.Count());
            }
            else
            {
                var errorMsg = string.Format("Source directory '{0}' does not exist", sourceDirectory);
                this.LogErrorFormat(errorMsg);
                throw new Exception(errorMsg);
            }

            var outParameters = this.GetCurrentOutParameters();

            yield return outParameters;
        }
    }
}