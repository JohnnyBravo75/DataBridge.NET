using DataBridge.Helper;

namespace DataBridge.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading;
    using System.Xml;
    using DataBridge.Extensions;

    /// <summary>
    /// a simple Class for a Webservice Request
    /// </summary>
    public class SimpleWebserviceRequest
    {
        // ************************************ Member**********************************************

        private const string SOAP11NAMESPACE = "http://schemas.xmlsoap.org/soap/envelope/";
        private const string SOAP12NAMESPACE = "http://www.w3.org/2003/05/soap-envelope";

        private string body = "";
        private string serviceUrl = "";
        private string soapAction;
        private string @namespace = "";
        private string namespacePrefix = "";
        private string methodName = "";
        private bool includeParameterType = false;
        private bool removeNamespaces = true;
        private bool? unescapeResult = null;
        private Encoding encoding = Encoding.GetEncoding("utf-8");
        private WebserviceParameterCollection parameters = new WebserviceParameterCollection();
        private string user = "";
        private string password = "";
        private string domain = "";

        // ************************************ Konstruktor**********************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleWebserviceRequest" /> class.
        /// </summary>
        /// <param name="serviceUrl">The service URL.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="namespace">The namespace.</param>
        /// <param name="namespacePrefix">The namespace prefix.</param>
        /// <param name="soapAction">The SOAP action.</param>
        /// <param name="encodingName">The encodingName.</param>
        /// <param name="removeNamespaces">if set to <c>true</c> [remove namespaces].</param>
        /// <param name="unescapeResult">if set to <c>true</c> [unescape result].</param>
        /// <param name="includeParameterType">if set to <c>true</c> [include parameter type].</param>
        public SimpleWebserviceRequest(string serviceUrl, string methodName, string @namespace = "", string namespacePrefix = "",
            string soapAction = null, string encodingName = "utf-8", bool removeNamespaces = true, bool? unescapeResult = null, bool includeParameterType = false)
        {
            this.serviceUrl = serviceUrl;
            this.methodName = methodName;
            this.soapAction = soapAction;
            this.@namespace = @namespace;
            this.namespacePrefix = namespacePrefix;
            if (string.IsNullOrEmpty(encodingName))
            {
                encodingName = "utf-8";
            }
            this.encoding = EncodingUtil.GetEncodingOrDefault(encodingName);
            this.removeNamespaces = removeNamespaces;
            this.unescapeResult = unescapeResult;
            this.includeParameterType = includeParameterType;
            //if (!string.IsNullOrEmpty(this.@namespace))
            //{
            //    this.namespacePrefix = "abc";
            //}
        }

        public SimpleWebserviceRequest()
        {
        }

        public SimpleWebserviceRequest(WebServiceInfo webServiceInfo, string methodName)
        {
            if (webServiceInfo == null)
            {
                throw new ArgumentNullException("webServiceInfo");
            }

            this.serviceUrl = webServiceInfo.ServiceUrls.First();
            var methodInfo = webServiceInfo.WebMethods.First(x => x.Name == methodName);
            this.methodName = methodInfo.Name;
            this.soapAction = methodInfo.Action;
            this.@namespace = methodInfo.TargetNamespace;
            this.namespacePrefix = "";
            this.removeNamespaces = false;
            this.unescapeResult = null;
            this.includeParameterType = false;

            if (methodInfo != null)
            {
                foreach (var inputParam in methodInfo.InputParameters)
                {
                    this.Parameters.Add(new WebserviceParameter()
                    {
                        Name = inputParam.Name,
                        Type = inputParam.Type,
                        Length = inputParam.Length
                    });
                }
            }
        }

        // ************************************Properties**********************************************

        public enum SoapVersions
        {
            Soap11,
            Soap12
        }

        public SoapVersions SoapVersion = SoapVersions.Soap11;

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        /// <value>
        /// The parameters.
        /// </value>
        public WebserviceParameterCollection Parameters
        {
            get
            {
                return this.parameters;
            }
        }

        /// <summary>
        /// Gets the SOAP action.
        /// </summary>
        /// <value>
        /// The SOAP action.
        /// </value>
        public string SoapAction
        {
            get
            {
                var action = "";
                if (string.IsNullOrEmpty(this.soapAction))
                {
                    // build default soap action
                    var ns = (string.IsNullOrEmpty(this.@namespace) ? "http://tempuri.org/" : this.@namespace);
                    if (!ns.EndsWith("/"))
                    {
                        ns += "/";
                    }

                    action = ns + this.methodName;
                }
                else
                {
                    action = this.soapAction;
                }

                return action;
            }
        }

        public string User
        {
            get { return this.user; }
            set { this.user = value; }
        }

        public string Password
        {
            get { return this.password; }
            set { this.password = value; }
        }

        public string Domain
        {
            get { return this.domain; }
            set { this.domain = value; }
        }

        private string SoapNamespace
        {
            get
            {
                if (this.SoapVersion == SoapVersions.Soap11)
                {
                    return SOAP11NAMESPACE;
                }
                else if (this.SoapVersion == SoapVersions.Soap12)
                {
                    return SOAP12NAMESPACE;
                }

                return "";
            }
        }

        public WebProxy Proxy { get; set; }

        public string RequestXml
        {
            get
            {
                return this.BuildRequestXml();
            }
        }

        public Encoding Encoding
        {
            get { return this.encoding; }
            set { this.encoding = value; }
        }

        // ************************************Funktionen**********************************************
        /// <summary>
        /// Invokes the Webservice request.
        /// </summary>
        /// <returns>the returned result as xmlDocument</returns>
        /// <exception cref="System.ArgumentException">is thrown, when a soapfault occurrs</exception>
        public XmlDocument Invoke()
        {
            var hasError = false;
            ServicePointManager.ServerCertificateValidationCallback =
                                                (sender, certificate, chain, sslPolicyErrors) =>
                                                {
                                                    //to check certificate
                                                    return true;
                                                };

            var requestXml = this.BuildRequestXml();
            Stream responseStream = null;
            HttpWebRequest request = null;
            XmlDocument responseDoc = null;

            try
            {
                request = this.CreateRequestWithHeader(requestXml);

                var requestBytes = this.encoding.GetBytes(requestXml);

                using (var stream = request.GetRequestStream())
                {
                    stream.Write(requestBytes, 0, requestBytes.Length);
                }

                using (var response = this.GetResponseWithRetry(request))
                {
                    using (responseStream = response.GetResponseStream())
                    {
                        responseDoc = this.ExtractResponseXml(responseStream, requestXml, hasError: false);
                    }
                }
            }
            catch (WebException ex)
            {
                if (ex.Response != null)
                {
                    using (responseStream = ex.Response.GetResponseStream())
                    {
                        responseDoc = this.ExtractResponseXml(responseStream, requestXml, hasError: true);
                    }

                    //var httpResponse = ex.Response as HttpWebResponse;
                    //if (httpResponse != null)
                    //{
                    //    throw new ApplicationException(string.Format(
                    //        "Remote server call {0} {1} resulted in a http error {2} {3}.",
                    //        request.Method,
                    //        this.serviceUrl,
                    //        httpResponse.StatusCode,
                    //        httpResponse.StatusDescription), ex);
                    //}
                }
                else
                {
                    throw;
                }
            }

            return responseDoc;
        }

        private HttpWebRequest CreateRequestWithHeader(string requestXml)
        {
            var request = (HttpWebRequest)WebRequest.Create(this.serviceUrl);

            request.Credentials = this.GetCredential(new Uri(this.serviceUrl));

            var myCookie = new CookieContainer();
            request.CookieContainer = myCookie;

            request.PreAuthenticate = true;
            if (this.Proxy != null)
            {
                request.Proxy = this.Proxy;
            }
            request.Headers.Add("SOAPAction", this.SoapAction);

            if (this.SoapVersion == SoapVersions.Soap11)
            {
                request.ContentType = "text/xml;charset=\"" + this.Encoding.WebName + "\"" + ";action=\"" + this.SoapAction + "\"";
            }
            else
            {
                request.ContentType = "application/soap+xml;charset=\"" + this.Encoding.WebName + "\"" + ";action=\"" + this.SoapAction + "\"";
            }

            var requestBytes = this.encoding.GetBytes(requestXml);
            request.ContentLength = requestBytes.Length;
            request.Method = "POST";

            this.SetBasicAuthHeader(request, this.User, this.Password);

            return request;
        }

        private HttpWebResponse GetResponseWithRetry(HttpWebRequest request, int maxRetries = 5, int milliSecondsRetryInterval = 5000)
        {
            var isDone = false;
            var attempts = 0;

            while (!isDone)
            {
                attempts++;

                try
                {
                    var response = (HttpWebResponse)request.GetResponse();
                    isDone = true;
                    return response;
                }
                catch (WebException ex)
                {
                    if (ex.Status != WebExceptionStatus.ReceiveFailure &&
                        ex.Status != WebExceptionStatus.ConnectFailure &&
                        ex.Status != WebExceptionStatus.KeepAliveFailure)
                    {
                        throw;
                    }

                    if (attempts >= maxRetries)
                    {
                        throw;
                    }

                    Thread.Sleep(milliSecondsRetryInterval);
                }
            }

            return null;
        }

        /// <summary>
        /// Extracts the response XML.
        /// </summary>
        /// <param name="responseStream">The response stream.</param>
        /// <param name="requestXml">The request XML.</param>
        /// <param name="hasError">if set to <c>true</c> [has error].</param>
        /// <returns></returns>
        /// <exception cref="System.ArgumentException"></exception>
        private XmlDocument ExtractResponseXml(Stream responseStream, string requestXml, bool hasError = false)
        {
            var responseXml = new XmlDocument();
            XmlDocument exctratedXml = null;
            if (responseStream != null && responseStream.CanRead)
            {
                //string str1 = StreamToString(responseStream);
                responseXml.Load(responseStream);

                exctratedXml = this.ExtractResponseXml(responseXml, requestXml, hasError);
            }
            else
            {
                // no/empty response
                throw new Exception(string.Format("Response from '{0}' is empty", this.serviceUrl));
            }

            return exctratedXml;
        }

        public XmlDocument ExtractResponseXml(XmlDocument xmlResponse, string requestXml, bool hasError = false)
        {
            var responseDoc = new XmlDocument();
            var responseNode = this.ExtractSoapResponse(xmlResponse);
            if (responseNode != null)
            {
                // When Soap Envelope detected...
                responseDoc.LoadXml(responseNode.OuterXml);

                if (hasError)
                {
                    // Error Handling
                    var errorMsg = this.ExtractErrorMessage(responseDoc, requestXml);
                    if (!string.IsNullOrEmpty(errorMsg))
                    {
                        throw new Exception(errorMsg);
                    }
                }
                else
                {
                    responseDoc = this.ExtractSoapResult(responseDoc);
                }
            }
            else
            {
                // no soap envelope/format, return the complete xml
                responseDoc = xmlResponse;
            }
            return responseDoc;
        }

        private bool IsReponseElement(XmlElement element)
        {
            if (element == null)
            {
                return false;
            }

            if (element.Name.EndsWith("Response"))
            {
                return true;
            }

            return false;
        }

        public XmlDocument ExtractSoapResult(XmlDocument responseDoc)
        {
            if (responseDoc == null)
            {
                return null;
            }

            if (this.removeNamespaces)
            {
                responseDoc = XmlHelper.RemoveXmlns(responseDoc);
            }

            //if (this.IsReponseElement(responseDoc.DocumentElement))
            //{
            //    responseDoc = responseDoc.DocumentElement.OwnerDocument;
            //}

            return responseDoc;
        }

        //public XmlDocument ExtractSoapResult(XmlDocument responseDoc)
        //{
        //    if (responseDoc == null)
        //    {
        //        return null;
        //    }

        //    if (this.removeNamespaces)
        //    {
        //        responseDoc = XmlHelper.RemoveXmlns(responseDoc);
        //    }

        //    XmlNode resultNode = null;
        //    string innerXml = "";

        //    if (responseDoc.DocumentElement.ChildNodes.Count == 1)
        //    {
        //        resultNode = responseDoc.DocumentElement.FirstChild;

        //        if (resultNode != null && this.IsXml(resultNode.InnerXml))
        //        {
        //            innerXml = resultNode.InnerXml;
        //        }
        //        else
        //        {
        //            innerXml = responseDoc.DocumentElement.InnerXml;
        //        }

        //        if (this.unescapeResult.HasValue && this.unescapeResult.Value == true)
        //        {
        //            innerXml = XmlHelper.UnescapeXml(innerXml);
        //        }
        //        else
        //        {
        //            // autodetect escaping
        //            if (resultNode != null && this.IsEscapedXml(resultNode.InnerXml) && !this.StartsWithXmlDeclaration(resultNode.InnerXml))
        //            {
        //                innerXml = XmlHelper.UnescapeXml(innerXml);
        //            }
        //        }
        //    }
        //    else
        //    {
        //        innerXml = responseDoc.DocumentElement.OuterXml;
        //    }
        //    try
        //    {
        //        responseDoc.LoadXml(innerXml);
        //    }
        //    catch
        //    { }

        //    return responseDoc;
        //}

        private string ExtractErrorMessage(XmlDocument responseDoc, string requestXml)
        {
            //string errorMsg = responseDoc.DocumentElement.SelectSingleNode("/soap:Fault/faultstring", nsMgr).InnerText;
            var errorMsg = responseDoc.DocumentElement.InnerText;
            if (!string.IsNullOrEmpty(errorMsg))
            {
                errorMsg += string.Format("{0}{1}Request:{2}{3}", Environment.NewLine, Environment.NewLine, Environment.NewLine, requestXml);
            }

            return errorMsg;
        }

        private XmlNode ExtractSoapResponse(XmlDocument xmlDoc)
        {
            XmlNode root = xmlDoc.DocumentElement;
            var nsMgr = new XmlNamespaceManager(xmlDoc.NameTable);
            nsMgr.AddNamespace("soap", this.SoapNamespace);

            var responseNode = root.SelectSingleNode("/soap:Envelope/soap:Body/child::node()", nsMgr);
            return responseNode;
        }

        private string BuildRequestXml()
        {
            this.body = this.BuildBody();
            var requestXml = this.BuildSoapEnvelope();
            return requestXml;
        }

        /// <summary>
        /// Builds the SOAP envelope.
        /// </summary>
        /// <returns></returns>
        private string BuildSoapEnvelope()
        {
            var namespacePrefix = this.namespacePrefix;
            if (!string.IsNullOrEmpty(namespacePrefix))
            {
                namespacePrefix = namespacePrefix + ':';
            }

            var namespacesuffix = this.namespacePrefix;
            if (!string.IsNullOrEmpty(namespacesuffix))
            {
                namespacesuffix = ':' + namespacesuffix;
            }

            var soapEnvelope = "<?xml version=\"1.0\" encoding=\"" + this.Encoding.WebName + "\"?>";
            soapEnvelope += "<soap:Envelope xmlns:soap=\"" + this.SoapNamespace + "\" xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\"" + (!string.IsNullOrEmpty(namespacePrefix) ? (" xmlns" + namespacesuffix + "=\"" + this.@namespace + "\"") : "") + ">";
            soapEnvelope += "<soap:Header/>";
            soapEnvelope += "<soap:Body>";
            soapEnvelope += "<" + namespacePrefix + this.methodName + (string.IsNullOrEmpty(namespacePrefix) ? (" xmlns" + namespacesuffix + "=\"" + this.@namespace + "\"") : "") + ">";
            soapEnvelope += this.body;
            soapEnvelope += "</" + namespacePrefix + this.methodName + ">";
            soapEnvelope += "</soap:Body>";
            soapEnvelope += "</soap:Envelope>";
            return soapEnvelope;
        }

        /// <summary>
        /// Builds the body.
        /// </summary>
        /// <returns></returns>
        private string BuildBody()
        {
            var body = "";
            foreach (var parameter in this.Parameters)
            {
                body += this.BuildParameterString(parameter);
            }

            return body;
        }

        /// <summary>
        /// Builds the parameter string.
        /// </summary>
        /// <param name="parameter">The parameter.</param>
        /// <returns></returns>
        private string BuildParameterString(WebserviceParameter parameter)
        {
            var namespacePrefix = this.namespacePrefix;
            if (!string.IsNullOrEmpty(namespacePrefix))
            {
                namespacePrefix = namespacePrefix + ':';
            }

            var parameterValue = "";
            switch (parameter.Type)
            {
                case "xml":
                    parameterValue = "<![CDATA[" + parameter.Value.ToStringOrEmpty() + "]]>";
                    break;

                default:
                    if (parameter.Value is IDictionary<string, object>)
                    {
                        // complex type
                        var complexType = parameter.Value as IDictionary<string, object>;
                        foreach (var property in complexType)
                        {
                            var complexParameter = new WebserviceParameter()
                            {
                                Name = property.Key,
                                Value = property.Value,
                                Type = property.Value.GetType().Name
                            };
                            parameterValue += this.BuildParameterString(complexParameter) + Environment.NewLine;
                        }
                    }
                    else
                    {
                        // primitive type
                        parameterValue = XmlHelper.EscapeXml(parameter.Value.ToStringOrEmpty());
                    }
                    break;
            }

            if (!string.IsNullOrEmpty(parameter.Type))
            {
                parameter.Type = " xsi:type=\"" + parameter.Type + "\"";
            }

            return "<" + namespacePrefix + parameter.Name + (this.includeParameterType ? parameter.Type : "") + ">" + parameterValue + "</" + namespacePrefix + parameter.Name + ">";
        }

        private CredentialCache GetCredential(Uri uri)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            var credentialCache = new CredentialCache();
            credentialCache.Add(uri, "NTLM", new NetworkCredential(this.User, this.Password, this.Domain));
            return credentialCache;
        }

        private void SetBasicAuthHeader(WebRequest request, String userName, String userPassword)
        {
            var authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            request.Headers["Authorization"] = "Basic " + authInfo;
        }

        private bool IsXml(string xml)
        {
            var isXml = false;

            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            xml = xml.Trim();
            if (!xml.StartsWith("<") && !xml.EndsWith(">"))
            {
                return false;
            }

            var xmlDoc = new XmlDocument();
            try
            {
                xmlDoc.LoadXml(xml);
                isXml = true;
            }
            catch
            { }

            return isXml;
        }

        private bool IsEscapedXml(string xml)
        {
            var regex = new Regex("&lt;.*&gt;", RegexOptions.IgnoreCase);
            var match = regex.Match(xml);
            if (match.Success && match.Groups.Count > 0 && xml.StartsWith("&lt;"))
            {
                return true;
            }

            return false;
        }

        private bool StartsWithXmlDeclaration(string xml)
        {
            if (string.IsNullOrEmpty(xml))
            {
                return false;
            }

            if (xml.StartsWith("&lt;?xml version") || xml.StartsWith("<?xml version"))
            {
                return true;
            }

            return false;
        }

        public static string StreamToString(Stream stream)
        {
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }

            using (var reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static Stream StringToStream(string src)
        {
            var byteArray = Encoding.UTF8.GetBytes(src);
            return new MemoryStream(byteArray);
        }

        public class WebserviceParameterCollection : List<WebserviceParameter>
        {
            public object this[string name]
            {
                get
                {
                    // Get accessor implementation.
                    var param = this.FirstOrDefault(x => x.Name == name);
                    if (param != null)
                    {
                        return param.Value;
                    }

                    return null;
                }
                set
                {
                    // Set accessor implementation.
                    var param = this.First(x => x.Name == name);
                    if (param != null)
                    {
                        param.Value = value;
                    }
                }
            }
        }

        /// <summary>
        /// a Parameter for the Parameters of the SimpleWebserviceRequest
        /// </summary>
        public class WebserviceParameter
        {
            public string Name { get; set; }

            public string Type { get; set; }

            public object Value { get; set; }

            public int? Length { get; set; }

            public override string ToString()
            {
                return !string.IsNullOrEmpty(this.Name)
                           ? this.Name + " = " + this.Value.ToStringOrEmpty()
                           : base.ToString();
            }
        }
    }
}