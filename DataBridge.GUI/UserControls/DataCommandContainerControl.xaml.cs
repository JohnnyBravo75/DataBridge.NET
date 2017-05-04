namespace DataBridge.GUI.UserControls
{
    using System;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using DataBridge;
    using DataBridge.GUI.Core.DependencyInjection;
    using DataBridge.GUI.Core.View;
    using DataBridge.GUI.Core.View.WPFControls;
    using DataBridge.GUI.ViewModels;
    using Microsoft.Practices.Unity;

    public partial class DataCommandContainerControl : WPFUserControl
    {
        // ************************************Fields**********************************************

        /// <summary>
        /// The current data command
        /// </summary>
        private DataCommand currentDataCommand;

        // ************************************Constructors**********************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCommandContainerControl" /> class.
        /// </summary>
        public DataCommandContainerControl()
        {
            this.InitializeComponent();

            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.ViewModel = DependencyContainer.Container.Resolve<DataCommandViewModel>();
            }
        }

        // ************************************Properties**********************************************

        /// <summary>
        /// Gets or sets the current data command.
        /// </summary>
        /// <value>
        /// The current data command.
        /// </value>
        public DataCommand CurrentDataCommand
        {
            get
            {
                return this.currentDataCommand;
            }
            set
            {
                if (this.currentDataCommand != value)
                {
                    this.currentDataCommand = value;
                    this.RaisePropertyChanged("CurrentDataCommand");
                }
            }
        }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>
        /// The view model.
        /// </value>
        public DataCommandViewModel ViewModel
        {
            get
            {
                return this.DataContext as DataCommandViewModel;
            }

            set
            {
                this.DeregisterPropertyChanged(this.DataContext as DataCommandViewModel, this.DataCommandViewModel_PropertyChangedChanged);
                this.DataContext = value;
                this.RegisterPropertyChanged(this.DataContext as DataCommandViewModel, this.DataCommandViewModel_PropertyChangedChanged);

                this.CurrentDataCommand = this.ViewModel.CurrentDataCommand;

                if (!DesignerProperties.GetIsInDesignMode(this))
                {
                    this.SwitchControls(this.CurrentDataCommand);
                }
            }
        }

        // ************************************Functions**********************************************

        /// <summary>
        /// Verwirft das aktuelle Control
        /// </summary>
        public override void Dispose()
        {
            if (this.ViewModel != null)
            {
                this.DeregisterPropertyChanged(this.ViewModel, this.DataCommandViewModel_PropertyChangedChanged);
                this.ViewModel.Dispose();
            }

            foreach (UIElement item in this.LayoutRoot.Children)
            {
                if (item is UserControl)
                {
                    if (item is IDisposable)
                    {
                        (item as IDisposable).Dispose();
                    }

                    (item as UserControl).DataContext = null;
                }
            }

            this.currentDataCommand = null;
            this.currentCommandControl = null;

            base.Dispose();

            this.DataContext = null;
        }

        private void ClearAndHideControls()
        {
            foreach (UIElement item in this.LayoutRoot.Children)
            {
                if (item is UserControl)
                {
                    (item as UserControl).DataContext = null;
                    item.Visibility = Visibility.Collapsed;
                }
            }
        }

        /// <summary>
        /// Creates a editor control depending on the parameters controltype
        /// </summary>
        /// <param name="dataCommand">The data command.</param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        private UserControl CreateDataCommandControl(DataCommand dataCommand)
        {
            if (string.IsNullOrEmpty(dataCommand.CustomControlName))
            {
                throw new Exception(string.Format("No control for the datacommand '{0}' is found", dataCommand.Name));
            }

            var commandControl = ViewManager.Instance.CreateUserControl(dataCommand.CustomControlName);
            commandControl.Name = dataCommand.GetType().Name;

            return commandControl;
        }

        private DataCommandEditor currentCommandControl;

        public DataCommandEditor CurrentCommandControl
        {
            get { return this.currentCommandControl; }
        }

        private void DataCommandViewModel_PropertyChangedChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case "CurrentDataCommand":
                    if (this.ViewModel != null)
                    {
                        this.CurrentDataCommand = this.ViewModel.CurrentDataCommand;
                        this.SwitchControls(this.CurrentDataCommand);
                    }

                    break;
            }
        }

        private UserControl GetExistingDataCommandControl(string controlName)
        {
            foreach (UIElement item in this.LayoutRoot.Children)
            {
                if (item is UserControl)
                {
                    if ((item as UserControl).Name == controlName)
                    {
                        return (item as UserControl);
                    }
                }
            }

            return null;
        }

        private void SwitchControls(DataCommand dataCommand)
        {
            this.ClearAndHideControls();

            if (dataCommand == null)
            {
                return;
            }

            string controlName = dataCommand.GetType().Name;
            var commandControl = this.GetExistingDataCommandControl(controlName);
            if (commandControl == null)
            {
                commandControl = this.CreateDataCommandControl(dataCommand);
                this.LayoutRoot.Children.Add(commandControl);
            }

            commandControl.Visibility = Visibility.Visible;

            ((DataCommandEditor)commandControl).CurrentDataCommand = dataCommand;

            this.currentCommandControl = (DataCommandEditor)commandControl;
        }
    }
}