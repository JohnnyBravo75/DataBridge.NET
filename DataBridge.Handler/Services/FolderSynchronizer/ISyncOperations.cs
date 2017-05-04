using System.Collections.Generic;

namespace DataBridge.Services
{
    public interface ISyncOperations
    {
        bool FileExits(string fileName);

        void FileDelete(string fileName);

        //void FileCopy(string sourceFileName, string targetFileName);

        IEnumerable<string> GetFiles(string directoryName, string[] fileFilters);

        FolderSynchronizer.SimpleFileinfo GetFileInfo(string fileName);

        bool DirectoryExists(string directoryName);

        void DirectoryDelete(string directoryName);

        void DirectoryCreate(string directoryName);

        IEnumerable<string> GetDirectories(string directoryName);
    }
}