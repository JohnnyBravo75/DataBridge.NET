using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using DataBridge.GUI.Core.View;
using DataBridge.GUI.ViewModels;
using Xceed.Wpf.Toolkit.PropertyGrid;
using Xceed.Wpf.Toolkit.PropertyGrid.Editors;

namespace DataBridge.GUI.UserControls
{
    using Core.View.WPFControls;

    public partial class DataCommandControl : WPFUserControl
    {
        // ************************************Routed Events**********************************************

        public static readonly RoutedEvent DeleteCommandRequestedEvent =
            EventManager.RegisterRoutedEvent(
                "DeleteCommandRequested",
                RoutingStrategy.Bubble,
                typeof(DeleteCommandRoutedEventHandler),
                typeof(DataCommandControl));

        public static void AddDeleteCommandRequestedHandler(DependencyObject d, DeleteCommandRoutedEventHandler handler)
        {
            ((UIElement)d).AddHandler(DeleteCommandRequestedEvent, handler);
        }

        public static void RemoveDeleteCommandRequestedHandler(DependencyObject d, DeleteCommandRoutedEventHandler handler)
        {
            ((UIElement)d).RemoveHandler(DeleteCommandRequestedEvent, handler);
        }

        // ************************************Properties**********************************************

        public static readonly DependencyProperty ValidationMessagesProperty =
            DependencyProperty.Register(
                "ValidationMessages",
                typeof(IList<string>),
                typeof(DataCommandControl),
                new PropertyMetadata(null));

        public IList<string> ValidationMessages
        {
            get { return (IList<string>)this.GetValue(ValidationMessagesProperty); }
            set { this.SetValue(ValidationMessagesProperty, value); }
        }

        public static readonly DependencyProperty AvailableTokensProperty =
            DependencyProperty.Register(
                "AvailableTokens",
                typeof(IList<string>),
                typeof(DataCommandControl),
                new PropertyMetadata(null, OnAvailableTokensChanged));

        public IList<string> AvailableTokens
        {
            get { return (IList<string>)this.GetValue(AvailableTokensProperty); }
            set { this.SetValue(AvailableTokensProperty, value); }
        }

        private static void OnAvailableTokensChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DataCommandControl)d).UpdateTokenEditor(e.NewValue as IList<string>);
        }

        // ************************************Constructor**********************************************

        public DataCommandControl()
        {
            this.InitializeComponent();
            this.AddHandler(
                DeleteCommandRequestedEvent,
                new DeleteCommandRoutedEventHandler(this.OnChildDeleteCommandRequested));
            this.DataContextChanged += this.DataCommandControl_DataContextChanged;
        }

        // ************************************Event Handlers**********************************************

        private void DataCommandControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            var cmd = e.NewValue as DataCommand;
            this.ValidationMessages = cmd != null
                ? cmd.Validate(null, ValidationContext.Static)
                : null;
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            var dataCommand = this.DataContext as DataCommand;
            if (dataCommand == null)
            {
                return;
            }

            var args = new DeleteCommandEventArgs(DeleteCommandRequestedEvent, dataCommand);
            this.RaiseEvent(args);
            e.Handled = true;
        }

        private void OnChildDeleteCommandRequested(object sender, DeleteCommandEventArgs e)
        {
            if (ReferenceEquals(e.OriginalSource, this))
            {
                return;
            }

            var parentCommand = this.DataContext as DataCommand;
            if (parentCommand == null)
            {
                return;
            }

            if (parentCommand.Commands.Remove(e.CommandToDelete))
            {
                e.Handled = true;
                this.DataContext = null;
                this.DataContext = parentCommand;
            }
        }

        private void btnAddSubStep_Click(object sender, RoutedEventArgs e)
        {
            var dataCommand = this.DataContext as DataCommand;
            if (dataCommand == null)
            {
                return;
            }

            var viewModel = new AddCommandViewModel();
            ViewManager.Instance.ShowWindow("AddCommandWindow",
                (result, resultType) =>
                {
                    if (result is AddCommandViewModel)
                    {
                        var addVm = (AddCommandViewModel)result;
                        if (addVm.SelectedCommand != null)
                        {
                            dataCommand.Commands.Add(addVm.SelectedCommand);
                            this.DataContext = null;
                            this.DataContext = dataCommand;
                        }
                    }
                },
                viewModel);
        }

        private void btnOutBadge_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as System.Windows.Controls.Button;
            if (btn == null) return;
            var token = btn.Tag as string;
            if (!string.IsNullOrEmpty(token))
            {
                System.Windows.Clipboard.SetText("{" + token + "}");
            }
        }

        private void UpdateTokenEditor(IList<string> tokens)
        {
            if (this.PropertyGrid == null) return;

            var existing = this.PropertyGrid.EditorDefinitions;
            if (existing == null)
            {
                existing = new EditorDefinitionCollection();
                this.PropertyGrid.EditorDefinitions = existing;
            }

            for (int i = existing.Count - 1; i >= 0; i--)
            {
                var ed = existing[i] as EditorDefinition;
                if (ed != null && ed.TargetType == typeof(string))
                    existing.RemoveAt(i);
            }

            if (tokens == null || tokens.Count == 0) return;

            var tokensCopy = new System.Collections.Generic.List<string>(tokens);
            var factory = new System.Windows.FrameworkElementFactory(typeof(System.Windows.Controls.ComboBox));
            factory.SetValue(System.Windows.Controls.ComboBox.IsEditableProperty, true);
            factory.SetValue(System.Windows.Controls.ComboBox.StaysOpenOnEditProperty, true);
            factory.SetValue(System.Windows.Controls.ComboBox.ItemsSourceProperty, tokensCopy);
            factory.SetValue(System.Windows.Controls.ComboBox.VerticalContentAlignmentProperty, System.Windows.VerticalAlignment.Center);
            factory.SetBinding(
                System.Windows.Controls.ComboBox.TextProperty,
                new Binding("Value")
                {
                    Mode = BindingMode.TwoWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged
                });
            var template = new System.Windows.DataTemplate { VisualTree = factory };
            template.Seal();

            existing.Add(new EditorDefinition { TargetType = typeof(string), EditorTemplate = template });

            var selectedObject = this.PropertyGrid.SelectedObject;
            if (selectedObject != null)
            {
                this.PropertyGrid.SelectedObject = null;
                this.PropertyGrid.SelectedObject = selectedObject;
            }
        }
    }

    // ************************************Delegate + Event Args**********************************************

    public delegate void DeleteCommandRoutedEventHandler(object sender, DeleteCommandEventArgs e);

    public class DeleteCommandEventArgs : RoutedEventArgs
    {
        public DataCommand CommandToDelete { get; private set; }

        public DeleteCommandEventArgs(RoutedEvent routedEvent, DataCommand command)
            : base(routedEvent)
        {
            this.CommandToDelete = command;
        }
    }
}
