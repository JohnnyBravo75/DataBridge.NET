using System;
using Quartz;

namespace DataBridge.Runtime
{
    [DisallowConcurrentExecution]
    public class NotificationJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            var notificationInfo = context.JobDetail.JobDataMap["NotificationInfo"] as NotificationInfo;
            if (notificationInfo == null)
            {
                throw new ArgumentNullException("notificationInfo");
            }

            try
            {
                notificationInfo.Notify(notificationInfo.PipelineName);
            }
            catch (Exception ex)
            {
                throw new JobExecutionException(ex);
            }
        }
    }
}