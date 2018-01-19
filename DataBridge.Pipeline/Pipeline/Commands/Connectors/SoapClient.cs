using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataBridge.Services;
using DataConnectors.Formatters;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "SoapClient", Title = "SoapClient", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png")]
    public class SoapClient : DataCommand
    {
        private DataMappings dataMappings = new DataMappings();
        private FormatterBase formatter = new DefaultFormatter();

        public SoapClient()
        {
            this.AddParameters();
        }

        private void AddParameters()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Wsdl" });
            this.Parameters.Add(new CommandParameter() { Name = "Url" });
            this.Parameters.Add(new CommandParameter() { Name = "MethodName" });
            this.Parameters.Add(new CommandParameter() { Name = "Namespace" });
            this.Parameters.Add(new CommandParameter() { Name = "SoapAction" });
            this.Parameters.Add(new CommandParameter() { Name = "EncodingName" });
            this.Parameters.Add(new CommandParameter() { Name = "User" });
            this.Parameters.Add(new CommandParameter() { Name = "Password" });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
        }

        public SoapClient(WebServiceInfo webServiceInfo, string methodName)
        {
            this.AddParameters();

            this.Url = webServiceInfo.ServiceUrls.First();
            var methodInfo = webServiceInfo.WebMethods.First(x => x.Name == methodName);
            this.MethodName = methodInfo.Name;
            this.SoapAction = methodInfo.Action;
            this.Namespace = methodInfo.TargetNamespace;

            foreach (Services.Parameter inputParam in methodInfo.InputParameters)
            {
                this.DataMappings.AddOrUpdate(new DataMapping()
                {
                    Name = inputParam.Name
                });
            }
        }

        public DataMappings DataMappings
        {
            get { return this.dataMappings; }
            set { this.dataMappings = value; }
        }

        public FormatterBase Formatter
        {
            get { return this.formatter; }
            set { this.formatter = value; }
        }

        [XmlIgnore]
        public string Wsdl
        {
            get { return this.Parameters.GetValue<string>("Wsdl"); }
            set { this.Parameters.SetOrAddValue("Wsdl", value); }
        }

        [XmlIgnore]
        public string Url
        {
            get { return this.Parameters.GetValue<string>("Url"); }
            set { this.Parameters.SetOrAddValue("Url", value); }
        }

        [XmlIgnore]
        public string MethodName
        {
            get { return this.Parameters.GetValue<string>("MethodName"); }
            set { this.Parameters.SetOrAddValue("MethodName", value); }
        }

        [XmlIgnore]
        public string Namespace
        {
            get { return this.Parameters.GetValue<string>("Namespace"); }
            set { this.Parameters.SetOrAddValue("Namespace", value); }
        }

        [XmlIgnore]
        public string SoapAction
        {
            get { return this.Parameters.GetValue<string>("SoapAction"); }
            set { this.Parameters.SetOrAddValue("SoapAction", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string serviceUrl = inParameters.GetValue<string>("Url");
                string nameSpace = inParameters.GetValue<string>("Namespace");
                string soapAction = inParameters.GetValue<string>("SoapAction");
                string methodName = inParameters.GetValue<string>("MethodName");
                string wsdlUrl = inParameters.GetValue<string>("Wsdl");
                if (!string.IsNullOrEmpty(wsdlUrl))
                {
                    var webServiceInfo = new WebServiceInfo(wsdlUrl);
                    serviceUrl = webServiceInfo.ServiceUrls.First();
                    var methodInfo = webServiceInfo.WebMethods.First(x => x.Name == methodName);
                    methodName = methodInfo.Name;
                    soapAction = methodInfo.Action;
                    nameSpace = methodInfo.TargetNamespace;

                    if (!this.DataMappings.Any())
                    {
                        foreach (Services.Parameter inputParam in methodInfo.InputParameters)
                        {
                            this.DataMappings.AddOrUpdate(new DataMapping()
                            {
                                Name = inputParam.Name
                            });
                        }
                    }
                }

                string encodingName = inParameters.GetValueOrDefault<string>("EncodingName", "utf-8");
                string passWord = inParameters.GetValue<string>("Password");
                string user = inParameters.GetValue<string>("User");

                foreach (var data in this.ReadData(serviceUrl, methodName, nameSpace, soapAction, encodingName, inParameters.ToDictionary()))
                {
                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("Data", data);
                    yield return outParameters;
                }
            }
        }

        private IEnumerable<object> ReadData(string serviceUrl, string methodName, string nameSpace, string soapAction, string encodingName, IDictionary<string, object> parameters)
        {
            var webService = new SimpleWebserviceRequest(serviceUrl, methodName, nameSpace, "", soapAction, encodingName, removeNamespaces: true, unescapeResult: null);

            var data = this.InvokeWebserviceWithParameters(webService, parameters);
            yield return data;
        }

        private object InvokeWebserviceWithParameters(SimpleWebserviceRequest webService, IDictionary<string, object> values = null)
        {
            webService.Parameters.Clear();

            // Take the values, which are defined in the mapping
            foreach (var dataMapping in this.DataMappings)
            {
                var webserviceParameter = new SimpleWebserviceRequest.WebserviceParameter()
                {
                    Name = dataMapping.Name,
                    Type = dataMapping.DataType.ToString()
                };

                if (dataMapping.Value != null)
                {
                    // replace token e.g {Filename} ->  C:\Temp\Test.txt
                    webserviceParameter.Value = TokenProcessor.ReplaceTokens(dataMapping.Value.ToStringOrEmpty(), values);
                }
                //else
                //{
                //    // set the value
                //    webserviceParameter.Value = values.GetValue(dataMapping.Name);
                //}

                webService.Parameters.Add(webserviceParameter);
            }

            // call the service
            var result = webService.Invoke();

            // format the result
            object data = null;
            if (this.formatter != null && result != null)
            {
                data = this.formatter.Format(result.OuterXml, null);
            }

            return data;
        }
    }
}