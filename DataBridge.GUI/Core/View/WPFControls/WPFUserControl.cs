namespace DataBridge.GUI.Core.View.WPFControls
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using GUI.Core.Extensions;

    /// <summary>
    /// Baseclass for UserControls
    /// </summary>
    public class WPFUserControl : UserControl, IDisposable, INotifyPropertyChanged
    {
        // ************************************Member**********************************************

        // ************************************Konstruktor**********************************************

        /// <summary>
        /// Initializes a new instance of the <see cref="WPFUserControl" /> class.
        /// </summary>
        public WPFUserControl()
            : base()
        {
            //Culture setzen
            this.Language = System.Windows.Markup.XmlLanguage.GetLanguage(System.Threading.Thread.CurrentThread.CurrentCulture.Name);
        }

        // ************************************Events**********************************************

        /// <summary>
        /// Benachrichtigt über die Veränderung einer Property
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        // ************************************Properties**********************************************

        // ************************************Funktionen**********************************************

        /// <summary>
        /// Verwirft das aktuelle Control
        /// </summary>
        public virtual void Dispose()
        {
            // Dispose-Handling für Childcontrols des Controls
            IList<UIElement> childControls = ((UIElement)this).GetChildrenByType<UIElement>()
                                                              .Where(s => s is IDisposable)
                                                              .ToList();
            foreach (UIElement control in childControls)
            {
                if (control != null)
                {
                    ((IDisposable)control).Dispose();
                }
            }
        }

        /// <summary>
        /// Auf Propertychanged registrieren
        /// </summary>
        /// <param name="target">Zielobjekt, auf welches man sich registrieren möchte</param>
        /// <param name="eventhandler">Callbackfunktion</param>
        protected void RegisterPropertyChanged(object target, PropertyChangedEventHandler eventhandler)
        {
            //Immer zuerst deregistrieren, damit man nicht mehrfach daruf registriert ist
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
    }
}