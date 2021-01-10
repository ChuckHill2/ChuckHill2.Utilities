using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace ChuckHill2
{
    //Useful notes.... Google: c# app.config include file
    //http://stackoverflow.com/questions/480538/use-xml-includes-or-config-references-in-app-config-to-include-other-config-file
    //http://stackoverflow.com/questions/6150644/change-default-app-config-at-runtime

    /// <summary>
    /// Use an alternate application configuration file.
    /// Similar to Configuration config = ConfigurationManager.OpenExeConfiguration(exePath);
    /// except it becomes the default application configuration until Disposed().
    /// Use Dispose() to restore the original application configuration file.
    /// Useful when child assemblies reference the static System.Configuration.ConfigurationManager class.
    /// </summary>
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
        /// Full or relative path to appconfig file. 
        /// If file does not end with ".config", it will be appended.
        /// If the app config is already being used, null is returned and the config is not changed.
        /// </param>
        /// <param name="sysDiagChanged">
        /// True if anything changed in the system.diagnostics node.
        /// </param>
        /// <returns>
        /// Previous appconfig object or NULL if app config already in use
        /// Call Dispose() to restore previous app config.
        /// </returns>
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
        ///    The appconfig file is not found.
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
        /// Appconfig filename associated with this appconfig instance object.
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
        /// Modify preexisting WebConfig or AppConfig value
        /// </summary>
        /// <param name="xpath">Full xPath to value to change</param>
        /// <param name="value">string value to write</param>
        /// <returns>new AppConfig object. Dispose to revert. Will throw exception upon failure.</returns>
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
