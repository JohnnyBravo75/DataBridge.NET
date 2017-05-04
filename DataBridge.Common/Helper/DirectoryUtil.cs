using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using DataBridge.Common.Helper;

namespace DataBridge.Helper
{
    public static class DirectoryUtil
    {
        public static void CopyDirectory(string sourceDirName, string destDirName, bool copySubDirs, bool overwrite)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            foreach (FileInfo file in dir.GetFiles())
            {
                file.CopyTo(Path.Combine(destDirName, file.Name), overwrite);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dir.GetDirectories())
                {
                    CopyDirectory(subdir.FullName, Path.Combine(destDirName, subdir.Name), copySubDirs, overwrite);
                }
            }
        }

        public static void ClearDirectory(string directory)
        {
            ClearDirectory(new DirectoryInfo(directory));
        }

        public static void ClearDirectory(DirectoryInfo directory)
        {
            foreach (FileInfo file in directory.GetFiles())
            {
                file.IsReadOnly = false;
                file.Delete();
            }

            foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            {
                // subDirectory.Delete(true);
                DeleteDirectory(subDirectory.FullName);
            }
        }

        public static void RemoveEmptyDirectory(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return;

            if (File.Exists(path))
                path = Path.GetDirectoryName(path);

            if (!Directory.Exists(path))
                return;

            if (Directory.EnumerateFiles(path).Any())
                return;

            if (Directory.EnumerateDirectories(path).Any())
                return;

            Directory.Delete(path);

            string parent;
            try
            {
                parent = Path.GetDirectoryName(path);
            }
            catch (Exception)
            {
                parent = null;
            }

            if (parent != null)
            {
                RemoveEmptyDirectory(parent);
            }
        }

        public static IEnumerable<string> GetFiles(string path, string searchPatternExpression = "", SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            Regex reSearchPattern = new Regex(searchPatternExpression);
            return Directory.EnumerateFiles(path, "*", searchOption)
                            .Where(file =>
                                     reSearchPattern.IsMatch(Path.GetExtension(file)));
        }

        public static IEnumerable<string> GetFilesParallel(string sourceFolder, string[] searchPatterns, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return searchPatterns.AsParallel()
                                 .SelectMany(filter =>
                                                Directory.EnumerateFiles(sourceFolder, filter, searchOption));
        }

        public static DirectoryInfo CreateDirectoryIfNotExists(string directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                return null;
            }

            if (Directory.Exists(directory))
            {
                return null;
            }

            return Directory.CreateDirectory(directory);
        }

        public static IEnumerable<string> GetFiles(string path, string[] searchPatterns, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            List<string> files = new List<string>();
            foreach (string sp in searchPatterns)
            {
                files.AddRange(Directory.GetFiles(path, sp, searchOption));
            }

            files.Sort();
            return files;
        }

        public static IEnumerable<string> GetFileNames(IEnumerable<string> fileNames)
        {
            var fileNamesUnrooted = new List<string>();
            foreach (string fileName in fileNames)
            {
                fileNamesUnrooted.Add(Path.GetFileName(fileName));
            }

            return fileNamesUnrooted;
        }

        public static IEnumerable<string> CombinePath(IEnumerable<string> fileNames, string path)
        {
            var fileNamesRooted = new List<string>();
            foreach (string fileName in fileNames)
            {
                fileNamesRooted.Add(Path.Combine(path, fileName));
            }

            return fileNamesRooted;
        }

        private static object _lock = new object();

        public static void DeleteDirectory2(string dir, bool secondAttempt = false)
        {
            // If this is a second try, we are going to manually
            // delete the files and sub-directories.
            if (secondAttempt)
            {
                // Interrupt the current thread to allow Explorer time to release a directory handle
                Thread.Sleep(0);

                // Delete any files in the directory
                foreach (var f in Directory.GetFiles(dir, "*.*", SearchOption.TopDirectoryOnly))
                    File.Delete(f);

                // Try manually recursing and deleting sub-directories
                foreach (var d in Directory.GetDirectories(dir))
                    DeleteDirectory(d);

                // Now we try to delete the current directory
                Directory.Delete(dir, false);
                return;
            }

            try
            {
                // First attempt: use the standard MSDN approach.
                // This will throw an exception a directory is open in explorer
                Directory.Delete(dir, true);
            }
            catch (IOException)
            {
                // Try again to delete the directory manually recursing.
                DeleteDirectory2(dir, true);
            }
            catch (UnauthorizedAccessException)
            {
                // Try again to delete the directory manually recursing.
                DeleteDirectory2(dir, true);
            }
        }

        public static void DeleteRecursivelyWithMagicDust(string destinationDir)
        {
            const int magicDust = 10;
            for (var gnomes = 1; gnomes <= magicDust; gnomes++)
            {
                try
                {
                    Directory.Delete(destinationDir, true);
                }
                catch (DirectoryNotFoundException)
                {
                    return;  // good!
                }
                catch (IOException)
                { // System.IO.IOException: The directory is not empty
                    System.Diagnostics.Debug.WriteLine("Gnomes prevent deletion of {0}! Applying magic dust, attempt #{1}.", destinationDir, gnomes);

                    // see http://stackoverflow.com/questions/329355/cannot-delete-directory-with-directory-deletepath-true for more magic
                    Thread.Sleep(50);
                    continue;
                }
                return;
            }
            // depending on your use case, consider throwing an exception here
        }

        public static void DeleteDirectory(string directory)
        {
            DeleteDirectoryFiles(directory);

            while (Directory.Exists(directory))
            {
                lock (_lock)
                {
                    DeleteDirectoryDirs(directory);
                }
            }
        }

        private static void DeleteDirectoryDirs(string directory)
        {
            Thread.Sleep(100);

            if (Directory.Exists(directory))
            {
                var dirs = Directory.GetDirectories(directory);

                if (dirs.Length == 0)
                {
                    Directory.Delete(directory, false);
                }
                else
                {
                    foreach (string dir in dirs)
                    {
                        DeleteDirectoryDirs(dir);
                    }
                }
            }
        }

        private static void DeleteDirectoryFiles(string directory)
        {
            var files = Directory.GetFiles(directory);
            var dirs = Directory.GetDirectories(directory);

            foreach (string file in files)
            {
                try
                {
                    File.SetAttributes(file, FileAttributes.Normal);
                    File.Delete(file);
                }
                catch (Exception)
                {
                    Thread.Sleep(100);
                    File.SetAttributes(file, FileAttributes.Normal);
                }
            }

            foreach (string dir in dirs)
            {
                DeleteDirectoryFiles(dir);
            }
        }
    }
}