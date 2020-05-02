using System.Windows.Media;

namespace DataBridge.GUI.Core.View.WPFControls
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Threading;

    public class WPFButton : Button
    {
        private DispatcherTimer keyPressTimer;

        private bool clickAllowed = true;

        public bool IsMultiClickAllowed
        {
            get;
            set;
        }

        public WPFButton()
        {
            this.SetTimer();
            this.IsMultiClickAllowed = true;
            this.Background = new SolidColorBrush(Colors.Transparent);
            this.BorderBrush = new SolidColorBrush(Colors.LightGray);
        }

        private void SetTimer()
        {
            this.keyPressTimer = new DispatcherTimer();
            this.keyPressTimer.Interval = TimeSpan.FromMilliseconds(500);
            this.keyPressTimer.Tick += this.keyPressTimer_Tick;
        }

        private void keyPressTimer_Tick(object sender, EventArgs e)
        {
            this.keyPressTimer.Stop();
            this.clickAllowed = true;
        }

        protected override void OnClick()
        {
            if (this.IsMultiClickAllowed)
            {
                base.OnClick();
            }
            else if (this.clickAllowed)
            {
                base.OnClick();
                this.keyPressTimer.Start();
                this.clickAllowed = false;
            }
        }
    }
}