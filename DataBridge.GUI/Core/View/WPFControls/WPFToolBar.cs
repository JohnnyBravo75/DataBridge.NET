namespace DataBridge.GUI.Core.View.WPFControls
{
    using System.Windows.Controls;
    using System.Windows.Threading;

    public class WPFToolBar : ToolBar
    {
        private delegate void IvalidateMeasureJob();

        public override void OnApplyTemplate()
        {
            this.Dispatcher.BeginInvoke(new IvalidateMeasureJob(this.InvalidateMeasure), DispatcherPriority.Background, null);
            base.OnApplyTemplate();
        }
    }
}