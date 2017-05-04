using System;
using System.Runtime.InteropServices;

namespace DataBridge.Services
{
    public class NetworkShare
    {
        #region Member Variables

        [DllImport("mpr.dll")]
        private static extern int WNetAddConnection2A(ref NetResource refNetResource, string inPassword, string inUsername, int inFlags);

        [DllImport("mpr.dll")]
        private static extern int WNetCancelConnection2A(string inServer, int inFlags, int inForce);

        private String _Server;
        private String _Share;
        private String _DriveLetter = null;
        private String _Username = null;
        private String _Password = null;
        private int _Flags = 0;
        private NetResource _NetResource = new NetResource();
        private Int32 _AllowDisconnectWhenInUse = 0; // 0 = False; Any other value is True;

        #endregion Member Variables

        #region Constructors

        /// <summary>
        /// The default constructor
        /// </summary>
        public NetworkShare()
        {
        }

        /// <summary>
        /// This constructor takes a server and a share.
        /// </summary>
        public NetworkShare(String inServer, String inShare)
        {
            this._Server = inServer;
            this._Share = inShare;
        }

        /// <summary>
        /// This constructor takes a server and a share and a local drive letter.
        /// </summary>
        public NetworkShare(String inServer, String inShare, String inDriveLetter)
        {
            this._Server = inServer;
            this._Share = inShare;
            this.DriveLetter = inDriveLetter;
        }

        /// <summary>
        /// This constructor takes a server, share, username, and password.
        /// </summary>
        public NetworkShare(String inServer, String inShare, String inUsername, String inPassword)
        {
            this._Server = inServer;
            this._Share = inShare;
            this._Username = inUsername;
            this._Password = inPassword;
        }

        /// <summary>
        /// This constructor takes a server, share, drive letter, username, and password.
        /// </summary>
        public NetworkShare(String inServer, String inShare, String inDriveLetter, String inUsername, String inPassword)
        {
            this._Server = inServer;
            this._Share = inShare;
            this._DriveLetter = inDriveLetter;
            this._Username = inUsername;
            this._Password = inPassword;
        }

        #endregion Constructors

        #region Properties

        public String Server
        {
            get { return this._Server; }
            set { this._Server = value; }
        }

        public String Share
        {
            get { return this._Share; }
            set { this._Share = value; }
        }

        public String FullPath
        {
            get { return string.Format(@"\\{0}\{1}", this._Server, this._Share); }
        }

        public String DriveLetter
        {
            get { return this._DriveLetter; }
            set { this.SetDriveLetter(value); }
        }

        public String Username
        {
            get { return String.IsNullOrEmpty(this._Username) ? null : this._Username; }
            set { this._Username = value; }
        }

        public String Password
        {
            get { return String.IsNullOrEmpty(this._Password) ? null : this._Username; }
            set { this._Password = value; }
        }

        public int Flags
        {
            get { return this._Flags; }
            set { this._Flags = value; }
        }

        public NetResource Resource
        {
            get { return this._NetResource; }
            set { this._NetResource = value; }
        }

        public bool AllowDisconnectWhenInUse
        {
            get { return Convert.ToBoolean(this._AllowDisconnectWhenInUse); }
            set { this._AllowDisconnectWhenInUse = Convert.ToInt32(value); }
        }

        #endregion Properties

        #region Functions

        /// <summary>
        /// Establishes a connection to the share.
        ///
        /// Throws:
        ///
        ///
        /// </summary>
        public int Connect()
        {
            this._NetResource.Scope = (int)Scope.RESOURCE_GLOBALNET;
            this._NetResource.Type = (int)Type.RESOURCETYPE_DISK;
            this._NetResource.DisplayType = (int)DisplayType.RESOURCEDISPLAYTYPE_SHARE;
            this._NetResource.Usage = (int)Usage.RESOURCEUSAGE_CONNECTABLE;
            this._NetResource.RemoteName = this.FullPath;
            this._NetResource.LocalName = this.DriveLetter;

            return WNetAddConnection2A(ref this._NetResource, this._Password, this._Username, this._Flags);
        }

