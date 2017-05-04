using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using DataBridge.Helper;

namespace DataBridge.Services
{
    public class FolderSync
    {
        private string fileFilter = "*.*";

        private readonly List<string> deleted = new List<string>();
        private readonly List<string> changed = new List<string>();
        private readonly List<string> added = new List<string>();

        private DateTime? lastSynced = null;

        /// <summary>
        /// Gets or sets the last synced.
        /// </summary>
        /// <value>The last synced.</value>
        public DateTime? LastSynced
        {
            get { return this.lastSynced; }
        }

        /// <summary>
        /// Gets or sets the deleted files.
        /// </summary>
        /// <value>The deleted files.</value>
        public List<string> Deleted
        {
            get { return this.deleted; }
        }

        /// <summary>
        /// Gets or sets the changed files.
        /// </summary>
        /// <value>The changed files.</value>
        public List<string> Changed
        {
            get { return this.changed; }
        }

        /// <summary>
        /// Gets or sets the new files.
        /// </summary>
        /// <value>The new files.</value>
        public List<string> Added
        {
            get { return this.added; }
        }

        /// <summary>
        /// Gets or sets the file filter.
        /// </summary>
        /// <value>
        /// The file filter.
        /// </value>
        public string FileFilter
        {
            get { return this.fileFilter; }
            set { this.fileFilter = value; }
        }

        /// <summary>
        /// Gets or sets the source directory.
        /// </summary>
        /// <value>
        /// The source directory.
        /// </value>
        public string SourceDirectory { get; set; }

        /// <summary>
        /// Gets or sets the target directory.
        /// </summary>
        /// <value>
        /// The target directory.
        /// </value>
        public string TargetDirectory { get; set; }

        /// <summary>
        /// Gets or sets the backup directory.
        /// </summary>
        /// <value>
        /// The backup directory.
        /// </value>
        public string BackupDirectory { get; set; }

        /// <summary>
        /// Synchronizes the specified source directory.
        /// </summary>
        public void Sync()
        {
            this.added.Clear();
            this.changed.Clear();
            this.deleted.Clear();

            this.lastSynced = DateTime.Now;

            this.SyncDirectory(this.SourceDirectory, this.TargetDirectory);
        }

