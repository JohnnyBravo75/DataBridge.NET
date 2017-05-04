using System.Windows.Input;
using DataBridge.Helper;

namespace DataBridge.GUI.Core.View
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Windows;
    using System.Windows.Controls;
    using GUI.Core.View.Windows;
    using WPFControls;

    /// <summary>
    /// Managed die ViewKomponenten, der Anwendung.
    /// </summary>
    public class ViewManager
    {
        // ************************************Member**********************************************

        /// <summary>
        /// Instanz des ViewMangers
        /// </summary>
        private static ViewManager instance = null;

        // ************************************Konstruktor**********************************************

        private List<Type> windowTypes;

        /// <summary>
        /// Prevents a default instance of the <see cref="ViewManager" /> class from being created.
        /// </summary>
        private ViewManager()
        {
        }

        // ************************************Events**********************************************

        // ************************************Properties**********************************************

        /// <summary>
        /// Gets die einzige Instanz des ViewManagers
        /// </summary>
        public static ViewManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new ViewManager();
                }

                return instance;
            }
        }

        public UserControl CreateUserControl(string controlName)
        {
            if (string.IsNullOrEmpty(controlName))
            {
                throw new ArgumentNullException("controlName", "The parameter controlname must not be null");
            }

            return GenericFactory.GetInstance<UserControl>(controlName);
        }

        public void ShowWaitCursor()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = Cursors.Wait;
            });
        }

        public void HideWaitCursor()
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Mouse.OverrideCursor = null;
            });
        }

        /// <summary>
        /// Shows the standard file open dialog
        /// </summary>
        /// <param name="fileName">Default file name e.g. Document</param>
        /// <param name="defaultExt">Default file extension e.g. .txt</param>
        /// <param name="filter">Filter files by extension e.g. Text documents (.txt)|*.txt</param>
        public bool? ShowOpenFileDialog(string fileName, string defaultExt, string filter, Action<bool?, string> response)
        {
            // Configure open file dialog box
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.FileName = fileName;
            dlg.DefaultExt = defaultExt;
            dlg.Filter = filter;

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                fileName = dlg.FileName;
                if (response != null)
                {
                    response(result, fileName);
                }
            }

            return result;
        }

        /// <summary>
        /// Shows the standard file save dialog
        /// </summary>
        /// <param name="fileName">Default file name e.g. Document</param>
        /// <param name="defaultExt">Default file extension e.g. .txt</param>
        /// <param name="filter">Filter files by extension e.g. Text documents (.txt)|*.txt</param>
        public bool? ShowSaveFileDialog(string fileName, string defaultExt, string filter, Action<bool?, string> response)
        {
            // Configure open file dialog box
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.FileName = fileName;
            dlg.DefaultExt = defaultExt;
            dlg.Filter = filter;

            // Show open file dialog box
            bool? result = dlg.ShowDialog();

            // Process open file dialog box results
            if (result == true)
            {
                // Open document
                fileName = dlg.FileName;
                if (response != null)
                {
                    response(result, fileName);
                }
            }

            return result;
        }

        /// <summary>
        /// Zeigt einen Dialog an
        /// </summary>
        /// <param name="name">Name der anzuzeigenden Dialogs</param>
        /// <param name="response">Aktion, die beim Schliessen ausgeführt werden soll.</param>
        /// <returns>ein Verweis auf die erzeugte Instanz des Dialogs</returns>
        public IWindow ShowWindow(string name, Action<object, Type> response)
        {
            return this.ShowWindow(name, response, null);
        }

        /// <summary>
        /// Zeigt einen Dialog an
        /// </summary>
        /// <param name="name">Name der anzuzeigenden Dialogs</param>
        /// <param name="response">Aktion, die beim Schliessen ausgeführt werden soll.</param>
        /// <param name="dataContext">DataContext, der an den Dialog übergeben werden soll.</param>
        /// <param name="parameter">Optionaler Parameter für den Dialog</param>
        /// <param name="isReadOnly">Gibt an ob der Dialog im ReadOnlyModus geladen werden soll (falls unterstützt)</param>
        /// <param name="isModal">gibt an ob das Fenster modal geöffnet werden soll.</param>
        /// <returns>
        /// ein Verweis auf die erzeugte Instanz des Dialogs
        /// </returns>
        public IWindow ShowWindow(string name, Action<object, Type> response, object dataContext, object parameter = null, bool isModal = true)
        {
            IWindow window = this.GetWindowInstance(name);

            if (window != null)
            {
                if (dataContext != null)
                {
                    window.DataContext = dataContext;
                }

                window.Parameter = parameter;

                window.CloseAction = response;

                var rw = window as Window;

                rw.Closed += this.Window_Closed;

                if (isModal)
                {
                    rw.ShowDialog();
                }
                else
                {
                    rw.Show();
                }
            }
            else
            {
                throw new ArgumentException("The Window '" + name + "' does not exist.");
            }

            return window;
        }

        private void FindWindowTypes()
        {
            //var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            //foreach (var assembly in assemblies)
            //{
            Assembly assembly = Assembly.GetExecutingAssembly();

            var types = assembly.GetTypes();
            foreach (var type in types)
            {
                if (type.IsSubclassOf(typeof(WPFWindow)))
                {
                    this.windowTypes.Add(type);
                }
            }

            //}
        }

        /// <summary>
        /// Erzeugt eine Dialog-Instanz zu einem Namen
        /// </summary>
        /// <param name="name">Name des Dialogs</param>
        /// <returns>die Instanz des Dialogs</returns>
        private IWindow GetWindowInstance(string name)
        {
            if (this.windowTypes == null)
            {
                this.windowTypes = new List<Type>();
                this.FindWindowTypes();
            }

            Type windowType = this.windowTypes.FirstOrDefault(x => x.Name == name);
            if (windowType == null)
            {
                return null;
            }

            return (IWindow)windowType.Assembly.CreateInstance(windowType.FullName);
        }

        /// <summary>
        /// Reagiert auf das Schliessen eines Dialogs
        /// </summary>
        /// <param name="sender">Auslöser des Events</param>
        /// <param name="e">Argumente des Events.</param>
        private void Window_Closed(object sender, EventArgs e)
        {
            Exception closeException = null;

            IWindow window = sender as IWindow;

            try
            {
                if (window != null)
                {
                    // Return Values setzen und Close-Callback aufrufen
                    ((WPFWindow)window).Closed -= this.Window_Closed;

                    if (window.CloseAction != null)
                    {
                        object returnValue = window.ReturnValue;

                        if (window.DialogResult == true && returnValue != null)
                        {
                            window.CloseAction(returnValue, returnValue.GetType());
                        }
                        else
                        {
                            window.CloseAction(null, null);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                closeException = ex;
            }
            finally
            {
                try
                {
                    // Dialog schliessen
                    window.Close();

                    // Dispose-Handling für Dialog selbst
                    if (window is IDisposable)
                    {
                        (window as IDisposable).Dispose();
                    }

                    //((DMFWindow)dialog).Content = null;
                    window.CloseAction = null;

                    window = null;

                    // Müll einsammeln
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    GC.Collect();

                    if (closeException != null)
                    {
                        MessageBox.Show(string.Format("When closing the window the following error occured: \r\n{0}", closeException.Message), "Internal error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch
                {
                    if (Debugger.IsAttached)
                    {
                        Debugger.Break();
                    }

                    throw;
                }
            }
        }

        /// <summary>
        /// Liefert eine Instanz einer View-Komponente
        /// </summary>
        /// <param name="name">Name der anzuzeigenden Komponente</param>
        /// <returns>die Instanz der View-Komponente</returns>
        //public UserControl GetViewComponent(string name)
        //{
        //    string[] elements = name.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

        //    UserControl control = null;
        //    ViewDescription description = this.views.FirstOrDefault(p => p.Name == elements[0]);
        //    if (description != null)
        //    {
        //        Type controlType = description.ImplementingClass;

        //        if (controlType != null)
        //        {
        //            control = (UserControl)controlType.Assembly.CreateInstance(controlType.FullName);

        //            if (elements.Length > 1)
        //            {
        //                foreach (string parameter in elements.Skip(1))
        //                {
        //                    string[] values = parameter.Split(new string[] { "=" }, StringSplitOptions.None);
        //                    PropertyInfo pi = control.GetType().GetProperty(values[0]);
        //                    if (pi != null)
        //                    {
        //                        pi.SetValue(control, Convert.ChangeType(values[1], pi.PropertyType, CultureInfo.CurrentCulture), null);
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    return control;
        //}
    }
}