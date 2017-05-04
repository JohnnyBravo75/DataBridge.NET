using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Wpf.Toolkit.PropertyEditors
{
    /// <summary>
    /// Interaktionslogik für OpenFilePropertyEditor.xaml
    /// </summary>
    public partial class OpenFilePropertyEditor : UserControl, ITypeEditor
    {
        public static readonly DependencyProperty ValueProperty = DependencyProperty.Register("Value", typeof(string), typeof(OpenFilePropertyEditor), new PropertyMetadata(default(string), SetValue));

        /// <summary>
        /// Initializes a new instance of the <see cref="OpenFilePropertyEditor"/> class.
        /// </summary>
        public OpenFilePropertyEditor()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public string Value
        {
            get { return (string)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Resolves the editor.
        /// </summary>
        /// <param name="propertyItem">The property item.</param>
        /// <returns></returns>
        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            var binding = new Binding("Value")
            {
                Source = propertyItem,
                Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay,
                ValidatesOnExceptions = true,
                ValidatesOnDataErrors = true
            };

            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs"/> instance containing the event data.</param>
        private static void SetValue(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as OpenFilePropertyEditor;

            if (ctrl != null && e.NewValue is string)
            {
                ctrl.Value = (string)e.NewValue;
            }
        }

        /// <summary>
        /// Handles the Click event of the btnOpenFile control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            var item = this.DataContext as PropertyItem;
            if (null == item)
            {
                return;
            }

            var fileDlg = new Microsoft.Win32.OpenFileDialog();
            fileDlg.ShowDialog();
            item.Value = fileDlg.FileName;
        }
    }
}