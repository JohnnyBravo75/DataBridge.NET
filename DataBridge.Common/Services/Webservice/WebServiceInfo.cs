using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Services.Description;
using System.Xml;
using System.Xml.Schema;
using DataBridge.Extensions;

namespace DataBridge.Services
{
    /// <summary>
    /// Information about a web service
    /// </summary>
    public class WebServiceInfo
    {
        // ************************************Members**********************************************

        private WebMethodInfoCollection webMethods = new WebMethodInfoCollection();
        private List<ComplexTypeInfo> complexTypes = new List<ComplexTypeInfo>();
        private Uri wsdlUrl;
        private List<string> serviceUrls = new List<string>();

        // ************************************Constructors**********************************************

        /// <summary>
        /// Constructor creates the web service info from the given url.
        /// </summary>
        /// <param name="url">The URL.</param>
        public WebServiceInfo(Uri wsdlUrl, string user = "", string password = "")
        {
            if (wsdlUrl == null)
            {
                throw new ArgumentNullException("url");
            }

            this.wsdlUrl = wsdlUrl;
            this.User = user;
            this.Password = password;
            this.webMethods = this.GetWebServiceDescription(this.wsdlUrl.AbsoluteUri);
        }

        /// <summary>
        /// Constructor creates the web service info from the given wsdlUrl.
        /// </summary>
        /// <param name="wsdlUrl">The URL.</param>
        public WebServiceInfo(string wsdlUrl, string user = "", string password = "")
        {
            if (wsdlUrl == null)
            {
                throw new ArgumentNullException("url");
            }

            this.wsdlUrl = new Uri(wsdlUrl);
            this.User = user;
            this.Password = password;
            this.webMethods = this.GetWebServiceDescription(this.wsdlUrl.AbsoluteUri);
        }

        // ************************************Properties**********************************************

        /// <summary>
        /// WebMethodInfos
        /// </summary>
        public WebMethodInfoCollection WebMethods
        {
            get
            {
                return this.webMethods;
            }
        }

        public List<ComplexTypeInfo> ComplexTypes
        {
            get
            {
                return this.complexTypes;
            }
        }

        public Uri WsdlUrl
        {
            get
            {
                return this.wsdlUrl;
            }
            set
            {
                this.wsdlUrl = value;
            }
        }

        public List<string> ServiceUrls
        {
            get
            {
                return this.serviceUrls;
            }
        }

        public string User { get; private set; }

        public string Password { get; private set; }

        public string Domain { get; private set; }

        // ************************************Functions**********************************************

        /// <summary>
        /// Load the WSDL file from the given url.
        /// Use the ServiceDescription class to walk the wsdl and create the WebServiceInfo
        /// instance.
        /// </summary>
        /// <param name="url">
        private WebMethodInfoCollection GetWebServiceDescription(string url, string portTypeName = "")
        {
            WebMethodInfoCollection webMethodInfos = new WebMethodInfoCollection();

            ServiceDescription serviceDescription = this.LoadServiceDescription(url);
            webMethodInfos.AddRange(this.ParseServiceDescription(serviceDescription, portTypeName));

            // When no service url found (in the bindings), try to generate a default
            if (!this.ServiceUrls.Any())
            {
                string fallBackUrl = this.GenerateFallBackServiceUrl(url);
                if (!string.IsNullOrEmpty(fallBackUrl))
                {
                    this.serviceUrls.Add(fallBackUrl);
                }
            }

            return webMethodInfos;
        }

