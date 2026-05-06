using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace Wpf.Toolkit.PropertyEditors
{
    /// <summary>
    /// Property editor that shows a ComboBox with all concrete subclasses of FormatterBase.
    /// Selecting an entry replaces the current formatter instance with a new instance of that type.
    /// </summary>
    public class FormatterTypeEditor : UserControl, ITypeEditor
    {
        // ************************************Dependency Properties**********************************************

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(
                "Value",
                typeof(object),
                typeof(FormatterTypeEditor),
                new PropertyMetadata(null, OnValueChanged));

        // ************************************Fields**********************************************

        private ComboBox comboBox;
        private PropertyItem boundPropertyItem;
        private bool suppressSelectionChanged;

        // ************************************Constructor**********************************************

        public FormatterTypeEditor()
        {
            this.comboBox = new ComboBox
            {
                DisplayMemberPath = "Name",
                Margin = new Thickness(0)
            };

            this.comboBox.ItemsSource = GetFormatterTypes();
            this.comboBox.SelectionChanged += this.ComboBox_SelectionChanged;

            this.Content = this.comboBox;
        }

        // ************************************Properties**********************************************

        public object Value
        {
            get { return this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        // ************************************Functions**********************************************

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            this.boundPropertyItem = propertyItem;

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
        /// Returns all concrete (non-abstract) subclasses of FormatterBase found in all loaded assemblies.
        /// </summary>
        private static IList<Type> GetFormatterTypes()
        {
            Type formatterBaseType = null;

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    formatterBaseType = assembly.GetTypes()
                        .FirstOrDefault(t => t.Name == "FormatterBase" && !t.IsAbstract == false);
                }
                catch
                {
                    // skip assemblies that can't be reflected
                }

                if (formatterBaseType != null)
                {
                    break;
                }
            }

            if (formatterBaseType == null)
            {
                return new List<Type>();
            }

            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var found = assembly.GetTypes()
                        .Where(t => !t.IsAbstract && formatterBaseType.IsAssignableFrom(t))
                        .OrderBy(t => t.Name);
                    types.AddRange(found);
                }
                catch
                {
                    // skip assemblies that can't be reflected
                }
            }

            return types;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as FormatterTypeEditor;
            if (editor == null)
            {
                return;
            }

            editor.SyncComboBoxToValue(e.NewValue);
        }

        private void SyncComboBoxToValue(object value)
        {
            this.suppressSelectionChanged = true;
            try
            {
                if (value == null)
                {
                    this.comboBox.SelectedItem = null;
                }
                else
                {
                    var valueType = value.GetType();
                    this.comboBox.SelectedItem = (this.comboBox.ItemsSource as IEnumerable<Type>)
                        ?.FirstOrDefault(t => t == valueType);
                }
            }
            finally
            {
                this.suppressSelectionChanged = false;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.suppressSelectionChanged)
            {
                return;
            }

            var selectedType = this.comboBox.SelectedItem as Type;
            if (selectedType == null)
            {
                return;
            }

            // Create a new instance of the selected formatter type
            try
            {
                var newFormatter = Activator.CreateInstance(selectedType);
                if (this.boundPropertyItem != null)
                {
                    this.boundPropertyItem.Value = newFormatter;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("Fehler beim Erstellen des Formatters '{0}':\n{1}", selectedType.Name, ex.Message),
                    "Fehler",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
