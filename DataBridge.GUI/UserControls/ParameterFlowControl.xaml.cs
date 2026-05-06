using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace DataBridge.GUI.UserControls
{
    /// <summary>
    /// Displays the parameter flow between two consecutive DataCommands in the pipeline.
    /// Shows which Out/InOut parameters from the previous command match In/InOut parameters
    /// of the next command by name, and the token name used to reference them.
    /// </summary>
    public partial class ParameterFlowControl : UserControl
    {
        public static readonly DependencyProperty FlowItemsProperty =
            DependencyProperty.Register(
                "FlowItems",
                typeof(IList<ParameterFlowItem>),
                typeof(ParameterFlowControl),
                new PropertyMetadata(null, OnFlowItemsChanged));

        public static readonly DependencyProperty HasFlowItemsProperty =
            DependencyProperty.Register(
                "HasFlowItems",
                typeof(bool),
                typeof(ParameterFlowControl),
                new PropertyMetadata(false));

        public static readonly DependencyProperty FromCommandTitleProperty =
            DependencyProperty.Register(
                "FromCommandTitle",
                typeof(string),
                typeof(ParameterFlowControl),
                new PropertyMetadata(string.Empty));

        public static readonly DependencyProperty ToCommandTitleProperty =
            DependencyProperty.Register(
                "ToCommandTitle",
                typeof(string),
                typeof(ParameterFlowControl),
                new PropertyMetadata(string.Empty));

        public ParameterFlowControl()
        {
            this.InitializeComponent();
        }

        public IList<ParameterFlowItem> FlowItems
        {
            get { return (IList<ParameterFlowItem>)this.GetValue(FlowItemsProperty); }
            set { this.SetValue(FlowItemsProperty, value); }
        }

        public bool HasFlowItems
        {
            get { return (bool)this.GetValue(HasFlowItemsProperty); }
            set { this.SetValue(HasFlowItemsProperty, value); }
        }

        public string FromCommandTitle
        {
            get { return (string)this.GetValue(FromCommandTitleProperty); }
            set { this.SetValue(FromCommandTitleProperty, value); }
        }

        public string ToCommandTitle
        {
            get { return (string)this.GetValue(ToCommandTitleProperty); }
            set { this.SetValue(ToCommandTitleProperty, value); }
        }

        private static void OnFlowItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as ParameterFlowControl;
            if (ctrl != null)
            {
                var items = e.NewValue as IList<ParameterFlowItem>;
                ctrl.HasFlowItems = items != null && items.Count > 0;
            }
        }
    }

    /// <summary>
    /// Represents a single matched parameter flowing from one command to the next.
    /// </summary>
    public class ParameterFlowItem
    {
        /// <summary>Name of the Out/InOut parameter on the source command.</summary>
        public string ParameterName { get; set; }

        /// <summary>Token name used to reference this value in the target command (e.g. {File}).</summary>
        public string TokenName { get; set; }

        /// <summary>Whether this output parameter is consumed by the next command.</summary>
        public bool IsMatched { get; set; }
    }
}
