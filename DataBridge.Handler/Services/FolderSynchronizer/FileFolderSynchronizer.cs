namespace DataBridge.Services
{
    using System.IO;

    public class FileFolderSynchronizer : FolderSynchronizer
    {
        public FileFolderSynchronizer()
        {
            this.SrcSyncOperations = new FileSyncOperations();
            this.TgtSyncOperations = new FileSyncOperations();

            this.MoveFileToTarget = (
            (sourceFileName, targetFileName) =>
            {
                File.Copy(sourceFileName, targetFileName);
            });

            this.BackupSyncOperations = new FileSyncOperations();

            this.MoveFileToBackup = (
            (sourceFileName, targetFileName) =>
            {
                File.Copy(sourceFileName, targetFileName);
            });
        }
    }
}