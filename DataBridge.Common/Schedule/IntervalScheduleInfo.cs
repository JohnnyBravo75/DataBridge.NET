using System.Xml.Serialization;

namespace DataBridge.Runtime
{
    using System;
    using Quartz;

    [Serializable]
    public class IntervalScheduleInfo : ScheduleInfo
    {
        public IntervalScheduleInfo()
        {          
        }

        [XmlAttribute]
        public int Days { get; set; }

        [XmlAttribute]
        public int Hours { get; set; }

        [XmlAttribute]
        public int Minutes { get; set; }

        [XmlAttribute]
        public int Seconds { get; set; }

        public override void SetDefaultValues()
        {
            this.Minutes = 15;
        }

        public override ITrigger CreateTrigger()
        {
            var startTime = DateBuilder.EvenMinuteDate(DateTime.Now);

            ITrigger trigger = TriggerBuilder.Create()
                                            .WithSimpleSchedule(x => x.WithInterval(new TimeSpan(this.Days, this.Hours, this.Minutes, this.Seconds)).RepeatForever())
                                            .StartAt(startTime)
                                            .Build();

            return trigger;
        }

        public override string ToString()
        {
            return string.Format("Interval: {0:D2} days, {1:D2} hrs, {2:D2} min, {3:D2} sec", this.Days, this.Hours, this.Minutes, this.Seconds);
        }
    }
}