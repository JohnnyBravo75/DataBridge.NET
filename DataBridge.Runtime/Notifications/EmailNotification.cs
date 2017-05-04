using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Xml.Serialization;
using DataBridge.ConnectionInfos;

namespace DataBridge.Runtime
{
    [Serializable]
    public class EmailNotification : NotificationInfo
    {
        private string mailFrom = "";
        private string mailTo = "";
        private string mailCc = "";
        private string mailBcc = "";
        private string mailSubject = "";
        private string mailBody = "";
        private SmtpConnectionInfo connectionInfo = new SmtpConnectionInfo();

        public SmtpConnectionInfo ConnectionInfo
        {
            get { return this.connectionInfo; }
            set { this.connectionInfo = value; }
        }

        [XmlAttribute]
        public string From
        {
            get { return this.mailFrom; }
            set { this.mailFrom = value; }
        }

        [XmlAttribute]
        public string To
        {
            get
            {
                return this.mailTo;
            }
            set
            {
                this.mailTo = value;
            }
        }

        [XmlAttribute]
        public string Cc
        {
            get
            {
                return this.mailCc;
            }
            set
            {
                this.mailCc = value;
            }
        }

        [XmlAttribute]
        public string Bcc
        {
            get
            {
                return this.mailBcc;
            }
            set
            {
                this.mailBcc = value;
            }
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

        public override bool Notify(string pipelineName)
        {
            var extraTokens = new Dictionary<string, object>();
            extraTokens.Add("Status", DataBridgeInfo.MailLogStatus.Info.ToString());
            extraTokens.Add("Pipeline", pipelineName);

            var smtpConnectionInfo = this.ConnectionInfo as SmtpConnectionInfo;
            if (smtpConnectionInfo == null)
            {
                throw new ArgumentNullException("ConnectionInfo");
            }

            try
            {

                using (var message = new MailMessage())
                {
                    // From
                    message.From = new MailAddress(this.From, this.From, Encoding.UTF8);

                    // To
                    var to = this.To.Replace(";", ",");
                    foreach (string emailaddress in to.Split(','))
                    {
                        message.To.Add(new MailAddress(emailaddress, string.Empty, Encoding.UTF8));
                    }

                    // CC
                    if (!string.IsNullOrEmpty(this.Cc))
                    {
                        message.CC.Add(this.Cc.Replace(";", ","));
                    }

                    // BCC
                    if (!string.IsNullOrEmpty(this.Bcc))
                    {
                        message.Bcc.Add(this.Bcc.Replace(";", ","));
                    }
                    message.Subject = TokenManager.Instance.ReplaceTokens(this.Subject, extraTokens, pipelineName);
                    message.Body = TokenManager.Instance.ReplaceTokens(this.Body, extraTokens, pipelineName);

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
                            smtpClient.Credentials = new NetworkCredential(smtpConnectionInfo.UserName, smtpConnectionInfo.DecryptedPassword);
                        }

                        smtpClient.Send(message);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogError(this.GetType(), string.Format("Email notification failed SmptpServer='{0}', SmptpPort='{1}', Username='{2}'", smtpConnectionInfo.SmtpServer, smtpConnectionInfo.SmtpPort, smtpConnectionInfo.UserName), ex);
                return false;
            }
        }
    }
}