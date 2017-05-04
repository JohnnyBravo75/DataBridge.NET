namespace DataBridge.GUI.Model
{
    using DataBridge.Common;

    public class Service : ModelBase
    {
        private string status = "";
        private string serviceName;
        private string machineName;
        private string serviceDirectory;

        public Service(ServiceController controller = null)
        {
            this.Controller = controller;
        }

        public string ServiceName
        {
            get
            {
                if (this.Controller != null)
                {
                    return this.Controller.ServiceName;
                }
                return this.serviceName;
            }
            set { this.serviceName = value; }
        }

        public string MachineName
        {
            get
            {
                if (this.Controller != null)
                {
                    return this.Controller.MachineName;
                }
                return this.machineName;
            }
            set { this.machineName = value; }
        }

        public string ServiceDirectory
        {
            get
            {
                if (this.Controller != null)
                {
                    return this.Controller.ServiceDirectory;
                }
                return this.serviceDirectory;
            }
            set { this.serviceDirectory = value; }
        }

        public string Status
        {
            get
            {
                if (this.Controller != null)
                {
                    return this.Controller.Status.ToString();
                }
                return this.status;
            }
        }

        public ServiceController Controller { get; set; }
    }
}