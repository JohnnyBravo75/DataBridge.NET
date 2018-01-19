using System;
using System.Collections.Generic;
using DataBridge.Extensions;
using DataBridge.Schedule;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace DataBridge.Commands
{
    public class ScheduleTrigger : DataCommand
    {
        private bool isFirstTick = true;
        private bool startInitial = true;
        private ScheduleInfo schedule = new DailyScheduleInfo();
        private Scheduler scheduler;

        public ScheduleTrigger()
        {
        }

        [ExpandableObject]
        public ScheduleInfo Schedule
        {
            get { return this.schedule; }
            set { this.schedule = value; }
        }

        public override void Initialize()
        {
            base.Initialize();

            this.scheduler = new Scheduler(this.schedule.CreateTrigger());
            this.scheduler.OnTick += this.Scheduler_OnTick;
            this.scheduler.Start();

            this.LogDebugFormat("Waiting for schedule '{0}'...", this.schedule.ToStringOrEmpty());
        }

        public override void DeInitialize()
        {
            if (this.scheduler != null)
            {
                this.scheduler.Stop();
                this.scheduler.OnTick -= this.Scheduler_OnTick;
                this.scheduler.Dispose();
                this.scheduler = null;
            }

            base.DeInitialize();
        }

        public override bool BeforeExecute()
        {
            if (this.isFirstTick && this.startInitial || !this.isFirstTick)
            {
                this.LogDebugFormat("Triggered by schedule '{0}'", this.schedule.ToStringOrEmpty());
                this.scheduler.EnableRaisingEvents = false;

                return this.OnSignalNext.Invoke(new InitializationResult(this, this.LoopCounter));
            }

            return true;
        }

        protected override IEnumerable<CommandParameters> Execute(IEnumerable<CommandParameters> inParameters)
        {
            return base.Execute(inParameters);
        }

        public override bool AfterExecute()
        {
            if (this.scheduler != null)
            {
                this.scheduler.EnableRaisingEvents = true;
            }

            return base.AfterExecute();
        }

        public override void Dispose()
        {
            this.DeInitialize();
        }

        private void Scheduler_OnTick(object sender, EventArgs eventArgs)
        {
            this.SetCurrentThreadName();

            this.isFirstTick = false;
            this.BeforeExecute();
        }
    }
}