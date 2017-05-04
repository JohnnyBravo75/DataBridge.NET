namespace DataBridge.GUI.Windows
{
    using System.ComponentModel;
    using System.Windows;
    using DataBridge.GUI.Core.View.WPFControls;
    using DataBridge.GUI.ViewModels;

    /// <summary>
    /// Interaction logic for DataCommandWindow.xaml
    /// </summary>
    public partial class DataCommandWindow : WPFWindow
    {
        // ************************************Fields**********************************************

        //private PropertyObserver<DataCommandViewModel> dataCommandViewModelObserver;

        // ************************************Constructors**********************************************

        public DataCommandWindow()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                this.DataContextChanged += this.DataCommandWindow_DataContextChanged;
            }

            this.InitializeComponent();
        }

        // ************************************Properties**********************************************

        // ************************************Functions**********************************************

        public override void Dispose()
        {
            this.DataContextChanged -= this.DataCommandWindow_DataContextChanged;

            base.Dispose();
            this.DataContext = null;
        }

        private void DataCommandWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is DataCommandViewModel)
            {
                //this.DeregisterPropertyChanged(e.OldValue as DataCommandViewModel, this.DataCommandViewModel_PropertyChangedChanged);
                //(e.OldValue as DataCommandViewModel).Dispose();
                //this.dataCommandViewModelObserver = null;
            }

            if (e.NewValue is DataCommandViewModel)
            {
                //this.RegisterPropertyChanged(e.NewValue as DataCommandViewModel, this.DataCommandViewModel_PropertyChangedChanged);
                //this.dataCommandViewModelObserver = new PropertyObserver<DataCommandViewModel>(e.NewValue as DataCommandViewModel)
                //                                   .RegisterPropertyChanged("CurrentDataCommand", this.DataCommandViewModel_PropertyChangedChanged);
                this.DataCommandContainerControl.ViewModel = e.NewValue as DataCommandViewModel;

                if (this.DataCommandContainerControl.ViewModel.CurrentDataCommand != null)
                {
                    this.Title = this.DataCommandContainerControl.ViewModel.CurrentDataCommand.Group;
                }
            }
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.DataCommandContainerControl.CurrentCommandControl != null)
            {
                this.DataCommandContainerControl.CurrentCommandControl.PrepareChanges();

                string validationMessage = this.DataCommandContainerControl.CurrentCommandControl.Validate();
                if (!string.IsNullOrEmpty(validationMessage))
                {
                    MessageBox.Show(validationMessage);
                    return;
                }

                this.DataCommandContainerControl.CurrentCommandControl.ApplyChanges();
            }

            this.ReturnValue = this.DataCommandContainerControl.ViewModel;
            this.DialogResult = true;
            this.Close();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}