using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Xml;
using ChuckHill2.Extensions;

namespace ChuckHill2
{
    /// <summary>
    /// Extension to System.Configuration.ConfigurationManager for getting/setting 
    /// configuration properties using xPath as well as handling configuration refresh notification events.
    /// </summary>
    public static class ConfigurationManagerEx // : ConfigurationManager //error CS0713: Static class cannot derive from type.
    {
        /// <summary>
        /// Notify subscriber that the specified Configuration section has been refreshed.
        /// </summary>
        public static event Action<string> Refreshed;

        #region ConfigurationManager Pseudo-base class
        /// <summary>
        ///     Gets the System.Configuration.AppSettingsSection data for the current application's default configuration.
        /// </summary>
        /// <returns>
        ///     Returns a System.Collections.Specialized.NameValueCollection object that
        ///     contains the contents of the System.Configuration.AppSettingsSection object
        ///     for the current application's default configuration.
        /// </returns>
        /// <exception>
        ///     System.Configuration.ConfigurationErrorsException:
        ///     Could not retrieve a System.Collections.Specialized.NameValueCollection object
        ///     with the application settings data.
        /// </exception> 
        public static NameValueCollection AppSettings { get { return ConfigurationManager.AppSettings; } }
        /// <summary>
        ///     Gets the System.Configuration.ConnectionStringsSection data for the current
        ///     application's default configuration.
        /// </summary>
        /// <returns>
        ///     Returns a System.Configuration.ConnectionStringSettingsCollection object
        ///     that contains the contents of the System.Configuration.ConnectionStringsSection
        ///     object for the current application's default configuration.
        /// </returns>
        /// <exception>
        ///     System.Configuration.ConfigurationErrorsException:
        ///     Could not retrieve a System.Configuration.ConnectionStringSettingsCollection object.
        /// </exception> 
        public static ConnectionStringSettingsCollection ConnectionStrings { get { return ConfigurationManager.ConnectionStrings; } }
        /// <summary>
        ///   Retrieves a specified configuration section for the current application's default configuration.
        /// </summary>
        /// <param name="sectionName">The configuration section path and name.</param>
        /// <returns>The specified System.Configuration.ConfigurationSection object, or null if the section does not exist.</returns>
        /// <exception> 
        /// System.Configuration.ConfigurationErrorsException: A configuration file could not be loaded.
        /// </exception>
        /// <remarks>
        /// Known Custom Sections--
        ///    cachingConfiguration
        ///    MyHL7Adapters
        ///    securityConfiguration
        /// Known Sections--
        ///    appSettings
        ///    connectionStrings
        ///    system.diagnostics
        /// Known CLR Startup Sections--
        ///    runtime //((System.Configuration.IgnoreSection)(sect))._rawXml = @"<runtime><generatePublisherEvidence enabled="false" /></runtime>"
        ///    startup //((System.Configuration.IgnoreSection)(sect))._rawXml = @"<startup><supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.5" /></startup>"
        /// Known Nested Sections--
        ///    system.net/mailSettings/smtp
        ///    system.serviceModel/behaviors
        ///    system.serviceModel/bindings
        ///    system.serviceModel/client
        ///    system.serviceModel/diagnostics
        ///    system.serviceModel/services
        ///    system.web/membership
        ///    system.web/profile
        ///    system.web/roleManager
        /// Internal ConfigurationManager Sections--
        ///    configSections
        /// </remarks>
        public static object GetSection(string sectionName) { return ConfigurationManager.GetSection(sectionName); }
        /// <summary>
        ///     Opens the configuration file for the current application as a System.Configuration.Configuration
        ///     object.
        /// </summary>
        /// <param name="userLevel">
        ///     The System.Configuration.ConfigurationUserLevel for which you are opening
        ///     the configuration.
        /// </param>
        /// <returns>
        ///     A System.Configuration.Configuration object.
        /// </returns>
        /// <exception>
        ///     System.Configuration.ConfigurationErrorsException:
        ///       A configuration file could not be loaded.
        /// </exception> 
        public static Configuration OpenExeConfiguration(ConfigurationUserLevel userLevel) { return ConfigurationManager.OpenExeConfiguration(userLevel); }
        /// <summary>
        ///     Opens the specified client configuration file as a System.Configuration.Configuration object.
        /// </summary>
        /// <param name="exePath">
        ///     The path of the executable (exe) file.
        /// </param>
        /// <returns>
        ///     A System.Configuration.Configuration object.
        /// </returns>
        /// <exception>
        ///   System.Configuration.ConfigurationErrorsException:
        ///     A configuration file could not be loaded.
        /// </exception> 
        public static Configuration OpenExeConfiguration(string exePath) { return ConfigurationManager.OpenExeConfiguration(exePath); }
        /// <summary>
        ///     Opens the machine configuration file on the current computer as a System.Configuration.Configuration object.
        /// </summary>
        /// <returns>
        ///     A System.Configuration.Configuration object.
        /// </returns>
        /// <exception>
        ///   System.Configuration.ConfigurationErrorsException:
        ///     A configuration file could not be loaded.
        /// </exception> 
        public static Configuration OpenMachineConfiguration() { return ConfigurationManager.OpenMachineConfiguration(); }
        /// <summary>
        ///     Opens the specified client configuration file as a System.Configuration.Configuration
        ///     object that uses the specified file mapping and user level.
        /// </summary>
        /// <param name="fileMap">
        ///     An System.Configuration.ExeConfigurationFileMap object that references configuration
        ///     file to use instead of the application default configuration file.
        /// </param>
        /// <param name="userLevel">
        ///     The System.Configuration.ConfigurationUserLevel object for which you are
        ///     opening the configuration.
        /// </param>
        /// <returns>
        ///     The configuration object.
        /// </returns>
        /// <exception>
        ///   System.Configuration.ConfigurationErrorsException:
        ///     A configuration file could not be loaded.
        /// </exception> 
        public static Configuration OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel) { return ConfigurationManager.OpenMappedExeConfiguration(fileMap, userLevel); }
        /// <summary>
        ///     Opens the specified client configuration file as a System.Configuration.Configuration
        ///     object that uses the specified file mapping, user level, and preload option.
        /// </summary>
        /// <param name="fileMap">
        ///     An System.Configuration.ExeConfigurationFileMap object that references the
        ///     configuration file to use instead of the default application configuration
        ///     file.
        /// </param>
        /// <param name="userLevel">
        ///     The System.Configuration.ConfigurationUserLevel object for which you are
        ///     opening the configuration.
        /// </param>
        /// <param name="preLoad">
        ///     true to preload all section groups and sections; otherwise, false.
        /// </param>
        /// <returns>
        ///     The configuration object.
        /// </returns>
        /// <exception>
        ///   System.Configuration.ConfigurationErrorsException:
        ///     A configuration file could not be loaded.
        /// </exception> 
        public static Configuration OpenMappedExeConfiguration(ExeConfigurationFileMap fileMap, ConfigurationUserLevel userLevel, bool preLoad) { return ConfigurationManager.OpenMappedExeConfiguration(fileMap, userLevel, preLoad); }
        /// <summary>
        ///     Opens the machine configuration file as a System.Configuration.Configuration
        ///     object that uses the specified file mapping.
        /// 
        /// </summary>
        /// <param name="fileMap">
        ///     An System.Configuration.ExeConfigurationFileMap object that references configuration
        ///     file to use instead of the application default configuration file.
        /// </param>
        /// <returns>
        ///     A System.Configuration.Configuration object.
        /// </returns>
        /// <exception>
        ///   System.Configuration.ConfigurationErrorsException:
        ///     A configuration file could not be loaded.
        /// </exception> 
        public static Configuration OpenMappedMachineConfiguration(ConfigurationFileMap fileMap) { return ConfigurationManager.OpenMappedMachineConfiguration(fileMap); }
        /// <summary>
        ///     Refreshes the named section so the next time that it is retrieved it will
        ///     be re-read from disk. Also triggers the Refreshed event to notify subscriber 
        ///     that the specified section has been reloaded.
        /// </summary>
        /// <param name="sectionName">
        ///     The configuration section name or the configuration path and section name
        ///     of the section to refresh.
        /// </param>
        private static void RefreshSection(string sectionName) { ConfigurationManager.RefreshSection(sectionName); if (Refreshed != null) Refreshed(sectionName); }
        #endregion

