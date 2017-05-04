using System;
using System.Collections.Generic;
using System.IO;
using Renci.SshNet;

namespace DataBridge.Services
{
    /// <summary>
    /// https://sshnet.codeplex.com/
    /// Renci.SshNet, Copyright (c) 2010, RENCI
    /// </summary>
    public class SFtp : FtpBase, IFtp
    {
        private string host = "";
        private string user = "";
        private string password = "";

        public SFtp()
        {
        }

        public void SetConnectionInfos(string host, string userName = "", string password = "")
        {
            this.host = host;
            this.user = userName;
            this.password = password;
        }

        public bool CanConnect()
        {
            bool canConnect = false;
            SftpClient ftp = null;

            try
            {
                ftp = new SftpClient(this.host, this.user, this.password);
                ftp.Connect();
                if (ftp.IsConnected)
                {
                    canConnect = true;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (ftp != null)
                {
                    if (ftp.IsConnected)
                    {
                        ftp.Disconnect();
                    }

                    ftp.Dispose();
                }
            }

            return canConnect;
        }

        public void DownloadFile(string remoteFile, string localFile, bool deleteRemoteFile = false)
        {
            SftpClient ftp = null;
            string localDownloadFile = localFile;
            try
            {
                ftp = new SftpClient(this.host, this.user, this.password);
                ftp.Connect();
                ftp.ChangeDirectory("\\");

                if (Directory.Exists(localFile) && !Path.HasExtension(localFile))
                {
                    localDownloadFile = Path.Combine(localFile, Path.GetFileName(remoteFile));
                }

                using (var file = File.OpenWrite(localDownloadFile))
                {
                    ftp.DownloadFile(remoteFile, file);
                    file.Close();
                }

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
                    if (ftp.IsConnected)
                    {
                        ftp.Disconnect();
                    }

                    ftp.Dispose();
                }
            }
        }

        public IEnumerable<string> DownloadFiles(string remoteFiles, string localDirectory, bool deleteRemoteFile = false)
        {
            var downloadedFiles = new List<string>();

            SftpClient ftp = null;

            try
            {
                ftp = new SftpClient(this.host, this.user, this.password);
                ftp.Connect();
                ftp.ChangeDirectory("\\");

                var directoryName = Path.GetDirectoryName(remoteFiles);
                var remoteFileInfos = this.GetDirectoryList(directoryName);
                var remoteFilePattern = Path.GetFileName(remoteFiles);

                foreach (var remoteFileInfo in remoteFileInfos)
                {
                    if (string.IsNullOrEmpty(remoteFilePattern) || FileUtil.MatchesWildcard(remoteFileInfo.Name, remoteFilePattern))
                    {
                        var localFileName = Path.Combine(localDirectory, Path.GetFileName(remoteFileInfo.Name));

                        using (var fileStream = File.OpenWrite(localFileName))
                        {
                            ftp.DownloadFile(remoteFileInfo.FullName, fileStream);
                            fileStream.Close();
                        }

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
                    if (ftp.IsConnected)
                    {
                        ftp.Disconnect();
                    }

                    ftp.Dispose();
                }
            }

            return downloadedFiles;
        }

        public void UploadFile(string localFile, string remoteFile, bool deleteLocalFile = false)
        {
            SftpClient ftp = null;

            try
            {
                ftp = new SftpClient(this.host, this.user, this.password);
                ftp.Connect();

                ftp.ChangeDirectory("\\");

                using (var file = File.OpenRead(localFile))
                {
                    ftp.UploadFile(file, remoteFile, canOverride: true);
                    file.Close();
                }

                if (deleteLocalFile == true)
                {
                    File.Delete(localFile);
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
                    if (ftp.IsConnected)
                    {
                        ftp.Disconnect();
                    }

                    ftp.Dispose();
                }
            }
        }

        public IEnumerable<string> UploadFiles(string localFiles, string remoteDirectory, bool deleteLocalFile = false)
        {
            var uploadedFiles = new List<string>();

            var localDirectoryName = Path.GetDirectoryName(localFiles);
            var localFilePattern = Path.GetFileName(localFiles);

            string[] localFilePaths = Directory.GetFiles(localDirectoryName, localFilePattern, SearchOption.AllDirectories);
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
            string remoteDirectoryName = remoteDirectory;
            if (!this.IsDirectory(remoteDirectory))
            {
                remoteDirectoryName = this.GetDirectoryName(remoteDirectory);
            }

            if (string.IsNullOrEmpty(remoteDirectory))
            {
                remoteDirectory = ".";
            }

            SftpClient ftp = null;

            try
            {
                ftp = new SftpClient(this.host, this.user, this.password);
                ftp.Connect();

                var ftpFiles = ftp.ListDirectory(remoteDirectory);
                var files = new List<FtpFileInfo>();
                foreach (var ftpFile in ftpFiles)
                {
                    var ftpFileInfo = new FtpFileInfo();
                    ftpFileInfo.Name = ftpFile.Name;
                    ftpFileInfo.LastWriteTime = ftpFile.LastWriteTime;
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
                    if (ftp.IsConnected)
                    {
                        ftp.Disconnect();
                    }

                    ftp.Dispose();
                }
            }
        }

        public void CreateDirectory(string newDirectory)
        {
            SftpClient ftp = null;
            try
            {
                ftp = new SftpClient(this.host, this.user, this.password);
                ftp.Connect();
                ftp.CreateDirectory(newDirectory);
            }
            catch (Exception ex)
            {
            }
            finally
            {
                if (ftp != null)
                {
                    if (ftp.IsConnected)
                    {
                        ftp.Disconnect();
                    }

                    ftp.Dispose();
                }
            }
        }
    }
}