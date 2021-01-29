namespace DataBridge.GUI.Core.Utils
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Resources;

    public class ResourceUtil
    {
        public static bool ResourceExists(string resourcePath)
        {
            var assembly = Assembly.GetExecutingAssembly();

            return ResourceExists(assembly, resourcePath);
        }

        public static bool ResourceExists(Assembly assembly, string resourcePath)
        {
            if (string.IsNullOrEmpty(resourcePath))
            {
                return false;
            }

            resourcePath = resourcePath.Replace("\\", "/");

            if (resourcePath.StartsWith("/"))
            {
                resourcePath = resourcePath.Substring(1, resourcePath.Length - 1);
            }
            return GetResourcePaths(assembly).Contains(resourcePath.ToLower());
        }

        public static IEnumerable<object> GetResourcePaths(Assembly assembly)
        {
            var culture = System.Threading.Thread.CurrentThread.CurrentCulture;
            var resourceName = assembly.GetName().Name + ".g";
            var resourceManager = new ResourceManager(resourceName, assembly);

            try
            {
                var resourceSet = resourceManager.GetResourceSet(culture, true, true);

                foreach (System.Collections.DictionaryEntry resource in resourceSet)
                {
                    yield return resource.Key;
                }
            }
            finally
            {
                resourceManager.ReleaseAllResources();
            }
        }
    }
}