using System;
using System.Collections.Generic;
using System.Threading;
using Quartz;
using Quartz.Impl;

namespace DataBridge.Schedule
{
    public class Scheduler : IDisposable
    {
        private Guid name = Guid.NewGuid();
        private static Dictionary<Guid, Scheduler> schedulers = new Dictionary<Guid, Scheduler>();
        private ITrigger trigger;
        private IScheduler quartzScheduler = new StdSchedulerFactory().GetScheduler();
        private IJobDetail jobDetail;
        private bool enableRaisingEvents = true;

        public event EventHandler<EventArgs> OnTick;

        public Scheduler(ITrigger trigger = null)
        {
            this.jobDetail = JobBuilder.Create<EventRaiserJob>()
                                             .WithIdentity(this.name.ToString(), "Scheduler")
                                             .Build();
            Schedulers.Add(this.name, this);
            this.Trigger = trigger;
        }

        public static Dictionary<Guid, Scheduler> Schedulers
        {
            get
            {
                return schedulers;
            }
        }

        public ITrigger Trigger
        {
            get { return this.trigger; }
            set { this.trigger = value; }
        }

        public bool EnableRaisingEvents
        {
            get { return this.enableRaisingEvents; }
            set { this.enableRaisingEvents = value; }
        }

        public void Start()
        {
            this.quartzScheduler.ScheduleJob(this.jobDetail, this.trigger);
            this.quartzScheduler.Start();

            //var times = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, 20);

            //foreach (var time in times)
            //    Debug.WriteLine(time);
        }

        public void Stop()
        {
            this.quartzScheduler.Standby();
        }

        public void Dispose()
        {
            this.jobDetail = null;

            if (this.quartzScheduler != null)
            {
                this.Stop();
                if (!this.quartzScheduler.IsShutdown)
                {
                    this.quartzScheduler.Shutdown();
                }

                this.quartzScheduler = null;
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(200);
                }
            }

            if (Schedulers.ContainsKey(this.name))
            {
                Schedulers.Remove(this.name);
            }

            this.Trigger = null;

            if (schedulers != null)
            {
                schedulers.Clear();
            }
            this.jobDetail = null;
        }

        public void RaiseEvent()
        {
            if (this.OnTick != null && this.EnableRaisingEvents)
            {
                this.OnTick(this, null);
            }
        }
    }

    public class EventRaiserJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            Guid name = new Guid(context.JobDetail.Key.Name);
            Scheduler.Schedulers[name].RaiseEvent();
        }
    }
}