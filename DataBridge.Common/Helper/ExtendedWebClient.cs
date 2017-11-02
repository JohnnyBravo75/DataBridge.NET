using System;
using System.Net;

namespace DataBridge.Helper
{
    public class ExtendedWebClient : WebClient
    {
        private CookieContainer cookieContainer = new CookieContainer();
        private string userAgent = "";
        private int timeout = 15000;

        public ExtendedWebClient()
        {
            this.UserAgent = @"CodeGator Crawler v1.0";
        }

        public CookieContainer CookieContainer
        {
            get { return this.cookieContainer; }
            set { this.cookieContainer = value; }
        }

        public string UserAgent
        {
            get { return this.userAgent; }
            set
            {
                this.userAgent = value;

                //if (!string.IsNullOrEmpty(this.userAgent))
                //{
                //    this.Headers.Add("user-agent", this.userAgent);
                //}
            }
        }

        public int Timeout
        {
            get { return this.timeout; }
            set { this.timeout = value; }
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            WebRequest request = base.GetWebRequest(address);

            if (request != null && request.GetType() == typeof(HttpWebRequest))
            {
                ((HttpWebRequest)request).CookieContainer = this.CookieContainer;
                ((HttpWebRequest)request).UserAgent = this.userAgent;
                ((HttpWebRequest)request).Timeout = this.timeout;
            }

            return request;
        }
    }
}