namespace DataBridge.GUI.Core.Utils
{
    using System;
    using System.Windows;
    using System.Windows.Data;

    /// <summary>
    /// Ein Proxy Element, um den DataContext eines UserControls auch in einem DataTemplate verfügbar zu machen
    /// </summary>
    public class DataContextProxy : FrameworkElement
    {
        /// <summary>
        /// die DataSource als DependencyProperty
        /// </summary>
        private static readonly DependencyProperty DataSourceProperty = DependencyProperty.Register("DataSource", typeof(object), typeof(DataContextProxy), null);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataContextProxy" /> class.
        /// </summary>
        public DataContextProxy()
        {
            this.Loaded += new RoutedEventHandler(this.DataContextProxy_Loaded);
        }

        /// <summary>
        /// Gets or sets die DataSource
        /// </summary>
        public object DataSource
        {
            get
            {
                return this.GetValue(DataSourceProperty);
            }

            set
            {
                this.SetValue(DataSourceProperty, value);
            }
        }

        /// <summary>
        /// Gets or sets den Namen der BindingProperty
        /// </summary>
        public string BindingPropertyName { get; set; }

        /// <summary>
        /// Gets or sets den Binding Mode
        /// </summary>
        public BindingMode BindingMode { get; set; }

        /// <summary>
        /// Regiert auf das Laden des DataContextProxies
        /// </summary>
        /// <param name="sender">Auslöser des Events</param>
        /// <param name="e">Argumente des Events</param>
        private void DataContextProxy_Loaded(object sender, RoutedEventArgs e)
        {
            Binding binding = new Binding();
            if (!String.IsNullOrEmpty(this.BindingPropertyName))
            {
                binding.Path = new PropertyPath(this.BindingPropertyName);
            }

            binding.Source = this.DataContext;
            binding.Mode = this.BindingMode;
            this.SetBinding(DataSourceProperty, binding);
        }
    }
}