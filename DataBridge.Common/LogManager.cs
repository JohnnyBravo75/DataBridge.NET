using System;
using System.Collections.Generic;
using System.IO;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Layout;
using log4net.Repository;
using log4net.Repository.Hierarchy;

namespace DataBridge
{
    public class LogManager : Singleton<LogManager>
    {
        protected readonly ILog defaultLogger;
        private readonly Dictionary<string, ILog> loggers = new Dictionary<string, ILog>();

        public string DefaultLogDirectory
        {
            get
            {
                return "Logs";
            }
        }

        public LogManager()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo("./Configs/LogConfig.log4net"));
            //this.defaultLogger = log4net.LogManager.GetLogger("Default");
            this.defaultLogger = this.GetLoggerInternal("Default");
        }

        //private void LogDebug(Type type, string message)
        //{
        //    message = type.Name + ": " + message;
        //    this.defaultLogger.Logger.Log(null, Level.Debug, message, null);
        //}

        //private void LogDebugFormat(Type type, string message, params object[] args)
        //{
        //    /*
        //    var frame = new StackFrame(1, false);
        //    var method = frame.GetMethod();
        //    var type = method.DeclaringType;
        //    */

        //    message = type.Name + ": " + message;
        //    this.defaultLogger.Logger.Log(null, Level.Debug, new SystemStringFormat(CultureInfo.InvariantCulture, message, args), null);
        //}

        public void LogError(Type type, string message, Exception ex = null)
        {
            this.defaultLogger.Error(message, ex);
        }

        public void LogErrorFormat(Type type, string message, params object[] args)
        {
            this.defaultLogger.ErrorFormat(message, args);
        }

        public void FlushBuffers()
        {
            ILoggerRepository rep = log4net.LogManager.GetRepository();
            foreach (IAppender appender in rep.GetAppenders())
            {
                var buffered = appender as BufferingAppenderSkeleton;
                if (buffered != null)
                {
                    buffered.Flush();
                }
            }
        }

        public void LogNamedDebug(string loggerName, Type type, string message)
        {
            message = type.Name + ": " + message;
            this.GetLoggerInternal(loggerName).Debug(message);
        }

        public void LogNamedDebugFormat(string loggerName, Type type, string message, params object[] args)
        {
            ILog logger = !string.IsNullOrEmpty(loggerName)
                                ? this.GetLoggerInternal(loggerName)
                                : this.defaultLogger;

            message = type.Name + ": " + message;
            logger.DebugFormat(message, args);
        }

        public void LogNamedError(string loggerName, Type type, string message, Exception ex = null)
        {
            ILog logger = !string.IsNullOrEmpty(loggerName)
                                ? this.GetLoggerInternal(loggerName)
                                : this.defaultLogger;

            message = type.Name + ": " + "######### " + message;
            logger.Error(message, ex);

            if (logger != this.defaultLogger)
            {
                this.defaultLogger.Error(message, ex);
            }
        }

        public void LogNamedErrorFormat(string loggerName, Type type, string message, params object[] args)
        {
            ILog logger = !string.IsNullOrEmpty(loggerName)
                                ? this.GetLoggerInternal(loggerName)
                                : this.defaultLogger;

            message = type.Name + ": " + "######### " + message;

            logger.ErrorFormat(message, args);

            if (logger != this.defaultLogger)
            {
                this.defaultLogger.ErrorFormat(message, args);
            }
        }

        private ILog GetLoggerInternal(string loggerName)
        {
            if (!this.loggers.ContainsKey(loggerName))
            {
                var appender = this.CreateRollingFileAppender(loggerName);
                appender.ActivateOptions();
                this.loggers.Add(loggerName, log4net.LogManager.GetLogger(loggerName));
                ((Logger)this.loggers[loggerName].Logger).AddAppender(appender);
            }
            return this.loggers[loggerName];
        }

        private RollingFileAppender CreateRollingFileAppender(string name)
        {
            var layout = new PatternLayout
            {
                ConversionPattern = "%d{ISO8601} %level %thread - %message%newline"
            };
            layout.ActivateOptions();

            return new RollingFileAppender
            {
                Name = name,
                AppendToFile = true,
                DatePattern = "yyyyMMdd",
                MaximumFileSize = "1MB",
                MaxSizeRollBackups = 10,
                RollingStyle = RollingFileAppender.RollingMode.Composite,
                File = Path.Combine(this.DefaultLogDirectory, name + ".log"),
                Layout = layout
            };
        }
    }
}