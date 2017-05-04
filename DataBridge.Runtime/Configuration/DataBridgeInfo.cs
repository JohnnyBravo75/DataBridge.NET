using System;
using System.Collections.ObjectModel;

namespace DataBridge.Runtime
{
    [Serializable]
    public class DataBridgeInfo
    {
        private ObservableCollection<NotificationInfo> notificationInfos = new ObservableCollection<NotificationInfo>();
        private ObservableCollection<PipelineInfo> pipelineInfos = new ObservableCollection<PipelineInfo>();
        private SystemInfo systemInfo = new SystemInfo();

        public ObservableCollection<PipelineInfo> PipelineInfos
        {
            get { return this.pipelineInfos; }
            set { this.pipelineInfos = value; }
        }

        public ObservableCollection<NotificationInfo> NotificationInfos
        {
            get { return this.notificationInfos; }
            set { this.notificationInfos = value; }
        }

        public SystemInfo SystemInfo
        {
            get { return this.systemInfo; }
            set { this.systemInfo = value; }
        }

        public void SetDefaultValues()
        {
            var notificationInfo = new EmailNotification();
            notificationInfo.SetDefaultValues();
            this.NotificationInfos.Clear();
            this.NotificationInfos.Add(notificationInfo);

            var pipelineInfo = new PipelineInfo();
            pipelineInfo.SetDefaultValues();
            this.PipelineInfos.Clear();
            this.PipelineInfos.Add(pipelineInfo);
        }

        public enum MailLogStatus
        {
            Error = 1,
            Warning = 2,
            Info = 3
        };
    }
}