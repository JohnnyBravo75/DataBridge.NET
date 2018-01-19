using System.Collections.Generic;
using System.Xml.Serialization;
using DataBridge.Common.Helper;
using DataBridge.Extensions;
using OpenPop.Mime;
using OpenPop.Mime.Header;
using OpenPop.Pop3;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "EmailDownloader", Title = "EmailDownloader", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png", CustomControlName = "DataImportAdapterControl")]
    public class EmailDownloader : DataCommand
    {
        private Pop3Client pop3Client = new Pop3Client();

        public EmailDownloader()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Host", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "User", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "Port", Direction = Directions.In, DataType = DataTypes.Number });
            this.Parameters.Add(new CommandParameter() { Name = "EnableSecure", Direction = Directions.In, DataType = DataTypes.Boolean, Value = false });
            this.Parameters.Add(new CommandParameter() { Name = "File", Direction = Directions.InOut });
        }

        [XmlIgnore]
        public string Host
        {
            get { return this.Parameters.GetValue<string>("Host"); }
            set { this.Parameters.SetOrAddValue("Host", value); }
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

        [XmlIgnore]
        public int Port
        {
            get { return this.Parameters.GetValue<int>("Port"); }
            set { this.Parameters.SetOrAddValue("Port", value); }
        }

        [XmlIgnore]
        public bool EnableSecure
        {
            get { return this.Parameters.GetValue<bool>("EnableSecure"); }
            set { this.Parameters.SetOrAddValue("EnableSecure", value); }
        }

        private Conditions filterConditions = new Conditions();

        public Conditions FilterConditions
        {
            get { return this.filterConditions; }
            set { this.filterConditions = value; }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string host = inParameters.GetValue<string>("Host");
                string user = inParameters.GetValue<string>("User");
                string passWord = inParameters.GetValue<string>("Password");
                int? port = inParameters.GetValue<int?>("Port");
                bool enableSecure = inParameters.GetValue<bool>("EnableSecure");

                if (!port.HasValue)
                {
                    if (enableSecure)
                    {
                        port = 995;
                    }
                    else
                    {
                        port = 110;
                    }
                }

                this.pop3Client.Connect(host, port.Value, enableSecure);
                this.pop3Client.Authenticate(user, passWord, AuthenticationMethod.UsernameAndPassword);

                this.LogDebugFormat("Start reading emails from Host='{0}', User='{1}'", host, user);

                int count = this.pop3Client.GetMessageCount();

                this.LogDebugFormat("Found {0} emails", count);

                int emailIdx = 0;
                int emailDownloaded = 0;

                for (int i = count; i >= 1; i--)
                {
                    MessageHeader pop3MessageHeader = this.pop3Client.GetMessageHeaders(i);
                    emailIdx++;

                    this.ExecuteParameters.SetOrAddValue("EmailFrom", pop3MessageHeader.From.Address);
                    this.ExecuteParameters.SetOrAddValue("EmailTo", string.Join(",", pop3MessageHeader.To));
                    this.ExecuteParameters.SetOrAddValue("EmailSubject", pop3MessageHeader.Subject);
                    this.ExecuteParameters.SetOrAddValue("EmailDate", pop3MessageHeader.DateSent.ToString());

                    if (this.FilterConditions.IsNullOrEmpty() ||
                        ConditionEvaluator.CheckMatchingConditions(this.FilterConditions,
                            this.ExecuteParameters.ToDictionary()))
                    {
                        Message pop3Message = this.pop3Client.GetMessage(i);
                        emailDownloaded++;

                        {
                            // Body
                            string bodyText = "";
                            string bodyFileName = !string.IsNullOrEmpty(pop3Message.Headers.Subject)
                                ? pop3Message.Headers.Subject
                                : pop3Message.Headers.DateSent.ToStringOrEmpty();

                            var body = pop3Message.FindFirstHtmlVersion();
                            if (body != null)
                            {
                                bodyText = body.GetBodyAsText();
                                bodyFileName += ".html";
                            }
                            else
                            {
                                body = pop3Message.FindFirstPlainTextVersion();
                                if (body != null)
                                {
                                    bodyText = body.GetBodyAsText();
                                    bodyFileName += ".txt";
                                }
                            }

                            var outParameters = this.GetCurrentOutParameters();
                            outParameters.SetOrAddValue("Data", bodyText);
                            outParameters.SetOrAddValue("File", FileUtil.SanitizeFileName(bodyFileName));
                            yield return outParameters;
                        }

                        // Attachments
                        var attachments = pop3Message.FindAllAttachments();

                        foreach (var attachment in attachments)
                        {
                            var outParameters = this.GetCurrentOutParameters();
                            outParameters.SetOrAddValue("Data", attachment.Body);
                            outParameters.SetOrAddValue("File", FileUtil.SanitizeFileName(attachment.FileName));
                            yield return outParameters;
                        }
                    }
                }

                this.LogDebugFormat(
                    "End reading emails from Host='{0}', User='{1}': EmailChecked={2}, EmailDownloaded={3}", host, user,
                    emailIdx, emailDownloaded);
            }
        }
    }
}