        /// <summary>
        ///     Refreshes all the loaded sections so the next time that it is retrieved it will
        ///     be re-read from disk. Also triggers the Refreshed event for all loaded events to 
        ///     notify subscriber that the specified sections have been reloaded.
        /// </summary>
        private static void RefreshAll()
        {
            foreach (string sect in GetLoadedSections())
            {
                ConfigurationManagerEx.RefreshSection(sect);
            }
        }

        /// <summary>
        /// Get list of currently loaded configuration sections.
        /// </summary>
        /// <returns>list of currently loaded configuration sections.</returns>
        private static string[] GetLoadedSections()
        {
            // s_configSystem can be null if the ConfigurationManager is not properly loaded. Accessing the AppSettings *should* do the trick.
            var appSettings = ConfigurationManager.AppSettings;

            FieldInfo s_configSystemField = typeof(ConfigurationManager).GetField("s_configSystem", BindingFlags.NonPublic | BindingFlags.Static);
            object s_configSystem = s_configSystemField.GetValue(null);
            FieldInfo _completeConfigRecordField = s_configSystem.GetType().GetField("_completeConfigRecord", BindingFlags.NonPublic | BindingFlags.Instance);
            object _completeConfigRecord = _completeConfigRecordField.GetValue(s_configSystem);
            FieldInfo _sectionRecordsField = _completeConfigRecord.GetType().GetField("_sectionRecords", BindingFlags.NonPublic | BindingFlags.Instance);
            Hashtable _sectionRecords = (Hashtable)_sectionRecordsField.GetValue(_completeConfigRecord);
            var keys = _sectionRecords.Keys;
            string[] array = new string[keys.Count];
            int i = 0;
            foreach (string key in keys) { array[i++] = key; }
            return array;
        }