        private WebMethodInfoCollection ParseServiceDescription(ServiceDescription serviceDescription, string portTypeName = "")
        {
            var webMethodInfos = new WebMethodInfoCollection();

            foreach (PortType portType in serviceDescription.PortTypes)
            {
                // skip, when a special Porttype is needed
                if (!string.IsNullOrEmpty(portTypeName) && portTypeName != portType.Name)
                {
                    continue;
                }

                foreach (Operation operation in portType.Operations)
                {
                    // get the input parameters
                    string inputMessageName = operation.Messages.Input != null ? operation.Messages.Input.Message.Name : "";
                    string inputMessagePartName = !string.IsNullOrEmpty(inputMessageName)
                                                                ? serviceDescription.Messages[inputMessageName].Parts.Count > 0
                                                                     ? serviceDescription.Messages[inputMessageName].Parts[0].Element.Name
                                                                     : ""
                                                                : "";

                    var inputParameters = new List<Parameter>();
                    if (!string.IsNullOrEmpty(inputMessagePartName))
                    {
                        inputParameters = this.GetParameters(serviceDescription, inputMessagePartName);
                    }
                    else if (!string.IsNullOrEmpty(inputMessageName))
                    {
                        foreach (MessagePart item in serviceDescription.Messages[inputMessageName].Parts)
                        {
                            inputParameters.Add(new Parameter(item.Name, item.Type.ToString()));
                        }
                    }

                    // get the output parameters
                    string outputMessageName = operation.Messages.Output != null ? operation.Messages.Output.Message.Name : "";
                    string outputMessagePartName = !string.IsNullOrEmpty(outputMessageName)
                                                                ? serviceDescription.Messages[outputMessageName].Parts.Count > 0
                                                                            ? serviceDescription.Messages[outputMessageName].Parts[0].Element.Name
                                                                            : ""
                                                                : "";

                    var outputParameters = new List<Parameter>();
                    if (!string.IsNullOrEmpty(outputMessagePartName))
                    {
                        outputParameters = this.GetParameters(serviceDescription, outputMessagePartName);
                    }
                    else if (!string.IsNullOrEmpty(outputMessageName))
                    {
                        foreach (MessagePart item in serviceDescription.Messages[outputMessageName].Parts)
                        {
                            outputParameters.Add(new Parameter(item.Name, item.Type.ToString()));
                        }
                    }

                    string action = this.GetAction(serviceDescription, operation.Name);

                    // add new method
                    var webMethodInfo = new WebMethodInfo(operation.Name, inputParameters, outputParameters, "", serviceDescription.TargetNamespace);
                    if (!webMethodInfos.Contains(webMethodInfo.Name))
                    {
                        webMethodInfos.Add(webMethodInfo);
                    }
                }
            }

            // Load and parse includes
            foreach (Import import in serviceDescription.Imports)
            {
                ServiceDescription importedDescription = this.LoadServiceDescription(import.Location);
                webMethodInfos.AddRange(this.ParseServiceDescription(importedDescription, portTypeName));
            }

            // set Soap Action
            foreach (var webMethod in webMethodInfos)
            {
                if (string.IsNullOrEmpty(webMethod.Action))
                {
                    webMethod.Action = this.GetAction(serviceDescription, webMethod.Name);
                }
            }

            // find Service Urls
            foreach (Service service in serviceDescription.Services)
            {
                foreach (Port port in service.Ports)
                {
                    if (port.Extensions[0] is SoapAddressBinding)
                    {
                        string serviceUrl = (port.Extensions[0] as SoapAddressBinding).Location;

                        if (!string.IsNullOrEmpty(serviceUrl))
                        {
                            // Location does not point to the service or endpoint -> generate default
                            if (serviceUrl.EndsWith("/") && !serviceUrl.Contains(port.Service.Name) && !serviceUrl.Contains("."))
                            {
                                serviceUrl += port.Service.Name + ".svc";
                            }

                            this.ServiceUrls.Add(serviceUrl);
                        }
                    }
                }
            }

            return webMethodInfos;
        }

        private string GetAction(ServiceDescription serviceDescription, string operationName)
        {
            foreach (Binding binding in serviceDescription.Bindings)
            {
                foreach (OperationBinding operationBinding in binding.Operations)
                {
                    if (operationBinding.Name == operationName)
                    {
                        var extension = operationBinding.Extensions.Count > 0 ? (SoapOperationBinding)operationBinding.Extensions[0]
                                                                              : (SoapOperationBinding)null;
                        if (extension != null)
                        {
                            return extension.SoapAction;
                        }
                    }
                }
            }

            return string.Empty;
        }

        private ServiceDescription LoadServiceDescription(string url)
        {
            using (WebClient webClient = new WebClient())
            {
                webClient.Credentials = this.GetCredential(new Uri(url));
                //webClient.UseDefaultCredentials = true;
                this.SetBasicAuthHeader(webClient, this.User, this.Password);
                //webClient.Headers.Add(HttpRequestHeader.UserAgent, "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)");
                ServicePointManager.ServerCertificateValidationCallback =
                    (sender, certificate, chain, sslPolicyErrors) =>
                    {
                        // check certificate
                        return true;
                    };

                using (Stream stream = webClient.OpenRead(url))
                {
                    ServiceDescription serviceDescription = ServiceDescription.Read(stream);

                    return serviceDescription;
                }
            }
        }

