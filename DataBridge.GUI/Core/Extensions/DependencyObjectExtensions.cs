namespace DataBridge.GUI.Core.Extensions
{
    using System.Windows;
    using System.Windows.Media;

    public static class DependencyObjectExtensions
    {
        public static T FindParent<T>(this DependencyObject child) where T : DependencyObject
        {
            DependencyObject parent = VisualTreeHelper.GetParent(child);

            if (parent is T)
            {
                return parent as T;
            }
            else
            {
                return parent != null ? FindParent<T>(parent) : null;
            }
        }
    }
}
