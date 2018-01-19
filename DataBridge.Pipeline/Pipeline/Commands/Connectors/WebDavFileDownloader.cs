using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider;
using DataBridge.Helper;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "WebDavFileDownloader", Title = "WebDavFileDownloader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class WebDavFileDownloader : DataCommand
    {
        private CloudStorage cloudStorage = new CloudStorage();

        public WebDavFileDownloader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Url", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "User", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "RemoteDirectory", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "LocalDirectory", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "File" });
        }

        [XmlIgnore]
        public string Url
        {
            get { return this.Parameters.GetValue<string>("Url"); }
            set { this.Parameters.SetOrAddValue("Url", value); }
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

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string url = inParameters.GetValue<string>("Url");
                string user = inParameters.GetValue<string>("User");
                string passWord = inParameters.GetValue<string>("Password");
                string remoteDirectory = inParameters.GetValueOrDefault<string>("RemoteDirectory", "/");
                string localDirectory = inParameters.GetValue<string>("LocalDirectory");

                var cloudConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.WebDav, new Uri(url));
                var cloudCredentials = new GenericNetworkCredentials() { UserName = user, Password = passWord };

                this.cloudStorage.Open(cloudConfig, cloudCredentials);

                this.LogDebugFormat("Start reading files from Url='{0}', RemoteDirectory='{1}'", url, remoteDirectory);

                int fileIdx = 0;
                foreach (var localFileName in this.ReadData(remoteDirectory, localDirectory))
                {
                    fileIdx++;

                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("File", localFileName);

                    yield return outParameters;
                }

                this.LogDebugFormat("End reading files from Url='{0}', RemoteDirectory='{1}': FilesCount={2}", url,
                    remoteDirectory, fileIdx);

                this.cloudStorage.Close();
            }
        }

        private IEnumerable<string> ReadData(string remoteDirectory, string localDirectory)
        {
            DirectoryUtil.CreateDirectoryIfNotExists(localDirectory);

            var directory = this.cloudStorage.GetFolder(remoteDirectory);
            foreach (var entry in directory.ToList())
            {
                if (!(entry is ICloudDirectoryEntry))
                {
                    string remoteFileName = (entry as ICloudFileSystemEntry).Name;
                    string localFileName = Path.Combine(localDirectory, remoteFileName);
                    this.cloudStorage.DownloadFile(directory, remoteFileName, localDirectory);

                    yield return localFileName;
                }
            }
        }
    }
}