namespace DataBridge.GUI.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using DataBridge;
    using DataBridge.GUI.UserControls;

    /// <summary>
    /// Wraps a DataCommand for display in the pipeline editor.
    /// Carries the command itself plus the computed parameter flow
    /// items that connect this command to the next one in the pipeline.
    /// </summary>
    public class PipelineStepViewModel
    {
        // ************************************Constructor**********************************************

        public PipelineStepViewModel(DataCommand command, DataCommand nextCommand, IList<DataCommand> previousCommands)
        {
            this.Command = command;
            this.FlowItems = BuildFlowItems(command, nextCommand);
            this.NextCommandTitle = nextCommand != null ? nextCommand.Title : null;
            this.ValidationMessages = command.Validate(null, ValidationContext.Static);
            this.AvailableTokens = BuildAvailableTokens(previousCommands);
        }

        // ************************************Properties**********************************************

        /// <summary>The DataCommand this step wraps.</summary>
        public DataCommand Command { get; private set; }

        /// <summary>Matched Out→In parameters flowing to the next command.</summary>
        public IList<ParameterFlowItem> FlowItems { get; private set; }

        /// <summary>Title of the next command (for flow label), or null if last step.</summary>
        public string NextCommandTitle { get; private set; }

        /// <summary>Static validation results for this command.</summary>
        public IList<string> ValidationMessages { get; private set; }

        /// <summary>True if there are parameters flowing to the next command.</summary>
        public bool HasFlow { get { return this.FlowItems != null && this.FlowItems.Count > 0; } }

        /// <summary>
        /// Token strings available from all previous pipeline steps, e.g. "{File}", "{Data}".
        /// Used to populate the autocomplete dropdown in the PropertyGrid.
        /// </summary>
        public IList<string> AvailableTokens { get; private set; }

        // ************************************Helpers**********************************************

        /// <summary>
        /// Collects all Out/InOut token names from all previous commands as "{Token}" strings.
        /// </summary>
        private static IList<string> BuildAvailableTokens(IList<DataCommand> previousCommands)
        {
            var tokens = new List<string>();
            if (previousCommands == null)
            {
                return tokens;
            }

            var seen = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var cmd in previousCommands)
            {
                foreach (var param in cmd.Parameters
                    .Where(p => p.Direction == Directions.Out || p.Direction == Directions.InOut))
                {
                    var token = "{" + param.Token + "}";
                    if (seen.Add(token))
                    {
                        tokens.Add(token);
                    }
                }
            }
            return tokens;
        }

        /// <summary>
        /// Computes which Out/InOut parameters of <paramref name="from"/> flow to
        /// <paramref name="to"/>. Shows all Out/InOut parameters; marks matched ones.
        /// </summary>
        private static IList<ParameterFlowItem> BuildFlowItems(DataCommand from, DataCommand to)
        {
            var result = new List<ParameterFlowItem>();
            if (from == null)
            {
                return result;
            }

            var outParams = from.Parameters
                .Where(p => p.Direction == Directions.Out || p.Direction == Directions.InOut)
                .ToList();

            if (outParams.Count == 0)
            {
                return result;
            }

            var inParamNames = new HashSet<string>(System.StringComparer.OrdinalIgnoreCase);
            if (to != null)
            {
                foreach (var p in to.Parameters.Where(p => p.Direction == Directions.In || p.Direction == Directions.InOut))
                {
                    inParamNames.Add(p.Name);
                }
            }

            foreach (var outParam in outParams)
            {
                result.Add(new ParameterFlowItem
                {
                    ParameterName = outParam.Name,
                    TokenName = outParam.Token,
                    IsMatched = inParamNames.Contains(outParam.Name)
                });
            }

            return result;
        }
    }
}
