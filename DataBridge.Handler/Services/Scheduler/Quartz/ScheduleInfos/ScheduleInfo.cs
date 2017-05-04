namespace DataBridge.Schedule
{
    using System;
    using Quartz;

    [Serializable]
    public abstract class ScheduleInfo
    {
        public abstract ITrigger CreateTrigger();

        public abstract void SetDefaultValues();
    }
}