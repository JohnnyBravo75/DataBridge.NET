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
    public class DbConnectionInfoTypeEditor : UserControl, ITypeEditor
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(object), typeof(DbConnectionInfoTypeEditor), new PropertyMetadata(null, OnValueChanged));

        private ComboBox comboBox;
        private PropertyItem boundPropertyItem;
        private bool suppressSelectionChanged;

        public DbConnectionInfoTypeEditor()
        {
            this.comboBox = new ComboBox { DisplayMemberPath = "Name", Margin = new Thickness(0) };
            this.comboBox.ItemsSource = GetConnectionInfoTypes();
            this.comboBox.SelectionChanged += this.ComboBox_SelectionChanged;
            this.Content = this.comboBox;
        }

        public object Value
        {
            get { return this.GetValue(ValueProperty); }
            set { this.SetValue(ValueProperty, value); }
        }

        public FrameworkElement ResolveEditor(PropertyItem propertyItem)
        {
            this.boundPropertyItem = propertyItem;
            var binding = new Binding("Value") { Source = propertyItem, Mode = propertyItem.IsReadOnly ? BindingMode.OneWay : BindingMode.TwoWay, ValidatesOnExceptions = true, ValidatesOnDataErrors = true };
            BindingOperations.SetBinding(this, ValueProperty, binding);
            return this;
        }

        private static IList<Type> GetConnectionInfoTypes()
        {
            Type baseType = null;
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    baseType = assembly.GetTypes().FirstOrDefault(t => t.Name == "DbConnectionInfoBase" && !t.IsAbstract);
                }
                catch { }
                if (baseType != null) break;
            }
            if (baseType == null) return new List<Type>();
            var types = new List<Type>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    types.AddRange(assembly.GetTypes().Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t)).OrderBy(t => t.Name));
                }
                catch { }
            }
            return types;
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var editor = d as DbConnectionInfoTypeEditor;
            if (editor != null) editor.SyncComboBoxToValue(e.NewValue);
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
                    var items = this.comboBox.ItemsSource as IList<Type>;
                    if (items != null) this.comboBox.SelectedItem = items.FirstOrDefault(t => t == valueType);
                }
            }
            finally
            {
                this.suppressSelectionChanged = false;
            }
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.suppressSelectionChanged) return;
            var selectedType = this.comboBox.SelectedItem as Type;
            if (selectedType == null) return;
            try
            {
                var newInstance = Activator.CreateInstance(selectedType);
                if (this.boundPropertyItem != null) this.boundPropertyItem.Value = newInstance;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Fehler: " + ex.Message, "Fehler", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
