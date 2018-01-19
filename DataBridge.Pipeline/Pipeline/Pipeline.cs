using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Serialization;
using DataBridge.Extensions;
using DataBridge.Helper;

namespace DataBridge
{
    [Serializable]
    public class Pipeline : IDisposable
    {
        // ************************************ Member **********************************************

        private List<DataCommand> commands = new List<DataCommand>();
        private bool useStreaming = true;
        private int streamingBlockSize = 100000;
        private string name = "";
        private string workingDirectory = @".\";

        // ************************************ Constructor **********************************************
        public Pipeline()
        {
        }

        public Pipeline(string name)
        {
            this.Name = name;
        }

        // ************************************ Properties **********************************************

        [XmlArray("Commands", IsNullable = false)]
        public List<DataCommand> Commands
        {
            get { return this.commands; }
            set { this.commands = value; }
        }

        [XmlAttribute]
        public bool UseStreaming
        {
            get { return this.useStreaming; }
            set { this.useStreaming = value; }
        }

        [XmlAttribute]
        public int StreamingBlockSize
        {
            get { return this.streamingBlockSize; }
            set { this.streamingBlockSize = value; }
        }

        [XmlIgnore]
        public string Name
        {
            get { return this.name; }
            protected set { this.name = value; }
        }

        [XmlAttribute]
        public string WorkingDirectory
        {
            get { return this.workingDirectory; }
            set { this.workingDirectory = value; }
        }

        [XmlIgnore]
        public DateTime? BlackoutStart { get; set; }

        [XmlAttribute(DataType = "string")]
        [Browsable(false)]
        public string BlackoutStartTime
        {
            get { return this.BlackoutStart.HasValue ? this.BlackoutStart.ToString("hh:mi:ss") : string.Empty; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.BlackoutStart = DateTimeUtil.TryParseExact(value, "hh:mi:ss");
                }
            }
        }

        [XmlIgnore]
        public DateTime? BlackoutEnd { get; set; }

