using System.Collections.Generic;
using System.Timers;
using System.Xml.Serialization;
using DataBridge.Extensions;

namespace DataBridge.Commands
{
    public class TimerTrigger : DataCommand
    {
        private Timer timer;
        private bool isFirstTick = true;
        private bool startInitial = true;

        public TimerTrigger()
        {
            this.Parameters.Add(new CommandParameter() { Name = "Interval", DataType = DataTypes.Number, Direction = Directions.In });
        }

        [XmlIgnore]
        public double Interval
        {
            get { return this.Parameters.GetValueOrDefault<double>("Interval", (this.timer != null ? this.timer.Interval : 0.0)); }
            set { this.Parameters.SetOrAddValue("Interval", value); }
        }

        public override void Initialize()
        {
            base.Initialize();

            this.timer = new Timer();
            this.timer.Elapsed += this.Timer_Tick;
            this.timer.Interval = this.Interval;
            this.timer.Start();

            this.LogDebugFormat("Waiting for interval '{0}'...", this.Interval.ToStringOrEmpty());
        }

        public override void DeInitialize()
        {
            if (this.timer != null)
            {
                this.timer.Stop();
                this.timer.Elapsed -= this.Timer_Tick;
                this.timer.Dispose();
                this.timer = null;
            }

            base.DeInitialize();
        }

        public override bool BeforeExecute()
        {
            if (this.isFirstTick && this.startInitial || !this.isFirstTick)
            {
                this.LogDebugFormat("Triggered in Interval '{0}'", this.timer.Interval);
                //timer.Stop();

                return this.OnSignalNext.Invoke(new InitializationResult(this, this.LoopCounter));
            }

            return true;
        }

        private void Timer_Tick(object sender, ElapsedEventArgs e)
        {
            this.isFirstTick = false;
            this.BeforeExecute();
        }

        protected override IEnumerable<CommandParameters> Execute(CommandParameters inParameters)
        {
            this.LogDebug(string.Format("Triggering Interval='{0}'", this.Interval));

            return base.Execute(inParameters);
        }

        public override bool AfterExecute()
        {
            this.timer.Start();
            return base.AfterExecute();
        }

        public override void Dispose()
        {
            this.DeInitialize();
        }
    }
}