        /// <summary>
        /// Walk the schema definition to find the parameters of the given message parts.
        /// </summary>
        /// <param name="serviceDescription">
        /// <param name="messagePartName">
        /// <returns></returns>
        private List<Parameter> GetParameters(ServiceDescription serviceDescription, string messagePartName)
        {
            var allParameters = new List<Parameter>();

            Types types = serviceDescription.Types;

            foreach (XmlSchema xmlSchema in types.Schemas)
            {
                XmlSchema completeXmlSchema = this.ReadXmlSchemaIncludes(xmlSchema);

                var parameters = this.GetParametersFromSchema(completeXmlSchema, messagePartName);
                allParameters.AddRange(parameters);
            }

            return allParameters;
        }

        private List<Parameter> GetParametersFromSchema(XmlSchema xmlSchema, string messagePartName)
        {
            List<Parameter> parameters = new List<Parameter>();

            // Complex types
            foreach (object item in xmlSchema.Items)
            {
                var schemaElement = item as XmlSchemaElement;
                var complexType = item as XmlSchemaComplexType;

                if (schemaElement != null)
                {
                    if (schemaElement.Name == messagePartName)
                    {
                        var schemaComplexType = schemaElement.SchemaType as XmlSchemaComplexType;
                        if (schemaComplexType != null)
                        {
                            XmlSchemaParticle particle = schemaComplexType.Particle;
                            XmlSchemaSequence sequence = particle as XmlSchemaSequence;
                            if (sequence != null)
                            {
                                foreach (XmlSchemaElement childElement in sequence.Items)
                                {
                                    string parameterName = childElement.Name;
                                    string parameterType = childElement.SchemaTypeName.Name;
                                    parameters.Add(new Parameter(parameterName, parameterType));
                                }
                            }
                        }
                    }
                }
                else if (complexType != null)
                {
                    ComplexTypeInfo complexTypeInfo = null;

                    if (complexType.Particle != null)
                    {
                        complexTypeInfo = this.ParseComplexParticle(complexType.Particle);
                        complexTypeInfo.Name = complexType.Name;
                    }
                    else
                    {
                        if (complexType.ContentModel != null &&
                            complexType.ContentModel.Content != null &&
                            ((XmlSchemaComplexContentExtension)(complexType.ContentModel.Content)).Particle != null)
                        {
                            complexTypeInfo = this.ParseComplexParticle(((XmlSchemaComplexContentExtension)(complexType.ContentModel.Content)).Particle);
                            complexTypeInfo.Name = complexType.Name;
                        }
                    }

                    if (complexTypeInfo != null && !this.ComplexTypes.Any(x => x.Name == complexTypeInfo.Name))
                    {
                        this.ComplexTypes.Add(complexTypeInfo);
                    }
                }
            }

            // Schema elements (referencing to complex types) e.g. <xs:element name="processShoppingCart" type="tns:processShoppingCart" />
            foreach (object item in xmlSchema.Items)
            {
                XmlSchemaElement schemaElement = item as XmlSchemaElement;
                if (schemaElement != null)
                {
                    if (schemaElement.Name == messagePartName)
                    {
                        if (schemaElement.SchemaTypeName != null && !string.IsNullOrEmpty(schemaElement.SchemaTypeName.Name))
                        {
                            // search the complex type and add the parameters of them
                            var complexType = this.ComplexTypes.First(x => x.Name == schemaElement.SchemaTypeName.Name);
                            if (complexType != null)
                            {
                                parameters.AddRange(complexType.Properties);
                            }
                        }
                    }
                }
            }

            // Includes
            if (xmlSchema.Includes != null)
            {
                foreach (var schemaImport in xmlSchema.Includes)
                {
                    if (schemaImport is XmlSchemaImport)
                    {
                        if ((schemaImport as XmlSchemaImport).Schema != null)
                        {
                            parameters.AddRange(this.GetParametersFromSchema((schemaImport as XmlSchemaImport).Schema, messagePartName));
                        }
                    }
                    else if (schemaImport is XmlSchemaInclude)
                    {
                        var schemaInclude = (schemaImport as XmlSchemaInclude);
                        if (schemaInclude.SchemaLocation != null && schemaInclude.Schema == null)
                        {
                            var includeSchema = this.LoadSchema(schemaInclude.SchemaLocation);
                            includeSchema.TargetNamespace = xmlSchema.TargetNamespace;
                            schemaInclude.Schema = includeSchema;
                        }

                        if (schemaInclude.Schema != null)
                        {
                            parameters.AddRange(this.GetParametersFromSchema(schemaInclude.Schema, messagePartName));
                        }
                    }
                }
            }

            return parameters;
        }

