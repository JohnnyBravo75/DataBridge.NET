namespace DataBridge.GUI.Core.View.WPFControls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    public class WPFGridView : DataGrid
    {
        public static readonly DependencyProperty CellTemplateSelectorProperty = DependencyProperty.Register("Selector", typeof(DataTemplateSelector), typeof(WPFGridView), new FrameworkPropertyMetadata(null));

        public static readonly DependencyProperty DoubleClickCommandProperty = DependencyProperty.Register("DoubleClickCommand", typeof(ICommand), typeof(WPFGridView), new PropertyMetadata(SetDoubleClickCommand));

        public WPFGridView()
        {
            this.HorizontalGridLinesBrush = new SolidColorBrush(Colors.LightGray);
            this.VerticalGridLinesBrush = new SolidColorBrush(Colors.LightGray);
            this.CanUserAddRows = false;
            this.CanUserDeleteRows = false;
            this.HeadersVisibility = DataGridHeadersVisibility.Column;
            this.EnableColumnVirtualization = true;
            this.EnableRowVirtualization = true;
            VirtualizingPanel.SetVirtualizationMode(this, VirtualizationMode.Recycling);
            this.MouseDoubleClick += WPFGridView_MouseDoubleClick;
            this.DataContextChanged += this.WPFGridView_DataContextChanged;
        }

        public ICommand DoubleClickCommand
        {
            get
            {
                return (ICommand)GetValue(DoubleClickCommandProperty);
            }
            set
            {
                SetValue(DoubleClickCommandProperty, value);
            }
        }

        private static void SetDoubleClickCommand(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var ctrl = d as WPFGridView;

            if (ctrl != null && e.NewValue is ICommand)
            {
                ctrl.DoubleClickCommand = (ICommand)e.NewValue;
            }
        }

        public object CommandParameter { get; set; }

        private void WPFGridView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.DoubleClickCommand != null)
            {
                if (this.DoubleClickCommand.CanExecute(this.CommandParameter))
                {
                    this.DoubleClickCommand.Execute(this.CommandParameter);
                }
            }
        }

        private void WPFGridView_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var grid = sender as DataGrid;
            if (grid != null)
            {
                // Set DataContext into the columns
                foreach (DataGridColumn col in grid.Columns)
                {
                    col.SetValue(DataContextProperty, e.NewValue);
                    var header = col.Header as FrameworkElement;
                    if (header != null)
                    {
                        header.SetValue(DataContextProperty, e.NewValue);
                    }
                }
            }
        }

        public DataTemplateSelector CellTemplateSelector
        {
            get { return (DataTemplateSelector)this.GetValue(CellTemplateSelectorProperty); }
            set { this.SetValue(CellTemplateSelectorProperty, value); }
        }



        protected override void OnAutoGeneratingColumn(DataGridAutoGeneratingColumnEventArgs e)
        {
            if (this.CellTemplateSelector != null)
            {
                e.Cancel = true;
                this.Columns.Add(new DataGridTemplateColumn
                {
                    Header = e.Column.Header,
                    CellTemplateSelector = this.CellTemplateSelector
                });
            }
        }
    }
}