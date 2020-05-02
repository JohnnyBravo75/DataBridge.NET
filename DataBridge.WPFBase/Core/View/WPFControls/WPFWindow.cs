using System.Threading;
using System.Windows.Markup;
using System.Windows.Media;
using DataBridge.GUI.Core.View.Windows;

namespace DataBridge.GUI.Core.View.WPFControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using GUI.Core.Extensions;

    public class WPFWindow : Window, IDisposable, INotifyPropertyChanged, IWindow
    {
        // ************************************Konstruktor**********************************************

        public WPFWindow()
        {
            this.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentCulture.Name);
            TextOptions.SetTextFormattingMode(this, TextFormattingMode.Display);

            //this.SetResourceReference(BackgroundProperty, SystemColors.ControlBrushKey);
        }

        // ************************************Events**********************************************

        /// <summary>
        /// Benachrichtigt über die Veränderung einer Property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets den return-Wert eines Dialogs
        /// </summary>
        public virtual object ReturnValue { get; set; }

        /// <summary>
        /// Gets or sets einen Parameter
        /// </summary>
        public virtual object Parameter { get; set; }

        /// <summary>
        /// Gets or sets die Aktion, die beim Schliessen ausgeführt werden soll.
        /// </summary>
        public Action<object, Type> CloseAction { get; set; }

        // ************************************Funktionen**********************************************

        /// <summary>
        /// Auf Propertychanged registrieren
        /// </summary>
        /// <param name="target">Zielobjekt, auf welches man sich registrieren möchte</param>
        /// <param name="eventhandler">Callbackfunktion</param>
        protected void RegisterPropertyChanged(object target, PropertyChangedEventHandler eventhandler)
        {
            this.DeregisterPropertyChanged(target, eventhandler);

            if (target != null && target is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)target).PropertyChanged += eventhandler;
            }
        }

        /// <summary>
        /// Von Propertychanged deregistrieren
        /// </summary>
        /// <param name="target">Zielobjekt, von welchem man sich deregistrieren möchte</param>
        /// <param name="eventhandler">Callbackfunktion</param>
        protected void DeregisterPropertyChanged(object target, PropertyChangedEventHandler eventhandler)
        {
            if (target != null && target is INotifyPropertyChanged)
            {
                ((INotifyPropertyChanged)target).PropertyChanged -= eventhandler;
            }
        }

        /// <summary>
        /// Feuert das PropertyChanged-Events
        /// </summary>
        /// <param name="propertyName">Name der geänderten Property</param>
        public void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public new void ShowDialog()
        {
            base.ShowDialog();
        }

        public virtual void Dispose()
        {
            var childControls = ((UIElement)this).GetChildrenByType<UIElement>().Where(s => s is IDisposable).ToList();
            foreach (UIElement control in childControls)
            {
                ((IDisposable)control).Dispose();
            }

            if (this.Resources != null)
            {
                foreach (var resourceEntry in this.Resources)
                {
                    var resource = ((System.Collections.DictionaryEntry)resourceEntry).Value;

                    if (resource is IDisposable)
                    {
                        (resource as IDisposable).Dispose();
                    }
                }

                this.Resources.Clear();
            }
        }
    }
}