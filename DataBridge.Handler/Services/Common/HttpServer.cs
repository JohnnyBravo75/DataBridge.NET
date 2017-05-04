using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;

namespace DataBridge.Services
{
    public class HttpServer
    {
        private static IDictionary<string, string> mimeTypeMappings = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase) {

        #region extension to MIME type list

        {".asf", "video/x-ms-asf"},
        {".asx", "video/x-ms-asf"},
        {".avi", "video/x-msvideo"},
        {".bin", "application/octet-stream"},
        {".cco", "application/x-cocoa"},
        {".crt", "application/x-x509-ca-cert"},
        {".css", "text/css"},
        {".deb", "application/octet-stream"},
        {".der", "application/x-x509-ca-cert"},
        {".dll", "application/octet-stream"},
        {".dmg", "application/octet-stream"},
        {".ear", "application/java-archive"},
        {".eot", "application/octet-stream"},
        {".exe", "application/octet-stream"},
        {".flv", "video/x-flv"},
        {".gif", "image/gif"},
        {".hqx", "application/mac-binhex40"},
        {".htc", "text/x-component"},
        {".htm", "text/html"},
        {".html", "text/html"},
        {".ico", "image/x-icon"},
        {".img", "application/octet-stream"},
        {".iso", "application/octet-stream"},
        {".jar", "application/java-archive"},
        {".jardiff", "application/x-java-archive-diff"},
        {".jng", "image/x-jng"},
        {".jnlp", "application/x-java-jnlp-file"},
        {".jpeg", "image/jpeg"},
        {".jpg", "image/jpeg"},
        {".js", "application/x-javascript"},
        {".mml", "text/mathml"},
        {".mng", "video/x-mng"},
        {".mov", "video/quicktime"},
        {".mp3", "audio/mpeg"},
        {".mpeg", "video/mpeg"},
        {".mpg", "video/mpeg"},
        {".msi", "application/octet-stream"},
        {".msm", "application/octet-stream"},
        {".msp", "application/octet-stream"},
        {".pdb", "application/x-pilot"},
        {".pdf", "application/pdf"},
        {".pem", "application/x-x509-ca-cert"},
        {".pl", "application/x-perl"},
        {".pm", "application/x-perl"},
        {".png", "image/png"},
        {".prc", "application/x-pilot"},
        {".ra", "audio/x-realaudio"},
        {".rar", "application/x-rar-compressed"},
        {".rpm", "application/x-redhat-package-manager"},
        {".rss", "text/xml"},
        {".run", "application/x-makeself"},
        {".sea", "application/x-sea"},
        {".shtml", "text/html"},
        {".sit", "application/x-stuffit"},
        {".swf", "application/x-shockwave-flash"},
        {".tcl", "application/x-tcl"},
        {".tk", "application/x-tcl"},
        {".txt", "text/plain"},
        {".war", "application/java-archive"},
        {".wbmp", "image/vnd.wap.wbmp"},
        {".wmv", "video/x-ms-wmv"},
        {".xml", "text/xml"},
        {".xpi", "application/x-xpinstall"},
        {".zip", "application/zip"},
        #endregion extension to MIME type list
        };

        private HttpListener listener;

        private string rootPath = "";

        private Thread serverThread;

        private int port = 80;

        private Action<HttpListenerContext> requestHandlerAction;

        public int Port
        {
            get
            {
                return this.port;
            }
            set
            {
                this.port = value;
            }
        }

        public HttpServer(int? port = null, string path = "")
        {
            if (!port.HasValue)
            {
                port = this.GetFreePort();
            }

            if (string.IsNullOrEmpty(path))
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                if (entryAssembly != null)
                {
                    path = Path.GetDirectoryName(entryAssembly.Location);
                }
            }
            if (string.IsNullOrEmpty(path))
            {
                var entryAssembly = new StackTrace().GetFrames().Last().GetMethod().Module.Assembly;
                if (entryAssembly != null)
                {
                    path = Path.GetDirectoryName(entryAssembly.Location);
                }
            }