        /// <summary>
        /// Disconnects from the share.
        /// </summary>
        public int Disconnect()
        {
            int retVal = 0;
            if (null != this._DriveLetter)
            {
                retVal = WNetCancelConnection2A(this._DriveLetter, this._Flags, this._AllowDisconnectWhenInUse);
                retVal = WNetCancelConnection2A(this.FullPath, this._Flags, this._AllowDisconnectWhenInUse);
            }
            else
            {
                retVal = WNetCancelConnection2A(this.FullPath, this._Flags, this._AllowDisconnectWhenInUse);
            }
            return retVal;
        }

        private void SetDriveLetter(String inString)
        {
            if (inString.Length == 1)
            {
                if (char.IsLetter(inString.ToCharArray()[0]))
                {
                    this._DriveLetter = inString + ":";
                }
                else
                {
                    // The character is not a drive letter
                    this._DriveLetter = null;
                }
            }
            else if (inString.Length == 2)
            {
                char[] drive = inString.ToCharArray();
                if (char.IsLetter(drive[0]) && drive[1] == ':')
                {
                    this._DriveLetter = inString;
                }
                else
                {
                    // The character is not a drive letter
                    this._DriveLetter = null;
                }
            }
            else
            {
                // If we get here the value passed in is not valid
                // so make it null.
                this._DriveLetter = null;
            }
        }

        #endregion Functions

        #region NetResource Struct

        [StructLayout(LayoutKind.Sequential)]
        public struct NetResource
        {
            public uint Scope;
            public uint Type;
            public uint DisplayType;
            public uint Usage;
            public string LocalName;
            public string RemoteName;
            public string Comment;
            public string Provider;
        }

        #endregion NetResource Struct

        #region Enums

        public enum Scope
        {
            RESOURCE_CONNECTED = 1,
            RESOURCE_GLOBALNET,
            RESOURCE_REMEMBERED,
            RESOURCE_RECENT,
            RESOURCE_CONTEXT
        }

        public enum Type : uint
        {
            RESOURCETYPE_ANY,
            RESOURCETYPE_DISK,
            RESOURCETYPE_PRINT,
            RESOURCETYPE_RESERVED = 8,
            RESOURCETYPE_UNKNOWN = 4294967295
        }

        public enum DisplayType
        {
            RESOURCEDISPLAYTYPE_GENERIC,
            RESOURCEDISPLAYTYPE_DOMAIN,
            RESOURCEDISPLAYTYPE_SERVER,
            RESOURCEDISPLAYTYPE_SHARE,
            RESOURCEDISPLAYTYPE_FILE,
            RESOURCEDISPLAYTYPE_GROUP,
            RESOURCEDISPLAYTYPE_NETWORK,
            RESOURCEDISPLAYTYPE_ROOT,
            RESOURCEDISPLAYTYPE_SHAREADMIN,
            RESOURCEDISPLAYTYPE_DIRECTORY,
            RESOURCEDISPLAYTYPE_TREE,
            RESOURCEDISPLAYTYPE_NDSCONTAINER
        }

        public enum Usage : uint
        {
            RESOURCEUSAGE_CONNECTABLE = 1,
            RESOURCEUSAGE_CONTAINER = 2,
            RESOURCEUSAGE_NOLOCALDEVICE = 4,
            RESOURCEUSAGE_SIBLING = 8,
            RESOURCEUSAGE_ATTACHED = 16,
            RESOURCEUSAGE_ALL = 31,
            RESOURCEUSAGE_RESERVED = 2147483648
        }

        public enum ConnectionFlags : uint
        {
            CONNECT_UPDATE_PROFILE = 1,
            CONNECT_UPDATE_RECENT = 2,
            CONNECT_TEMPORARY = 4,
            CONNECT_INTERACTIVE = 8,
            CONNECT_PROMPT = 16,
            CONNECT_NEED_DRIVE = 32,
            CONNECT_REFCOUNT = 64,
            CONNECT_REDIRECT = 128,
            CONNECT_LOCALDRIVE = 256,
            CONNECT_CURRENT_MEDIA = 512,
            CONNECT_DEFERRED = 1024,
            CONNECT_COMMANDLINE = 2048,
            CONNECT_CMD_SAVECRED = 4096,
            CONNECT_CRED_RESET = 8192,
            CONNECT_RESERVED = 4278190080
        }

        #endregion Enums
    }
}