        private XmlSchema ReadXmlSchemaIncludes(XmlSchema xmlSchema)
        {
            foreach (XmlSchemaImport schemaImport in xmlSchema.Includes)
            {
                if (schemaImport.SchemaLocation != null)
                {
                    var includeSchema = this.LoadSchema(schemaImport.SchemaLocation);
                    includeSchema.TargetNamespace = xmlSchema.TargetNamespace;
                    schemaImport.Schema = includeSchema;
                }
            }

            return xmlSchema;
        }

        private XmlSchema LoadSchema(string schemaLocation)
        {
            XmlSchema schema = null;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += new ValidationEventHandler(
                 (sender, args) =>
                 {
                     if (args.Severity == XmlSeverityType.Warning)
                     {
                         Console.WriteLine("'{0}': Warning: {1}", schemaLocation, args.Message);
                     }
                     else if (args.Severity == XmlSeverityType.Error)
                     {
                         Console.WriteLine("'{0}': Error: {1}", schemaLocation, args.Message);
                     }
                 }
                );

            using (XmlReader reader = XmlReader.Create(schemaLocation, settings))
            {
                schema = XmlSchema.Read(reader, null);
            }

            return schema;
        }

        public bool IsComplexType(string typeName)
        {
            return this.ComplexTypes.Any(x => x.Name == typeName);
        }

        public ExpandoObject CreateComplexType(string name)
        {
            var complexType = this.ComplexTypes.First(x => x.Name == name);
            if (complexType == null)
            {
                throw new KeyNotFoundException(string.Format("The complex type '{0}' was not found", name));
            }

            var obj = new ExpandoObject() as IDictionary<string, Object>;
            foreach (var property in complexType.Properties)
            {
                if (this.IsComplexType(property.Type))
                {
                    // When type is complex type, create nested complex type
                    obj.Add(property.Name, this.CreateComplexType(property.Type));
                }
                else
                {
                    obj.Add(property.Name, string.Empty);
                }
            }

            return obj as ExpandoObject;
        }

        private ComplexTypeInfo ParseComplexParticle(XmlSchemaParticle particle)
        {
            var complexTypeInfo = new ComplexTypeInfo();

            var sequence = particle as XmlSchemaSequence;
            var choice = particle as XmlSchemaChoice;
            var all = particle as XmlSchemaAll;

            if (sequence != null)
            {
                for (int i = 0; i < sequence.Items.Count; i++)
                {
                    var childElement = sequence.Items[i] as XmlSchemaElement;
                    var innerSequence = sequence.Items[i] as XmlSchemaSequence;
                    var innerChoice = sequence.Items[i] as XmlSchemaChoice;
                    var innerAll = sequence.Items[i] as XmlSchemaAll;

                    if (childElement != null)
                    {
                        var parameter = new Parameter(name: childElement.Name, type: childElement.SchemaTypeName.Name);
                        if (childElement.QualifiedName != null)
                        {
                            parameter.Namespace = childElement.QualifiedName.Namespace;
                        }
                        complexTypeInfo.Properties.Add(parameter);
                    }
                    else
                    {
                        this.ParseComplexParticle(sequence.Items[i] as XmlSchemaParticle);
                    }
                }
            }
            else if (choice != null)
            {
                for (int i = 0; i < choice.Items.Count; i++)
                {
                    var childElement = choice.Items[i] as XmlSchemaElement;
                    var innerSequence = choice.Items[i] as XmlSchemaSequence;
                    var innerChoice = choice.Items[i] as XmlSchemaChoice;
                    var innerAll = choice.Items[i] as XmlSchemaAll;

                    if (childElement != null)
                    {
                        complexTypeInfo.Properties.Add(new Parameter(name: childElement.Name, type: childElement.SchemaTypeName.Name));
                    }
                    else
                    {
                        this.ParseComplexParticle(choice.Items[i] as XmlSchemaParticle);
                    }
                }
            }
            else if (all != null)
            {
                for (int i = 0; i < all.Items.Count; i++)
                {
                    var childElement = all.Items[i] as XmlSchemaElement;
                    var innerSequence = all.Items[i] as XmlSchemaSequence;
                    var innerChoice = all.Items[i] as XmlSchemaChoice;
                    var innerAll = all.Items[i] as XmlSchemaAll;

                    if (childElement != null)
                    {
                        complexTypeInfo.Properties.Add(new Parameter(name: childElement.Name, type: childElement.SchemaTypeName.Name));
                    }
                    else
                    {
                        this.ParseComplexParticle(all.Items[i] as XmlSchemaParticle);
                    }
                }
            }

            return complexTypeInfo;
        }