            this.Initialize(port.Value, path);
        }

        private void Initialize(int port, string path)
        {

            this.requestHandlerAction = this.RequestHandler;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                this.rootPath = path;
            }

            this.port = port;
        }

        private int GetFreePort()
        {
            //get an empty port
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int freePort = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();

            return freePort;
        }

        public bool ElevateRights { get; set; }

        public void Start()
        {
            this.serverThread = new Thread(this.Listen);
            this.serverThread.Start();
        }

        public bool IsListening
        {
            get
            {
                if (this.listener == null)
                {
                    return false;
                }

                return this.listener.IsListening;
            }
        }

        public void Stop()
        {
            try
            {
                if (this.listener.IsListening)
                {
                    this.listener.Abort();
                    this.listener.Stop();
                    this.listener.Close();
                }
            }
            catch (ObjectDisposedException)
            {
            }

            try
            {
                this.serverThread.Interrupt();
            }
            catch (ThreadInterruptedException)
            {
            }
        }

        private void RegisterPrefix(string prefix)
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "netsh",
                Arguments = @" http add urlacl url=" + prefix + " user = \"" + Environment.UserDomainName + @"\" + Environment.UserName + "\" listen =yes",
                Verb = "runas",
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });

            if (process != null)
            {
                process.WaitForExit();
            }
        }

        private void Listen()
        {
            string prefix = "http://+:" + this.port + "/";

            if (this.ElevateRights)
            {
                this.RegisterPrefix(prefix);
            }
            this.listener = new HttpListener()
            {
                AuthenticationSchemes = AuthenticationSchemes.Basic |
                                        AuthenticationSchemes.Anonymous |
                                        AuthenticationSchemes.IntegratedWindowsAuthentication
            };

            this.listener.Prefixes.Clear();
            this.listener.Prefixes.Add(prefix);
            // this.listener.Prefixes.Add("https://*:" + this.port + "/");

            this.listener.Start();

            while (this.listener.IsListening)
            {
                this.WaitRequest();
            }
        }

        private void WaitRequest()
        {
            IAsyncResult result = this.listener.BeginGetContext(this.ListenerCallback, this.listener);
            result.AsyncWaitHandle.WaitOne();
        }

        private void ListenerCallback(IAsyncResult result)
        {
            if (!this.IsListening)
            {
                return;
            }

            HttpListenerContext context = this.listener.EndGetContext(result);
            this.RequestHandlerAction(context);
        }

        public Action<HttpListenerContext> RequestHandlerAction
        {
            get
            {
                return this.requestHandlerAction;
            }
            set
            {
                this.requestHandlerAction = value;
            }
        }

        private void RequestHandler(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            string document = request.RawUrl;
            string docpath = this.rootPath + document;
            byte[] responsebyte = new byte[0];

            //var requestParams = HttpUtility.ParseQueryString(request.Url.Query);

            if (File.Exists(docpath))
            {
                try
                {
                    string mime;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    response.ContentType = mimeTypeMappings.TryGetValue(Path.GetExtension(docpath), out mime) ? mime : "application/octet-stream";
                    response.AddHeader("Date", DateTime.Now.ToString("r"));
                    response.AddHeader("Last-Modified", File.GetLastWriteTime(docpath).ToString("r"));

                    responsebyte = File.ReadAllBytes(docpath);
                }
                catch (Exception ex)
                {
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                }
            }
            else
            {
                response.StatusCode = (int)HttpStatusCode.NotFound;
                response.ContentType = "text/html";

                string errorFile = this.rootPath + "//error.html";
                responsebyte = File.Exists(errorFile)
                                        ? File.ReadAllBytes(errorFile)
                                        : Encoding.UTF8.GetBytes("<H2>404 Error! File does not exist.</H2>");

            }

            this.WriteToReponse(response, responsebyte);
        }

        public void WriteToReponse(HttpListenerResponse response, string responseString)
        {
            this.WriteToReponse(response, Encoding.UTF8.GetBytes(responseString));
        }

        public void WriteToReponse(HttpListenerResponse response, byte[] responsebyte)
        {
            response.ContentLength64 = responsebyte.Length;
            Stream output = response.OutputStream;
            output.Write(responsebyte, 0, responsebyte.Length);
            output.Close();
            response.OutputStream.Flush();
        }

        public bool IsAuthenticated(HttpListenerContext context, string user, string password)
        {
            if (context.User != null &&
                context.User.Identity != null &&
                context.User.Identity.IsAuthenticated)
            {
                var identity = context.User.Identity as HttpListenerBasicIdentity;
                if (identity == null)
                {
                    return false;
                }

                if (identity.Name == user && identity.Password == password)
                {
                    return true;
                }
            }
            else
            {
                return false;
            }

            return false;
        }
    }
}