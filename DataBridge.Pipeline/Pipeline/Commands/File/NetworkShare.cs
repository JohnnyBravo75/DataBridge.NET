using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Services;

namespace DataBridge.Commands
{
    public class NetworkShare : DataCommand
    {
        public NetworkShare()
        {
            this.Parameters.Add(new CommandParameter() { Name = "UNCPath" });
            this.Parameters.Add(new CommandParameter() { Name = "DriveLetter" });
            this.Parameters.Add(new CommandParameter() { Name = "UserName", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Mode", Value = WindowsNetwork.ConnectionModes.Connect, Direction = Directions.In });
        }

        [XmlIgnore]
        public string UNCPath
        {
            get { return this.Parameters.GetValue<string>("UNCPath"); }
            set { this.Parameters.SetOrAddValue("UNCPath", value); }
        }

        [XmlIgnore]
        public string DriveLetter
        {
            get { return this.Parameters.GetValue<string>("DriveLetter"); }
            set { this.Parameters.SetOrAddValue("DriveLetter", value); }
        }

        [XmlIgnore]
        public string UserName
        {
            get { return this.Parameters.GetValue<string>("UserName"); }
            set { this.Parameters.SetOrAddValue("UserName", value); }
        }

        [XmlIgnore]
        public string Password
        {
            get { return this.Parameters.GetValue<string>("Password"); }
            set { this.Parameters.SetOrAddValue("Password", value); }
        }

        [XmlIgnore]
        public WindowsNetwork.ConnectionModes Mode
        {
            get { return this.Parameters.GetValue<WindowsNetwork.ConnectionModes>("Mode"); }
            set { this.Parameters.SetOrAddValue("Mode", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            //inParameters = GetCurrentInParameters();
            string uncPath = inParameters.GetValue<string>("UNCPath");
            string driveLetter = inParameters.GetValue<string>("DriveLetter");
            string userName = inParameters.GetValue<string>("UserName");
            string password = inParameters.GetValue<string>("Password");
            var mode = inParameters.GetValue<WindowsNetwork.ConnectionModes>("Mode");

            string errorMessage = string.Empty;

            switch (mode)
            {
                case WindowsNetwork.ConnectionModes.Connect:
                    errorMessage = WindowsNetwork.ConnectRemote(uncPath, userName, password, driveLetter);
                    break;

                case WindowsNetwork.ConnectionModes.Disconnect:
                    errorMessage = WindowsNetwork.DisconnectRemote(uncPath);
                    break;
            }

            if (!string.IsNullOrEmpty(errorMessage))
            {
                throw new Exception(errorMessage);
            }
            var outParameters = this.GetCurrentOutParameters();
            outParameters.AddOrUpdate(new CommandParameter() { Name = "Drive", Value = driveLetter });
            yield return outParameters;
        }
    }
}