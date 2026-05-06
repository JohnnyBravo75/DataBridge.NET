namespace DataBridge.GUI
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
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
            RegisterPropertyGridTypeDescriptorFixes();

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
                MessageBox.Show(e.Exception.Message, "Exception Caught", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            e.Handled = true;
        }

        private void App_CurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;

            if (Debugger.IsAttached)
            {
                MessageBox.Show(ex.Message, "Uncaught Thread Exception", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void RegisterPropertyGridTypeDescriptorFixes()
        {
            var formatterOptionsType = Type.GetType("DataConnectors.Formatters.Model.FormatterOptions, DataConnectors", false);
            if (formatterOptionsType == null)
            {
                return;
            }

            var formatterOptionsProvider = TypeDescriptor.GetProvider(formatterOptionsType);
            TypeDescriptor.AddProviderTransparent(
                new FilteringTypeDescriptionProvider(formatterOptionsProvider, p => !string.Equals(p.Name, "Item", StringComparison.Ordinal)),
                formatterOptionsType);

            //var editorTypeName = "Wpf.Toolkit.PropertyEditors.FormatterOptionsEditor, Wpf.Toolkit.PropertyEditors";
            //var editorBaseTypeName = "Xceed.Wpf.Toolkit.PropertyGrid.Editors.ITypeEditor, Xceed.Wpf.Toolkit";
            //TypeDescriptor.AddAttributes(formatterOptionsType, new EditorAttribute(editorTypeName, editorBaseTypeName));


            var formatterBaseType = Type.GetType("DataConnectors.Formatters.FormatterBase, DataConnectors", false);
            if (formatterBaseType == null)
            {
                return;
            }

            var formatterBaseProvider = TypeDescriptor.GetProvider(formatterBaseType);
            TypeDescriptor.AddProviderTransparent(
                new FilteringTypeDescriptionProvider(formatterBaseProvider, p => !string.Equals(p.Name, "FormatterOptions", StringComparison.Ordinal)),
                formatterBaseType);
        }

        private sealed class FilteringTypeDescriptionProvider : TypeDescriptionProvider
        {
            private readonly Func<PropertyDescriptor, bool> includeProperty;

            public FilteringTypeDescriptionProvider(TypeDescriptionProvider parent, Func<PropertyDescriptor, bool> includeProperty)
                : base(parent)
            {
                this.includeProperty = includeProperty;
            }

            public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
            {
                var baseDescriptor = base.GetTypeDescriptor(objectType, instance);
                return new FilteringTypeDescriptor(baseDescriptor, this.includeProperty);
            }
        }

        private sealed class FilteringTypeDescriptor : CustomTypeDescriptor
        {
            private readonly Func<PropertyDescriptor, bool> includeProperty;

            public FilteringTypeDescriptor(ICustomTypeDescriptor parent, Func<PropertyDescriptor, bool> includeProperty)
                : base(parent)
            {
                this.includeProperty = includeProperty;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return Filter(base.GetProperties());
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return Filter(base.GetProperties(attributes));
            }

            private static PropertyDescriptorCollection Filter(PropertyDescriptorCollection properties, Func<PropertyDescriptor, bool> includeProperty)
            {
                var result = new List<PropertyDescriptor>(properties.Count);
                foreach (PropertyDescriptor property in properties)
                {
                    if (!includeProperty(property))
                    {
                        continue;
                    }

                    result.Add(property);
                }

                return new PropertyDescriptorCollection(result.ToArray(), true);
            }

            private PropertyDescriptorCollection Filter(PropertyDescriptorCollection properties)
            {
                return Filter(properties, this.includeProperty ?? (_ => true));
            }
        }
    }
}