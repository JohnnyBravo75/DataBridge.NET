using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using AppLimit.CloudComputing.SharpBox;
using AppLimit.CloudComputing.SharpBox.StorageProvider;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "WebDavFileUploader", Title = "WebDavFileUploader", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class WebDavFileUploader : DataCommand
    {
        private CloudStorage cloudStorage = new CloudStorage();

        public WebDavFileUploader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Url", Direction = Directions.InOut, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "User", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "RemoteDirectory", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "File" });
        }

        [XmlIgnore]
        [System.ComponentModel.Editor(typeof(Wpf.Toolkit.PropertyEditors.OpenFilePropertyEditor), typeof(Wpf.Toolkit.PropertyEditors.OpenFilePropertyEditor))]
        public string File
        {
            get { return this.Parameters.GetValue<string>("File"); }
            set { this.Parameters.SetOrAddValue("File", value); }
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

        public override void Initialize()
        {
            base.Initialize();
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string file = inParameters.GetValue<string>("File");
                string url = inParameters.GetValue<string>("Url");
                string passWord = inParameters.GetValue<string>("Password");
                string user = inParameters.GetValue<string>("User");
                string remoteDirectory = inParameters.GetValueOrDefault<string>("RemoteDirectory", "/");

                var cloudConfig = CloudStorage.GetCloudConfigurationEasy(nSupportedCloudConfigurations.WebDav,
                    new Uri(url));
                var cloudCredentials = new GenericNetworkCredentials() { UserName = user, Password = passWord };

                this.cloudStorage.Open(cloudConfig, cloudCredentials);

                this.WriteData(remoteDirectory, file);

                var outParameters = this.GetCurrentOutParameters();
                outParameters.SetOrAddValue("File", file);
                yield return outParameters;

                this.cloudStorage.Close();
            }
        }

        private void WriteData(string remoteDirectory, string localFileName)
        {
            this.cloudStorage.UploadFile(localFileName, remoteDirectory);
        }
    }
}