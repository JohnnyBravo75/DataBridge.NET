using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using DataBridge.ConnectionInfos;
using DataBridge.Services;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "FtpFileUploader", Title = "FtpFileUploader", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class FtpFileUploader : DataCommand
    {
        private IFtp ftp;
        private FtpConnectionInfo connectionInfo = new FtpConnectionInfo();

        public FtpFileUploader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Host", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "User", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "File" });
            this.Parameters.Add(new CommandParameter() { Name = "RemoteDirectory" });
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
        [System.ComponentModel.Editor(typeof(Wpf.Toolkit.PropertyEditors.OpenFilePropertyEditor), typeof(Wpf.Toolkit.PropertyEditors.OpenFilePropertyEditor))]
        public string File
        {
            get { return this.Parameters.GetValue<string>("File"); }
            set { this.Parameters.SetOrAddValue("File", value); }
        }

        [XmlIgnore]
        public string RemoteDirectory
        {
            get { return this.Parameters.GetValue<string>("RemoteDirectory"); }
            set { this.Parameters.SetOrAddValue("RemoteDirectory", value); }
        }

        [XmlIgnore]
        public FtpTypes FtpType
        {
            get { return this.Parameters.GetValue<FtpTypes>("FtpType"); }
            set { this.Parameters.SetOrAddValue("FtpType", value); }
        }

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string host = inParameters.GetValue<string>("Host");
                string passWord = inParameters.GetValue<string>("Password");
                string user = inParameters.GetValue<string>("User");
                string file = inParameters.GetValue<string>("File");
                string remoteDirectory = inParameters.GetValueOrDefault<string>("RemoteDirectory", "\\");
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

                this.LogDebugFormat("Start writing files To Host='{0}', RemoteDirectory='{1}'", host, remoteDirectory);

                this.WriteData(remoteDirectory, file);

                var outParameters = this.GetCurrentOutParameters();
                outParameters.SetOrAddValue("File", file);
                yield return outParameters;
            }
        }

        private void WriteData(string remoteDirectory, string localFileName)
        {
            string remoteFileName = Path.Combine(remoteDirectory, Path.GetFileName(localFileName));
            this.ftp.UploadFile(localFileName, remoteFileName);
        }
    }
}