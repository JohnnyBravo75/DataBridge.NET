using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DataBridge.ConnectionInfos;
using DataBridge.Helper;
using DataBridge.Services;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "FtpFileDownloader", Title = "FtpFileDownloader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class FtpFileDownloader : DataCommand
    {
        private IFtp ftp;
        private FtpConnectionInfo connectionInfo = new FtpConnectionInfo();

        public FtpFileDownloader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Host", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "User", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "RemoteDirectory", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "LocalDirectory", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut });
            this.Parameters.Add(new CommandParameter() { Name = "FtpType", Direction = Directions.In, Value = FtpTypes.FTP });
        }

        //public FtpConnectionInfo ConnectionInfo
        //{
        //    get { return this.connectionInfo; }
        //    set { this.connectionInfo = value; }
        //}

        [XmlIgnore]
        public string Host
        {
            get { return this.Parameters.GetValue<string>("Host"); }
            set { this.Parameters.SetOrAddValue("Host", value); }
        }

        [XmlIgnore]
        public string Password
        {
            get { return this.Parameters.GetValue<string>("Password"); }
            set { this.Parameters.SetOrAddValue("Password", value); }
        }

        [XmlIgnore]
        public string User
        {
            get { return this.Parameters.GetValue<string>("User"); }
            set { this.Parameters.SetOrAddValue("User", value); }
        }

        [XmlIgnore]
        public string RemoteDirectory
        {
            get { return this.Parameters.GetValue<string>("RemoteDirectory"); }
            set { this.Parameters.SetOrAddValue("RemoteDirectory", value); }
        }

        [XmlIgnore]
        public string LocalDirectory
        {
            get { return this.Parameters.GetValue<string>("LocalDirectory"); }
            set { this.Parameters.SetOrAddValue("LocalDirectory", value); }
        }

        [XmlIgnore]
        public FtpTypes FtpType
        {
            get { return this.Parameters.GetValue<FtpTypes>("FtpType"); }
            set { this.Parameters.SetOrAddValue("FtpType", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string host = inParameters.GetValue<string>("Host");
                string passWord = inParameters.GetValue<string>("Password");
                string user = inParameters.GetValue<string>("User");
                string remoteDirectory = inParameters.GetValueOrDefault<string>("RemoteDirectory", "\\*.*");
                string localDirectory = inParameters.GetValue<string>("LocalDirectory");
                FtpTypes ftpType = inParameters.GetValue<FtpTypes>("FtpType");

                if (this.ftp == null)
                {
                    switch (ftpType)
                    {
                        case FtpTypes.FTP:
                            this.ftp = new Ftp();
                            break;

                        case FtpTypes.FTPS:
                            this.ftp = new FtpS();
                            break;

                        case FtpTypes.SFTP:
                            this.ftp = new SFtp();
                            break;
                    }
                }

                this.ftp.SetConnectionInfos(host, user, passWord);

                //this.ftp.SetConnectionInfos(this.ConnectionInfo.FtpServer, this.ConnectionInfo.UserName, this.ConnectionInfo.DecryptedPassword);

                this.LogDebugFormat("Start reading files from Host='{0}', RemoteDirectory='{1}'", host, remoteDirectory);

                int fileIdx = 0;
                foreach (var localFileName in this.ReadData(remoteDirectory, localDirectory))
                {
                    fileIdx++;

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("File", localFileName);

                    yield return outParameters;
                }

                this.LogDebugFormat("End reading files from Host='{0}', RemoteDirectory='{1}': FilesCount={2}", host,
                    remoteDirectory, fileIdx);
            }
        }

        private IEnumerable<string> ReadData(string remoteDirectory, string localDirectory)
        {
            DirectoryUtil.CreateDirectoryIfNotExists(localDirectory);

            var remoteFileNames = this.ftp.GetDirectoryList(remoteDirectory);
            foreach (var remoteFileName in remoteFileNames)
            {
                string localFileName = Path.Combine(localDirectory, remoteFileName.Name);
                this.ftp.DownloadFile(remoteFileName.FullName, localFileName);

                yield return localFileName;
            }
        }
    }
}