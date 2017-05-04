namespace DataBridge.GUI.Core.DependencyInjection
{
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Interface zur Konfiguration des Dependency-Injection Containers
    /// </summary>
    public interface IUnityConfigurator
    {
        /// <summary>
        /// Konfiguriert einen unity-Container
        /// </summary>
        /// <param name="container">der zu konfigurierende Container</param>
        void Configure(IUnityContainer container);
    }
}
