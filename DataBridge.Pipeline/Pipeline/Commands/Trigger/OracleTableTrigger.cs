using System.Data;
using System.Xml.Serialization;
using DataBridge.Services;

namespace DataBridge.Commands
{
    public class OracleTableTrigger : DataCommand
    {
        private OracleTableWatcher watcher;
        private bool isFirstWatch = true;
        private bool startInitial = true;
        private DataTable lastTableEventArgs;

        public OracleTableTrigger()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Table", Direction = Directions.In });
        }

        [XmlIgnore]
        public string Table
        {
            get { return this.Parameters.GetValueOrDefault<string>("Table", ""); }
            set { this.Parameters.SetOrAddValue("Table", value); }
        }

        public override void Initialize()
        {
            base.Initialize();

            this.watcher = new OracleTableWatcher();
            this.watcher.OnChanged += this.Watcher_OnChanged;
            this.watcher.AddWatchSource(this.Table);

            this.LogDebugFormat("Listening for data in '{1}'...", this.Table);
        }

        public override void DeInitialize()
        {
            if (this.watcher != null)
            {
                this.watcher.OnChanged -= this.Watcher_OnChanged;
                this.watcher.Dispose();
                this.watcher = null;
            }

            base.DeInitialize();
        }

        public override bool BeforeExecute()
        {
            if (this.isFirstWatch && this.startInitial || !this.isFirstWatch)
            {
                //watcher.EnableRaisingEvents = false;
                this.LogDebugFormat("Triggered in '{0}' by '{1}' action", this.Table, (this.isFirstWatch
                                                                                                ? "initial"
                                                                                                : ""));
                return this.OnSignalNext.Invoke(new InitializationResult(this, this.LoopCounter));
            }

            return true;
        }

        private void Watcher_OnChanged(object sender, DataTable e)
        {
            this.SetCurrentThreadName();
            this.lastTableEventArgs = e;

            this.isFirstWatch = false;
            this.BeforeExecute();
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
    }
}