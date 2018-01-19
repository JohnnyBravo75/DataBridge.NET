using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml.Serialization;
using DataBridge.ConnectionInfos;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "EmailSender", Title = "EmailSender", Group = "Connectors", Image = "\\Resources\\Images\\ExportConnector.png", CustomControlName = "DataExportAdapterControl")]
    public class EmailSender : DataCommand
    {
        private List<string> attachments = new List<string>();
        private string mailSubject = "";
        private string mailBody = "";
        private SmtpConnectionInfo connectionInfo = new SmtpConnectionInfo();

        public EmailSender()
        {
            this.Parameters.Add(new CommandParameter() { Name = "From", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "To", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "CC", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "BCC", Direction = Directions.In });
        }

        [ExpandableObject]
        public SmtpConnectionInfo ConnectionInfo
        {
            get { return this.connectionInfo; }
            set { this.connectionInfo = value; }
        }

        [XmlIgnore]
        public string From
        {
            get { return this.Parameters.GetValue<string>("From"); }
            set { this.Parameters.SetOrAddValue("From", value); }
        }

        [XmlIgnore]
        public string To
        {
            get { return this.Parameters.GetValue<string>("To"); }
            set { this.Parameters.SetOrAddValue("To", value); }
        }

        [XmlIgnore]
        public string CC
        {
            get { return this.Parameters.GetValue<string>("CC"); }
            set { this.Parameters.SetOrAddValue("CC", value); }
        }

        [XmlIgnore]
        public string BCC
        {
            get { return this.Parameters.GetValue<string>("BCC"); }
            set { this.Parameters.SetOrAddValue("BCC", value); }
        }

        [XmlElement]
        public string Subject
        {
            get { return this.mailSubject; }
            set { this.mailSubject = value; }
        }

        [XmlElement]
        public string Body
        {
            get { return this.mailBody; }
            set { this.mailBody = value; }
        }

        public List<string> Attachments
        {
            get { return this.attachments; }
            set { this.attachments = value; }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string mailFrom = inParameters.GetValue<string>("From");
                string mailTo = inParameters.GetValue<string>("To");
                string mailCc = inParameters.GetValue<string>("CC");
                string mailBcc = inParameters.GetValue<string>("BCC");
                //object data = inParameters.GetValue<object>("Data");

                var smtpConnectionInfo = this.ConnectionInfo as SmtpConnectionInfo;
                if (smtpConnectionInfo == null)
                {
                    throw new ArgumentNullException("ConnectionInfo");
                }

                using (var message = new MailMessage())
                {
                    // From
                    message.From = new MailAddress(mailFrom, mailFrom, Encoding.UTF8);

                    // To
                    var to = mailTo.Replace(";", ",");
                    foreach (string emailaddress in to.Split(','))
                    {
                        message.To.Add(new MailAddress(emailaddress, string.Empty, Encoding.UTF8));
                    }

                    message.Subject = TokenProcessor.ReplaceTokens(this.Subject, inParameters.ToDictionary());
                    message.Body = TokenProcessor.ReplaceTokens(this.Body, inParameters.ToDictionary());

                    // Attachments
                    foreach (var attachment in this.Attachments)
                    {
                        string fileName = TokenProcessor.ReplaceTokens(attachment, inParameters.ToDictionary());
                        message.Attachments.Add(new Attachment(fileName));
                    }

                    using (var smtpClient = new SmtpClient())
                    {
                        smtpClient.Host = smtpConnectionInfo.SmtpServer;
                        smtpClient.Port = smtpConnectionInfo.SmtpPort;
                        if (smtpConnectionInfo.EnableSecure)
                        {
                            smtpClient.EnableSsl = smtpConnectionInfo.EnableSecure;
                        }

                        if (!string.IsNullOrEmpty(smtpConnectionInfo.UserName))
                        {
                            smtpClient.UseDefaultCredentials = false;
                            smtpClient.Credentials = new NetworkCredential(smtpConnectionInfo.UserName,
                                smtpConnectionInfo.DecryptedPassword);
                        }

                        smtpClient.Send(message);
                    }
                }

                var outParameters = this.GetCurrentOutParameters();
                yield return outParameters;
            }
        }
    }
}