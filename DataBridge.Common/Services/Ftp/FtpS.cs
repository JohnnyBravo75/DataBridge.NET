using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using AlexPilotti.FTPS.Client;
using AlexPilotti.FTPS.Common;

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

        public FtpS()
        {
        }

        public void SetConnectionInfos(string host, string userName = "", string password = "")
        {
            this.host = host;
            this.user = userName;
            this.password = password;
        }

        public void DownloadFile(string remoteFile, string localFile, bool deleteRemoteFile = false)
        {
            FTPSClient ftp = new FTPSClient();
            string localDownloadFile = localFile;
            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(ETransferMode.Binary);
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

            FTPSClient ftp = new FTPSClient();

            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(ETransferMode.Binary);
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
            FTPSClient ftp = new FTPSClient();

            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(ETransferMode.Binary);
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

            FTPSClient ftp = new FTPSClient();

            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(ETransferMode.Binary);

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
            FTPSClient ftp = new FTPSClient();

            try
            {
                ftp.Connect(this.host, new NetworkCredential(this.user, this.password), ESSLSupportMode.ClearText);
                ftp.SetTransferMode(ETransferMode.Binary);

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

        ///// <summary>
        ///// Executes the in transaction context.
        /////  return ExecuteInTransactionContext<IList<IAbteilung>>(cmd =>
        /////                {
        /////                    IList<IAbteilung> result = new List<IAbteilung>();
        /////                    cmd.CommandText = " SELECT obje_name_1,....";
        /////                    using (OracleDataReader reader = cmd.ExecuteReader())
        /////                    {
        /////                        while (reader.Read())
        /////                        {
        /////                            ...
        /////                        }
        /////                    }
        /////
        /////                     return result;
        /////                  });
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="body">The body.</param>
        ///// <returns></returns>
        //public static T ExecuteInTransactionContext<T>(Func<OracleCommand, T> body)
        //{
        //    using (OracleConnection conn = BdlDriverConnectionProvider.CreateConnection())
        //    {
        //        conn.Open();
        //        var trans =  conn.BeginTransaction();

        //        try
        //        {
        //            T result = default(T);
        //            using (OracleCommand cmd = conn.CreateCommand())
        //            {
        //                cmd.Transaction = trans;
        //                result = body(cmd);
        //            }

        //            trans.Commit();

        //            return result;
        //        }
        //        catch (Exception e)
        //        {
        //            log.Fatal(e.ToString());
        //            trans.Rollback();
        //            throw;
        //        }
        //        finally
        //        {
        //            trans.Dispose();

        //            if (conn.State != System.Data.ConnectionState.Closed)
        //            {
        //                conn.Close();
        //            }
        //        }
        //    }
        //}
    }
}