namespace DataBridge.GUI
{
    using System;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Threading;
    using Core.View;
    using DataBridge.GUI.Core.Extensions;
    using DataBridge.GUI.Windows;
    using Runtime;
    using ViewModels;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="App" /> class.
        /// </summary>
        public App()
        {
            this.Startup += this.App_Startup;
            this.Exit += this.App_Exit;
            this.DispatcherUnhandledException += this.App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += this.App_CurrentDomainUnhandledException;

            // InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //var viewModel = new PipelineEditorViewModel();
            //viewModel.CurrentPipeline = Pipeline.Load(@"Q:\Develop\c#\DataBridge\DataBridge.Runtime\bin\Debug\Configs\ScheduleTest.config");
            //ViewManager.Instance.ShowWindow("PipelineEditorWindow",
            //    (result, resultType) =>
            //    {
            //        if (result is PipelineEditorViewModel)
            //        {
            //        }
            //    },
            //    viewModel);

            var mainWindow = new ServiceMonitorWindow();
            mainWindow.Show();

            Window mainWindowInternal = ((DependencyObject)mainWindow).FindParent<Window>() as Window;
            if (mainWindowInternal != null)
            {
                mainWindowInternal.ShowInTaskbar = true;
                var titleBinding = new Binding("Header") { Source = mainWindow, Mode = BindingMode.TwoWay };
                mainWindowInternal.SetBinding(Window.TitleProperty, titleBinding);
            }

            base.OnStartup(e);
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }

            MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void App_CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message, "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}