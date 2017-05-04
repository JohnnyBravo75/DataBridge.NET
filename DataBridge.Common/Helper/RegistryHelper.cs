using System;
using Microsoft.Win32;

namespace DataBridge.Helper
{
    /// <summary>
    /// All Registry Entry store in Local Machine Type
    /// path of Local machine - HEKY_LOCAL_MACHINE
    /// </summary>
    public static class RegistryHelper
    {
        /// <summary>
        /// Creates a registry entry at the given path with given key and value
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="path">Specify the path to Create Registry Entry</param>
        /// <param name="key">Specify Name of the key</param>
        /// <param name="value">Specify Value of the key</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <returns>
        /// Boolean
        /// </returns>
        /// <exception cref="Exception"></exception>
        public static bool Write(RegistryKey basePath, string path, string key, object value, bool throwOnError = true)
        {
            RegistryKey regKey = null;
            bool succesfull = false;
            try
            {
                // Create a New SubKey
                basePath.CreateSubKey(path);
                // Open a Sub key for Write Value
                regKey = basePath.OpenSubKey(path, true);
                // Set the Spcified key and Value
                if (regKey != null)
                {
                    regKey.SetValue(key, value);
                    succesfull = true;
                }
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw new Exception(ex.Message);
            }
            finally
            {
                if (basePath != null)
                {
                    basePath.Dispose();
                    basePath = null;
                }

                if (regKey != null)
                {
                    regKey.Dispose();
                    regKey = null;
                }
            }

            return succesfull;
        }

        /// <summary>
        /// Reads the registry value at the given path with the given key name
        /// </summary>
        /// <typeparam name="T">Specify the Return Type</typeparam>
        /// <param name="basePath">The base path.</param>
        /// <param name="path">Specify the path for Read Value</param>
        /// <param name="key">Specify the key</param>
        /// <param name="createWhenNotExists">if set to <c>true</c> [create when not exists].</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <returns>
        /// T
        /// </returns>
        /// <exception cref="Exception"></exception>
        public static T Read<T>(RegistryKey basePath, string path, string key, bool createWhenNotExists = false, bool throwOnError = true)
        {
            RegistryKey regKey = null;
            T result = default(T);
            try
            {
                //  Open a Sub key for Read Value
                regKey = basePath.OpenSubKey(path, false);

                if (regKey != null)
                {
                    // Get Value by given key
                    result = (T)Convert.ChangeType(regKey.GetValue(key), typeof(T));
                }
                else if (createWhenNotExists)
                {
                    basePath.CreateSubKey(path).SetValue(key, "");
                }
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw new Exception(ex.Message);
            }
            finally
            {
                if (basePath != null)
                {
                    basePath.Dispose();
                    basePath = null;
                }

                if (regKey != null)
                {
                    regKey.Dispose();
                    regKey = null;
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes the registry entry at the given path
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="path">Specify the path</param>
        /// <param name="throwOnError">if set to <c>true</c> [throw on error].</param>
        /// <returns>
        /// Boolean
        /// </returns>
        /// <exception cref="Exception"></exception>
        public static bool Delete(RegistryKey basePath, string path, bool throwOnError = true)
        {
            bool succesfull = false;
            try
            {
                // Delete the Specified Sub key
                basePath.DeleteSubKey(path);
                succesfull = true;
            }
            catch (Exception ex)
            {
                if (throwOnError)
                    throw new Exception(ex.Message);
            }
            finally
            {
                if (basePath != null)
                {
                    basePath.Dispose();
                    basePath = null;
                }
            }

            return succesfull;
        }

        /// <summary>
        /// Deletes the registry entry at the given key
        /// </summary>
        /// <param name="basePath">The base path.</param>
        /// <param name="path">Specify the path</param>
        /// <param name="key">Specify the key</param>
        /// <returns>
        /// Boolean
        /// </returns>
        /// <exception cref="Exception"></exception>
        public static bool Delete(RegistryKey basePath, string path, string key)
        {
            RegistryKey regKey = null;
            bool succesfull = false;
            try
            {
                //  Open a Sub key for Delete Value
                regKey = basePath.OpenSubKey(path, true);
                // Delete the Specified Value from this key
                regKey.DeleteValue(key);
                succesfull = true;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            finally
            {
                if (basePath != null)
                {
                    basePath.Dispose();
                    basePath = null;
                }

                if (regKey != null)
                {
                    regKey.Dispose();
                    regKey = null;
                }
            }

            return succesfull;
        }
    }
}