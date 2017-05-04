using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows.Input;
using System.Windows.Threading;
using DataBridge.Extensions;
using DataBridge.GUI.Core.DependencyInjection;
using DataBridge.GUI.Core.View;
using DataBridge.GUI.ViewModels;
using Microsoft.Practices.Unity;

namespace DataBridge.GUI.UserControls
{
    using System.Windows;
    using System.Windows.Data;
    using Core.View.WPFControls;

    public partial class ServiceControllerControl : WPFUserControl
    {
        // ************************************Fields**********************************************

        private DispatcherTimer checkTimer = new DispatcherTimer();

        public static readonly DependencyProperty CurrentServiceProperty = DependencyProperty.Register("CurrentService", typeof(ServiceController), typeof(ServiceControllerControl), new FrameworkPropertyMetadata()
        {
            PropertyChangedCallback = OnServiceController_PropertyChanged,
            BindsTwoWayByDefault = true,
            DefaultUpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
        });

        // ************************************Constructors**********************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceControllerControl" /> class.
        /// </summary>
        public ServiceControllerControl()
        {
            this.InitializeComponent();

            this.Loaded += this.ServiceControllerControl_Loaded;
            this.Unloaded += this.ServiceControllerControl_Unloaded;
            this.IsVisibleChanged += this.ServiceControllerControl_IsVisibleChanged;

            this.checkTimer.Tick += this.CheckTimer_Tick;
            this.checkTimer.Interval = new TimeSpan(0, 0, 10);
        }


        // ************************************Properties**********************************************

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public ServiceControllerViewModel ViewModel
        {
            get { return this.DataContext as ServiceControllerViewModel; }

            set { this.DataContext = value; }
        }

        public ServiceController CurrentService
        {
            get { return (ServiceController)this.GetValue(CurrentServiceProperty); }

            set { this.SetValue(CurrentServiceProperty, value); }
        }

        // ************************************Functions**********************************************

        private void ServiceControllerControl_Unloaded(object sender, RoutedEventArgs e)
        {
            this.checkTimer.Stop();
        }

        private void ServiceControllerControl_Loaded(object sender, RoutedEventArgs e)
        {
            this.checkTimer.Start();
        }

        private void ServiceControllerControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if ((bool)e.NewValue)
            {
                this.checkTimer.Start();
            }
            else
            {
                this.checkTimer.Stop();
            }
        }

        /// <summary>
        /// Called when [table property changed].
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="e">The <see cref="DependencyPropertyChangedEventArgs" /> instance containing the event data.</param>
        private static void OnServiceController_PropertyChanged(DependencyObject source, DependencyPropertyChangedEventArgs e)
        {
            var currentControl = source as ServiceControllerControl;
            var service = (ServiceController)e.NewValue;

            if (currentControl != null &&
                currentControl.ViewModel != null)
            {
                currentControl.ViewModel.CurrentService = service;
            }
        }

        private void ButtonEdit_OnClick(object sender, RoutedEventArgs e)
        {
            var editorViewModel = new PipelineEditorViewModel();
            editorViewModel.ConfigDirectory = this.ViewModel.ConfigDirectory;
            editorViewModel.CurrentDataBridgeInfo = this.ViewModel.LoadDataBridgeinDirectory(this.ViewModel.ConfigDirectory);

            ViewManager.Instance.ShowWindow("PipelineEditorWindow",
                (result, resultType) =>
                {
                    if (result is PipelineEditorViewModel)
                    {
                        //var currentDataCommand = (result as PipelineEditorViewModel).CurrentDataBridgeInfo = null;
                        //if (currentDataCommand != null)
                        //{
                        //    this.EditDataCommand(currentDataCommand, true);
                        //}
                    }
                },
                editorViewModel);
        }

        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            this.ViewModel.CheckServiceStatus();
        }

        public override void Dispose()
        {
            if (this.checkTimer != null)
            {
                this.checkTimer.Stop();
                this.checkTimer = null;
            }

            if (this.ViewModel != null)
            {
                this.ViewModel.Dispose();
                this.ViewModel = null;
            }

            base.Dispose();
        }
    }
}