        [XmlAttribute(DataType = "string")]
        [Browsable(false)]
        public string BlackoutEndTime
        {
            get { return this.BlackoutEnd.HasValue ? this.BlackoutEnd.ToString("hh:mi:ss") : string.Empty; }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    this.BlackoutEnd = DateTimeUtil.TryParseExact(value, "hh:mi:ss");
                }
            }
        }

        [XmlIgnore]
        public bool IsInBlackout
        {
            get
            {
                if (!this.BlackoutStart.HasValue)
                {
                    return false;
                }

                if (!this.BlackoutEnd.HasValue)
                {
                    return false;
                }

                if (this.BlackoutStart.Value.TimeOfDay == TimeSpan.Zero &&
                    this.BlackoutEnd.Value.TimeOfDay == TimeSpan.Zero)
                {
                    return false;
                }

                if (DateTime.Now.TimeOfDay.IsInTimeRange(this.BlackoutStart.Value.TimeOfDay, this.BlackoutEnd.Value.TimeOfDay))
                {
                    return true;
                }

                return false;
            }
        }

        // ************************************ Functions **********************************************

        public static IEnumerable<DataCommand> GetAllAvailableCommands()
        {
            var availableCommands = typeof(DataCommand)
                                    .Assembly.GetTypes()
                                    .Where(t => t.IsSubclassOf(typeof(DataCommand)) && !t.IsAbstract)
                                    .Select(t => (DataCommand)Activator.CreateInstance(t));

            return availableCommands;
        }

        public static void Save(string fileName, Pipeline pipeline)
        {
            var serializer = new XmlSerializerHelper<Pipeline>();
            serializer.Save(fileName, pipeline);
        }

        public static Pipeline Load(string fileName)
        {
            var serializer = new XmlSerializerHelper<Pipeline>();
            var pipeline = serializer.Load(fileName);
            pipeline.Name = Path.GetFileNameWithoutExtension(fileName);
            foreach (var command in pipeline.Commands)
            {
                command.WeakPipeLine = new WeakReference<Pipeline>(pipeline);
            }
            return pipeline;
        }

        public static void LoadAndExecute(string fileName)
        {
            var pipeline = Load(fileName);
            pipeline.ExecutePipeline();
        }

        public void StopPipeline()
        {
            //LogManager.Instance.LogDebugFormat(this.GetType(), "Deinitialize pipeline '{0}'", this.Name);

            foreach (var command in this.Commands)
            {
                command.DeInitialize();
            }
        }

        public event Action<DataCommand> OnExecuteCommand;

        public event EventHandler<EventArgs<string>> OnExecutionCanceled;

        protected bool ExecuteCommand(DataCommand currentCmd, int loopCounter, DataCommand previousCmd = null)
        {
            try
            {
                if (this.IsInBlackout)
                {
                    LogManager.Instance.LogNamedDebugFormat(this.Name, this.GetType(), "Blackout detected. Ignoring execution of pipeline '{0}'", this.Name);
                    if (this.OnExecutionCanceled != null)
                    {
                        this.OnExecutionCanceled(this, new EventArgs<string>("Blackout"));
                    }
                    return false;
                }

                if (currentCmd != null)
                {
                    // Execute the currentCmd command as often as possible and pull the parameters out and push them into the next command
                    int i = 0;
                    CommandParameters lastParameter = null;
                    TokenManager.Instance.SetTokens(currentCmd.ExecuteParameters.ToDictionary(), this.Name);

                    var commandParametersList = new List<CommandParameters> { currentCmd.ExecuteParameters };
                    foreach (CommandParameters outParameters in currentCmd.ExecuteCommand(commandParametersList))
                    {
                        if (this.OnExecuteCommand != null)
                        {
                            this.OnExecuteCommand(currentCmd);
                        }

                        if (currentCmd.HasChildCommands)
                        {
                            // execute the childs
                            var nextChildCmd = currentCmd.GetFirstChild();
                            if (nextChildCmd != null)
                            {
                                if (i == 0)
                                {
                                    if (!nextChildCmd.IsInitialized)
                                    {
                                        nextChildCmd.Initialize();
                                        nextChildCmd.UseStreaming = this.UseStreaming;
                                        nextChildCmd.StreamingBlockSize = this.StreamingBlockSize;
                                    }
                                }

                                nextChildCmd.SetParameters(outParameters, this.OnSignalExecution, currentCmd);
                                nextChildCmd.LoopCounter = i;
                                nextChildCmd.BeforeExecute();
                            }
                        }

                        lastParameter = outParameters;

                        i++;
                    }

                    currentCmd.AfterExecute();

                    // execute the siblings
                    var nextSiblingCmd = currentCmd.GetNextSibling();
                    if (nextSiblingCmd != null)
                    {
                        if (!nextSiblingCmd.IsInitialized)
                        {
                            nextSiblingCmd.Initialize();
                            nextSiblingCmd.UseStreaming = this.UseStreaming;
                            nextSiblingCmd.StreamingBlockSize = this.StreamingBlockSize;
                        }

                        if (lastParameter == null)
                        {
                            lastParameter = currentCmd.GetCurrentOutParameters();
                        }
                        nextSiblingCmd.SetParameters(lastParameter, this.OnSignalExecution, currentCmd);
                        nextSiblingCmd.LoopCounter = loopCounter;
                        nextSiblingCmd.BeforeExecute();
                    }
                }
            }
            catch (ThreadAbortException ex)
            {
                // ignore them
            }
            catch (ThreadInterruptedException ex)
            {
                // ignore them
            }
            return true;
        }

        protected bool OnSignalExecution(InitializationResult initializationResult)
        {
            var currentCmd = initializationResult.Command;
            return this.ExecuteCommand(currentCmd, initializationResult.LoopCounter, initializationResult.PrevCommand);
        }

        public bool ExecutePipeline(CommandParameters inParameters = null)
        {
            var startCommand = this.Commands.FirstOrDefault();
            if (startCommand == null)
            {
                throw new Exception(string.Format("No Commands in pipeline '{0}'", this.Name));
            }

            startCommand.Initialize();
            startCommand.UseStreaming = this.UseStreaming;
            startCommand.StreamingBlockSize = this.StreamingBlockSize;
            startCommand.SetParameters(inParameters, this.OnSignalExecution);

            return startCommand.BeforeExecute();
        }

        public List<string> ValidatePipeline()
        {
            var validationResults = new List<string>();
            var allCommands = this.Commands.Traverse(x => x.Commands);

            foreach (var command in allCommands)
            {
                var messages = command.Validate(null, ValidationContext.Static);
                foreach (var message in messages)
                {
                    validationResults.Add(command.GetType().Name + ": " + message);
                }
            }

            return validationResults;
        }

        public void Dispose()
        {
            if (this.commands != null)
            {
                foreach (var command in this.commands)
                {
                    command.Dispose();
                }

                this.commands.Clear();
            }
        }

        public bool Equals(Pipeline other)
        {
            return string.Equals(this.Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            if (obj.GetType() != this.GetType())
            {
                return false;
            }
            return this.Equals((Pipeline)obj);
        }

        public override int GetHashCode()
        {
            return (this.Name != null ? this.Name.GetHashCode() : 0);
        }
    }
}