using System;
using System.Management;
using DataBridge.Helper;

namespace DataBridge.Services
{
    public class ProcessWatcher : IDisposable
    {
        private ManagementEventWatcher startWatcher = new ManagementEventWatcher();

        private ManagementEventWatcher stopWatcher = new ManagementEventWatcher();

        private bool enableRaisingEvents = true;

        public event EventHandler<EventArgs<ProcessInfo>> OnProcessStarted;

        public event EventHandler<EventArgs<ProcessInfo>> OnProcessStopped;

        public ProcessWatcher()
        {
            this.startWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_ProcessStartTrace");
            this.startWatcher.EventArrived += this.Process_Started;
            this.startWatcher.Start();

            this.stopWatcher.Query = new WqlEventQuery("SELECT * FROM Win32_ProcessStopTrace");
            this.stopWatcher.EventArrived += this.Process_Stopped;
            this.stopWatcher.Start();
        }

        public bool EnableRaisingEvents
        {
            get { return this.enableRaisingEvents; }
            set { this.enableRaisingEvents = value; }
        }

        private void Process_Started(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent;

            var processInfo = new ProcessInfo(
                                                (string)instance.GetPropertyValue("ProcessName")
                                             );

            if (this.OnProcessStarted != null && this.EnableRaisingEvents)
            {
                this.OnProcessStarted(this, new EventArgs<ProcessInfo>(processInfo));
            }
        }

        private void Process_Stopped(object sender, EventArrivedEventArgs e)
        {
            ManagementBaseObject instance = (ManagementBaseObject)e.NewEvent;

            var processInfo = new ProcessInfo(
                                                (string)instance.GetPropertyValue("ProcessName")
                                             );

            if (this.OnProcessStopped != null && this.EnableRaisingEvents)
            {
                this.OnProcessStopped(this, new EventArgs<ProcessInfo>(processInfo));
            }
        }

        public void Dispose()
        {
            if (this.startWatcher != null)
            {
                this.startWatcher.Stop();
                this.startWatcher.EventArrived -= this.Process_Started;
                this.startWatcher.Dispose();
                this.startWatcher = null;
            }

            if (this.stopWatcher != null)
            {
                this.stopWatcher.Stop();
                this.stopWatcher.EventArrived -= this.Process_Stopped;
                this.stopWatcher.Dispose();
                this.stopWatcher = null;
            }
        }

        public class ProcessInfo
        {
            public ProcessInfo(string name, string id = null)
            {
                this.Name = name;
                this.Id = id;
            }

            public string Name { get; private set; }

            public string Id { get; private set; }
        }
    }
}