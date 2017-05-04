using System;
using Quartz;

namespace DataBridge.Runtime
{
    [DisallowConcurrentExecution]
    public class HeartbeatJob : IJob
    {
        public void Execute(IJobExecutionContext context)
        {
            try
            {
                LogManager.Instance.LogNamedDebug("Heartbeat", this.GetType(), "Heartbeat");

                //DataBridge dataBridge = context.JobDetail.JobDataMap["DataBridge"] as DataBridge;

                //if (dataBridge != null)
                //{
                //    foreach (var pipelineThread in DataBridge.PipelineThreads.ToList())
                //    {
                //        if (!pipelineThread.IsAlive && pipelineThread.ThreadState != ThreadState.Running)
                //        {
                //            // Thread is broken, restart
                //            dataBridge.StopPipeline(pipelineThread.Name);
                //            dataBridge.StartPipeline(pipelineThread.Name);
                //        }
                //    }
                //}
            }
            catch (Exception ex)
            {
                throw new JobExecutionException(ex);
            }
        }
    }
}