namespace DataBridge.GUI.Core.DependencyInjection
{
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Container für Inverse Of Control
    /// </summary>
    public class DependencyContainer
    {
        /// <summary>
        /// Der Unity-Container
        /// </summary>
        private static IUnityContainer container = null;

        /// <summary>
        /// Prevents a default instance of the <see cref="DependencyContainer" /> class from being created.
        /// </summary>
        private DependencyContainer()
        {
        }

        /// <summary>
        /// Gets den UnityContainer
        /// </summary>
        public static IUnityContainer Container
        {
            get
            {
                if (container == null)
                {
                    container = new UnityContainer();
                }

                return container;
            }
        }
    }
}
