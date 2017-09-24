using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Xml.Serialization;
using DataBridge.Commands;
using DataBridge.Common;
using DataBridge.Extensions;
using DataBridge.Formatters;
using DataBridge.Helper;
using DataBridge.Services;
using log4net.Core;

namespace DataBridge.Runtime
{
    public class Program
    {
        private static void Main(string[] args)
        {
            var configName = Path.GetFileName(Assembly.GetExecutingAssembly().Location) + ".config";
            RemotingConfiguration.Configure(configName, false);

            var logListener = new RemoteLogListener();
            RemotingServices.Marshal(logListener, "RemoteLogListener");
            logListener.MessageReceived += (sender, a) => AddLog(a.LoggingEvents);

            var bridge = new DataBridge();

            DataBridgeManager.Instance.CreateAllCommandsDataBridge(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "AllPipelineCommands.config"));

            var idx = 0;
            if (bridge.Start())
            {
                while (true)
                {
                    Thread.Sleep(100);
                    idx++;
                }
            }

            RemotingServices.Disconnect(logListener);
        }

        private static void AddLog(IEnumerable<LoggingEvent> loggingEvents)
        {
            foreach (var loggingEvent in loggingEvents)
            {
                Console.WriteLine("{0} [{1}] {2} {3} - {4}", loggingEvent.TimeStamp.ToLongTimeString(), loggingEvent.ThreadName, loggingEvent.Level.Name, loggingEvent.LoggerName, loggingEvent.RenderedMessage);
            }
        }
    }
}