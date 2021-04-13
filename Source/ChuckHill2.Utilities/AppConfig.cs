//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="AppConfig.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/ChuckHill2.Utilities</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ChuckHill2
{
    /// <summary>
    /// Use an alternate application configuration file. Similar to:
    /// @code{.cs}
    /// Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);
    /// @endcode
    /// _Except_ it becomes the default application configuration for the entire application until it is Disposed().<br/>
    /// Use Dispose() to restore the original application configuration file.<br/>
    /// Useful when child assemblies also reference the static System.Configuration.ConfigurationManager class.<br/>
    /// This is not a merge. It is an entire replacement. Any in-memory changes to ConfigurationManager will be lost.
    /// </summary>
    /// <see cref="https://stackoverflow.com/questions/6150644/change-default-app-config-at-runtime"/>
    /// <seealso cref="https://stackoverflow.com/questions/480538/use-xml-includes-or-config-references-in-app-config-to-include-other-config-file"/>
    public abstract class AppConfig : IDisposable
    {
        /// <summary>
        /// Gets the current app config filename.
        /// </summary>
        public static string CurrentPath
        {
            get
            {
                object o = null;
                try { o = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE"); }
                catch { return string.Empty; }
                return o.ToString();
            }
        }

        /// <summary>
        /// Change App config to filename specified.
        /// </summary>
        /// <param name="path">
        /// Full or relative path to app config file. 
        /// If file does not end with ".config", it will be appended.
        /// If the app config is already being used, null is returned and the config is not changed.
        /// </param>
        /// <param name="sysDiagChanged">
        /// True if anything changed in the system.diagnostics node.
        /// </param>
        /// <returns>
        /// Previous app config object or NULL if app config already in use
        /// Call Dispose() to restore previous app config.
        /// </returns>
        /// 
        /// <exception cref="System.ArgumentException">
        ///    path is a zero-length string, contains only white space, or contains one
        ///    or more of the invalid characters defined in System.IO.Path.GetInvalidPathChars().
        ///    -or- The system could not retrieve the absolute path.
        /// </exception>
        /// <exception cref="System.Security.SecurityException">
        ///    The caller does not have the required permissions.
        /// </exception>
        /// <exception cref="System.ArgumentNullException">
        ///    path is null.
        /// </exception>
        /// <exception cref="System.NotSupportedException">
        ///    path contains a colon (":") that is not part of a volume identifier (for example, "c:\").
        /// </exception>
        /// <exception cref="System.IO.PathTooLongException">
        ///    The specified path, file name, or both exceed the system-defined maximum
        ///    length. For example, on Windows-based platforms, paths must be less than
        ///    248 characters, and file names must be less than 260 characters.
        /// </exception>
        /// <exception cref="System.IO.FileNotFoundException">
        ///    The app config file is not found.
        /// </exception>
        /// <exception cref="System.UnauthorizedAccessException">
        ///    path is read-only or is a directory.
        /// </exception>
        /// <exception cref="System.IO.DirectoryNotFoundException">
        ///    The specified path is invalid, such as being on an unmapped drive.
        /// </exception>
        /// <exception cref="System.IO.IOException">
        ///    The file is already open.
        /// </exception>
        /// <exception cref="System.Configuration.ConfigurationErrorsException">
        ///     A configuration file could not be loaded.
        /// </exception>
        public static AppConfig Change(string path, bool sysDiagChanged = false)
        {
            if (path == null) path = string.Empty;
            path = path.Trim();
            if (path.Length == 0) return null; //nobody's home

            //Need to filter out "Config already loaded" exceptions, 
            //because this is not really an error, just a warning.
            try { return new ChangeAppConfig(path, sysDiagChanged); }
            catch (Exception ex)
            {
                if (ex.Message == "ALREADYLOADED") return null;
                throw;
            }
        }

        /// <summary>
        /// Restores original app config to this instance object.
        /// </summary>
        public abstract void Dispose();

        /// <summary>
        /// Check if this appconfig instance is disposed/restored.
        /// </summary>
        public abstract bool IsDisposed { get; }

        /// <summary>
        /// App config filename associated with this app config instance object.
        /// </summary>
        public abstract string Path { get; }

        /// <summary>
        /// Force refresh entire Application/Web Config, in-place.
        /// </summary>
        public static void RefreshConfig()
        {
            ChangeAppConfig.ResetConfigMechanism();
            ConfigurationManager.GetSection("appSettings");
        }

        /// <summary>
        /// Modify a single preexisting Web config or App config value.
        /// </summary>
        /// <param name="xpath">Full xPath to value to change</param>
        /// <param name="value">string value to write</param>
        /// <returns>new AppConfig object. Dispose to revert. Will throw exception upon failure.</returns>
        /// <remarks>
        /// This creates a temporary copy of the current app.config with a random name in the TEMP directory, modifies the value as Xml and loads the new app.config.
        /// If the purpose is to decrypt an encrypted value, THIS IS NOT SECURE.
        /// See <see cref="Encryption.DecryptConfigurationManagerConnectionString(string key)"/> for a secure in-memory example.
        /// </remarks>
        public static AppConfig SetConfigValue(string xpath, string value)
        {
            var xdoc = new XmlDocument();
            var currentConfig = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
            xdoc.Load(currentConfig);
            XmlNode node = xdoc.GetNode(xpath);

            node.SetValue(value);

            var temp = System.IO.Path.GetTempFileName();
            var newConfig = System.IO.Path.ChangeExtension(temp,".config");
            File.Move(temp, newConfig);

            xdoc.Save(newConfig);
            var newAppConfig = AppConfig.Change(newConfig, xpath.Contains("system.diagnostics"));

            return newAppConfig;
        }

        private class ChangeAppConfig : AppConfig
        {
            private readonly string oldConfig = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
            private bool disposedValue;
            private bool sysDiagChanged;

            public override string Path { get { return oldConfig; } }
            public override bool IsDisposed { get { return disposedValue; } }

            public ChangeAppConfig(string path, bool sysDiagChanged = false)
            {
                this.sysDiagChanged = sysDiagChanged;
                //Validate as much as we can without actually loading the file as ConfigurationManager will have to do that anyway.
                path = path.Trim();
                if (!path.EndsWith(".config", StringComparison.InvariantCultureIgnoreCase)) path += ".config";
                path = System.IO.Path.GetFullPath(path); //Validate file path (does not check for existence)
                if (!System.IO.File.Exists(path)) throw new System.IO.FileNotFoundException("App.Config file not found",path);
                if (string.Compare(oldConfig, path, true) == 0) throw new System.Exception("ALREADYLOADED");  
                AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", path);
                ResetConfigMechanism();
                //config loading is delayed, so we load something right now to verify that the app.config format is valid.
                try { ConfigurationManager.GetSection("appSettings"); }
                catch
                {
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", oldConfig);
                    ResetConfigMechanism();
                    disposedValue = true;
                    throw;
                }
                if (this.sysDiagChanged) Trace.Refresh(); //system.diagnostics properties were changed so we need to re-read these values.
            }

            public override void Dispose()
            {
                if (!disposedValue)
                {
                    var prevConfig = AppDomain.CurrentDomain.GetData("APP_CONFIG_FILE").ToString();
                    AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", oldConfig);
                    ResetConfigMechanism();
                    disposedValue = true;

                    if (prevConfig.StartsWith(System.IO.Path.GetTempPath(), StringComparison.InvariantCultureIgnoreCase))
                    {
                        File.Delete(prevConfig);
                    }
                }
                GC.SuppressFinalize(this);
                if (this.sysDiagChanged) Trace.Refresh(); //system.diagnostics properties were changed so we need to re-read these values.
            }

            internal static void ResetConfigMechanism()
            {
                typeof(ConfigurationManager).GetField("s_initState", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, 0);
                typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, null);
                typeof(ConfigurationManager).Assembly.GetTypes().Where(x => x.FullName == "System.Configuration.ClientConfigPaths")
                    .First()
                    .GetField("s_current", BindingFlags.NonPublic | BindingFlags.Static)
                    .SetValue(null, null);
            }
        }
    }
}
