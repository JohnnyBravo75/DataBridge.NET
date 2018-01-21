using System;
using DataBridge.Helper;

namespace DataBridge
{
    public interface IPipelineExecuter
    {
        Pipeline CurrentPipeline { get; set; }

        event Action<DataCommand> OnExecuteCommand;

        event EventHandler<EventArgs<string>> OnExecutionCanceled;

        bool ExecutePipeline(CommandParameters inParameters = null);
    }
}