        /// <summary>
        /// Retrieve a value from in-memory ConfigurationManager via XPath
        /// </summary>
        /// <param name="xPath">XPath to value to read</param>
        /// <returns></returns>
        public static string GetValue(string xPath)
        {
            return null;
        }

        /// <summary>
        /// Set value to opened configuration. Value is not visible in current in-memory 
        /// configuration until config.Save() AND ConfigurationManagerEx.Refresh() is called.
        /// </summary>
        /// <param name="config">Opened Configuration from OpenExeConfiguration()</param>
        /// <param name="xPath">XPath to value to write</param>
        /// <param name="value">value to write</param>
        public static void SetValue(this Configuration config, string xPath, string value)
        {
        }

        /// <summary>
        /// Set value to current in-memory configuration. This value is NOT written to app.config. 
        /// Calling Refresh() will reset the value back to it's original value.
        /// </summary>
        /// <param name="xPath">XPath to value to write</param>
        /// <param name="value">value to write</param>
        public static void SetValue(string xPath, string value)
        {
        }

        #region AppConfigWatcher
        private static Dictionary<string, string> _xmlSections = GetXmlSections();
        private static readonly FileSystemWatcher _appConfigWatcher = AppConfigWatcher();

        /// <summary>
        /// Watch for changes to current App.Config for the life of the application.
        /// Note: If dynamically swapping in another app.Config via ChuckHill2.AppConfig, 
        /// do not reference anything from this class until AFTER the swap is complete!
        /// </summary>
        /// <returns>handle to hold so GC will not collect this.</returns>
        private static FileSystemWatcher AppConfigWatcher()
        {
            string appConfig = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            if (appConfig.IsNullOrEmpty() || !File.Exists(appConfig)) return null;
            FileSystemWatcher fsw = new FileSystemWatcher(Path.GetDirectoryName(appConfig), Path.GetFileName(appConfig));
            fsw.NotifyFilter = NotifyFilters.LastWrite;
            //fsw.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            fsw.Changed += delegate(object sender, FileSystemEventArgs e)
            {
                _appConfigWatcher.EnableRaisingEvents = false;
                if (e.ChangeType != WatcherChangeTypes.Changed)
                {
                    _appConfigWatcher.EnableRaisingEvents = true;
                    return;
                }
                var xmlSections = GetXmlSections();
                foreach (var sect in xmlSections)
                {
                    string xml = null;
                    _xmlSections.TryGetValue(sect.Key, out xml);
                    if (xml.EqualsI(sect.Value)) continue;
                    ConfigurationManagerEx.RefreshSection(sect.Key);
                }
                _xmlSections.Clear();
                _xmlSections = xmlSections;
                _appConfigWatcher.EnableRaisingEvents = true;
            };
            fsw.EnableRaisingEvents = true;
            return fsw;
        }

