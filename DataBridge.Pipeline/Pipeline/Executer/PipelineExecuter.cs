using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using DataBridge.Helper;

namespace DataBridge
{
    public class PipelineExecuter : IPipelineExecuter
    {
        public PipelineExecuter(Pipeline pipeline)
        {
            this.CurrentPipeline = pipeline;
        }

        public Pipeline CurrentPipeline { get; set; }

        public event Action<DataCommand> OnExecuteCommand;

        public event EventHandler<EventArgs<string>> OnExecutionCanceled;

        protected bool ExecuteCommand(DataCommand currentCmd, int loopCounter, DataCommand previousCmd = null)
        {
            try
            {
                if (this.CurrentPipeline.IsInBlackout)
                {
                    LogManager.Instance.LogNamedDebugFormat(this.CurrentPipeline.Name, this.GetType(), "Blackout detected. Ignoring execution of pipeline '{0}'", this.CurrentPipeline.Name);
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
                    TokenManager.Instance.SetTokens(currentCmd.ExecuteParameters.ToDictionary(), this.CurrentPipeline.Name);

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
                                        nextChildCmd.UseStreaming = this.CurrentPipeline.UseStreaming;
                                        nextChildCmd.StreamingBlockSize = this.CurrentPipeline.StreamingBlockSize;
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
                            nextSiblingCmd.UseStreaming = this.CurrentPipeline.UseStreaming;
                            nextSiblingCmd.StreamingBlockSize = this.CurrentPipeline.StreamingBlockSize;
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
            var startCommand = this.CurrentPipeline.Commands.FirstOrDefault();
            if (startCommand == null)
            {
                throw new Exception(string.Format("No Commands in pipeline '{0}'", this.CurrentPipeline.Name));
            }

            startCommand.Initialize();
            startCommand.UseStreaming = this.CurrentPipeline.UseStreaming;
            startCommand.StreamingBlockSize = this.CurrentPipeline.StreamingBlockSize;
            startCommand.SetParameters(inParameters, this.OnSignalExecution);

            return startCommand.BeforeExecute();
        }
    }
}