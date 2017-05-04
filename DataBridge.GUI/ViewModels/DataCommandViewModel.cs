namespace DataBridge.GUI.ViewModels
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Windows.Controls.Primitives;
    using DataBridge;
    using DataBridge.GUI.Core.View.ViewModels;
    using DataBridge.Runtime;
    using Microsoft.Practices.Prism.Commands;

    public class DataCommandViewModel : DataCommandViewModelBase
    {
        // ************************************Fields**********************************************

        private DataCommand currentDataCommand;

        // ************************************Constructors**********************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCommandViewModel" /> class.
        /// </summary>
        public DataCommandViewModel()
        {
        }

        // ************************************Events**********************************************

        // ************************************Delegate**********************************************

        // ************************************Properties**********************************************

        public DataCommand CurrentDataCommand
        {
            get { return this.currentDataCommand; }
            set
            {
                if (this.currentDataCommand != value)
                {
                    this.currentDataCommand = value;
                    this.RaisePropertyChanged("CurrentDataCommand");
                }
            }
        }

        // ************************************Functions**********************************************

        public override string Validate()
        {
            if (this.CurrentDataCommand == null)
            {
                return string.Empty;
            }

            return string.Join(Environment.NewLine, this.CurrentDataCommand.Validate(null));
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        public override void Dispose()
        {
            this.currentDataCommand = null;

            base.Dispose();
        }
    }
}