        /// <summary>
        /// Synchronizes the directory.
        /// </summary>
        /// <param name="srcDirectory">The source directory.</param>
        /// <param name="tgtDirectory">The TGT directory.</param>
        private void SyncDirectory(string srcDirectory, string tgtDirectory)
        {
            if (srcDirectory == tgtDirectory)
            {
                return;
            }

            try
            {
                if (!DirectoryOperations.Exists(tgtDirectory))
                {
                    DirectoryOperations.CreateDirectory(tgtDirectory);
                    this.added.Add(tgtDirectory);
                }

                // check files
                var srcFileNames = FileOperations.GetFiles(srcDirectory, this.FileFilter.Split(';', '|')).ToList();

                this.CopyFiles(srcFileNames, tgtDirectory);
                this.DeleteFiles(srcFileNames, tgtDirectory);

                // check directories
                var srcSubDirectories = DirectoryOperations.GetDirectories(srcDirectory).ToList();

                this.CopyDirectories(srcSubDirectories, tgtDirectory);
                this.DeleteDirectories(srcSubDirectories, tgtDirectory);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Copies the files.
        /// </summary>
        /// <param name="srcFileNames">The source file names.</param>
        /// <param name="tgtPath">The TGT path.</param>
        private void CopyFiles(List<string> srcFileNames, string tgtPath)
        {
            foreach (var srcFileName in srcFileNames)
            {
                var tgtFileName = this.BuildTgtPath(tgtPath, srcFileName);

                if (FileOperations.Exists(tgtFileName))
                {
                    if (this.CheckOverwrite(srcFileName, tgtFileName))
                    {
                        this.Backup(tgtFileName, FileSyncActions.Changed);

                        FileOperations.Delete(tgtFileName);
                        FileOperations.Copy(srcFileName, tgtFileName);
                        this.changed.Add(tgtFileName);
                    }
                }
                else
                {
                    FileOperations.Copy(srcFileName, tgtFileName);

                    this.Backup(tgtFileName, FileSyncActions.Added);
                    this.added.Add(tgtFileName);
                }
            }
        }

        /// <summary>
        /// Deletes the files.
        /// </summary>
        /// <param name="srcFileNames">The source file names.</param>
        /// <param name="tgtDirectory">The TGT directory.</param>
        private void DeleteFiles(List<string> srcFileNames, string tgtDirectory)
        {
            var tgtFileNames = FileOperations.GetFiles(tgtDirectory, this.FileFilter.Split(';', '|')).ToList();
            var tgtFileNamesUnrooted = DirectoryUtil.GetFileNames(tgtFileNames).ToList();
            var srcFileNamesUnrooted = DirectoryUtil.GetFileNames(srcFileNames).ToList();

            var deletedTgtFileNames = tgtFileNamesUnrooted.Except(srcFileNamesUnrooted);
            deletedTgtFileNames = DirectoryUtil.CombinePath(deletedTgtFileNames, tgtDirectory);
            foreach (var deletedFileName in deletedTgtFileNames)
            {
                this.Backup(deletedFileName, FileSyncActions.Deleted);

                FileOperations.Delete(deletedFileName);
                this.deleted.Add(deletedFileName);
            }
        }

        /// <summary>
        /// Copies the directories.
        /// </summary>
        /// <param name="srcDirectories">The source sub directories.</param>
        /// <param name="tgtDirectory">The TGT directory.</param>
        private void CopyDirectories(List<string> srcDirectories, string tgtDirectory)
        {
            foreach (var srcDirectory in srcDirectories)
            {
                // prevent endless recursion when tgt path is subfolder of src path
                if (srcDirectory != tgtDirectory)
                {
                    var tgtSubDirectory = this.BuildTgtPath(tgtDirectory, srcDirectory);

                    if (!DirectoryOperations.Exists(tgtSubDirectory))
                    {
                        DirectoryOperations.CreateDirectory(tgtSubDirectory);
                        this.added.Add(tgtSubDirectory);
                    }

                    // go recursive through subfolders
                    this.SyncDirectory(srcDirectory, tgtSubDirectory);
                }
            }
        }

        /// <summary>
        /// Deletes the directories.
        /// </summary>
        /// <param name="srcDirectories">The source sub directories.</param>
        /// <param name="tgtDirectory">The TGT directory.</param>
        private void DeleteDirectories(List<string> srcDirectories, string tgtDirectory)
        {
            // delete non existing directories
            var tgtSubDirectories = DirectoryOperations.GetDirectories(tgtDirectory);
            var tgtDirectoriesUnrooted = DirectoryUtil.GetFileNames(tgtSubDirectories).ToList();
            var srcDirectoriesUnrooted = DirectoryUtil.GetFileNames(srcDirectories).ToList();

            var deletedTgtDirectories = tgtDirectoriesUnrooted.Except(srcDirectoriesUnrooted);
            deletedTgtDirectories = DirectoryUtil.CombinePath(deletedTgtDirectories, tgtDirectory);
            foreach (var deletedTgtDirectory in deletedTgtDirectories)
            {
                DirectoryOperations.Delete(deletedTgtDirectory);
                this.deleted.Add(deletedTgtDirectory);
            }
        }

        /// <summary>
        /// Check if the files differ.
        /// </summary>
        /// <param name="srcFileName">Name of the source file.</param>
        /// <param name="tgtFileName">Name of the TGT file.</param>
        /// <returns></returns>
        private bool CheckOverwrite(string srcFileName, string tgtFileName)
        {
            var srcFileinfo = FileOperations.GetFileInfo(srcFileName);
            var tgtFileinfo = FileOperations.GetFileInfo(tgtFileName);

            // length differ
            if (srcFileinfo.Length != tgtFileinfo.Length)
            {
                return true;
            }

            // src file is newer
            if (srcFileinfo.LastWriteTime > tgtFileinfo.LastWriteTime)
            {
                return true;
            }

            // src file is older
            if (srcFileinfo.LastWriteTime < tgtFileinfo.LastWriteTime)
            {
                return false;
            }

            // same date, same length, check hashes if contents differs
            var srcHash = this.BuildHashcode(srcFileName);
            var tgtHash = this.BuildHashcode(tgtFileName);
            if (srcHash != tgtHash)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Builds the hashcode.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <returns></returns>
        private string BuildHashcode(string fileName)
        {
            using (var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read))
            {
                using (var sha1 = new SHA1Managed())
                {
                    var hash = sha1.ComputeHash(stream);
                    return Encoding.UTF8.GetString(hash);
                }
            }
        }

        /// <summary>
        /// Builds the TGT path.
        /// </summary>
        /// <param name="tgtPath">The TGT path.</param>
        /// <param name="filePath">The file path.</param>
        /// <returns></returns>
        private string BuildTgtPath(string tgtPath, string filePath)
        {
            string fileName = Path.GetFileName(filePath);
            string path = Path.Combine(tgtPath, fileName);
            return path;
        }

        public static class FileOperations
        {
            public static Func<string, bool> Exists = (
                (fileName) =>
                {
                    return File.Exists(fileName);
                });

            public static Action<string> Delete = (
                (fileName) =>
                {
                    File.Delete(fileName);
                });

            public static Action<string, string> Copy = (
                (sourceFileName, targetFileName) =>
                {
                    File.Copy(sourceFileName, targetFileName);
                });

            public static Func<string, string[], IEnumerable<string>> GetFiles = (
                (sourceDirectory, fileFilters) =>
                {
                    return DirectoryUtil.GetFiles(sourceDirectory, fileFilters);
                });

            public static Func<string, SimpleFileinfo> GetFileInfo = (
                (fileName) =>
                {
                    var fileInfo = new FileInfo(fileName);
                    return new SimpleFileinfo()
                    {
                        LastWriteTime = fileInfo.LastWriteTimeUtc,
                        Length = fileInfo.Length
                    };
                });
        }

        public static class DirectoryOperations
        {
            public static Func<string, bool> Exists = (
                (directoryName) =>
                {
                    return Directory.Exists(directoryName);
                });

            public static Func<string, IEnumerable<string>> GetDirectories = (
               (directoryName) =>
               {
                   return Directory.GetDirectories(directoryName);
               });

            public static Action<string> CreateDirectory = (
               (directoryName) =>
               {
                   Directory.CreateDirectory(directoryName);
               });

            public static Action<string> Delete = (
                (directoryName) =>
                {
                    Directory.Delete(directoryName, true);
                });
        }

        /// <summary>
        /// Backups the specified file name.
        /// </summary>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="fileSyncAction">The file synchronize action.</param>
        private void Backup(string fileName, FileSyncActions fileSyncAction)
        {
            if (string.IsNullOrEmpty(this.BackupDirectory))
            {
                return;
            }

            string currentBackupDirectory = Path.Combine(this.BackupDirectory, ((DateTime)this.LastSynced).ToString("yyyyMMdd-HHmmss"), fileSyncAction.ToString());

            if (!DirectoryOperations.Exists(currentBackupDirectory))
            {
                DirectoryOperations.CreateDirectory(currentBackupDirectory);
            }

            FileOperations.Copy(fileName, currentBackupDirectory);
        }

        private enum FileSyncActions
        {
            Added,
            Changed,
            Deleted
        }

        public class SimpleFileinfo
        {
            public DateTime? LastWriteTime { get; set; }

            public long? Length { get; set; }
        }
    }
}