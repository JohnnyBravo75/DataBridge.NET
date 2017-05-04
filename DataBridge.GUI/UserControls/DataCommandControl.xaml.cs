using System.ComponentModel;
using System.Windows.Input;
using DataBridge.GUI.Core.DependencyInjection;
using DataBridge.GUI.ViewModels;
using Microsoft.Practices.Unity;

namespace DataBridge.GUI.UserControls
{
    using Core.View.WPFControls;

    public partial class DataCommandControl : WPFUserControl
    {
        // ************************************Fields**********************************************

        // ************************************Constructors**********************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCommandControl" /> class.
        /// </summary>
        public DataCommandControl()
        {
            this.InitializeComponent();

        }

        // ************************************Properties**********************************************


    }
}