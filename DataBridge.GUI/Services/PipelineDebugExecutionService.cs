namespace DataBridge.GUI.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using DataBridge.GUI.Model;
    using DataBridge.Helper;

    public class PipelineDebugExecutionService : IDataCommandDebugService
    {
        public Task<DebugResult> ExecuteAsync(Pipeline pipeline, IProgress<DebugLogEntry> progress, CancellationToken cancellationToken)
        {
            if (pipeline == null)
            {
                throw new ArgumentNullException("pipeline");
            }

            return Task.Run(() =>
            {
                var result = DebugResult.CreateDefault();
                var stopwatch = Stopwatch.StartNew();
                var currentCommandName = string.Empty;
                DataCommand lastCommand = null;
                var logFilePosition = 0L;

                Action<DataCommand> reportOutParameters = command =>
                {
                    if (command == null)
                    {
                        return;
                    }

                    var entry = new DebugLogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = DebugLogLevel.Info,
                        Source = GetCommandDisplayName(command),
                        Message = "Out-Parameter => " + FormatParameters(command.GetCurrentOutParameters())
                    };

                    lock (result)
                    {
                        result.Logs.Add(entry);
                    }

                    if (progress != null)
                    {
                        progress.Report(entry);
                    }
                };

                Action<DataCommand, DebugLogLevel, string> reportCurrentParameters = (command, level, prefix) =>
                {
                    if (command == null)
                    {
                        return;
                    }

                    var entry = new DebugLogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = level,
                        Source = GetCommandDisplayName(command),
                        Message = prefix + " => " + FormatParameters(command.ExecuteParameters)
                    };

                    lock (result)
                    {
                        result.Logs.Add(entry);
                    }


                };

                Action<DataCommand> executeCommandHandler = command =>
                {
                    if (lastCommand != null && !ReferenceEquals(lastCommand, command))
                    {
                        reportOutParameters(lastCommand);
                    }

                    lastCommand = command;
                    //currentCommandName = GetCommandDisplayName(command);

                    //var entry = new DebugLogEntry
                    //{
                    //    Timestamp = DateTime.Now,
                    //    Level = DebugLogLevel.Info,
                    //    Source = currentCommandName,
                    //    Message = "Command execution started."
                    //};

                    //lock (result)
                    //{
                    //    result.Logs.Add(entry);                      
                    //}

                    //if (progress != null)
                    //{
                    //    progress.Report(entry);
                    //}

                    reportCurrentParameters(command, DebugLogLevel.Info, "Current parameters on start");
                };

                EventHandler<EventArgs<string>> canceledHandler = (sender, args) =>
                {
                    var entry = new DebugLogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = DebugLogLevel.Warn,
                        Source = string.IsNullOrWhiteSpace(currentCommandName) ? "Pipeline" : currentCommandName,
                        Message = "Pipeline execution canceled: " + (args != null ? args.Result : string.Empty)
                    };

                    lock (result)
                    {
                        result.Logs.Add(entry);
                    }

                    if (progress != null)
                    {
                        progress.Report(entry);
                    }
                };

                pipeline.OnExecuteCommand += executeCommandHandler;
                pipeline.OnExecutionCanceled += canceledHandler;

                CancellationTokenRegistration cancellationRegistration = default(CancellationTokenRegistration);

                try
                {
                    cancellationRegistration = cancellationToken.Register(pipeline.StopPipeline);
                    logFilePosition = GetCurrentPipelineLogFileLength(pipeline);

                    var startEntry = new DebugLogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = DebugLogLevel.Info,
                        Source = string.IsNullOrWhiteSpace(currentCommandName) ? "Pipeline" : currentCommandName,
                        Message = "Debug-Ausführung gestartet."
                    };

                    lock (result)
                    {
                        result.Logs.Add(startEntry);
                    }

                    if (progress != null)
                    {
                        progress.Report(startEntry);
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                    }

                    result.Success = pipeline.ExecutePipeline();
                    AppendPipelineLogTail(pipeline, ref logFilePosition, result, progress);
                    reportOutParameters(lastCommand);
                    if (!result.Success)
                    {
                        reportCurrentParameters(lastCommand, DebugLogLevel.Error, "Current parameters on failure");
                    }
                    result.ExitCode = result.Success ? 0 : 1;
                    result.SummaryMessage = result.Success
                        ? "Debug-Ausführung erfolgreich abgeschlossen."
                        : "Debug-Ausführung wurde mit Fehler beendet.";

                    var completionEntry = new DebugLogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = result.Success ? DebugLogLevel.Info : DebugLogLevel.Error,
                        Source = string.IsNullOrWhiteSpace(currentCommandName) ? "Pipeline" : currentCommandName,
                        Message = result.SummaryMessage
                    };

                    result.Logs.Add(completionEntry);

                    if (progress != null)
                    {
                        progress.Report(completionEntry);
                    }
                }
                catch (OperationCanceledException)
                {
                    pipeline.StopPipeline();
                    AppendPipelineLogTail(pipeline, ref logFilePosition, result, progress);

                    result.WasCanceled = true;
                    result.Success = false;
                    result.ExitCode = -1;
                    result.SummaryMessage = "Debug-Ausführung wurde abgebrochen.";

                    var canceledEntry = new DebugLogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = DebugLogLevel.Warn,
                        Source = string.IsNullOrWhiteSpace(currentCommandName) ? "Pipeline" : currentCommandName,
                        Message = result.SummaryMessage
                    };

                    result.Logs.Add(canceledEntry);


                    if (progress != null)
                    {
                        progress.Report(canceledEntry);
                    }
                }
                catch (Exception ex)
                {
                    pipeline.StopPipeline();
                    AppendPipelineLogTail(pipeline, ref logFilePosition, result, progress);

                    reportCurrentParameters(lastCommand, DebugLogLevel.Error, "Current parameters on exception");

                    var errorEntry = new DebugLogEntry
                    {
                        Timestamp = DateTime.Now,
                        Level = DebugLogLevel.Error,
                        Source = string.IsNullOrWhiteSpace(currentCommandName) ? "PipelineDebugExecutionService" : currentCommandName,
                        Message = ex.Message,
                        ExceptionText = ex.ToString()
                    };

                    result.Logs.Add(errorEntry);

                    if (progress != null)
                    {
                        progress.Report(errorEntry);
                    }

                    result.Success = false;
                    result.ExitCode = -1;
                    result.SummaryMessage = "Debug-Ausführung ist mit Exception fehlgeschlagen.";
                }
                finally
                {
                    cancellationRegistration.Dispose();
                    stopwatch.Stop();
                    result.Duration = stopwatch.Elapsed;

                    pipeline.OnExecuteCommand -= executeCommandHandler;
                    pipeline.OnExecutionCanceled -= canceledHandler;
                    pipeline.StopPipeline();
                }

                return result;
            }, cancellationToken);
        }

        private static string GetCommandDisplayName(DataCommand command)
        {
            if (command == null)
            {
                return "Pipeline";
            }

            var typeName = command.GetType().Name;
            if (string.IsNullOrWhiteSpace(command.Title))
            {
                return typeName;
            }

            return command.Title + " (" + typeName + ")";
        }

        private static string FormatParameters(CommandParameters parameters)
        {
            if (parameters == null || parameters.Count == 0)
            {
                return "<leer>";
            }

            var pairs = parameters.Select(p =>
            {
                var key = !string.IsNullOrWhiteSpace(p.Token) ? p.Token : p.Name;
                return key + "=" + (p.Value != null ? p.Value.ToString() : "<null>");
            });

            return string.Join(", ", pairs);
        }

        private static long GetCurrentPipelineLogFileLength(Pipeline pipeline)
        {
            var path = GetPipelineLogPath(pipeline);
            if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
            {
                return 0;
            }

            return new FileInfo(path).Length;
        }

        private static string GetPipelineLogPath(Pipeline pipeline)
        {
            if (pipeline == null || string.IsNullOrWhiteSpace(pipeline.Name))
            {
                return null;
            }

            return Path.Combine(LogManager.Instance.DefaultLogDirectory, pipeline.Name + ".log");
        }

        private static void AppendPipelineLogTail(Pipeline pipeline, ref long currentPosition, DebugResult result, IProgress<DebugLogEntry> progress)
        {
            var logPath = GetPipelineLogPath(pipeline);
            if (string.IsNullOrWhiteSpace(logPath) || !File.Exists(logPath))
            {
                return;
            }

            using (var stream = new FileStream(logPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                if (currentPosition < 0 || currentPosition > stream.Length)
                {
                    currentPosition = 0;
                }

                stream.Seek(currentPosition, SeekOrigin.Begin);

                using (var reader = new StreamReader(stream))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line))
                        {
                            continue;
                        }

                        var entry = new DebugLogEntry
                        {
                            Timestamp = DateTime.Now,
                            Level = DebugLogLevel.Info,
                            Source = pipeline != null ? pipeline.Name : "Pipeline",
                            Message = "PipelineLog => " + line
                        };

                        result.Logs.Add(entry);

                        if (progress != null)
                        {
                            progress.Report(entry);
                        }
                    }

                    currentPosition = stream.Position;
                }
            }
        }

    }
}
