using System.Collections.Generic;
using System.Xml.Serialization;
using WinSCP;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "WinSCPFileDownloader", Title = "WinSCPFileDownloader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class WinScpFileDownloader : DataCommand
    {
        public WinScpFileDownloader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Host", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "User", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "RemoteDirectory", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "LocalDirectory", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut });
            this.Parameters.Add(new CommandParameter() { Name = "SshHostKeyFingerprint", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Protocol", Direction = Directions.In, Value = Protocol.Ftp });
        }

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
        public string SshHostKeyFingerprint
        {
            get { return this.Parameters.GetValue<string>("SshHostKeyFingerprint"); }
            set { this.Parameters.SetOrAddValue("SshHostKeyFingerprint", value); }
        }

        [XmlIgnore]
        public Protocol Protocol
        {
            get { return this.Parameters.GetValue<Protocol>("Protocol"); }
            set { this.Parameters.SetOrAddValue("Protocol", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string host = inParameters.GetValue<string>("Host");
            string passWord = inParameters.GetValue<string>("Password");
            string user = inParameters.GetValue<string>("User");
            string remoteDirectory = inParameters.GetValueOrDefault<string>("RemoteDirectory", "\\*.*");
            string localDirectory = inParameters.GetValue<string>("LocalDirectory");
            Protocol protocol = inParameters.GetValue<Protocol>("Protocol");
            string sshHostKeyFingerprint = inParameters.GetValue<string>("SshHostKeyFingerprint");

            this.LogDebugFormat("Start reading files from Host='{0}', RemoteDirectory='{1}', Mode='{2}'", host, remoteDirectory, protocol);

            // Setup session options
            var sessionOptions = new SessionOptions();
            sessionOptions.Protocol = protocol;
            sessionOptions.HostName = host;
            sessionOptions.UserName = user;
            sessionOptions.Password = passWord;
            if (!string.IsNullOrEmpty(sshHostKeyFingerprint))
            {
                sessionOptions.SshHostKeyFingerprint = sshHostKeyFingerprint;
            }

            using (var session = new Session())
            {
                // Connect
                session.Open(sessionOptions);

                // Upload files
                var transferOptions = new TransferOptions();
                transferOptions.TransferMode = TransferMode.Binary;

                var transferResult = session.GetFiles(remoteDirectory, localDirectory, false, transferOptions);

                // Throw on any error
                transferResult.Check();

                int fileIdx = 0;
                foreach (TransferEventArgs transfer in transferResult.Transfers)
                {
                    fileIdx++;

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("File", transfer.FileName);

                    yield return outParameters;
                }

                this.LogDebugFormat("End reading files from Host='{0}', RemoteDirectory='{1}': FilesCount={2}", host, remoteDirectory, fileIdx);
            }
        }
    }
}