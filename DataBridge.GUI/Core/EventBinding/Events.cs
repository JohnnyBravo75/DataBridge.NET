namespace DataBridge.GUI.Core.EventBinding
{
    using System;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;

    public class Events
    {
        private static readonly DependencyProperty EventBehaviorsProperty =
            DependencyProperty.RegisterAttached(
            "EventBehaviors",
            typeof(EventBehaviorCollection),
            typeof(Control),
            null);

        private static readonly DependencyProperty InternalDataContextProperty =
            DependencyProperty.RegisterAttached(
            "InternalDataContext",
            typeof(Object),
            typeof(Control),
            new PropertyMetadata(null, DataContextChanged));

        private static void DataContextChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var target = dependencyObject as Control;
            if (target == null) return;

            foreach (var behavior in GetOrCreateBehavior(target))
                behavior.Bind();
        }

        public static readonly DependencyProperty CommandsProperty =
            DependencyProperty.RegisterAttached(
            "Commands",
            typeof(EventCommandCollection),
            typeof(Events),
            new PropertyMetadata(null, CommandsChanged));

        public static EventCommandCollection GetCommands(DependencyObject dependencyObject)
        {
            return dependencyObject.GetValue(CommandsProperty) as EventCommandCollection;
        }

        public static void SetCommands(DependencyObject dependencyObject, EventCommandCollection eventCommands)
        {
            dependencyObject.SetValue(CommandsProperty, eventCommands);
        }

        private static void CommandsChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var target = dependencyObject as Control;
            if (target == null) return;

            var behaviors = GetOrCreateBehavior(target);
            foreach (var eventCommand in e.NewValue as EventCommandCollection)
            {
                var behavior = new EventBehavior(target);
                behavior.Bind(eventCommand);
                behaviors.Add(behavior);
            }

        }

        private static EventBehaviorCollection GetOrCreateBehavior(FrameworkElement target)
        {
            var behavior = target.GetValue(EventBehaviorsProperty) as EventBehaviorCollection;
            if (behavior == null)
            {
                behavior = new EventBehaviorCollection();
                target.SetValue(EventBehaviorsProperty, behavior);
                target.SetBinding(InternalDataContextProperty, new Binding());
            }

            return behavior;
        }
    }

}
