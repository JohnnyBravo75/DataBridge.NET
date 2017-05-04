using System;
using System.Xml.Serialization;
using DataBridge.Schedule;

namespace DataBridge.Runtime
{
    [Serializable]
    public abstract class NotificationInfo
    {
        private ScheduleInfo schedule = new DailyScheduleInfo();
        private bool isActive = true;
        private string pipelineName = "";

        [XmlAttribute]
        public bool IsActive
        {
            get { return this.isActive; }
            set { this.isActive = value; }
        }

        [XmlAttribute]
        public string PipelineName
        {
            get
            {
                return this.pipelineName;
            }
            set
            {
                this.pipelineName = value;
            }
        }

        [XmlElement("ScheduleInfo")]
        public ScheduleInfo Schedule
        {
            get { return this.schedule; }
            set { this.schedule = value; }
        }

        public virtual void SetDefaultValues()
        {
            this.Schedule.SetDefaultValues();
        }

        public abstract bool Notify(string pipelineName);
    }
}