using System;
using System.IO;

namespace DataBridge.Services
{
    public enum FtpTypes
    {
        FTP,
        FTPS,
        SFTP
    }

    public class FtpBase
    {
        public string GetDirectoryName(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return path;
            }

            if (path.EndsWith("/"))
            {
                return path;
            }

            string directoryName = "";

            if (path.Contains("\\"))
            {
                directoryName = path.Remove(path.LastIndexOf('\\') + 1);
            }
            else
            {
                // Path.GetDirectoryName  replaces "/" with "\", so we have to do it our own
                directoryName = path.Remove(path.LastIndexOf('/') + 1);
            }

            return directoryName;
        }

        public bool IsDirectory(string directory)
        {
            if (directory == null)
            {
                throw new ArgumentNullException();
            }

            //var pointIndex = directory.IndexOf(".");
            //if (pointIndex > 0)
            //{
            //    return false;
            //}

            // GetExtension(string) returns string.Empty when no extension found
            return Path.GetExtension(directory) == string.Empty;
        }
    }
}