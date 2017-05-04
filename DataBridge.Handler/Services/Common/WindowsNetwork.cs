using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DataBridge.Services
{
    /// <summary>
    /// TODO: Look at https://github.com/psmacchia/NDepend.Path
    ///
    ///   var existsPath = WindowsNetwork.IsPathAvailable(@"\\COMPUTER01\develop");
    /// var errorText = WindowsNetwork.ConnectRemote(@"\\COMPUTER01\develop", @"DOMAIN\User", "", "Y:");
    /// WindowsNetwork.DisconnectRemote(@"\\COMPUTER01\develop");
    /// </summary>
    public class WindowsNetwork
    {
        #region Consts

        private const int RESOURCE_CONNECTED = 0x00000001;
        private const int RESOURCE_GLOBALNET = 0x00000002;
        private const int RESOURCE_REMEMBERED = 0x00000003;

        private const int RESOURCETYPE_ANY = 0x00000000;
        private const int RESOURCETYPE_DISK = 0x00000001;
        private const int RESOURCETYPE_PRINT = 0x00000002;

        private const int RESOURCEDISPLAYTYPE_GENERIC = 0x00000000;
        private const int RESOURCEDISPLAYTYPE_DOMAIN = 0x00000001;
        private const int RESOURCEDISPLAYTYPE_SERVER = 0x00000002;
        private const int RESOURCEDISPLAYTYPE_SHARE = 0x00000003;
        private const int RESOURCEDISPLAYTYPE_FILE = 0x00000004;
        private const int RESOURCEDISPLAYTYPE_GROUP = 0x00000005;

        private const int RESOURCEUSAGE_CONNECTABLE = 0x00000001;
        private const int RESOURCEUSAGE_CONTAINER = 0x00000002;

        private const int CONNECT_INTERACTIVE = 0x00000008;
        private const int CONNECT_PROMPT = 0x00000010;
        private const int CONNECT_REDIRECT = 0x00000080;
        private const int CONNECT_UPDATE_PROFILE = 0x00000001;
        private const int CONNECT_COMMANDLINE = 0x00000800;
        private const int CONNECT_CMD_SAVECRED = 0x00001000;

        private const int CONNECT_LOCALDRIVE = 0x00000100;

        #endregion Consts

        #region Errors

        private const int NO_ERROR = 0;

        private const int ERROR_ACCESS_DENIED = 5;
        private const int ERROR_ALREADY_ASSIGNED = 85;
        private const int ERROR_BAD_DEVICE = 1200;
        private const int ERROR_BAD_NET_NAME = 67;
        private const int ERROR_BAD_PROVIDER = 1204;
        private const int ERROR_CANCELLED = 1223;
        private const int ERROR_EXTENDED_ERROR = 1208;
        private const int ERROR_INVALID_ADDRESS = 487;
        private const int ERROR_INVALID_PARAMETER = 87;
        private const int ERROR_INVALID_PASSWORD = 1216;
        private const int ERROR_MORE_DATA = 234;
        private const int ERROR_NO_MORE_ITEMS = 259;
        private const int ERROR_NO_NET_OR_BAD_PATH = 1203;
        private const int ERROR_NO_NETWORK = 1222;

        private const int ERROR_BAD_PROFILE = 1206;
        private const int ERROR_CANNOT_OPEN_PROFILE = 1205;
        private const int ERROR_DEVICE_IN_USE = 2404;
        private const int ERROR_NOT_CONNECTED = 2250;
        private const int ERROR_OPEN_FILES = 2401;

        private struct ErrorClass
        {
            public int errorNumber;
            public string message;

            public ErrorClass(int num, string message)
            {
                this.errorNumber = num;
                this.message = message;
            }
        }

        // Created with excel formula:
        // ="new ErrorClass("&A1&", """&PROPER(SUBSTITUTE(MID(A1,7,LEN(A1)-6), "_", " "))&"""), "
        private static ErrorClass[] ERROR_LIST = new ErrorClass[] {
            new ErrorClass(ERROR_ACCESS_DENIED, "Error: Access Denied"),
            new ErrorClass(ERROR_ALREADY_ASSIGNED, "Error: Already Assigned"),
            new ErrorClass(ERROR_BAD_DEVICE, "Error: Bad Device"),
            new ErrorClass(ERROR_BAD_NET_NAME, "Error: Bad Net Name"),
            new ErrorClass(ERROR_BAD_PROVIDER, "Error: Bad Provider"),
            new ErrorClass(ERROR_CANCELLED, "Error: Cancelled"),
            new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
            new ErrorClass(ERROR_INVALID_ADDRESS, "Error: Invalid Address"),
            new ErrorClass(ERROR_INVALID_PARAMETER, "Error: Invalid Parameter"),
            new ErrorClass(ERROR_INVALID_PASSWORD, "Error: Invalid Password"),
            new ErrorClass(ERROR_MORE_DATA, "Error: More Data"),
            new ErrorClass(ERROR_NO_MORE_ITEMS, "Error: No More Items"),
            new ErrorClass(ERROR_NO_NET_OR_BAD_PATH, "Error: No Net Or Bad Path"),
            new ErrorClass(ERROR_NO_NETWORK, "Error: No Network"),
            new ErrorClass(ERROR_BAD_PROFILE, "Error: Bad Profile"),
            new ErrorClass(ERROR_CANNOT_OPEN_PROFILE, "Error: Cannot Open Profile"),
            new ErrorClass(ERROR_DEVICE_IN_USE, "Error: Device In Use"),
            new ErrorClass(ERROR_EXTENDED_ERROR, "Error: Extended Error"),
            new ErrorClass(ERROR_NOT_CONNECTED, "Error: Not Connected"),
            new ErrorClass(ERROR_OPEN_FILES, "Error: Open Files"),
        };

        private static string GetErrorText(int errNum)
        {
            foreach (ErrorClass er in ERROR_LIST)
            {
                if (er.errorNumber == errNum)
                {
                    return er.message;
                }
            }
            return "Error: Unknown, " + errNum;
        }

        #endregion Errors

        [DllImport("Mpr.dll")]
        private static extern int WNetUseConnection(
            IntPtr hwndOwner,
            NETRESOURCE lpNetResource,
            string lpPassword,
            string lpUserID,
            int dwFlags,
            string lpAccessName,
            string lpBufferSize,
            string lpResult
            );

        [DllImport("Mpr.dll")]
        private static extern int WNetCancelConnection2(
            string lpName,
            int dwFlags,
            bool fForce
            );

        [DllImport("mpr.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern int WNetGetConnection([MarshalAs(UnmanagedType.LPTStr)] string localName, [MarshalAs(UnmanagedType.LPTStr)] StringBuilder remoteName, ref int length);

        public static string GetUNCPath(string originalPath)
        {
            StringBuilder sb = new StringBuilder(512);
            int size = sb.Capacity;

            // look for the {LETTER}: combination ...
            if (originalPath.Length > 2 && originalPath[1] == ':')
            {
                // don't use char.IsLetter here - as that can be misleading
                // the only valid drive letters are a-z && A-Z.
                char c = originalPath[0];
                if ((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z'))
                {
                    int error = WNetGetConnection(originalPath.Substring(0, 2), sb, ref size);
                    if (error == 0)
                    {
                        DirectoryInfo dir = new DirectoryInfo(originalPath);
                        string path = Path.GetFullPath(originalPath).Substring(Path.GetPathRoot(originalPath).Length);
                        return Path.Combine(sb.ToString().TrimEnd(), path);
                    }
                }
            }

            return originalPath;
        }

        [StructLayout(LayoutKind.Sequential)]
        private class NETRESOURCE
        {
            public int dwScope = 0;
            public int dwType = 0;
            public int dwDisplayType = 0;
            public int dwUsage = 0;
            public string lpLocalName = "";
            public string lpRemoteName = "";
            public string lpComment = "";
            public string lpProvider = "";
        }

        public enum ConnectionModes
        {
            Connect,
            Disconnect
        }

        public static string ConnectRemote(string remoteUNC, string username, string password, string driveLetter, bool promptUser = false)
        {
            if (driveLetter != null && driveLetter.Length == 1)
            {
                if (char.IsLetter(driveLetter.ToCharArray()[0]))
                {
                    driveLetter = driveLetter + ":";
                }
            }

            var netResource = new NETRESOURCE();
            netResource.dwType = RESOURCETYPE_DISK;
            netResource.lpRemoteName = remoteUNC;
            netResource.lpLocalName = driveLetter;

            int returnCode;
            if (promptUser)
            {
                returnCode = WNetUseConnection(IntPtr.Zero, netResource, "", "", CONNECT_INTERACTIVE | CONNECT_PROMPT, null, null, null);
            }
            else
            {
                returnCode = WNetUseConnection(IntPtr.Zero, netResource, password, username, 0, null, null, null);
            }

            if (returnCode == NO_ERROR)
            {
                return string.Empty;
            }

            return GetErrorText(returnCode);
        }

        public static string DisconnectRemote(string remoteUNC)
        {
            int returnCode = WNetCancelConnection2(remoteUNC, CONNECT_UPDATE_PROFILE, false);

            if (returnCode == NO_ERROR)
            {
                return string.Empty;
            }

            return GetErrorText(returnCode);
        }

        // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
        /// <summary>
        /// A quick method to test is the path exists
        /// </summary>
        /// <param name="path"></param>
        /// <param name="timeOutMs"></param>
        /// <returns></returns>
        public static bool IsPathAvailable(string path, int timeOutMs = 120)
        {
            if (path.StartsWith(@"\\"))
            {
                return IsNetworkPathAvailable(path);

                //Uri uri = new Uri(path);
                //if (uri.Segments.Length == 0 || string.IsNullOrWhiteSpace(uri.Host))
                //{
                //    return false;
                //}

                //if (uri.Host != Dns.GetHostName())
                //{
                //    WebRequest request;
                //    WebResponse response;
                //    request = WebRequest.Create(uri);
                //    request.Method = "HEAD";
                //    request.Timeout = timeOutMs;
                //    try
                //    {
                //        response = request.GetResponse();
                //    }
                //    catch (Exception ex)
                //    {
                //        return false;
                //    }

                //    return response.ContentLength > 0;

                //    // - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
                //    // Do a Ping to see if the server is there
                //    // This method doesn't work well using OPenDNS since it always succeeds
                //    // regardless if the IP is a valid or not
                //    // OpenDns always maps every host to an IP. If the host is not valid the
                //    // OpenDNS will map it to 67.215.65.132
                //    /* Example:
                //        C:\>ping xxx

                //        Pinging xxx.RT-AC66R [67.215.65.132] with 32 bytes of data:
                //        Reply from 67.215.65.132: bytes=32 time=24ms TTL=55
                //        */

                //    //Ping pingSender = new Ping();
                //    //PingOptions options = new PingOptions();
                //    // Use the default Ttl value which is 128,
                //    // but change the fragmentation behavior.
                //    //options.DontFragment = true;

                //    // Create a buffer of 32 bytes of data to be transmitted.
                //    //string data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
                //    //byte[] buffer = Encoding.ASCII.GetBytes(data);
                //    //int timeout = 120;
                //    //PingReply reply = pingSender.Send(uri.Host, timeout, buffer, options);
                //    //if (reply == null || reply.Status != IPStatus.Success)
                //    //    return false;
                //}
            }

            return File.Exists(path);
        }

        public static bool DirectoryVisible(string path)
        {
            try
            {
                Directory.GetAccessControl(path);
                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool IsNetworkPathAvailable(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            string pathRoot = Path.GetPathRoot(path);
            if (string.IsNullOrEmpty(pathRoot))
            {
                return false;
            }

            ProcessStartInfo pinfo = new ProcessStartInfo("net", "use");
            pinfo.CreateNoWindow = true;
            pinfo.RedirectStandardOutput = true;
            pinfo.UseShellExecute = false;
            string output;
            using (Process p = Process.Start(pinfo))
            {
                output = p.StandardOutput.ReadToEnd();
            }

            foreach (string line in output.Split('\n'))
            {
                if (line.Contains(pathRoot) && line.Contains("OK"))
                {
                    return true; // shareIsProbablyConnected
                }
            }

            return false;
        }
    }
}