using System.Collections.Generic;

namespace DataBridge.Services
{
    public interface IFtp
    {
        void SetConnectionInfos(string host, string userName, string password);

        void DownloadFile(string remoteFile, string localFile, bool deleteRemoteFile = false);

        IEnumerable<string> DownloadFiles(string remoteFiles, string localDirectory, bool deleteRemoteFile = false);

        void UploadFile(string localFile, string remoteFile, bool deletelocalFile = false);

        IEnumerable<string> UploadFiles(string localFiles, string remoteDirectory, bool deleteLocalFile = false);

        IEnumerable<FtpFileInfo> GetDirectoryList(string remoteDirectory);

        void CreateDirectory(string newDirectory);
    }
}