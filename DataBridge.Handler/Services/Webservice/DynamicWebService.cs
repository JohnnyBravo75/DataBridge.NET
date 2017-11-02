namespace DataBridge.Services
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Runtime.Serialization.Formatters.Soap;
    using System.Security.Permissions;
    using System.Text;
    using System.Web.Services.Description;
    using System.Web.Services.Protocols;
    using System.Xml;
    using System.Xml.Schema;
    using System.Xml.Serialization;
    using DataBridge.Extensions;

    public class DynamicWebService : IDisposable
    {
        // ***********************Fields***********************

        private Assembly proxyAssembly;

        private Dictionary<string, object> serviceClasses = new Dictionary<string, object>();

        private string wsdlUrl = "";

        // ***********************Constructors***********************

        // ***********************Properties***********************

        public Assembly ProxyAssembly
        {
            get { return this.proxyAssembly; }
        }

        public int TimeOutInSeconds { get; set; }

        public NetworkCredential LoginCredential { get; set; }

        public WebProxy Proxy { get; set; }

        public string ServiceUrl { get; set; }

        public string WsdlUrl
        {
            get
            {
                return this.wsdlUrl;
            }
            private set
            {
                this.wsdlUrl = value;
            }
        }

        // ***********************Functions***********************

        public bool Initialize(string serviceUrl, NetworkCredential loginCredential = null, WebProxy webProxy = null)
        {
            if (string.IsNullOrEmpty(serviceUrl))
            {
                throw new ArgumentNullException("serviceUrl");
            }

            try
            {
                this.LoginCredential = loginCredential;
                this.Proxy = webProxy;
                if (serviceUrl.ToLower().EndsWith("wsdl"))
                {
                    this.WsdlUrl = serviceUrl;
                }
                else
                {
                    this.WsdlUrl = serviceUrl + "?wsdl";
                    this.ServiceUrl = serviceUrl;
                }

                this.proxyAssembly = this.CreateProxyAssemblyFromWsdl(this.WsdlUrl);
            }
            catch (Exception ex)
            {
                throw;
            }

            if (this.proxyAssembly == null)
            {
                return false;
            }

            return true;
        }

        public string GetFirstServiceName()
        {
            return this.GetServiceNames().FirstOrDefault();
        }

        public object CallServiceMethod(string serviceName, string methodName, object[] parameters)
        {
            var serviceClass = this.GetServiceClass(serviceName);

            if (serviceClass == null)
            {
                throw new ArgumentNullException("serviceClass", string.Format("The service '{0}' is not found or the serviceclass cannot be created", serviceName));
            }

            // TimeOut
            if (this.TimeOutInSeconds > 0)
            {
                var timeOutProperty = serviceClass.GetType().GetProperty("Timeout");
                if (timeOutProperty != null)
                {
                    timeOutProperty.SetValue(serviceClass, this.TimeOutInSeconds, null);
                }
            }

            // Login
            if (this.LoginCredential != null)
            {
                var defaultCredentialsProperty = serviceClass.GetType().GetProperty("UseDefaultCredentials");
                if (defaultCredentialsProperty != null)
                {
                    defaultCredentialsProperty.SetValue(serviceClass, false, null);
                }

                var credentialsProperty = serviceClass.GetType().GetProperty("Credentials");
                if (credentialsProperty != null)
                {
                    credentialsProperty.SetValue(serviceClass, this.LoginCredential, null);
                }
            }

            // Proxy
            if (this.Proxy != null)
            {
                var proxyProperty = serviceClass.GetType().GetProperty("Proxy");
                if (proxyProperty != null)
                {
                    proxyProperty.SetValue(serviceClass, this.Proxy, null);
                }
            }

            // Invoke
            var method = serviceClass.GetType().GetMethod(methodName);
            var result = method.Invoke(serviceClass, BindingFlags.Public, null, parameters, null);

            return result;
        }

        /// <summary>
        /// Calls the service method.
        /// </summary>
        /// <typeparam name="T">the expected returntype (xmlDocument)</typeparam>
        /// <param name="serviceName">Name of the service.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="parameters">The parameters.</param>
        /// <returns></returns>
        public T CallServiceMethod<T>(string serviceName, string methodName, object[] parameters)
        {
            return (T)this.CallServiceMethod(serviceName, methodName, parameters);
        }

        public void Dispose()
        {
            this.proxyAssembly = null;

            if (this.serviceClasses != null)
            {
                this.serviceClasses.Clear();
            }
        }

        public IEnumerable<MethodInfo> GetServiceMethods(string serviceName = "")
        {
            // when no service is given take the first (mostly there is only one)
            if (string.IsNullOrEmpty(serviceName))
            {
                serviceName = this.GetServiceNames().FirstOrDefault();
            }

            var methods = new List<MethodInfo>();
            if (this.proxyAssembly == null)
            {
                return methods;
            }

            var serviceType = this.proxyAssembly.GetTypes().FirstOrDefault(x => x.Name == serviceName);

            if (serviceType != null)
            {
                foreach (var method in serviceType.GetMethods())
                {
                    if (IsServiceMethod(method))
                    {
                        methods.Add(method);
                    }
                }
            }

            return methods;
        }

        public IEnumerable<string> GetServiceMethodNames(string serviceName = "")
        {
            IList<string> methods = new List<string>();

            var serviceMethods = this.GetServiceMethods(serviceName);
            methods.AddRange(serviceMethods.Select(x => x.Name));

            return methods;
        }

        public IEnumerable<string> GetServiceNames()
        {
            IList<string> serviceNames = new List<string>();

            if (this.proxyAssembly == null)
            {
                return serviceNames;
            }

            foreach (var type in this.proxyAssembly.GetTypes())
            {
                if (IsWebService(type))
                {
                    serviceNames.Add(type.Name);
                }
            }

            return serviceNames;
        }

        /// <summary>
        /// Builds the web service description importer, which allows us to generate a proxy class based on the
        /// content of the WSDL described by the wsdlStream.
        /// The WSDL content, described by XML.
        /// </summary>
        /// <returns>A ServiceDescriptionImporter that can be used to create a proxy class.</returns>
        private ServiceDescriptionImporter BuildServiceDescriptionImporter(string wsdlUrl)
        {
            if (string.IsNullOrEmpty(wsdlUrl))
            {
                return null;
            }

            using (var webClient = new WebClient())
            {
                // Login
                if (this.LoginCredential != null)
                {
                    // webClient.UseDefaultCredentials = false;
                    webClient.SetCredentials(this.LoginCredential);
                }

                // Proxy
                if (this.Proxy != null)
                {
                    webClient.Proxy = this.Proxy;
                }

                // Connect To the web service and read wsdl
                using (var wsdlStream = webClient.OpenRead(wsdlUrl))
                {
                    if (wsdlStream == null)
                    {
                        return null;
                    }

                    // Now read the WSDL file describing a service.
                    var description = ServiceDescription.Read(wsdlStream);

                    // Initialize a service description importer.
                    var serviceImporter = new ServiceDescriptionImporter();
                    serviceImporter.ProtocolName = "Soap";
                    serviceImporter.AddServiceDescription(description, null, null);

                    // Download any imported schemas (ie. WCF generated WSDL)
                    foreach (XmlSchema wsdlSchema in description.Types.Schemas)
                    {
                        // Loop through all detected imports in the main schema
                        foreach (var externalSchema in wsdlSchema.Includes)
                        {
                            // Read each external schema into a schema object and add to importer
                            if (externalSchema is XmlSchemaImport)
                            {
                                var baseUri = new Uri(wsdlUrl);
                                var schemaUri = new Uri(baseUri, ((XmlSchemaExternal)externalSchema).SchemaLocation);

                                var schemaStream = webClient.OpenRead(schemaUri);
                                if (schemaStream != null)
                                {
                                    var schema = XmlSchema.Read(schemaStream, null);
                                    serviceImporter.Schemas.Add(schema);
                                }
                            }
                        }
                    }

                    // get additional wsdl definitions from the imports
                    // e.g. <wsdl:import namespace="http://www.companyx.com/ns/Billing/externalWsdl" location="http://xsrvr/BillingWebServices/PremiumBill/AccountHistory.asmx?wsdl=wsdl1"/>
                    foreach (Import import in description.Imports)
                    {
                        var baseUri = new Uri(wsdlUrl);
                        var importUri = new Uri(baseUri, import.Location);

                        using (var importWsdlStream = webClient.OpenRead(importUri))
                        {
                            if (importWsdlStream != null)
                            {
                                var importDescription = ServiceDescription.Read(importWsdlStream);
                                serviceImporter.AddServiceDescription(importDescription, null, null);
                            }
                        }
                    }

                    // Generate a proxy client.
                    serviceImporter.Style = ServiceDescriptionImportStyle.Client;

                    // Generate properties to represent primitive values.
                    serviceImporter.CodeGenerationOptions = CodeGenerationOptions.GenerateProperties;

                    return serviceImporter;
                }
            }
        }

        private Assembly CompileAssembly(ServiceDescriptionImporter serviceImporter, bool inMemory = true)
        {
            if (serviceImporter == null)
            {
                return null;
            }

            // Initialize a Code-DOM tree into which we will import the service.
            var compileUnit = new CodeCompileUnit();
            var nameSpace = new CodeNamespace();
            compileUnit.Namespaces.Add(nameSpace);

            // Import the service into the Code-DOM tree. This creates proxy code that uses the service.
            var warnings = serviceImporter.Import(nameSpace, compileUnit);

            if (warnings == 0)
            {
                // create a c# compiler
                var codeProvider = CodeDomProvider.CreateProvider("CSharp");

                // include the assembly references needed to compile
                var parameters = new CompilerParameters();
                parameters.ReferencedAssemblies.Add("System.dll");
                parameters.ReferencedAssemblies.Add("System.Web.Services.dll");
                parameters.ReferencedAssemblies.Add("System.Web.dll");
                parameters.ReferencedAssemblies.Add("System.Xml.dll");
                parameters.ReferencedAssemblies.Add("System.Data.dll");
                // parameters.OutputAssembly = "";
                parameters.GenerateExecutable = !inMemory;
                parameters.GenerateInMemory = inMemory;
                parameters.TreatWarningsAsErrors = false;
                parameters.WarningLevel = 4;

                // compile into assembly
                var results = codeProvider.CompileAssemblyFromDom(parameters, compileUnit);

                // Check For Errors
                if (results.Errors.Count > 0)
                {
                    var errorText = "";
                    var i = 1;
                    foreach (CompilerError error in results.Errors)
                    {
                        errorText += "Error (" + i + "): " + error.ErrorText + Environment.NewLine;
                        i++;
                    }

                    throw new Exception(string.Format("Error on compiling proxy assembly for service '{0}'", this.ServiceUrl) + Environment.NewLine + errorText);
                }

                return results.CompiledAssembly;
            }
            else
            {
                return null;
            }
        }

        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        private Assembly CreateProxyAssemblyFromWsdl(string wsdlUrl)
        {
            var serviceImporter = this.BuildServiceDescriptionImporter(wsdlUrl);
            var assembly = this.CompileAssembly(serviceImporter);

            if (string.IsNullOrEmpty(this.ServiceUrl) && serviceImporter != null)
            {
                this.ServiceUrl = this.ExtractFirstServiceUrl(serviceImporter);
            }

            return assembly;
        }

        private string ExtractFirstServiceUrl(ServiceDescriptionImporter serviceImporter)
        {
            if (serviceImporter.ServiceDescriptions.Count > 0 &&
                serviceImporter.ServiceDescriptions[0].Services.Count > 0 &&
                serviceImporter.ServiceDescriptions[0].Services[0].Ports.Count > 0 &&
                serviceImporter.ServiceDescriptions[0].Services[0].Ports[0].Extensions.Count > 0)
            {
                var addrsBinding = serviceImporter.ServiceDescriptions[0].Services[0].Ports[0].Extensions[0] as SoapAddressBinding;
                if (addrsBinding == null)
                {
                    return string.Empty;
                }
                return addrsBinding.Location;
            }

            return string.Empty;
        }

        /// <summary>
        /// Gets the service class.
        /// </summary>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        private object GetServiceClass(string serviceName)
        {
            object serviceClass = null;

            if (this.serviceClasses.ContainsKey(serviceName))
            {
                // return existing
                serviceClass = this.serviceClasses[serviceName];
            }

            if (serviceClass == null)
            {
                //var defaultBinding = new System.ServiceModel.BasicHttpBinding(System.ServiceModel.BasicHttpSecurityMode.None);

                //if (this.LoginCredential != null)
                //{
                //    ((System.ServiceModel.BasicHttpBinding)defaultBinding).Security.Mode = System.ServiceModel.BasicHttpSecurityMode.TransportCredentialOnly;
                //    ((System.ServiceModel.BasicHttpBinding)defaultBinding).Security.Transport.ClientCredentialType = System.ServiceModel.HttpClientCredentialType.Ntlm;
                //}

                //serviceClass = this.proxyAssembly.CreateInstance(serviceName, false, BindingFlags.CreateInstance, null, new object[] { defaultBinding, new System.ServiceModel.EndpointAddress(this.ServiceUrl) }, null, null);

                // create new and add to the cache
                serviceClass = this.proxyAssembly.CreateInstance(serviceName);

                if (serviceClass != null)
                {
                    this.serviceClasses.Add(serviceName, serviceClass);
                }
            }

            return serviceClass;
        }

        /// <summary>
        /// Determines whether [is service method] [the specified method].
        /// </summary>
        /// <param name="method">The method.</param>
        /// <returns>
        ///   <c>true</c> if [is service method] [the specified method]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsServiceMethod(MethodInfo method)
        {
            var customAttributes = method.GetCustomAttributes(typeof(SoapRpcMethodAttribute), true);
            if (customAttributes != null && customAttributes.Length > 0)
            {
                return true;
            }

            customAttributes = method.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), true);
            if (customAttributes != null && customAttributes.Length > 0)
            {
                return true;
            }

            customAttributes = method.GetCustomAttributes(typeof(HttpMethodAttribute), true);
            if (customAttributes != null && customAttributes.Length > 0)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Determines whether [is web service] [the specified type].
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is web service] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsWebService(Type type)
        {
            return typeof(HttpWebClientProtocol).IsAssignableFrom(type);
        }

        public string SerializeObjectToXml<T>(T obj)
        {
            try
            {
                string xmlString = null;
                using (var memoryStream = new MemoryStream())
                {
                    using (var xmlTextWriter = new XmlTextWriter(memoryStream, Encoding.UTF8))
                    {
                        var xsoap = new SoapFormatter();
                        xsoap.Serialize(memoryStream, obj);

                        var encoding = new UTF8Encoding();
                        xmlString = encoding.GetString(memoryStream.ToArray());

                        return xmlString;
                    }
                }
            }
            catch (Exception ex)
            {
                return string.Empty;
            }
        }

        public T UnserializeObjectFromXml<T>(string xml)
        {
            var serializer = new XmlSerializer(typeof(T));

            var encoding = new UTF8Encoding();
            var byteArray = encoding.GetBytes(xml);

            using (var memoryStream = new MemoryStream(byteArray))
            {
                return (T)serializer.Deserialize(memoryStream);
            }
        }

        public Dictionary<string, Type> GetInParameters(string methodName)
        {
            var serviceMethods = this.GetServiceMethods();
            var serviceMethod = serviceMethods.FirstOrDefault(x => x.Name == methodName);

            var inParameters = new Dictionary<string, Type>();

            if (serviceMethod != null)
            {
                foreach (var parameter in serviceMethod.GetParameters().Where(x => !x.IsOut))
                {
                    inParameters.Add(parameter.Name, parameter.ParameterType);
                }
            }

            return inParameters;
        }

        public Dictionary<string, Type> GetOutParameters(string methodName)
        {
            var serviceMethods = this.GetServiceMethods();
            var serviceMethod = serviceMethods.FirstOrDefault(x => x.Name == methodName);

            var outParameters = new Dictionary<string, Type>();

            if (serviceMethod != null)
            {
                if (serviceMethod.ReturnParameter != null)
                {
                    outParameters.Add("ReturnValue", serviceMethod.ReturnParameter.ParameterType);
                }

                foreach (var parameter in serviceMethod.GetParameters().Where(x => x.IsOut))
                {
                    outParameters.Add(parameter.Name, parameter.ParameterType);
                }
            }

            return outParameters;
        }

        public IEnumerable<ParameterInfo> GetParameters(string methodName)
        {
            var serviceMethods = this.GetServiceMethods();
            var serviceMethod = serviceMethods.FirstOrDefault(x => x.Name == methodName);
            if (serviceMethod == null)
            {
                return Enumerable.Empty<ParameterInfo>();
            }
            return serviceMethod.GetParameters();
        }

        public Type GetParameterType(string methodName, string parameterName)
        {
            var parameter = this.GetParameters(methodName).FirstOrDefault(x => x.Name == parameterName);
            if (parameter == null)
            {
                throw new ArgumentException(string.Format("The parameter '{0}' was not found.", parameterName));
            }

            return parameter.ParameterType;
        }

        public bool GetParameterIsOut(string methodName, string parameterName)
        {
            var parameter = this.GetParameters(methodName).FirstOrDefault(x => x.Name == parameterName);
            if (parameter == null)
            {
                throw new ArgumentException(string.Format("The parameter '{0}' was not found.", parameterName));
            }

            return parameter.IsOut;
        }

        public bool GetParameterIsIn(string methodName, string parameterName)
        {
            var parameter = this.GetParameters(methodName).FirstOrDefault(x => x.Name == parameterName);

            if (parameter == null)
            {
                throw new ArgumentException(string.Format("The parameter '{0}' was not found.", parameterName));
            }

            return !parameter.IsOut;
        }
    }
}