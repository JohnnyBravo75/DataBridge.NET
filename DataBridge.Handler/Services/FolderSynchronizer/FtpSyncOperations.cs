using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataBridge.Services
{
    public class FtpSyncOperations : ISyncOperations
    {
        private Ftp ftp = new Ftp();

        public Ftp Ftp
        {
            get
            {
                return ftp;
            }

            set
            {
                this.ftp = value;
            }
        }

        public bool FileExits(string fileName)
        {
            return this.Ftp.FileExists(fileName);
        }

        public void FileDelete(string fileName)
        {
            this.Ftp.DeleteFile(fileName);
        }

        public IEnumerable<string> GetFiles(string directoryName, string[] fileFilters)
        {
            var files = this.Ftp.GetDirectoryList(directoryName);
            return files.Select(x => x.FullName);
        }

        public FolderSynchronizer.SimpleFileinfo GetFileInfo(string fileName)
        {
            var fileInfo = this.Ftp.GetFileInfo(fileName);
            return new FolderSynchronizer.SimpleFileinfo()
            {
                Name = fileName,
                LastWriteTime = fileInfo.LastWriteTime,
                Length = fileInfo.Size
            };
        }

        public bool DirectoryExists(string directoryName)
        {
            return this.Ftp.DirectoryExists(directoryName);
        }

        public void DirectoryDelete(string directoryName)
        {
            Directory.Delete(directoryName, true);
        }

        public void DirectoryCreate(string directoryName)
        {
            this.Ftp.CreateDirectory(directoryName);
        }

        public IEnumerable<string> GetDirectories(string directoryName)
        {
            return Directory.GetDirectories(directoryName);
        }
    }
}