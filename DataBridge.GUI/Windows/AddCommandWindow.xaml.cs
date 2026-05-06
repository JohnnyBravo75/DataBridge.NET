namespace DataBridge.GUI.Windows
{
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Controls;
    using DataBridge.GUI.Core.View.WPFControls;
    using DataBridge.GUI.ViewModels;

    /// <summary>
    /// Interaction logic for AddCommandWindow.xaml
    /// </summary>
    public partial class AddCommandWindow : WPFWindow
    {
        // ************************************Constructors**********************************************

        public AddCommandWindow()
        {
            this.InitializeComponent();
        }

        // ************************************Properties**********************************************

        private AddCommandViewModel AddCommandViewModel
        {
            get { return this.DataContext as AddCommandViewModel; }
        }

        // ************************************Functions**********************************************

        public override void Dispose()
        {
            base.Dispose();
            this.DataContext = null;
        }

        private void lstCommands_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Force PropertyGrid to refresh when selection changes
            this.propertyGrid.SelectedObject = null;
            this.propertyGrid.SelectedObject = this.AddCommandViewModel != null
                ? this.AddCommandViewModel.SelectedCommand
                : null;
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            if (this.AddCommandViewModel == null || this.AddCommandViewModel.SelectedCommand == null)
            {
                MessageBox.Show("Bitte einen Adapter auswählen.", "Hinweis", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            this.ReturnValue = this.AddCommandViewModel;
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
