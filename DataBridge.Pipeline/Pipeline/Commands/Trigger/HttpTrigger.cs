using System.Data;
using System.Net;
using System.Web;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataBridge.Services;

namespace DataBridge.Commands
{
    public class HttpTrigger : DataCommand
    {
        private HttpServer watcher;
        private bool isFirstWatch = true;
        private bool startInitial = false;

        public HttpTrigger()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Port", Direction = Directions.In, Value = 80 });
            this.Parameters.Add(new CommandParameter() { Name = "User", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "UseAuthentication", Direction = Directions.In, Value = false });
        }

        [XmlIgnore]
        public int Port
        {
            get { return this.Parameters.GetValue<int>("Port"); }
            set
            {
                this.Parameters.SetOrAddValue("Port", value);

                if (this.watcher != null && this.watcher.IsListening)
                {
                    // restart on changed port
                    this.watcher.Stop();
                    this.watcher.Port = value;
                    this.watcher.Start();

                    this.LogDebugFormat("Listening for Http-Request on Port '{0}'...", this.watcher.Port);
                }
            }
        }


        [XmlIgnore]
        public bool UseAuthentication
        {
            get { return this.Parameters.GetValueOrDefault<bool>("UseAuthentication", false); }
            set { this.Parameters.SetOrAddValue("UseAuthentication", value); }
        }

        [XmlIgnore]
        public string User
        {
            get { return this.Parameters.GetValue<string>("User"); }
            set { this.Parameters.SetOrAddValue("User", value); }
        }

        [XmlIgnore]
        public string Password
        {
            get { return this.Parameters.GetValue<string>("Password"); }
            set { this.Parameters.SetOrAddValue("Password", value); }
        }

        public override void Initialize()
        {
            base.Initialize();

            this.watcher = new HttpServer();
            this.watcher.ElevateRights = true;
            this.watcher.RequestHandlerAction = this.RequestHandler;
            this.watcher.Port = this.Port;
            this.watcher.Start();

            this.LogDebugFormat("Listening for http request on Port '{0}'...", this.watcher.Port);
        }

        public override void DeInitialize()
        {
            if (this.watcher != null)
            {
                this.watcher.Stop();
                this.watcher.RequestHandlerAction = null;
                this.watcher = null;
            }

            base.DeInitialize();
        }

        public override bool BeforeExecute()
        {
            if (this.isFirstWatch && this.startInitial || !this.isFirstWatch)
            {
                this.LogDebugFormat("Triggered on Port '{0}'", this.Port);
                //watcher.EnableRaisingEvents = false;
                return this.OnSignalNext.Invoke(new InitializationResult(this, this.LoopCounter));
            }

            return true;
        }

        private void RequestHandler(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            this.SetCurrentThreadName();

            if (this.UseAuthentication)
            {
                if (!this.watcher.IsAuthenticated(context, this.User, this.Password))
                {
                    this.watcher.WriteToReponse(response, "<Error>Authentication failed</Error>");
                    return;
                }

            }

            // add url parameters to the pipeline
            var requestParams = HttpUtility.ParseQueryString(request.Url.Query);
            foreach (var key in requestParams.AllKeys)
            {
                this.ExecuteParameters.AddOrUpdate(new CommandParameter() { Name = key, Value = requestParams[key] });
            }

            // add datatable as payload
            var table = new DataTable();
            table.AddRow(requestParams.ToDictionary(), checkForMissingColumns: true);
            this.ExecuteParameters.AddOrUpdate(new CommandParameter() { Name = "Data", Value = table });

            this.watcher.WriteToReponse(response, "<Message>Triggered successfull</Message>");

            this.isFirstWatch = false;
            this.BeforeExecute();
        }



        public override bool AfterExecute()
        {
            // this.watcher.EnableRaisingEvents = true;
            return base.AfterExecute();
        }

        public override void Dispose()
        {
            this.DeInitialize();
        }
    }
}