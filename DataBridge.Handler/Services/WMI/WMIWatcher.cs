using System;
using System.Management;
using DataBridge.Helper;

namespace DataBridge.Services
{
    public class WmiWatcher : IDisposable
    {
        private ManagementEventWatcher watcher = new ManagementEventWatcher();

        private bool enableRaisingEvents = true;

        public event EventHandler<EventArgs<ManagementBaseObject>> OnEventArrived;

        private string computerName = "localhost";
        private string wmiEventName;

        public WmiWatcher(string wmiEventName)
        {
            this.wmiEventName = wmiEventName;
        }

        public void StartWatcher()
        {
            ManagementScope scope;

            if (!this.computerName.Equals("localhost", StringComparison.OrdinalIgnoreCase))
            {
                var conn = new ConnectionOptions();
                conn.Username = this.Username;
                conn.Password = this.Password;
                conn.Authority = "ntlmdomain:DOMAIN";
                scope = new ManagementScope(String.Format("\\\\{0}\\root\\WMI", this.computerName), conn);
            }
            else
            {
                scope = new ManagementScope(String.Format("\\\\{0}\\root\\WMI", this.computerName), null);
            }

            scope.Connect();

            this.watcher.Scope = scope;
            this.watcher.Query = new WqlEventQuery("SELECT * FROM " + this.wmiEventName);
            this.watcher.EventArrived += this.Event_Arrived;
            this.watcher.Start();
        }

        public bool EnableRaisingEvents
        {
            get { return this.enableRaisingEvents; }
            set { this.enableRaisingEvents = value; }
        }

        public string ComputerName
        {
            get { return this.computerName; }
            set { this.computerName = value; }
        }

        public string Username { get; set; }
        public string Password { get; set; }

        public string CompareProperty { get; set; }
        public string CompareValue { get; set; }

        private void Event_Arrived(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent;

            bool doRaiseEvent = false;

            if (!string.IsNullOrEmpty(this.CompareProperty))
            {
                var value = (string)instance.GetPropertyValue(this.CompareProperty);

                if (this.CompareValue == value)
                {
                    doRaiseEvent = true;
                }
            }
            else
            {
                doRaiseEvent = true;
            }

            if (this.OnEventArrived != null && this.EnableRaisingEvents && doRaiseEvent)
            {
                this.OnEventArrived(this, new EventArgs<ManagementBaseObject>(instance));
            }
        }

        public void Dispose()
        {
            if (this.watcher != null)
            {
                this.watcher.Stop();
                this.watcher.EventArrived -= this.Event_Arrived;
                this.watcher.Dispose();
                this.watcher = null;
            }
        }
    }
}