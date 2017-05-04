using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;
using DataBridge.Common.Helper;

namespace DataBridge.Services
{
    /// <summary>
    /// http://ftps.codeplex.com/
    /// uses Alex FTPS Client (LGPL)
    /// </summary>
    public class FtpS : FtpBase, IFtp
    {
        private string host = "";
        private string user = "";
        private string password = "";
        private bool useBinary = true;
        //private int port = 21;

        public bool UseBinary
        {
            get { return this.useBinary; }
            set { this.useBinary = value; }
        }

        //public int Port
        //{
        //    get { return this.port; }
        //    set { this.port = value; }
        //}

        public void SetConnectionInfos(string host, string userName = "", string password = "")
        {
            this.host = host;
            this.user = userName;
            this.password = password;
        }

        public void DownloadFile(string remoteFile, string localFile, bool deleteRemoteFile = false)
        {
            var ftp = new FTPSClient();
            var localDownloadFile = localFile;
            try
            {
                //ftp.Connect(hostname: host, 
                //            port: this.port, 
                //            credential: new NetworkCredential(this.user, this.password), 
                //            sslSupportMode: ESSLSupportMode.All, 
                //            userValidateServerCertificate: null, 
                //            x509ClientCert: null, 
                //            dataConnectionMode: EDataConnectionMode.Passive);
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(this.UseBinary ? ETransferMode.Binary : ETransferMode.ASCII);

                //ftp.SetCurrentDirectory(remoteFolder);

                if (Directory.Exists(localFile) && !Path.HasExtension(localFile))
                {
                    localDownloadFile = Path.Combine(localFile, Path.GetFileName(remoteFile));
                }

                ftp.GetFile(remoteFile, localDownloadFile);

                if (deleteRemoteFile == true)
                {
                    ftp.DeleteFile(remoteFile);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (ftp != null)
                {
                    ftp.Close();
                    ftp.Dispose();
                }
            }
        }

        public IEnumerable<string> DownloadFiles(string remoteFiles, string localDirectory, bool deleteRemoteFile = false)
        {
            var downloadedFiles = new List<string>();

            var ftp = new FTPSClient();

            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(this.UseBinary ? ETransferMode.Binary : ETransferMode.ASCII);
                //ftp.SetCurrentDirectory(remoteFolder);

                var directoryName = this.GetDirectoryName(remoteFiles);
                var remoteFileInfos = this.GetDirectoryList(directoryName);
                var remoteFilePattern = Path.GetFileName(remoteFiles);

                foreach (var remoteFileInfo in remoteFileInfos)
                {
                    if (string.IsNullOrEmpty(remoteFilePattern) || FileUtil.MatchesWildcard(remoteFileInfo.Name, remoteFilePattern))
                    {
                        var localFileName = Path.Combine(localDirectory, Path.GetFileName(remoteFileInfo.Name));

                        ftp.GetFile(remoteFileInfo.FullName, localFileName);
                        downloadedFiles.Add(remoteFileInfo.FullName);

                        if (deleteRemoteFile == true)
                        {
                            ftp.DeleteFile(remoteFileInfo.FullName);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (ftp != null)
                {
                    ftp.Close();
                    ftp.Dispose();
                }
            }

            return downloadedFiles;
        }

        //public void DownloadFiles(string remoteFiles, string localDirectory, bool deleteRemoteFile = false)
        //{
        //    var directoryName = Path.GetDirectoryName(remoteFiles);
        //    var remoteFileNames = GetDirectoryList(directoryName);
        //    foreach (var remoteFileName in remoteFileNames)
        //    {
        //        var localFileName = Path.Combine(localDirectory, Path.GetFileName(remoteFileName));
        //        DownloadFile(remoteFileName, localFileName, deleteRemoteFile);
        //    }
        //}

        public void UploadFile(string localFile, string remoteFile, bool deleteLocalFile = false)
        {
            var ftp = new FTPSClient();

            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(this.UseBinary ? ETransferMode.Binary : ETransferMode.ASCII);
                //ftp.SetCurrentDirectory(remoteFolder);

                ftp.PutFile(localFile, remoteFile);

                if (deleteLocalFile == true)
                {
                    ftp.DeleteFile(localFile);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (ftp != null)
                {
                    ftp.Close();
                    ftp.Dispose();
                }
            }
        }

        public IEnumerable<string> UploadFiles(string localFiles, string remoteDirectory, bool deleteLocalFile = false)
        {
            var uploadedFiles = new List<string>();

            var localDirectoryName = Path.GetDirectoryName(localFiles);
            var localFilePattern = Path.GetFileName(localFiles);

            var localFilePaths = Directory.GetFiles(localDirectoryName, localFilePattern, SearchOption.AllDirectories);
            foreach (var localFile in localFilePaths)
            {
                var remoteFileName = Path.Combine(remoteDirectory, Path.GetFileName(localFile));
                this.UploadFile(localFile, remoteFileName, deleteLocalFile);
                uploadedFiles.Add(localFile);
            }

            return uploadedFiles;
        }

        public IEnumerable<FtpFileInfo> GetDirectoryList(string remoteDirectory)
        {
            var remoteDirectoryName = remoteDirectory;
            if (!this.IsDirectory(remoteDirectory))
            {
                remoteDirectoryName = this.GetDirectoryName(remoteDirectory);
            }

            if (string.IsNullOrEmpty(remoteDirectory))
            {
                remoteDirectory = ".";
            }

            var ftp = new FTPSClient();

            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(this.UseBinary ? ETransferMode.Binary : ETransferMode.ASCII);

                var ftpFiles = ftp.GetDirectoryList(remoteDirectory);
                var files = new List<FtpFileInfo>();
                foreach (var ftpFile in ftpFiles)
                {
                    var ftpFileInfo = new FtpFileInfo();
                    ftpFileInfo.Name = ftpFile.Name;
                    ftpFileInfo.CreationTime = ftpFile.CreationTime;
                    ftpFileInfo.Flags = ftpFile.Flags;
                    ftpFileInfo.IsDirectory = ftpFile.IsDirectory;
                    ftpFileInfo.FullName = Path.Combine(remoteDirectoryName, ftpFileInfo.Name);
                    files.Add(ftpFileInfo);
                }

                return files;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (ftp != null)
                {
                    ftp.Close();
                    ftp.Dispose();
                }
            }
        }

        public void CreateDirectory(string newDirectory)
        {
            var ftp = new FTPSClient();

            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(this.UseBinary ? ETransferMode.Binary : ETransferMode.ASCII);

                ftp.MakeDir(newDirectory);
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (ftp != null)
                {
                    ftp.Close();
                    ftp.Dispose();
                }
            }
        }
    }
}