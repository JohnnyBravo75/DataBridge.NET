using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace DataBridge.Services
{
    public class FileUtil
    {
        public static bool IsFileLocked(string filePath)
        {
            return IsFileLocked(new FileInfo(filePath));
        }

        public static bool IsFileLocked(FileInfo file)
        {
            if (!file.Exists)
                return false;

            try
            {
                using (var stream = file.Open(FileMode.Open, FileAccess.ReadWrite, FileShare.None))
                {
                    return false;
                }
            }
            catch (IOException)
            {
                return true;
            }

            return false;
        }

        // This method accepts two strings the represent two files to
        // compare. A return value of 0 indicates that the contents of the files
        // are the same. A return value of any other value indicates that the
        // files are not the same.
        public static bool CompareFiles(string file1, string file2)
        {
            int file1Byte;
            int file2Byte;

            // Determine if the same file was referenced two times.
            if (file1 == file2)
            {
                // Return true to indicate that the files are the same.
                return true;
            }

            // Open the two files.
            using (var fs1 = new FileStream(file1, FileMode.Open, FileAccess.Read))
            {
                using (var fs2 = new FileStream(file2, FileMode.Open, FileAccess.Read))
                {
                    if (fs1.Length != fs2.Length)
                    {
                        // Close the file
                        fs1.Close();
                        fs2.Close();

                        // Return false to indicate files are different
                        return false;
                    }

                    // Read and compare a byte from each file until either a
                    // non-matching set of bytes is found or until the end of
                    // file1 is reached.
                    do
                    {
                        // Read one byte from each file.
                        file1Byte = fs1.ReadByte();
                        file2Byte = fs2.ReadByte();
                    }
                    while ((file1Byte == file2Byte) && (file1Byte != -1));

                    // Close the files.
                    fs1.Close();
                    fs2.Close();
                }
            }

            // Return the success of the comparison. "file1byte" is
            // equal to "file2byte" at this point only if the files are
            // the same.
            return ((file1Byte - file2Byte) == 0);
        }

        public static bool WaitForIdleFile(string file, int maxAttempts = 10)
        {
            if (string.IsNullOrEmpty(file))
            {
                return false;
            }

            if (!File.Exists(file))
            {
                return false;
            }

            bool isFileReady = false;
            var attempts = 0;
            while (!isFileReady && attempts < maxAttempts)
            {
                try
                {
                    using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite))
                    {
                        isFileReady = true;
                    }
                }
                catch (IOException)
                {
                    //File isn't ready yet, so we need to keep on waiting until it is.
                }

                if (!isFileReady)
                {
                    Thread.Sleep(300);
                    attempts++;
                }
            }

            return isFileReady;
        }

        public static void WaitForLockRelease(params string[] files)
        {
            bool anyFileLocked;
            int iterations = 0;
            do
            {
                anyFileLocked = false;
                for (int i = 0; i < files.Length; i++)
                {
                    if (IsFileLocked(files[i]))
                    {
                        anyFileLocked = true;
                        break;
                    }
                }
                if (anyFileLocked)
                {
                    iterations++;
                    if (iterations > 0 && (iterations % 30) == 0)
                        Thread.Sleep(100);
                }
            } while (anyFileLocked);
        }

        public static string MakePathRelative(string path, string relativeToDir = ".")
        {
            string dir = Path.GetFullPath(path);
            string dirRel = Path.GetFullPath(relativeToDir);

            // Different disk drive: Cannot generate relative path.
            if (Directory.GetDirectoryRoot(dir) != Directory.GetDirectoryRoot(dirRel))
                return null;

            string resultDir = "";
            string[] dirToken = dir.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            string[] dirRelToken = dirRel.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);

            int numBackDir = dirRelToken.Length - dirToken.Length;
            int sameDirIndex = int.MaxValue;
            for (int i = 0; i < Math.Min(dirToken.Length, dirRelToken.Length); i++)
            {
                if (dirToken[i] != dirRelToken[i])
                {
                    numBackDir = dirRelToken.Length - i;
                    break;
                }
                else
                {
                    sameDirIndex = i;
                }
            }

            // Go back until we've reached the smallest mutual directory
            if (numBackDir > 0)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < numBackDir; i++)
                {
                    builder.Append("..");
                    builder.Append(Path.DirectorySeparatorChar);
                }
                resultDir = builder.ToString() + resultDir;
            }

            // ... and then go to the desired path from there
            for (int i = sameDirIndex + 1; i < dirToken.Length; i++)
            {
                resultDir = Path.Combine(resultDir, dirToken[i]);
            }

            return resultDir;
        }

        ///// <summary>
        /////   Copyright:   Julijan ?ribar, 2004-2007
        /////   Checks if name matches pattern with '?' and '*' wildcards.
        ///// </summary>
        ///// <param name="filename">
        /////   Name to match.
        ///// </param>
        ///// <param name="pattern">
        /////   Pattern to match to.
        ///// </param>
        ///// <returns>
        /////   <c>true</c> if name matches pattern, otherwise <c>false</c>.
        ///// </returns>
        //public static bool FilenameMatchesPattern(string filename, string pattern)
        //{
        //    // prepare the pattern to the form appropriate for Regex class
        //    StringBuilder sb = new StringBuilder(pattern);
        //    // remove superflous occurences of  "?*" and "*?"
        //    while (sb.ToString().IndexOf("?*") != -1)
        //    {
        //        sb.Replace("?*", "*");
        //    }

        //    while (sb.ToString().IndexOf("*?") != -1)
        //    {
        //        sb.Replace("*?", "*");
        //    }

        //    // remove superflous occurences of asterisk '*'
        //    while (sb.ToString().IndexOf("**") != -1)
        //    {
        //        sb.Replace("**", "*");
        //    }

        //    // if only asterisk '*' is left, the mask is ".*"
        //    if (sb.ToString().Equals("*"))
        //        pattern = ".*";
        //    else
        //    {
        //        // replace '.' with "\."
        //        sb.Replace(".", "\\.");
        //        // replaces all occurrences of '*' with ".*"
        //        sb.Replace("*", ".*");
        //        // replaces all occurrences of '?' with '.*'
        //        sb.Replace("?", ".");
        //        // add "\b" to the beginning and end of the pattern
        //        sb.Insert(0, "\\b");
        //        sb.Append("\\b");
        //        pattern = sb.ToString();
        //    }

        //    Regex regex = new Regex(pattern, RegexOptions.IgnoreCase);
        //    return regex.IsMatch(filename);
        //}

        /// <summary>
        /// <para>Tests if a file name matches the given wildcard pattern, uses the same rule as shell commands.</para>
        /// </summary>
        /// <param name="fileName">The file name to test, without folder.</param>
        /// <param name="pattern">A wildcard pattern which can use char * to match any amount of characters; or char ? to match one character.</param>
        /// <param name="unixStyle">If true, use the *nix style wildcard rules; otherwise use windows style rules.</param>
        /// <returns>true if the file name matches the pattern, false otherwise.</returns>
        public static bool MatchesWildcard(string fileName, string pattern, bool unixStyle = false)
        {
            if (fileName == null)
            {
                throw new ArgumentNullException("fileName");
            }

            if (pattern == null)
            {
                throw new ArgumentNullException("pattern");
            }

            var subPatterns = pattern.Split(new char[] { '|', ';' });

            foreach (var subPattern in subPatterns)
            {
                if (unixStyle)
                {
                    if (WildcardMatchesUnixStyle(fileName, subPattern))
                    {
                        return true;
                    }
                }
                else
                {
                    if (WildcardMatchesWindowsStyle(fileName, subPattern))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool WildcardMatchesWindowsStyle(string fileName, string pattern)
        {
            var dotdot = pattern.IndexOf("..", StringComparison.Ordinal);
            if (dotdot >= 0)
            {
                for (var i = dotdot; i < pattern.Length; i++)
                {
                    if (pattern[i] != '.')
                    {
                        return false;
                    }
                }
            }

            var normalized = Regex.Replace(pattern, @"\.+$", "");
            var endsWithDot = normalized.Length != pattern.Length;

            var endWeight = 0;
            if (endsWithDot)
            {
                var lastNonWildcard = normalized.Length - 1;
                for (; lastNonWildcard >= 0; lastNonWildcard--)
                {
                    var c = normalized[lastNonWildcard];
                    if (c == '*')
                    {
                        endWeight += short.MaxValue;
                    }
                    else if (c == '?')
                    {
                        endWeight += 1;
                    }
                    else
                    {
                        break;
                    }
                }

                if (endWeight > 0)
                    normalized = normalized.Substring(0, lastNonWildcard + 1);
            }

            var endsWithWildcardDot = endWeight > 0;
            var endsWithDotWildcardDot = endsWithWildcardDot && normalized.EndsWith(".");
            if (endsWithDotWildcardDot)
            {
                normalized = normalized.Substring(0, normalized.Length - 1);
            }

            normalized = Regex.Replace(normalized, @"(?!^)(\.\*)+$", @".*");

            var escaped = Regex.Escape(normalized);
            string head, tail;

            if (endsWithDotWildcardDot)
            {
                head = "^" + escaped;
                tail = @"(\.[^.]{0," + endWeight + "})?$";
            }
            else if (endsWithWildcardDot)
            {
                head = "^" + escaped;
                tail = "[^.]{0," + endWeight + "}$";
            }
            else
            {
                head = "^" + escaped;
                tail = "$";
            }

            if (head.EndsWith(@"\.\*") && head.Length > 5)
            {
                head = head.Substring(0, head.Length - 4);
                tail = @"(\..*)?" + tail;
            }

            var regex = head.Replace(@"\*", ".*").Replace(@"\?", "[^.]?") + tail;
            return Regex.IsMatch(fileName, regex, RegexOptions.IgnoreCase);
        }

        private static bool WildcardMatchesUnixStyle(string text, string pattern)
        {
            var regex = "^" + Regex.Escape(pattern)
                                   .Replace("\\*", ".*")
                                   .Replace("\\?", ".")
                        + "$";

            return Regex.IsMatch(text, regex);
        }

        /// <summary>
        ///   Kopiert ein Verzeichnis mit allen Unterverzeichnissen und Daten
        /// </summary>
        /// <param name="directorySource">Quellverzeichis</param>
        /// <param name="directoryTarget">Zielverzeichnis</param>
        public static int CopyDirectory(string directorySource, string directoryTarget, bool overwrite)
        {
            // alle zu Kopierenden Unterverzeichnisse ermitteln
            try
            {
                int files = 0;
                string[] subDirectories = Directory.GetDirectories(directorySource);
                // Zielpfad
                StringBuilder newTargetPath = new StringBuilder();
                {
                    newTargetPath.Append(directoryTarget);
                    newTargetPath.Append(directorySource.Substring(directorySource.LastIndexOf(@"\")));
                }

                // pruefen ob der aktuelle Ordner bereist Existiert (wenn nicht anlegen)
                if (!Directory.Exists(newTargetPath.ToString()))
                {
                    Directory.CreateDirectory(newTargetPath.ToString());
                }

                // Unterverzeichnisse durchlaufen(rekursion)
                foreach (string subDirectory in subDirectories)
                {
                    string newDirectoryPath = subDirectory;

                    // Backslash an letzter stelle Abschneiden
                    if (newDirectoryPath.LastIndexOf(@"\") == (newDirectoryPath.Length - 1))
                    {
                        newDirectoryPath = newDirectoryPath.Substring(0, newDirectoryPath.Length - 1);
                    }
                    // rekursiver Aufruf
                    files = files + CopyDirectory(newDirectoryPath, newTargetPath.ToString(), false);
                }

                // alle Dateien des Verzeichnisses ermitteln
                string[] fileNames = Directory.GetFiles(directorySource);

                // jede Datei kopieren
                foreach (string fileSource in fileNames)
                {
                    // Zielverzeichnis mit Dateiname erstellen
                    StringBuilder fileTarget = new StringBuilder();
                    {
                        fileTarget.Append(newTargetPath);
                        fileTarget.Append(fileSource.Substring(fileSource.LastIndexOf(@"\")));
                    }

                    System.IO.File.Copy(fileSource, fileTarget.ToString(), overwrite);
                    files = files + 1;
                }

                return files;
            }
            catch (Exception ex)
            {
                return -1;
            }
        }
    }
}