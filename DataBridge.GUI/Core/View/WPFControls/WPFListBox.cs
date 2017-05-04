namespace DataBridge.GUI.Core.View.WPFControls
{
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;

    public class WPFListBox : ListBox
    {
        public WPFListBox()
        {
            this.HorizontalContentAlignment = HorizontalAlignment.Stretch;
        }

        public static bool GetCanFocusParent(DependencyObject obj)
        {
            return (bool)obj.GetValue(CanFocusParentProperty);
        }

        public static void SetCanFocusParent(DependencyObject obj, bool value)
        {
            obj.SetValue(CanFocusParentProperty, value);
        }

        // Using a DependencyProperty as the backing store for CanFocusParent.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CanFocusParentProperty = DependencyProperty.RegisterAttached("CanFocusParent", typeof(bool), typeof(WPFListBox), new UIPropertyMetadata(false, OnCanFocusParentChanged));

        private static void OnCanFocusParentChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var element = obj as UIElement;
            if (element == null) return;

            if ((bool)args.NewValue)
                element.PreviewMouseDown += FocusOnParent;
            else
                element.PreviewMouseDown -= FocusOnParent;
        }

        private static void FocusOnParent(object sender, RoutedEventArgs e)
        {
            var listBoxItem = VisualUpwardSearch<ListBoxItem>(sender as DependencyObject) as ListBoxItem;
            if (listBoxItem != null) listBoxItem.IsSelected = true;
        }

        public static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
            {
                source = VisualTreeHelper.GetParent(source);
            }

            return source;
        }
    }
}