        private CredentialCache GetCredential(Uri uri)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3;
            CredentialCache credentialCache = new CredentialCache();
            credentialCache.Add(uri, "NTLM", new NetworkCredential(this.User, this.Password, this.Domain));
            return credentialCache;
        }

        private void SetBasicAuthHeader(WebClient webclient, String userName, String userPassword)
        {
            string authInfo = userName + ":" + userPassword;
            authInfo = Convert.ToBase64String(Encoding.Default.GetBytes(authInfo));
            webclient.Headers["Authorization"] = "Basic " + authInfo;
        }

        public string ConvertXmlSchemaToString(XmlSchema xmlSchema)
        {
            string schemaAsString = String.Empty;
            MemoryStream memStream = new MemoryStream(1024);

            xmlSchema.Write(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(memStream))
            {
                schemaAsString = reader.ReadToEnd();
            }

            return schemaAsString;
        }

        private string GenerateFallBackServiceUrl(string url)
        {
            string fallBackUrl = string.Empty;
            if (!string.IsNullOrEmpty(url) && this.IsHttpUri(url))
            {
                url = url.ToLower();
                if (url.EndsWith("?wsdl"))
                {
                    fallBackUrl = url.Replace("?wsdl", "");
                }
                else if (url.EndsWith("?singlewsdl"))
                {
                    fallBackUrl = url.Replace("?singlewsdl", "");
                }
            }

            return fallBackUrl;
        }

        private bool IsHttpUri(string uri)
        {
            Uri uriResult = null;
            return (Uri.TryCreate(uri, UriKind.Absolute, out uriResult) && uriResult.Scheme == Uri.UriSchemeHttp);
        }
    }

    /// <summary>
    /// Information about a web service operation
    /// </summary>
    public class WebMethodInfo
    {
        private string name;
        private string action;
        private List<Parameter> inputParameters = new List<Parameter>();
        private List<Parameter> outputParameters = new List<Parameter>();
        private string targetNamespace;

        /// <summary>
        /// OperationInfo
        /// </summary>
        public WebMethodInfo(string name, List<Parameter> inputParameters, List<Parameter> outputParameters, string action = "", string targetNamespace = "")
        {
            this.name = name;
            this.inputParameters = inputParameters;
            this.outputParameters = outputParameters;
            this.action = action;
            this.targetNamespace = targetNamespace;
        }

        public string TargetNamespace
        {
            get { return this.targetNamespace; }
            set { this.targetNamespace = value; }
        }

        public string Action
        {
            get { return this.action; }
            set { this.action = value; }
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name
        {
            get { return this.name; }
        }

        /// <summary>
        /// InputParameters
        /// </summary>
        public List<Parameter> InputParameters
        {
            get { return this.inputParameters; }
        }

        /// <summary>
        /// OutputParameters
        /// </summary>
        public List<Parameter> OutputParameters
        {
            get { return this.outputParameters; }
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(this.Name)
                         ? this.Name
                         : base.ToString();
        }
    }

    public class ComplexTypeInfo
    {
        private List<Parameter> properties = new List<Parameter>();

        public string Name { get; set; }

        public List<Parameter> Properties
        {
            get { return this.properties; }
        }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(this.Name)
                         ? this.Name
                         : base.ToString();
        }
    }

    /// <summary>
    /// A collection of WebMethodInfo objects
    /// </summary>
    public class WebMethodInfoCollection : KeyedCollection<string, WebMethodInfo>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public WebMethodInfoCollection()
            : base()
        {
        }

        protected override string GetKeyForItem(WebMethodInfo webMethodInfo)
        {
            return webMethodInfo.Name;
        }
    }

    /// <summary>
    /// represents a parameter (input or output) of a web method.
    /// </summary>
    public class Parameter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Parameter" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <param name="length">The length.</param>
        public Parameter(string name, string type, int? length = null)
        {
            this.Name = name;
            this.Type = type;
            this.Length = length;
        }

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Type
        /// </summary>
        public string Type { get; set; }

        public string Namespace { get; set; }

        public int? Length { get; set; }

        public override string ToString()
        {
            return !string.IsNullOrEmpty(this.Name)
                         ? this.Name
                         : base.ToString();
        }
    }
}