using System.ComponentModel;
using System.Windows.Input;
using DataBridge.GUI.Core.DependencyInjection;
using DataBridge.GUI.ViewModels;
using Microsoft.Practices.Unity;

namespace DataBridge.GUI.UserControls
{
    using Core.View.WPFControls;

    public partial class PipelineEditorControl : WPFUserControl
    {
        // ************************************Fields**********************************************

        // ************************************Constructors**********************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="PipelineEditorControl" /> class.
        /// </summary>
        public PipelineEditorControl()
        {
            this.InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.ViewModel = DependencyContainer.Container.Resolve<PipelineEditorViewModel>();
            }
        }

        // ************************************Properties**********************************************

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public PipelineEditorViewModel ViewModel
        {
            get
            {
                return this.DataContext as PipelineEditorViewModel;
            }

            set
            {
                this.DataContext = value;
            }
        }

        private void treeCommands_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.ViewModel.EditCurrentDataCommand();
        }

        private void MenuItemEdit_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ViewModel.EditCurrentDataCommand();
        }
    }
}