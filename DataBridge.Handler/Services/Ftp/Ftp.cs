using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DataBridge.Common.Helper;

namespace DataBridge.Services
{
    public class Ftp : FtpBase, IFtp
    {
        private string host = "";
        private string user = "";
        private string password = "";

        private int bufferSize = 2048;
        private bool useBinary = true;
        private bool keepAlive = false;
        bool usePassive = true;

        public Ftp()
        {
        }

        public bool UseBinary
        {
            get { return this.useBinary; }
            set { this.useBinary = value; }
        }

        public bool KeepAlive
        {
            get { return this.keepAlive; }
            set { this.keepAlive = value; }
        }

        public bool UsePassive
        {
            get { return this.usePassive; }
            set { this.usePassive = value; }
        }

        public bool UseProxy { get; set; }

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
                using (var ftpStream = ftpResponse.GetResponseStream())
                {
                    if (ftpStream != null)
                    {
                        using (var localFileStream = new FileStream(localFile, FileMode.Create))
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
            var fileInfo = new FileInfo(localFile);
            ftpRequest.ContentLength = fileInfo.Length;

            ftpRequest.Method = WebRequestMethods.Ftp.UploadFile;

            using (var ftpStream = ftpRequest.GetRequestStream())
            {
                using (var fileStream = new FileStream(localFile, FileMode.Open))
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

                if (deleteLocalFile)
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

        public FtpFileInfo GetFileInfo(string fileName)
        {
            var ftpFileInfo = new FtpFileInfo();
            ftpFileInfo.FullName = fileName;
            ftpFileInfo.Size = this.GetFileSize(fileName) ?? 0;
            ftpFileInfo.LastWriteTime = this.GetFileLastModified(fileName);
            return ftpFileInfo;
        }

        public bool DirectoryExists(string remoteDirectory)
        {
            try
            {
                var ftpRequest = this.CreateFtpRequest(remoteDirectory);

                ftpRequest.Method = WebRequestMethods.Ftp.ListDirectory;
                using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
                {
                }

                return true;
            }
            catch (WebException ex)
            {
                return false;
            }
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

            var ftpRequest = this.CreateFtpRequest(remoteDirectory);

            ftpRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                using (var dataStream = ftpResponse.GetResponseStream())
                {
                    using (var dataReader = new StreamReader(dataStream, this.Encoding))
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

        public int? GetFileSize(string fileName)
        {
            string fileInfo = null;

            var ftpRequest = this.CreateFtpRequest(fileName);

            ftpRequest.Method = WebRequestMethods.Ftp.GetFileSize;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                using (var dataStream = ftpResponse.GetResponseStream())
                {
                    using (var dataReader = new StreamReader(dataStream, this.Encoding))
                    {
                        while (!dataReader.EndOfStream)
                        {
                            fileInfo = dataReader.ReadToEnd();
                        }
                    }
                }
            }

            this.CloseFtpRequest(ftpRequest);

            if (string.IsNullOrEmpty(fileInfo))
            {
                return null;
            }

            return Convert.ToInt16(fileInfo);
        }

        public DateTime GetFileLastModified(string fileName)
        {
            DateTime lastModified;
            var ftpRequest = this.CreateFtpRequest(fileName);

            ftpRequest.Method = WebRequestMethods.Ftp.GetDateTimestamp;

            using (var ftpResponse = (FtpWebResponse)ftpRequest.GetResponse())
            {
                lastModified = ftpResponse.LastModified;
            }

            this.CloseFtpRequest(ftpRequest);

            return lastModified;
        }

        public bool FileExists(string fileName)
        {
            try
            {
                var fileSize = this.GetFileSize(fileName);
                if (fileSize.HasValue)
                {
                    return true;
                }

                return false;
            }
            catch (WebException ex)
            {
                var response = (FtpWebResponse)ex.Response;
                if (response.StatusCode == FtpStatusCode.ActionNotTakenFileUnavailable)
                {
                    // Does not exist
                    return false;
                }

                return false;
            }
        }


        private FtpWebRequest CreateFtpRequest(string remoteFile)
        {
            WebRequest.DefaultWebProxy = null;

            var requestUri = this.BuildRequestUri(remoteFile);

            var request = (FtpWebRequest)WebRequest.Create(requestUri);

            if (request != null)
            {
                request.Credentials = new NetworkCredential(this.user, this.password);
                request.UseBinary = this.UseBinary;
                request.UsePassive = this.UsePassive;
                request.KeepAlive = this.KeepAlive;

                if (this.UseProxy)
                {
                    var proxy = WebRequest.GetSystemWebProxy();
                    proxy.Credentials = CredentialCache.DefaultCredentials;
                    request.Proxy = proxy;
                }
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