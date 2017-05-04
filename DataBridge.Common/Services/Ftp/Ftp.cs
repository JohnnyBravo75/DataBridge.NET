using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace DataBridge.Services
{
    public class Ftp : FtpBase, IFtp
    {
        private string host = "";
        private string user = "";
        private string password = "";

        private int bufferSize = 2048;

        public Ftp()
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
            try
            {
                var ftpRequest = this.CreateFtpRequest("");
                if (ftpRequest == null)
                {
                    return false;
                }

                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                {
                }

                this.CloseFtpRequest(ftpRequest);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public void DownloadFile(string remoteFile, string localFile, bool deleteRemoteFile = false)
        {
            var ftpRequest = this.CreateFtpRequest(remoteFile);

            ftpRequest.Method = WebRequestMethods.Ftp.DownloadFile;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                using (Stream ftpStream = ftpResponse.GetResponseStream())
                {
                    if (ftpStream != null)
                    {
                        using (FileStream localFileStream = new FileStream(localFile, FileMode.Create))
                        {
                            byte[] byteBuffer = new byte[this.bufferSize];
                            int bytesRead = ftpStream.Read(byteBuffer, 0, this.bufferSize);

                            while (bytesRead > 0)
                            {
                                localFileStream.Write(byteBuffer, 0, bytesRead);
                                bytesRead = ftpStream.Read(byteBuffer, 0, this.bufferSize);
                            }

                            localFileStream.Close();
                        }

                        ftpStream.Close();
                    }
                }

                ftpResponse.Close();
            }

            this.CloseFtpRequest(ftpRequest);

            if (deleteRemoteFile)
            {
                this.DeleteFile(remoteFile);
            }
        }

        public IEnumerable<string> DownloadFiles(string remoteFiles, string localDirectory, bool deleteRemoteFile = false)
        {
            var downloadedFiles = new List<string>();

            var remoteDirectoryName = this.GetDirectoryName(remoteFiles);
            var remoteFileInfos = this.GetDirectoryList(remoteDirectoryName);
            var remoteFilePattern = Path.GetFileName(remoteFiles);

            foreach (var remoteFileInfo in remoteFileInfos)
            {
                if (string.IsNullOrEmpty(remoteFilePattern) || FileUtil.MatchesWildcard(remoteFileInfo.Name, remoteFilePattern))
                {
                    var localFileName = Path.Combine(localDirectory, Path.GetFileName(remoteFileInfo.Name));

                    this.DownloadFile(remoteFileInfo.FullName, localFileName, deleteRemoteFile);
                    downloadedFiles.Add(remoteFileInfo.FullName);
                }
            }

            return downloadedFiles;
        }

        public void UploadFile(string localFile, string remoteFile, bool deleteLocalFile = false)
        {
            var ftpRequest = this.CreateFtpRequest(remoteFile);

            // Notify the server about the size of the uploaded file
            var fileInf = new FileInfo(localFile);
            ftpRequest.ContentLength = fileInf.Length;

            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            using (Stream ftpStream = ftpRequest.GetRequestStream())
            {
                using (FileStream fileStream = new FileStream(localFile, FileMode.Open))
                {
                    byte[] buffer = new byte[this.bufferSize];
                    int bytesRead = 0;

                    do
                    {
                        bytesRead = fileStream.Read(buffer, 0, this.bufferSize);
                        ftpStream.Write(buffer, 0, bytesRead);
                    }
                    while (bytesRead != 0);

                    fileStream.Close();
                }

                ftpStream.Close();

                if (deleteLocalFile == true)
                {
                    File.Delete(localFile);
                }
            }

            this.CloseFtpRequest(ftpRequest);
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

        public void CreateDirectory(string newDirectory)
        {
            var ftpRequest = this.CreateFtpRequest(newDirectory);

            ftpRequest.Method = WebRequestMethods.Ftp.MakeDirectory;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
            }

            this.CloseFtpRequest(ftpRequest);
        }

        //public IEnumerable<string> GetDirectoryList(string remoteDirectory)
        //{
        //    var ftpRequest = CreateFtpRequest(remoteDirectory);

        //    ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;

        //    using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
        //    {
        //        using (Stream dataStream = ftpResponse.GetResponseStream())
        //        {
        //            using (StreamReader dataReader = new StreamReader(dataStream, this.Encoding))
        //            {
        //                var files = new List<string>();
        //                while (!dataReader.EndOfStream)
        //                {
        //                    files.Add(dataReader.ReadLine());
        //                }
        //                return files;
        //            }
        //        }
        //    }

        //    CloseFtpRequest(ftpRequest);
        //}

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

            var ftpRequest = this.CreateFtpRequest(remoteDirectory);

            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                using (Stream dataStream = ftpResponse.GetResponseStream())
                {
                    using (StreamReader dataReader = new StreamReader(dataStream, this.Encoding))
                    {
                        var files = new List<FtpFileInfo>();
                        while (!dataReader.EndOfStream)
                        {
                            var ftpFileInfo = this.ParseListDirectoryEntry(dataReader.ReadLine());
                            ftpFileInfo.FullName = Path.Combine(remoteDirectoryName, ftpFileInfo.Name);
                            files.Add(ftpFileInfo);
                        }

                        return files;
                    }
                }
            }

            this.CloseFtpRequest(ftpRequest);
        }

        private FtpFileInfo ParseListDirectoryEntry(string ftpFileInfoString)
        {
            string fileInfoRegEx =
                    @"^" +                          //# Start of line
                    @"(?<dir>[\-ld])" +             //# File size
                    @"(?<permission>[\-rwx]{9})" +  //# Whitespace          \n
                    @"\s+" +                        //# Whitespace          \n
                    @"(?<filecode>\d+)" +
                    @"\s+" +                        //# Whitespace          \n
                    @"(?<owner>\w+)" +
                    @"\s+" +                        //# Whitespace          \n
                    @"(?<group>\w+)" +
                    @"\s+" +                        //# Whitespace          \n
                    @"(?<size>\d+)" +
                    @"\s+" +                        //# Whitespace          \n
                    @"(?<month>\w{3})" +            //# Month (3 letters)   \n
                    @"\s+" +                        //# Whitespace          \n
                    @"(?<day>\d{1,2})" +            //# Day (1 or 2 digits) \n
                    @"\s+" +                        //# Whitespace          \n
                    @"(?<timeyear>[\d:]{4,5})" +    //# Time or year        \n
                    @"\s+" +                        //# Whitespace          \n
                    @"(?<filename>(.*))" +          //# Filename            \n
                    @"$";                           //# End of line

            var ftpFileInfo = new FtpFileInfo();
            var split = new Regex(fileInfoRegEx).Match(ftpFileInfoString);

            string dir = split.Groups["dir"].ToString();
            ftpFileInfo.Name = split.Groups["filename"].ToString();
            ftpFileInfo.IsDirectory = !string.IsNullOrWhiteSpace(dir) && dir.Equals("d", StringComparison.OrdinalIgnoreCase);

            return ftpFileInfo;
        }

        public void DeleteFile(string deleteFile)
        {
            var ftpRequest = this.CreateFtpRequest(deleteFile);

            ftpRequest.Method = WebRequestMethods.Ftp.DeleteFile;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
            }

            this.CloseFtpRequest(ftpRequest);
        }

        public void RenameFile(string currentFileNameAndPath, string newFileName)
        {
            var ftpRequest = this.CreateFtpRequest(currentFileNameAndPath);

            ftpRequest.Method = WebRequestMethods.Ftp.Rename;

            ftpRequest.RenameTo = newFileName;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
            }

            this.CloseFtpRequest(ftpRequest);
        }

        public DateTime GetFileCreatedDateTime(string fileName)
        {
            var ftpRequest = this.CreateFtpRequest(fileName);

            ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                var lastModifiedDate = ftpResponse.LastModified;
                return lastModifiedDate;
            }

            this.CloseFtpRequest(ftpRequest);
        }

        public string GetFileSize(string fileName)
        {
            var ftpRequest = this.CreateFtpRequest(fileName);

            ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                using (Stream dataStream = ftpResponse.GetResponseStream())
                {
                    using (StreamReader dataReader = new StreamReader(dataStream, this.Encoding))
                    {
                        string fileInfo = null;
                        while (!dataReader.EndOfStream)
                        {
                            fileInfo = dataReader.ReadToEnd();
                        }

                        return fileInfo;
                    }
                }
            }

            this.CloseFtpRequest(ftpRequest);
        }

        private FtpWebRequest CreateFtpRequest(string remoteFile)
        {
            WebRequest.DefaultWebProxy = null;

            var requestUri = this.BuildRequestUri(remoteFile);

            var request = (FtpWebRequest)WebRequest.Create(requestUri);

            if (request != null)
            {
                request.Credentials = new NetworkCredential(this.user, this.password);
                request.UseBinary = true;
                //request.UsePassive = true;
                request.Proxy = null;
                request.KeepAlive = false;
            }

            return request;
        }

        private string BuildRequestUri(string remoteFile)
        {
            string requestUri = "";

            if (!this.host.ToLower().StartsWith("ftp://"))
            {
                requestUri = "ftp://";
            }

            remoteFile = remoteFile.Replace(@"\", "/");
            if (remoteFile.StartsWith("/"))
            {
                if (remoteFile.StartsWith("//"))
                {
                    remoteFile = remoteFile.Remove(0, 1);
                }
                requestUri += this.host + remoteFile;
            }
            else
            {
                requestUri += this.host + "/" + remoteFile;
            }
            return requestUri;
        }

        private void CloseFtpRequest(FtpWebRequest ftpRequest)
        {
            ftpRequest = null;
        }

        public Encoding Encoding
        {
            get
            {
                return Encoding.UTF7;
            }
        }
    }
}