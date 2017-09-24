using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Threading;
using DataBridge.Common;
using DataBridge.Extensions;
using Quartz;
using Quartz.Impl;

namespace DataBridge.Runtime
{
    public class DataBridge
    {
        private Dictionary<Pipeline, Thread> runningPipelines = new Dictionary<Pipeline, Thread>();
        private Dictionary<NotificationInfo, Thread> runningNotifications = new Dictionary<NotificationInfo, Thread>();
        private static IScheduler scheduler;

        public string Name
        {
            get
            {
                return "DataBridge";
            }
        }

        public DataBridgeInfo DataBridgeInfo { get; private set; }

        public Dictionary<Pipeline, Thread> RunningPipelines
        {
            get
            {
                return this.runningPipelines;
            }
        }

        public Dictionary<NotificationInfo, Thread> RunningNotifications
        {
            get
            {
                return this.runningNotifications;
            }
        }

        public string ConfigPath
        {
            get { return Path.Combine(Environment.CurrentDirectory, DataBridgeManager.Instance.ConfigFolderName); }
        }

        public string PlugInPath
        {
            get { return ".\\PlugIns"; }
        }

        public string ConfigName
        {
            get { return this.Name + ".config"; }
        }

        public DataBridge(string configFileName = "")
        {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                Thread.CurrentThread.Name = this.GetType().Name + "Thread";
            }

            AppDomain.CurrentDomain.UnhandledException += this.CurrentDomain_UnhandledException;
            AppDomain.CurrentDomain.ProcessExit += this.CurrentDomain_ProcessExit;

            // Force assembly load (for KnowntypesProvider, because AppDomain.GetCurrentAssemblies() gets only loaded assemblies
            //var pipeline = new Pipeline();

