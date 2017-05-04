namespace DataBridge.Common
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using DataBridge;

    public class PlugInManager : Singleton<PlugInManager>
    {
        // ***********************Fields***********************

        /// <summary>
        /// Dictionary, das die vorhandenen PlugIns beinhaltet
        /// </summary>
        private readonly Dictionary<Type, IPlugIn> plugIns = new Dictionary<Type, IPlugIn>();

        private PlugInManager()
        {
        }

        // ***********************Properties***********************

        /// <summary>
        /// Gets eine Liste der vorhandenen PlugIns
        /// </summary>
        public Dictionary<Type, IPlugIn> PlugIns
        {
            get
            {
                return this.plugIns;
            }
        }

        // ***********************Functions***********************

        /// <summary>
        /// Loads the plugIns.
        /// </summary>
        /// <param name="path">The path to the plugin directory.</param>
        /// <param name="createInstance">if set to <c>true</c> if creates an instance of the type.</param>
        public string[] LoadPlugIns(string path, bool createInstance = true)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            string[] files = Directory.GetFiles(path, "*PlugIn.dll", SearchOption.AllDirectories);
            AppDomain.CurrentDomain.AssemblyResolve += this.CurrentDomain_AssemblyResolve;

            foreach (var file in files)
            {
                Assembly plugInAssembly = Assembly.LoadFrom(file);
                var plugInTypes = plugInAssembly.GetExportedTypes()
                                                .Where(p => p.GetInterfaces().Any(q => q == typeof(IPlugIn)));

                if (plugInTypes != null)
                {
                    foreach (var plugInType in plugInTypes)
                    {
                        IPlugIn plugIn = null;

                        if (createInstance)
                        {
                            plugIn = (IPlugIn)plugInType.GetConstructor(new Type[] { }).Invoke(new object[] { });
                        }

                        this.plugIns.Add(plugInType, plugIn);
                    }
                }
            }

            AppDomain.CurrentDomain.AssemblyResolve -= this.CurrentDomain_AssemblyResolve;

            return files;
        }

        /// <summary>
        /// Behandelt das Event, wenn eine Assembly nicht aufgelöst werden kann.
        /// Es wird nachgeschaut, ob die angeforderte dll eine PlugIn-Assembly ist.
        /// </summary>
        /// <param name="sender">Auslöser des Events</param>
        /// <param name="args">Argumente des Events</param>
        /// <returns>die Assembly, wenn eine passende gefunden wurde, <code>null</code> sonst.</returns>
        private Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly plugInAssembly = null;

            foreach (IPlugIn plugIn in this.plugIns.Values)
            {
                if (plugIn.GetType().Assembly.FullName.StartsWith(args.Name))
                {
                    plugInAssembly = plugIn.GetType().Assembly;
                    break;
                }
            }

            return plugInAssembly;
        }
    }
}