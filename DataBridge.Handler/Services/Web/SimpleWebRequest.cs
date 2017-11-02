namespace DataBridge.Services
{
    using System;
    using System.Collections.Specialized;
    using System.IO;
    using System.Net;
    using System.Text;
    using System.Web;
    using DataBridge.Helper;
    using DataBridge.Extensions;

    public class SimpleWebRequest : IDisposable
    {
        private ExtendedWebClient webClient = new ExtendedWebClient();
        private NetworkCredential credentials;
        private RequestModes requestMode = RequestModes.GET;
        private NameValueCollection parameters = new NameValueCollection();
        private string url = "";
        private Encoding requestEncoding = Encoding.Default;

        public WebClient WebClient
        {
            get
            {
                return this.webClient;
            }
        }

        public NetworkCredential Credentials
        {
            get
            {
                return this.credentials;
            }
            set
            {
                this.credentials = value;
            }
        }

        private string ToQueryString(NameValueCollection parameters)
        {
            if (parameters == null)
            {
                return string.Empty;
            }

            var queryString = new StringBuilder("?");

            bool first = true;

            foreach (string key in parameters.AllKeys)
            {
                foreach (string value in parameters.GetValues(key))
                {
                    if (!first)
                    {
                        queryString.Append("&");
                    }

                    queryString.AppendFormat("{0}={1}", HttpUtility.UrlEncode(key), HttpUtility.UrlEncode(value));

                    first = false;
                }
            }

            return queryString.ToString();
        }

        private string ReplaceInUrl(string url, NameValueCollection parameters)
        {
            if (string.IsNullOrEmpty(url))
            {
                return url;
            }

            if (parameters != null)
            {
                foreach (var key in parameters.AllKeys)
                {
                    url = url.Replace(string.Format("{{{0}}}", key), string.Join(" ", parameters.GetValues(key)));
                }
            }

            return url;
        }

        public void Dispose()
        {
            if (this.webClient != null)
            {
                this.webClient.Dispose();
                this.webClient = null;
            }
        }

        public NameValueCollection Parameters
        {
            get { return this.parameters; }
            set { this.parameters = value; }
        }

        public RequestModes RequestMode
        {
            get { return this.requestMode; }
            set { this.requestMode = value; }
        }

        public Encoding RequestEncoding
        {
            get { return this.requestEncoding; }
            set { this.requestEncoding = value; }
        }

        public string Url
        {
            get { return this.url; }
            set { this.url = value; }
        }

        public enum RequestModes
        {
            GET,
            POST
        }

        public bool ReplaceQueryParameters { get; set; }

        public object Invoke()
        {
            return this.Invoke(this.Url, this.RequestMode, this.Parameters, this.ReplaceQueryParameters);
        }

        private object Invoke(string url, RequestModes requestMode, NameValueCollection requestParams, bool replaceQueryParameters = false)
        {
            object result = null;

            // Authentication
            if (this.credentials != null)
            {
                this.webClient.SetCredentials(this.credentials);
            }

            // build url
            if (replaceQueryParameters)
            {
                url = this.ReplaceInUrl(url, requestParams);
                url = Uri.EscapeUriString(url);
            }
            else
            {
                url += this.ToQueryString(requestParams);
            }

            var address = new Uri(url);

            // send
            switch (requestMode)
            {
                case RequestModes.GET:
                    using (Stream responseStream = this.webClient.OpenRead(address))
                    {
                        if (responseStream != null)
                        {
                            using (var reader = new StreamReader(responseStream, this.RequestEncoding))
                            {
                                result = reader.ReadToEnd();
                            }
                        }
                    }

                    break;

                case RequestModes.POST:
                    byte[] responsebytes = this.webClient.UploadValues(address, "POST", requestParams);
                    result = Encoding.UTF8.GetString(responsebytes);

                    break;
            }

            return result;
        }
    }
}