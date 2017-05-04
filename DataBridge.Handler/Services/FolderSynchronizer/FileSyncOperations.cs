using System.Collections.Generic;
using System.IO;
using DataBridge.Helper;

namespace DataBridge.Services
{
    public class FileSyncOperations : ISyncOperations
    {
        public bool FileExits(string fileName)
        {
            return File.Exists(fileName);
        }

        public void FileDelete(string fileName)
        {
            File.Delete(fileName);
        }

        //public void FileCopy(string sourceFileName, string targetFileName)
        //{
        //    File.Copy(sourceFileName, targetFileName);
        //}

        public IEnumerable<string> GetFiles(string directoryName, string[] fileFilters)
        {
            return DirectoryUtil.GetFiles(directoryName, fileFilters);
        }

        public FolderSynchronizer.SimpleFileinfo GetFileInfo(string fileName)
        {
            var fileInfo = new FileInfo(fileName);
            return new FolderSynchronizer.SimpleFileinfo()
            {
                Name = fileName,
                LastWriteTime = fileInfo.LastWriteTimeUtc,
                Length = fileInfo.Length
            };
        }

        public bool DirectoryExists(string directoryName)
        {
            return Directory.Exists(directoryName);
        }

        public void DirectoryDelete(string directoryName)
        {
            Directory.Delete(directoryName, true);
        }

        public void DirectoryCreate(string directoryName)
        {
            Directory.CreateDirectory(directoryName);
        }

        public IEnumerable<string> GetDirectories(string directoryName)
        {
            return Directory.GetDirectories(directoryName);
        }
    }
}