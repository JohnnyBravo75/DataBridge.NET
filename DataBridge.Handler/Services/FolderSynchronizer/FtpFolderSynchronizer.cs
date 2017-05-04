namespace DataBridge.Services
{
    using System.IO;

    public class FtpFolderSynchronizer : FolderSynchronizer
    {
        private FtpSyncOperations ftpSyncOperations = new FtpSyncOperations();
        private FileSyncOperations fileSyncOperations = new FileSyncOperations();

        public FtpFolderSynchronizer()
        {
            this.SrcSyncOperations = this.ftpSyncOperations;
            this.TgtSyncOperations = this.fileSyncOperations;

            this.MoveFileToTarget = (
            (sourceFileName, targetFileName) =>
            {
                this.ftpSyncOperations.Ftp.DownloadFile(sourceFileName, targetFileName);
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