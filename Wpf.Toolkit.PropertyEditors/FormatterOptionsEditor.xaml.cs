using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Wpf.Toolkit.PropertyEditors
{
    public partial class FormatterOptionsEditor : UserControl, ITypeEditor
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(IEnumerable),
                typeof(FormatterOptionsEditor),
                new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        public FormatterOptionsEditor()
        {
            this.InitializeComponent();
        }

        public IEnumerable Value
        {
            get { return (IEnumerable)this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
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