            LogManager.Instance.LogNamedDebug(this.Name, this.GetType(), "".PadRight(80, '-'));
            LogManager.Instance.LogNamedDebug(this.Name, this.GetType(), this.Name + " initializing");
            LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), this.Name + " config path '{0}'", this.ConfigPath);

            // Load Plugins
            this.LoadPlugins();

            // Set working path
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            // Load DataBridge
            if (!string.IsNullOrEmpty(configFileName))
            {
                this.Load(configFileName);
            }
        }

        private void LoadPlugins()
        {
            try
            {
                LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Loading plugins in '{0}'", this.PlugInPath);

                if (Directory.Exists(this.PlugInPath))
                {
                    var loadedPlugIns = PlugInManager.Instance.LoadPlugIns(this.PlugInPath);
                    foreach (var loadedPlugIn in loadedPlugIns)
                    {
                        LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Plugin '{0}' loaded", loadedPlugIn);
                    }
                }
                else
                {
                    LogManager.Instance.LogNamedErrorFormat(this.Name, this.GetType(), "Plugin path '{0}' does not exist", this.PlugInPath);
                }
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogNamedError(this.Name, this.GetType(), "Plugins NOT loaded", ex);
            }
        }

        public bool LoadOrCreateDefault()
        {
            string configFileName = this.GetConfigFileName();

            if (!string.IsNullOrEmpty(configFileName))
            {
                this.DataBridgeInfo = DataBridgeManager.Instance.LoadDataBridge(configFileName);
            }
            else
            {
                configFileName = this.GetDefaultConfigFileName();

                this.DataBridgeInfo = DataBridgeManager.Instance.LoadOrCreateNewDataBridge(configFileName);
            }

            return (this.DataBridgeInfo != null);
        }

        public bool Load(string configFileName)
        {
            if (string.IsNullOrEmpty(configFileName))
            {
                return false;
            }

            this.DataBridgeInfo = DataBridgeManager.Instance.LoadDataBridge(configFileName);

            return (this.DataBridgeInfo != null);
        }

        public void Save(string configFileName)
        {
            DataBridgeManager.Instance.SaveDataBridge(configFileName, this.DataBridgeInfo);
        }

        public string GetDefaultConfigFileName()
        {
            string configFileName = Path.Combine(Environment.CurrentDirectory, this.ConfigName);

            return configFileName;
        }

        public string GetConfigFileName()
        {
            string configFileName = "";

            configFileName = Path.Combine(this.ConfigPath, this.ConfigName);

            if (!File.Exists(configFileName))
            {
                configFileName = "";
            }

            return configFileName;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            LogManager.Instance.LogNamedErrorFormat(this.Name, this.GetType(), "## EXCEPTION: Unhandeled exception in DataBridge: '{0}'", e.ExceptionObject.ToString());
        }

        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            LogManager.Instance.LogNamedErrorFormat(this.Name, this.GetType(), this.Name + " has exited");
            LogManager.Instance.FlushBuffers();
        }

        public bool Start()
        {
            if (this.DataBridgeInfo == null)
            {
                return false;
            }

            scheduler = new StdSchedulerFactory().GetScheduler();
            LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), this.Name + " starting...");

            int idx = 0;
            foreach (var notificationInfo in this.DataBridgeInfo.NotificationInfos)
            {
                if (notificationInfo.IsActive)
                {
                    if (!string.IsNullOrEmpty(notificationInfo.PipelineName))
                    {
                        if (!this.StartNotification(notificationInfo.PipelineName))
                        {
                            LogManager.Instance.LogNamedErrorFormat(this.Name, this.GetType(), "Notification '{0}' was not started", notificationInfo.PipelineName);
                        }
                    }
                    else
                    {
                        LogManager.Instance.LogNamedErrorFormat(this.Name, this.GetType(), "Notification No. '{0}' has no 'PipelineName'", idx);
                    }
                }
                else
                {
                    LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Notification '{0}' skipped (inactive)", notificationInfo.PipelineName);
                }

                idx++;
            }

            foreach (var pipelineInfo in this.DataBridgeInfo.PipelineInfos)
            {
                if (pipelineInfo.IsActive)
                {
                    this.StartPipeline(pipelineInfo.Name);
                }
                else
                {
                    LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Pipeline '{0}' skipped (inactive)", pipelineInfo.Name);
                }
            }
            return true;
        }

        public bool StartNotification(string pipelineName)
        {
            LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Notification '{0}' starting...", pipelineName);

            var notificationInfo = this.DataBridgeInfo.NotificationInfos.FirstOrDefault(x => x.PipelineName == pipelineName);

            if (notificationInfo == null)
            {
                return false;
            }

            if (!this.DataBridgeInfo.PipelineInfos.Any(x => x.Name == pipelineName))
            {
                LogManager.Instance.LogNamedErrorFormat(this.Name, this.GetType(), "Notification has invalid PipelineName '{0}'", pipelineName);
                return false;
            }

            var notificationThread = new Thread(() => this.NotificationThread(notificationInfo));
            notificationThread.Name = notificationInfo.PipelineName + "NotificationThread";
            notificationThread.Start();

            return true;
        }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public bool StopNotification(string pipelineName)
        {
            bool result = true;
            try
            {
                LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Notification '{0}' stopping...", pipelineName);

                var runningNotification = this.RunningNotifications.FirstOrDefault(t => t.Key.PipelineName == pipelineName);
                if (runningNotification.IsEmpty())
                {
                    return false;
                }

                if (runningNotification.Value != null && runningNotification.Value.IsAlive)
                {
                    runningNotification.Value.Interrupt();
                }

                this.RunningNotifications.Remove(runningNotification.Key);
            }
            catch (ThreadAbortException ex)
            {
                result = false;
            }
            catch (ThreadInterruptedException ex)
            {
                result = false;
            }

            LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Notification '{0}' stopped, result={1}", pipelineName, result);
            return result;
        }

        public bool StartPipeline(string pipelineName)
        {
            LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Pipeline '{0}' starting...", pipelineName);

            var pipelineInfo = this.DataBridgeInfo.PipelineInfos.FirstOrDefault(x => x.Name == pipelineName);
            if (pipelineInfo == null)
            {
                return false;
            }

            var piplineThread = new Thread(() => this.PipelineThread(pipelineInfo));
            piplineThread.Name = pipelineInfo.Name + "Thread";
            piplineThread.Start();

            return true;
        }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public bool StopPipeline(string pipelineName)
        {
            bool result = true;
            try
            {
                LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Pipeline '{0}' stopping...", pipelineName);

                var runningPipeline = this.RunningPipelines.FirstOrDefault(t => t.Key.Name == pipelineName);
                if (runningPipeline.IsEmpty())
                {
                    return false;
                }

                runningPipeline.Key.StopPipeline();
                runningPipeline.Key.Dispose();

                if (runningPipeline.Value != null && runningPipeline.Value.IsAlive)
                {
                    runningPipeline.Value.Interrupt();
                }

                this.RunningPipelines.Remove(runningPipeline.Key);
            }
            catch (ThreadAbortException ex)
            {
                result = false;
            }
            catch (ThreadInterruptedException ex)
            {
                result = false;
            }

            LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Pipeline '{0}' stopped, result={1}", pipelineName, result);
            return result;
        }

        [SecurityPermission(SecurityAction.Demand, ControlThread = true)]
        public void Stop()
        {
            try
            {
                if (scheduler != null)
                {
                    scheduler.Standby();
                    scheduler.Clear();

                    if (!scheduler.IsShutdown)
                    {
                        scheduler.Shutdown();
                    }

                    scheduler = null;
                }

                // Give time to shutdown
                for (int i = 0; i < 3; i++)
                {
                    Thread.Sleep(200);
                }

                if (this.RunningNotifications != null)
                {
                    foreach (var runningNotification in this.RunningNotifications.ToList())
                    {
                        this.StopNotification(runningNotification.Key.PipelineName);
                    }
                }

                if (this.RunningPipelines != null)
                {
                    foreach (var runningPipeline in this.RunningPipelines.ToList())
                    {
                        this.StopPipeline(runningPipeline.Key.Name);
                    }
                }

                Thread.Sleep(1000);
            }
            catch (ThreadAbortException ex)
            {
            }
            catch (ThreadInterruptedException ex)
            {
            }
        }

        public void PipelineThread(PipelineInfo pipelineInfo)
        {
            try
            {
                var fileName = Path.Combine(this.ConfigPath, pipelineInfo.FileName);
                var pipeline = Pipeline.Load(fileName);
                pipeline.ExecutePipeline();

                this.RunningPipelines.Add(pipeline, Thread.CurrentThread);

                LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Pipeline '{0}' started", pipelineInfo.Name);
            }
            catch (Exception ex)
            {
                LogManager.Instance.LogNamedError(this.Name, this.GetType(), ex.ToString());
                // throw;
            }
        }

        public void NotificationThread(NotificationInfo notificationInfo)
        {
            this.RunningNotifications.Add(notificationInfo, Thread.CurrentThread);

            LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "NotificationThread '{0}' started", notificationInfo.PipelineName);

            // create notification job
            IJobDetail notificationJobDetail = JobBuilder.Create<NotificationJob>()
                                                  .WithIdentity("notificationJob", "NotificationGroup")
                                                  .Build();
            notificationJobDetail.JobDataMap["NotificationInfo"] = notificationInfo;

            ITrigger notificationlTrigger = notificationInfo.Schedule.CreateTrigger();

            scheduler.ScheduleJob(notificationJobDetail, notificationlTrigger);

            // create heartbeat job
            //IJobDetail heartbeatJobDetail = JobBuilder.Create<HeartbeatJob>()
            //                                          .WithIdentity("heartbeatJob", "NotificationGroup")
            //                                          .Build();
            //heartbeatJobDetail.JobDataMap["DataBridge"] = this;

            //ITrigger heartbeatTrigger = TriggerBuilder.Create()
            //                                            .WithSimpleSchedule(x => x.WithInterval(new TimeSpan(0, 0, 1, 0, 0)).RepeatForever())
            //                                            .StartAt(DateBuilder.EvenMinuteDate(DateTime.Now))
            //                                            .Build();

            //scheduler.ScheduleJob(heartbeatJobDetail, heartbeatTrigger);

            scheduler.Start();

            //Console.WriteLine("Next Fire Time:" + trigger.GetNextFireTimeUtc().Value.LocalDateTime);
            //var times = TriggerUtils.ComputeFireTimes(trigger as IOperableTrigger, null, 10);

            //foreach (var time in times)
            //    Console.WriteLine(time);
        }
    }
}