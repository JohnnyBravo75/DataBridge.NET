using System.Globalization;
using System.Linq;
using System.Xml.Serialization;
using Quartz;

namespace DataBridge.Runtime
{
    using System;
    using System.Collections.Generic;

    [Serializable]
    public class DailyScheduleInfo : ScheduleInfo
    {
        private List<DayOfWeek> days = new List<DayOfWeek>();
        private DateTime startTime;

        public DailyScheduleInfo()
        {
        }

        [XmlAttribute(DataType = "dateTime")]
        public DateTime StartTime
        {
            get { return this.startTime; }
            set { this.startTime = value; }
        }

        public List<DayOfWeek> Days
        {
            get { return this.days; }
            set { this.days = value; }
        }

        public override void SetDefaultValues()
        {
            this.Days.Clear();
            this.Days.Add(DayOfWeek.Monday);
            this.Days.Add(DayOfWeek.Tuesday);
            this.Days.Add(DayOfWeek.Wednesday);
            this.Days.Add(DayOfWeek.Thursday);
            this.Days.Add(DayOfWeek.Friday);
            this.Days.Add(DayOfWeek.Saturday);
            this.Days.Add(DayOfWeek.Sunday);

            this.StartTime = new DateTime(1, 1, 1, 12, 0, 0);
        }

        public override ITrigger CreateTrigger()
        {
            ITrigger trigger = TriggerBuilder.Create()
                                             .WithDailyTimeIntervalSchedule
                                                (s => s.WithIntervalInHours(24)
                                                       .OnDaysOfTheWeek(this.Days.ToArray())
                                                       .StartingDailyAt(TimeOfDay.HourMinuteAndSecondOfDay(this.StartTime.Hour, this.StartTime.Minute, this.StartTime.Second))
                                                )
                                              .Build();

            return trigger;
        }

        public override string ToString()
        {
            var dayNames = string.Join(",", from day in this.days
                                            select DateTimeFormatInfo.CurrentInfo.GetAbbreviatedDayName(day));

            return string.Format("Daily: {0}", dayNames);
        }
    }
}