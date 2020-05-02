namespace DataBridge.GUI.Core.EventBinding
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using System.Windows.Controls;
    using System.Windows.Input;
    using Microsoft.Practices.Prism.Commands;

    public class EventBehavior : CommandBehaviorBase<Control>
    {
        private EventCommand _bindingInfo;

        public EventBehavior(Control control)
            : base(control)
        {
        }

        public void Bind(EventCommand bindingInfo)
        {
            this.ValidateBindingInfo(bindingInfo);

            this._bindingInfo = bindingInfo;

            this.Bind();
        }

        private void ValidateBindingInfo(EventCommand bindingInfo)
        {
            if (bindingInfo == null) throw new ArgumentException("bindingInfo");
            if (string.IsNullOrEmpty(bindingInfo.CommandName)) throw new ArgumentException("bindingInfo.CommandName");
            if (string.IsNullOrEmpty(bindingInfo.EventName)) throw new ArgumentException("bindingInfo.EventName");
        }

        public void Bind()
        {
            this.ValidateBindingInfo(this._bindingInfo);
            this.HookPropertyChanged();
            this.HookEvent();
            this.SetCommand();
            this.SetCommandParameter();
        }

        public void HookPropertyChanged()
        {
            var dataContext = this.TargetObject.DataContext as INotifyPropertyChanged;
            if (dataContext == null) return;

            dataContext.PropertyChanged -= this.DataContextPropertyChanged;
            dataContext.PropertyChanged += this.DataContextPropertyChanged;
        }

        private void DataContextPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == this._bindingInfo.CommandName)
            {
                this.SetCommand();
                this.SetCommandParameter();
            }
        }

        //private void SetCommand()
        //{
        //    Command = _bindingInfo.Command;
        //}

        private void SetCommand()
        {
            var dataContext = this.TargetObject.DataContext;
            if (dataContext == null)
                return;

            var propertyInfo = dataContext.GetType().GetProperty(this._bindingInfo.CommandName);
            if (propertyInfo == null)
                throw new ArgumentException("Command '" + this._bindingInfo.CommandName + "' not found in " + dataContext.ToString());

            this.Command = propertyInfo.GetValue(dataContext, null) as ICommand;
        }

        private void SetCommandParameter()
        {
            this.CommandParameter = this._bindingInfo.Parameter;
        }

        private void HookEvent()
        {
            var eventInfo = this.TargetObject.GetType().GetEvent(this._bindingInfo.EventName, BindingFlags.Public | BindingFlags.Instance);
            if (eventInfo == null) throw new ArgumentException("event");

            eventInfo.RemoveEventHandler(this.TargetObject, this.GetEventMethod(eventInfo));
            eventInfo.AddEventHandler(this.TargetObject, this.GetEventMethod(eventInfo));
        }

        private Delegate _method;

        private Delegate GetEventMethod(EventInfo eventInfo)
        {
            if (eventInfo == null) throw new ArgumentNullException("eventInfo");
            if (eventInfo.EventHandlerType == null) throw new ArgumentException("EventHandlerType is null");

            if (this._method == null)
            {
                this._method = Delegate.CreateDelegate(
                    eventInfo.EventHandlerType, this, this.GetType().GetMethod("OnEventRaised",
                    BindingFlags.NonPublic | BindingFlags.Instance));
            }

            return this._method;
        }

        private void OnEventRaised(object sender, EventArgs e)
        {
            this.Command.Execute(this._bindingInfo.Parameter);

            //ExecuteCommand();
        }
    }
}