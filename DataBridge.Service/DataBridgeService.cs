using System.Diagnostics;
using System.IO;
using System.Linq;

namespace DataBridge.Service
{
    using System;
    using System.Management;
    using System.ServiceProcess;
    using DataBridge.Runtime;

    public partial class DataBridgeService : ServiceBase
    {
        private DataBridge dataBridge = null;

        public DataBridgeService(string[] args)
        {
            try
            {
                // Debugger.Launch();

                if (args.Length > 0)
                {
                    this.ServiceName = args[0];
                }

                this.InitializeComponent();

                Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            }
            catch (Exception ex)
            {
                EventLogHelper.WriteEntry(Program.EventLogName, "Failed to start: " + ex.Message);

                throw new Exception("Error in Constructor, Service could not be started.\n", ex);
            }
        }

        /// <summary>
        /// When implemented in a derived class, executes when a Pause command is sent to the service by the Service Control Manager (SCM). Specifies actions to take when a service pauses.
        /// </summary>
        protected override void OnPause()
        {
        }

        private string serviceName;

        public new string ServiceName
        {
            get
            {
                if (!string.IsNullOrEmpty(base.ServiceName))
                {
                    return base.ServiceName;
                }

                if (string.IsNullOrEmpty(this.serviceName))
                {
                    this.serviceName = this.GetServiceName();
                }

                return this.serviceName;
            }
            set
            {
                base.ServiceName = value;
            }
        }

        /// <summary>
        /// Startet den Ablauf
        /// </summary>
        /// <param name="args">Data passed by the start command.</param>
        protected override void OnStart(string[] args)
        {
            // Debugger.Launch();

            //if (string.IsNullOrEmpty(this.ServiceName))
            //{
            //    this.ServiceName = GetServiceName();
            //}

            try
            {
                this.dataBridge = new DataBridge();
                this.dataBridge.Load(this.dataBridge.GetConfigFileName());
                this.dataBridge.Start();
            }
            catch (Exception ex)
            {
                EventLogHelper.WriteEntry(Program.EventLogName, "Failed to start: " + ex.Message);
                throw;
            }
        }

        protected override void OnStop()
        {
            this.dataBridge.Stop();
            this.dataBridge = null;
        }

        protected string GetServiceName()
        {
            // Calling System.ServiceProcess.ServiceBase::ServiceName allways returns
            // an empty string,
            // see https://connect.microsoft.com/VisualStudio/feedback/ViewFeedback.aspx?FeedbackID=387024

            // So we have to do some more work to find out our service name, this only works if
            // the process contains a single service, if there are more than one services hosted
            // in the process you will have to do something else

            int processId = Process.GetCurrentProcess().Id;
            string query = "SELECT * FROM Win32_Service where ProcessId = " + processId;
            var searcher = new ManagementObjectSearcher(query);

            using (var results = searcher.Get())
            {
                try
                {
                    var result = results.Cast<ManagementObject>().FirstOrDefault();

                    return result["Name"].ToString();
                }
                catch (Exception ex)
                {
                    throw new Exception("Can not get the ServiceName");
                }
            }
        }
    }
}