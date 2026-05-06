namespace DataBridge.GUI.ViewModels
{
    using System.Collections.Generic;
    using System.Linq;
    using DataBridge;
    using DataBridge.GUI.Core.View.ViewModels;

    public class AddCommandViewModel : ViewModelBase
    {
        // ************************************Fields**********************************************

        private IEnumerable<DataCommand> availableCommands;
        private DataCommand selectedCommand;

        // ************************************Constructors**********************************************

        public AddCommandViewModel()
        {
            this.availableCommands = Pipeline.GetAllAvailableCommands()
                                             .OrderBy(c => c.Group)
                                             .ThenBy(c => c.Title)
                                             .ToList();
        }

        // ************************************Properties**********************************************

        public IEnumerable<DataCommand> AvailableCommands
        {
            get { return this.availableCommands; }
        }

        public DataCommand SelectedCommand
        {
            get { return this.selectedCommand; }
            set
            {
                if (this.selectedCommand != value)
                {
                    this.selectedCommand = value;
                    this.RaisePropertyChanged("SelectedCommand");
                }
            }
        }
    }
}
