namespace DataBridge.GUI.Core.Utils
{
    using System.Windows.Media;

    /// <summary>
    /// Helper class for the Visual Tree
    /// </summary>
    public class VisualTreeUtil
    {
        /// <summary>
        /// Gets the visual child.
        /// </summary>
        /// <typeparam name="T">the type</typeparam>
        /// <param name="parent">The parent.</param>
        /// <returns>the visual child</returns>
        public static T GetVisualChild<T>(Visual parent) where T : Visual
        {
            T child = default(T);
            int numVisuals = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < numVisuals; i++)
            {
                Visual v = (Visual)VisualTreeHelper.GetChild(parent, i);
                child = v as T;
                if (child == null)
                {
                    child = GetVisualChild<T>(v);
                }

                if (child != null)
                {
                    break;
                }
            }

            return child;
        }
    }
}
