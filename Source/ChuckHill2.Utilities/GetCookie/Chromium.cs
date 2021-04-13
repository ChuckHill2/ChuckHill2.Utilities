//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Chromium.cs" company="Chuck Hill">
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

/// <summary>
/// Internal classes exclusively used by ChuckHill2.Cookie.Get()
/// </summary>
namespace GetCookie.Helper
{
    internal static class Chromium
    {
        private static class Paths
        {
            // Path info
            private static string default_appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\";
            private static string local_appdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\";

            // Browsers list - listed in order of market share.
            public static string[] chromiumBasedBrowsers = new string[]
            {
                local_appdata + "Google\\Chrome",
                local_appdata + "Google(x86)\\Chrome",
                //Apple Safari is no longer supported on Windows as of 2012
                //Firefox uses a dfferent storage mechanism. See Mozilla.GetCookie()
                local_appdata + "Microsoft\\Edge",
                default_appdata + "Opera Software\\Opera Stable",
                local_appdata + "Chromium",
                local_appdata + "BraveSoftware\\Brave-Browser",
                local_appdata + "Epic Privacy Browser",
                local_appdata + "Amigo",
                local_appdata + "Vivaldi",
                local_appdata + "Orbitum",
                local_appdata + "Mail.Ru\\Atom",
                local_appdata + "Kometa",
                local_appdata + "Comodo\\Dragon",
                local_appdata + "Torch",
                local_appdata + "Comodo",
                local_appdata + "Slimjet",
                local_appdata + "360Browser\\Browser",
                local_appdata + "Maxthon3",
                local_appdata + "K-Melon",
                local_appdata + "Sputnik\\Sputnik",
                local_appdata + "Nichrome",
                local_appdata + "CocCoc\\Browser",
                local_appdata + "uCozMedia\\Uran",
                local_appdata + "Chromodo",
                local_appdata + "Yandex\\YandexBrowser"
            };

            // Get user data path
            public static string GetUserData(string browser)
            {
                if (browser.Contains("Opera Software")) return browser + "\\";
                return browser + "\\User Data\\Default\\";
            }
        }

        private class CC
        {
            //This class is used solely for sorting by lastAccessed
            public string name;
            public string value;
            public long lastAccessed;
            public CC(string n, string v, string la)
            {
                name = n;
                value = v;
                lastAccessed = long.TryParse(la, out long ll) ? ll : 0;
            }
            public override string ToString() => $"{name} => {value}";
        }

        public static string GetCookie(string domain, ref string preferredBrowser)
        {
            string SqliteFile = "Cookies";
            var list = new List<CC>();

            // Database
            string tempCookieLocation = "";

            IEnumerable<string> browserList = Paths.chromiumBasedBrowsers;
            if (!string.IsNullOrWhiteSpace(preferredBrowser))
            {
                var pb = preferredBrowser;
                var x = Paths.chromiumBasedBrowsers.Where(b => b.IndexOf(pb, StringComparison.OrdinalIgnoreCase) != -1);
                browserList = x.Concat(Paths.chromiumBasedBrowsers.Where(b => b.IndexOf(pb, StringComparison.OrdinalIgnoreCase) == -1));
            }

            // Search all browsers
            foreach (string browser in browserList)
            {
                string Browser = Paths.GetUserData(browser) + SqliteFile;
                if (File.Exists(Browser)) //Must operate on a copy as it may be locked by the browser.
                {
                    tempCookieLocation = Environment.GetEnvironmentVariable("temp") + "\\browserCookies";
                    if (File.Exists(tempCookieLocation)) File.Delete(tempCookieLocation);
                    File.Copy(Browser, tempCookieLocation);
                }
                else continue;

                // Read chrome database
                SQLite sSQLite = new SQLite(tempCookieLocation);
                sSQLite.ReadTable("cookies");

                var found = false;
                for (int i = 0; i < sSQLite.GetRowCount(); i++)
                {
                    string hostKey = sSQLite.GetValue(i, 1);
                    if (!hostKey.Equals(domain, StringComparison.OrdinalIgnoreCase)) continue;

                    // Get data from database
                    string name = sSQLite.GetValue(i, 2);
                    string encryptedValue = sSQLite.GetValue(i, 12);
                    string lastAccessUtc = sSQLite.GetValue(i, 8);

                    // If no data => break
                    if (string.IsNullOrEmpty(name)) break;

                    var value = Crypt.GetUTF8(Crypt.decryptChrome(encryptedValue, Browser));
                    list.Add(new CC(name, value, lastAccessUtc));
                    found = true;
                    continue;
                }

                if (found)
                {
                    var b = Path.GetFileName(browser);
                    preferredBrowser = b.Equals("browser", StringComparison.OrdinalIgnoreCase) ? Path.GetFileName(Path.GetDirectoryName(browser)) : b;
                    break; //only return the cookies from the first browser found.
                }
            }

            if (list.Count == 0) return string.Empty; //no cookies

            //Build cookie string
            var sb = new StringBuilder();
            string delimiter = string.Empty;
            foreach (var cc in list.OrderBy(m => m.lastAccessed))
            {
                sb.Append(delimiter);
                sb.Append(cc.name);
                sb.Append('=');
                sb.Append(cc.value);
                delimiter = "; ";
            }

            return sb.ToString();
        }
    }
}
