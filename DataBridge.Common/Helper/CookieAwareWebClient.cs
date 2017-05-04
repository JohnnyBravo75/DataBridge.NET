using System;
using System.Net;

namespace DataBridge.Helper
{
    public class CookieAwareWebClient : WebClient
    {
        public CookieAwareWebClient()
        {
            this.CookieContainer = new CookieContainer();
        }

        public CookieContainer CookieContainer { get; private set; }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var request = (HttpWebRequest)base.GetWebRequest(address);
            if (request != null)
            {
                request.CookieContainer = this.CookieContainer;
            }

            return request;
        }
    }
}