        /// <summary>
        /// Get dictionary of ConfigurationManager section xml for change comparison.
        /// </summary>
        /// <returns>Dictionary of section xml's</returns>
        private static Dictionary<string, string> GetXmlSections()
        {
            string[] sections = GetLoadedSections();
            Dictionary<string, string> xmlSections = new Dictionary<string, string>(sections.Length, StringComparer.InvariantCultureIgnoreCase);
            if (sections.Length == 0) return xmlSections;

            string configFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            if (configFile.IsNullOrEmpty() || !File.Exists(configFile)) return xmlSections;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CloseInput = true;
            settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.IgnoreProcessingInstructions = true;
            XmlDocument xdoc = new XmlDocument();
            XmlReader reader = XmlReader.Create(configFile, settings);
            xdoc.Load(reader);
            reader.Dispose();

            foreach (string sect in sections)
            {
                XmlNode n = xdoc.SelectSingleNode("configuration/" + sect);
                if (n == null) continue;
                xmlSections.Add(sect,n.OuterXml);
            }
            xdoc = null;
            return xmlSections;
        }
        #endregion


#if INPROGRESS
            private static PropertyInformation GetNode(this XmlNode node, string path, XmlNamespaceManager nsmgr = null)
            {
                var s = GetSectionRoot(ref path);
                if (s == null) return null;

                //"appSettings[key='MailFrom']"
                if (s is System.Collections.Specialized.NameValueCollection) //Must be appSettings section
                {
                    var ass = s.GetReflectedValue("_root") as System.Configuration.AppSettingsSection;
                    ConfigurationElement ele = ass.Settings["MailFrom"];
                    return ele.ElementInformation.Properties["value"];
                }

                if (s is System.Collections.Specialized.NameValueCollection) //Must be appSettings section
                {
                    ConfigurationSection sect = s.GetReflectedValue("_root") as System.Configuration.ConfigurationSection;
                    ConfigurationElementCollection cec = sect.ElementInformation.Properties[""].Value as ConfigurationElementCollection;
                    ConfigurationElement ce = null;
                    foreach (ConfigurationElement e in cec)
                    {
                        if (string.Compare(e.ElementInformation.Properties["key"].Value.ToString(), "MailFrom", true) != 0) continue;
                        ce = e;
                        break;
                    }
                    return ce.ElementInformation.Properties["value"];
                }


                char[] delimiters = new char[] { '/', '[', ']', '=', '"', '\'' };
                // xpath == /configuration/node1/node2[subnode1/subnode2[@attr="str"]/@attr2
                //book[/bookstore/@specialty=@style]
                //author[last-name = "Bob"]

                if (path == null) return node;
                path = path.Trim();
                if (path[0] == '/') node = node.OwnerDocument ?? node;
                path = path.Trim('/');
                if (path.Length == 0) return node;
                XmlNode n = node.SelectNode(path, nsmgr);
                if (n != null) return n;
                char leadingDelimiter = '\0';
                char trailingDelimiter = '\0';
                while (path.Length > 0)
                {
                    int delimiterIndex = path.IndexOfAny(delimiters);
                    if (delimiterIndex == -1)
                    {
                        if (path[0] == '@') return node.Attributes.Append(node.OwnerDocument.CreateAttribute(path.TrimStart('@')));
                        else return node.AppendChild(node.OwnerDocument.CreateElement(path));
                    }

                    leadingDelimiter = trailingDelimiter;
                    trailingDelimiter = path[delimiterIndex];
                    string item = path.Substring(0, delimiterIndex).Trim();
                    path = path.Substring(delimiterIndex + 1, path.Length - delimiterIndex - 1).Trim();

                    if (trailingDelimiter == '[')
                    {
                        int bracketCount = 1;
                        for (delimiterIndex = 0; delimiterIndex < path.Length; delimiterIndex++)
                        {
                            if (path[delimiterIndex] == '[') { bracketCount++; continue; }
                            if (path[delimiterIndex] == ']') { bracketCount--; if (bracketCount > 0) continue; else break; }
                        }
                        n = node.SelectSingleNode(item + "[" + path.Substring(0, delimiterIndex + 1), nsmgr);
                        if (n == null)
                        {
                            n = node.AppendChild(node.OwnerDocument.CreateElement(item));
                            n.GetNode(path.Substring(0, delimiterIndex));
                        }
                        leadingDelimiter = trailingDelimiter;
                        trailingDelimiter = path[delimiterIndex];
                        if ((delimiterIndex + 2) > path.Length) path = string.Empty;
                        else path = path.Substring(delimiterIndex + 2, path.Length - delimiterIndex - 2);
                        node = n;
                        continue;
                    }
                    if (trailingDelimiter == '/')
                    {
                        if (item.Length == 0 && trailingDelimiter == '/') { n = node.OwnerDocument; continue; }
                        n = node.SelectSingleNode(item, nsmgr);
                        if (n == null)
                        {
                            if (item[0] == '@') n = node.Attributes.Append(node.OwnerDocument.CreateAttribute(item.TrimStart('@')));
                            else n = node.AppendChild(node.OwnerDocument.CreateElement(item));
                        }
                        node = n;
                        continue;
                    }
                    if (trailingDelimiter == '=')
                    {
                        n = node.SelectSingleNode(item, nsmgr);
                        if (n == null)
                        {
                            if (item[0] == '@') n = node.Attributes.Append(node.OwnerDocument.CreateAttribute(item.TrimStart('@')));
                            else n = node.AppendChild(node.OwnerDocument.CreateElement(item));
                        }
                        node = n;
                        continue;
                    }
                    if (trailingDelimiter == '"' || trailingDelimiter == '\'')
                    {
                        delimiterIndex = path.IndexOf(trailingDelimiter);
                        if (delimiterIndex == -1) throw new FormatException("Invalid XPath format. Missing trailing quote");
                        leadingDelimiter = trailingDelimiter;
                        trailingDelimiter = path[delimiterIndex];
                        item = path.Substring(0, delimiterIndex).Trim();
                        path = path.Substring(delimiterIndex + 1, path.Length - delimiterIndex - 1).Trim();
                        //node.SetValue(item);
                        continue;
                    }
                }
                return node;
            }
            private static XmlNode SelectNode(this XmlNode node, string path, XmlNamespaceManager nsmgr = null)
            {
                //Used exclusively by GetNode(), above.
                XmlNode n = null;
                int equalIndex = path.IndexOf('=');
                if (equalIndex != 1)
                {
                    int bracketIndex = path.IndexOf('[');
                    if (bracketIndex == -1) bracketIndex = int.MaxValue;
                    if (equalIndex < bracketIndex) return null;
                }
                try { n = node.SelectSingleNode(path, nsmgr); }
                catch { }
                return n;
            }

            private static Object GetSectionRoot(ref string xpath)
            {
                if (xpath.IsNullOrEmpty()) return null;
                var items = xpath.Split(new char[]{'/'},StringSplitOptions.RemoveEmptyEntries);
                if (items==null || items.Length==0) return null;
                int maxdepth = (items.Length > 3 ? 3 : items.Length); //nested sections are only up to 3 levels deep.
                string item = string.Empty;
                Object sect = null;
                int i = 0;
                string delimiter = string.Empty;
                for (i = 0; i < maxdepth; i++)
                {
                    item = delimiter + items[i];
                    sect = ConfigurationManager.GetSection(item);
                    if (sect != null) break;
                    delimiter = "/";
                }
                xpath = string.Join("/", items, i);
                return sect;
            }
#endif
    }
}
