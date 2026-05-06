using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Wpf.Toolkit.PropertyEditors
{
    /// <summary>
    /// An editable ComboBox property editor that shows available pipeline tokens
    /// (e.g. {File}, {Data}) as autocomplete suggestions for string properties.
    /// The user can still type any free-form value.
    /// </summary>
    public partial class TokenStringEditor : UserControl, ITypeEditor
    {
        // ************************************Dependency Properties**********************************************

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(string),
                typeof(TokenStringEditor),
                new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// The list of available token strings, e.g. ["{File}", "{Data}", "{DirectoryName}"].
        /// Set by the parent DataCommandControl before the editor is displayed.
        /// </summary>
        public static readonly DependencyProperty AvailableTokensProperty =
            DependencyProperty.Register(
                "AvailableTokens",
                typeof(IList<string>),
                typeof(TokenStringEditor),
                new PropertyMetadata(null));

        // ************************************Constructor**********************************************

        public TokenStringEditor()
        {
            this.InitializeComponent();
        }

        // ************************************Properties**********************************************

        /// <summary>The current string value bound to the PropertyItem.</summary>
        public string Value
        {
            get { return (string)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        /// <summary>Token suggestions shown in the dropdown.</summary>
        public IList<string> AvailableTokens
        {
            get { return (IList<string>)this.GetValue(AvailableTokensProperty); }
            set { this.SetValue(AvailableTokensProperty, value); }
        }

        // ************************************ITypeEditor**********************************************

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            // Two-way bind the ComboBox text to the PropertyGrid item value
            var binding = new Binding("Value")
            {
                Source = propertyItem,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                ValidatesOnExceptions = true,
                ValidatesOnDataErrors = true,
                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
            };

            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }
    }
}
