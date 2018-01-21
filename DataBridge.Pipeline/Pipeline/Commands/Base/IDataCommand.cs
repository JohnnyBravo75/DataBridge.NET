using System.Collections.Generic;

namespace DataBridge
{
    public interface IDataCommand
    {
        bool HasChildCommands { get; }

        List<DataCommand> Commands { get; set; }

        int StreamingBlockSize { get; set; }

        bool UseStreaming { get; set; }

        void AddChild(DataCommand childNode, int index = -1);

        void Dispose();

        IEnumerable<CommandParameters> ExecuteCommand(IEnumerable<CommandParameters> inParameters);

        IList<string> Validate(CommandParameters parameters, ValidationContext context);

    }
}