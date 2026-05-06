namespace DataBridge.GUI.Converters
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Windows.Data;
    using DataBridge;
    using DataBridge.GUI.ViewModels;

    /// <summary>
    /// Converts a list of child DataCommands into a list of PipelineStepViewModels
    /// so that ParameterFlowControl can be displayed between sub-steps.
    /// </summary>
    public class CommandsToStepsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var commands = value as IList<DataCommand>;
            if (commands == null || commands.Count == 0)
            {
                return null;
            }

            var steps = new List<PipelineStepViewModel>();
            for (int i = 0; i < commands.Count; i++)
            {
                var previousCommands = new List<DataCommand>();
                for (int j = 0; j < i; j++)
                {
                    previousCommands.Add(commands[j]);
                }

                var nextCommand = (i < commands.Count - 1) ? commands[i + 1] : null;
                steps.Add(new PipelineStepViewModel(commands[i], nextCommand, previousCommands));
            }

            return steps;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
