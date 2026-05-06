using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using DataBridge.GUI.Core.DependencyInjection;
using DataBridge.GUI.ViewModels;
using Microsoft.Practices.Unity;

namespace DataBridge.GUI.UserControls
{
    using Core.View.WPFControls;

    public partial class PipelineEditorControl : WPFUserControl
    {
        // ************************************Constructors**********************************************

        public PipelineEditorControl()
        {
            this.InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.ViewModel = DependencyContainer.Container.Resolve<PipelineEditorViewModel>();
            }

            // Catch delete requests for top-level pipeline commands
            this.lbCommands.AddHandler(
                DataCommandControl.DeleteCommandRequestedEvent,
                new DeleteCommandRoutedEventHandler(this.lbCommands_DeleteCommandRequested));
        }

        // ************************************Properties**********************************************

        public PipelineEditorViewModel ViewModel
        {
            get { return this.DataContext as PipelineEditorViewModel; }
            set { this.DataContext = value; }
        }

        // ************************************Event Handlers**********************************************

        private void lbCommands_DeleteCommandRequested(object sender, DeleteCommandEventArgs e)
        {
            var vm = this.ViewModel;
            if (vm == null || vm.CurrentPipeline == null)
            {
                return;
            }

            if (vm.CurrentPipeline.Commands.Remove(e.CommandToDelete))
            {
                e.Handled = true;
                vm.RaisePropertyChanged("CurrentPipelineCommands");
                vm.RaisePropertyChanged("CurrentPipelineSteps");
            }
        }

        private void treeCommands_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ViewModel.EditCurrentDataCommand();
        }

        private void MenuItemEdit_Click(object sender, RoutedEventArgs e)
        {
            this.ViewModel.EditCurrentDataCommand();
        }
    }
}
