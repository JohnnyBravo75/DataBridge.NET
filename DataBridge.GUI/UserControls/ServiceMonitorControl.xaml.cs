using System.ComponentModel;
using DataBridge.GUI.Core.DependencyInjection;
using DataBridge.GUI.ViewModels;
using Microsoft.Practices.Unity;

namespace DataBridge.GUI.UserControls
{
    using Core.View.WPFControls;

    public partial class ServiceMonitorControl : WPFUserControl
    {
        // ************************************Fields**********************************************

        // ************************************Constructors**********************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceMonitorControl" /> class.
        /// </summary>
        public ServiceMonitorControl()
        {
            this.InitializeComponent();
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.ViewModel = DependencyContainer.Container.Resolve<ServiceMonitorViewModel>();
            }
        }

        // ************************************Properties**********************************************

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public ServiceMonitorViewModel ViewModel
        {
            get
            {
                return this.DataContext as ServiceMonitorViewModel;
            }

            set
            {
                this.DataContext = value;
            }
        }
    }
}