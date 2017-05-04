using System.Xml.Serialization;
using Quartz;

namespace DataBridge.Schedule
{
    using System;

    [Serializable]
    public class CronScheduleInfo : ScheduleInfo
    {
        public CronScheduleInfo()
        {
        }

        [XmlAttribute]
        public string CronExpression { get; set; }

        public override void SetDefaultValues()
        {
        }

        public override ITrigger CreateTrigger()
        {
            ITrigger trigger = TriggerBuilder.Create()
                                             .WithCronSchedule(this.CronExpression)
                                             .Build();

            return trigger;
        }

        public override string ToString()
        {
            return this.CronExpression;
        }
    }
}