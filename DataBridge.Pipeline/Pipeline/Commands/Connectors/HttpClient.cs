using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataBridge.Helper;
using DataBridge.Services;
using DataConnectors.Formatters;

namespace DataBridge.Commands
{
    [DataCommandDescription(Name = "HttpClient", Title = "HttpClient", Group = "Connectors", Image = "\\Resources\\Images\\ImportConnector.png")]
    public class HttpClient : DataCommand
    {
        private DataMappings dataMappings = new DataMappings();
        private FormatterBase formatter = new DefaultFormatter();
        private SimpleWebRequest webRequest = new SimpleWebRequest();

        public HttpClient()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Url", Direction = Directions.In, NotNull = true });
            this.Parameters.Add(new CommandParameter() { Name = "EncodingName", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Password", Direction = Directions.In, UseEncryption = true });
            this.Parameters.Add(new CommandParameter() { Name = "User", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "Data", Direction = Directions.InOut });
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
        public string Url
        {
            get { return this.Parameters.GetValue<string>("Url"); }
            set { this.Parameters.SetOrAddValue("Url", value); }
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParametersList)
        {
            foreach (var inParameters in inParametersList)
            {
                //inParameters = GetCurrentInParameters();
                string url = inParameters.GetValue<string>("Url");
                string encodingName = inParameters.GetValueOrDefault<string>("EncodingName", "utf-8");
                string passWord = inParameters.GetValue<string>("Password");
                string user = inParameters.GetValue<string>("User");
                var table = inParameters.GetValueOrDefault<DataTable>("Data");

                foreach (var data in this.ReadData(url, encodingName, user, passWord, inParameters.ToDictionary()))
                {
                    var outParameters = this.GetCurrentOutParameters();
                    outParameters.SetOrAddValue("Data", data);
                    yield return outParameters;
                }
            }
        }

        private IEnumerable<object> ReadData(string url, string encodingName, string user, string passWord, IDictionary<string, object> parameters)
        {
            this.webRequest.RequestMode = SimpleWebRequest.RequestModes.GET;
            this.webRequest.Url = url;
            this.webRequest.RequestEncoding = EncodingUtil.GetEncodingOrDefault(encodingName);
            if (!string.IsNullOrEmpty(user))
            {
                this.webRequest.Credentials = new NetworkCredential(user, passWord);
            }

            var data = this.InvokeWebRequestWithParameters(this.webRequest, parameters);
            yield return data;
        }

        private object InvokeWebRequestWithParameters(SimpleWebRequest webRequest, IDictionary<string, object> values = null)
        {
            // Take the values, which are defined in the mapping
            foreach (var dataMapping in this.DataMappings)
            {
                object value = null;

                if (dataMapping.Value != null)
                {
                    // replace token e.g {Filename} ->  C:\Temp\Test.txt
                    value = TokenProcessor.ReplaceTokens(dataMapping.Value.ToStringOrEmpty(), values);
                }
                else
                {
                    value = values.GetValue(dataMapping.Name);
                }

                webRequest.Parameters.Add(dataMapping.Name, value.ToStringOrEmpty());
            }

            // call the service
            var result = webRequest.Invoke();

            // format the result
            object data = null;
            if (this.formatter != null && result != null)
            {
                data = this.formatter.Format(result, null);
            }

            return data;
        }

        public override void Dispose()
        {
            if (this.webRequest != null)
            {
                this.webRequest.Dispose();
            }

            base.Dispose();
        }
    }
}