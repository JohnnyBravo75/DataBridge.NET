namespace DataBridge.GUI.Core.View.WPFControls
{
    using System.Windows;
    using System.Windows.Controls;

    public class WPFTreeView : TreeView
    {
        public static readonly DependencyProperty SelectedItem_Property = DependencyProperty.Register("CurrentItem", typeof(object), typeof(WPFTreeView), new UIPropertyMetadata(null));

        public WPFTreeView()
            : base()
        {
            this.SelectedItemChanged += this.WPFTreeView_SelectedItemChanged;
        }


        public object CurrentItem
        {
            get { return (object)this.GetValue(SelectedItem_Property); }
            set { this.SetValue(SelectedItem_Property, value); }
        }


        private void WPFTreeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (this.SelectedItem != null)
            {
                this.SetValue(SelectedItem_Property, this.SelectedItem);
            }
        }
    }
}