using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DataBridge.GUI.UserControls
{
    /// <summary>
    /// Displays a horizontal row of colored badges for each parameter of a DataCommand.
    /// Use DirectionFilter ("In", "Out", "InOut", or empty for all) to show only a subset.
    /// When IsClickable=true, clicking a badge copies the {Token} to the clipboard.
    /// </summary>
    public partial class ParameterBadgesControl : UserControl
    {
        public static readonly DependencyProperty ParametersProperty =
            DependencyProperty.Register(
                "Parameters",
                typeof(IEnumerable<CommandParameter>),
                typeof(ParameterBadgesControl),
                new PropertyMetadata(null, OnFilterChanged));

        /// <summary>
        /// Comma-separated direction filter, e.g. "In" or "Out,InOut".
        /// Empty = show all directions.
        /// </summary>
        public static readonly DependencyProperty DirectionFilterProperty =
            DependencyProperty.Register(
                "DirectionFilter",
                typeof(string),
                typeof(ParameterBadgesControl),
                new PropertyMetadata(string.Empty, OnFilterChanged));

        /// <summary>
        /// When true, badges act as buttons — clicking copies "{Token}" to the clipboard.
        /// </summary>
        public static readonly DependencyProperty IsClickableProperty =
            DependencyProperty.Register(
                "IsClickable",
                typeof(bool),
                typeof(ParameterBadgesControl),
                new PropertyMetadata(false));

        /// <summary>Filtered list used by the ItemsControl in XAML.</summary>
        public static readonly DependencyProperty FilteredParametersProperty =
            DependencyProperty.Register(
                "FilteredParameters",
                typeof(IEnumerable<CommandParameter>),
                typeof(ParameterBadgesControl),
                new PropertyMetadata(null));

        public ParameterBadgesControl()
        {
            this.InitializeComponent();
        }

        public IEnumerable<CommandParameter> Parameters
        {
            get { return (IEnumerable<CommandParameter>)this.GetValue(ParametersProperty); }
            set { this.SetValue(ParametersProperty, value); }
        }

        public string DirectionFilter
        {
            get { return (string)this.GetValue(DirectionFilterProperty); }
            set { this.SetValue(DirectionFilterProperty, value); }
        }

        public bool IsClickable
        {
            get { return (bool)this.GetValue(IsClickableProperty); }
            set { this.SetValue(IsClickableProperty, value); }
        }

        public IEnumerable<CommandParameter> FilteredParameters
        {
            get { return (IEnumerable<CommandParameter>)this.GetValue(FilteredParametersProperty); }
            private set { this.SetValue(FilteredParametersProperty, value); }
        }

        private static void OnFilterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ParameterBadgesControl)d).ApplyFilter();
        }

        private void ApplyFilter()
        {
            var all = this.Parameters;
            if (all == null)
            {
                this.FilteredParameters = null;
                return;
            }

            var filter = (this.DirectionFilter ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(filter))
            {
                this.FilteredParameters = all;
                return;
            }

            var allowed = filter.Split(',')
                .Select(s => s.Trim())
                .ToList();

            this.FilteredParameters = all
                .Where(p => allowed.Contains(p.Direction.ToString()))
                .ToList();
        }

        /// <summary>Handles badge click — copies "{Token}" to clipboard.</summary>
        private void Badge_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            if (btn == null) return;
            var param = btn.DataContext as CommandParameter;
            if (param == null) return;
            System.Windows.Clipboard.SetText("{" + param.Token + "}");
        }
    }
}

