using System.Collections.Generic;
using System.Threading;
using System.Xml.Serialization;
using DataBridge.Helper;
using DataBridge.Services;

namespace DataBridge.Commands
{
    public class UsbTrigger : DataCommand
    {
        private UsbDeviceWatcher watcher = null;
        private bool isFirstWatch = true;
        private bool startInitial = false;

        public UsbTrigger()
        {
            this.Parameters.Add(new CommandParameter() { Name = "DeviceName", Direction = Directions.In });
            this.Parameters.Add(new CommandParameter() { Name = "DriveLetter", Direction = Directions.Out });
        }

        [XmlIgnore]
        public string DriveLetter
        {
            get { return this.Parameters.GetValue<string>("DriveLetter"); }
            set { this.Parameters.SetOrAddValue("DriveLetter", value); }
        }

        [XmlIgnore]
        public string DeviceName
        {
            get { return this.Parameters.GetValue<string>("DeviceName"); }
            set { this.Parameters.SetOrAddValue("DeviceName", value); }
        }

        public override void Initialize()
        {
            base.Initialize();

            this.watcher = new UsbDeviceWatcher();
            this.watcher.OnDeviceInserted += this.Watcher_OnDeviceInserted;
            this.watcher.Start();

            this.LogDebugFormat("Listening for USB-Device '{0}'...", !string.IsNullOrEmpty(this.DeviceName)
                                                                            ? this.DeviceName
                                                                            : "every");
        }

        public override void DeInitialize()
        {
            if (this.watcher != null)
            {
                this.watcher.Stop();
                this.watcher.OnDeviceInserted -= this.Watcher_OnDeviceInserted;
                this.watcher.Dispose();
                this.watcher = null;
            }

            base.DeInitialize();
        }

        public override bool BeforeExecute()
        {
            if (this.isFirstWatch && this.startInitial || !this.isFirstWatch)
            {
                this.LogDebugFormat("Triggered on USB-Device '{0}' in '{1}'", this.DeviceName, this.DriveLetter);
                //watcher.EnableRaisingEvents = false;

                return this.OnSignalNext.Invoke(new InitializationResult(this, this.LoopCounter));
            }

            return true;
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParameters)
        {
            var outParameters = this.GetCurrentOutParameters();
            outParameters.AddOrUpdate(new CommandParameter() { Name = "DeviceName", Value = this.DeviceName });
            outParameters.AddOrUpdate(new CommandParameter() { Name = "DriveLetter", Value = this.DriveLetter });
            yield return outParameters;
        }

        public override bool AfterExecute()
        {
            this.watcher.EnableRaisingEvents = true;
            return base.AfterExecute();
        }

        public override void Dispose()
        {
            this.DeInitialize();
        }

        private void Watcher_OnDeviceInserted(object sender, EventArgs<UsbDeviceWatcher.UsbDeviceInfo> eventArgs)
        {
            this.SetCurrentThreadName();

            if (eventArgs.Result == null)
            {
                return;
            }

            this.LogDebugFormat("USB-Device '{0} ({1})' inserted. PNPDeviceID='{2}'", eventArgs.Result.VolumeName, eventArgs.Result.DriveLetter, eventArgs.Result.PnpDeviceID);

            if (!string.IsNullOrEmpty(this.DeviceName) &&
                this.DeviceName != eventArgs.Result.Description &&
                this.DeviceName != eventArgs.Result.VolumeName)
            {
                this.LogDebugFormat("Wrong USB-Device '{0}' inserted, still waiting for '{1}'", eventArgs.Result.VolumeName, this.DeviceName);
                return;
            }

            this.DriveLetter = eventArgs.Result.DriveLetter;
            this.DeviceName = eventArgs.Result.VolumeName;

            if (string.IsNullOrEmpty(this.DriveLetter))
            {
                this.LogErrorFormat("Could not get driveletter from USB-Device. Driveletter='{0}'", this.DriveLetter);
                return;
            }

            this.isFirstWatch = false;
            Thread.Sleep(100);
            this.BeforeExecute();
        }
    }
}