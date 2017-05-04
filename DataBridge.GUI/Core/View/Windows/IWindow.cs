namespace DataBridge.GUI.Core.View.Windows
{
    using System;

    public interface IWindow
    {  
        /// <summary>
        /// Gets den return-Wert eines Dialogs
        /// </summary>
        object ReturnValue { get; }

        /// <summary>
        /// Gets or sets das Ergebnis des Dialogs
        /// </summary>
        bool? DialogResult { get; set; }

        /// <summary>
        /// Gets or sets den DataContext
        /// </summary>
        object DataContext { get; set; }
        
        /// <summary>
        /// Gets or sets die Aktion, die beim Schliessen ausgeführt werden soll.
        /// </summary>
        Action<object, Type> CloseAction { get; set; }

        /// <summary>
        /// Gets or sets einen Parameter
        /// </summary>
        object Parameter { get; set; }
    
        /// <summary>
        /// Zeigt den Dialog nicht modal an
        /// </summary>
        void Show();

        /// <summary>
        /// Zeigt den Dialog modal an
        /// </summary>
        void ShowDialog();

        /// <summary>
        /// Schließt den Dialog
        /// </summary>
        void Close();
    }
}
