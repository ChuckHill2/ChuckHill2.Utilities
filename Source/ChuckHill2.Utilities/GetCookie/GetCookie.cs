//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="GetCookie.cs" company="Chuck Hill">
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
using GetCookie.Helper;

namespace ChuckHill2
{
    /// <summary>
    /// The single public method for retrieving a browser cookie for a specific domain/host. This directly reads the browser's cookie file. The browser does not need to be running.
    /// </summary>
    public static class Cookie
    {
        /// <summary>
        /// Get browser cookie string for a specific domain from the first browser it is found in, from a list of browsers. 
        /// These include Google Chrome, Microsoft Edge, Mozilla Firefox, plus many other chromium-based browsers as of 2021. Apple Safari not supported.
        /// </summary>
        /// <param name="domain">
        ///    Domain/Host name to retrieve. ex: www.google.com or .google.com or google.com (leading '.' assumes all subdomains)
        /// </param>
        /// <returns>Browser cookie string with domain key-value pairs delimited by semi-colons or empty if not found.</returns>
        public static string Get(string domain)
        {
            string dummy = null;
            return Get(domain, ref dummy);
        }

        /// <summary>
        /// Get browser cookie string for a specific domain from the first browser it is found in, from a list of browsers. 
        /// These include Google Chrome, Microsoft Edge, Mozilla Firefox, plus many other chromium-based browsers as of 2021. Apple Safari not supported.
        /// </summary>
        /// <param name="domain">
        ///    Domain/Host name to retrieve. ex: www.google.com or .google.com or google.com (leading '.' assumes all subdomains)
        /// </param>
        /// <param name="preferredBrowser">
        /// Optional preferred browser to search first. Upon return, contains the name of the browser this cookie was retrived from. If the returned cookie is empty, this value is undefined.<br />
        /// Possible values are:<br />
        ///   • Firefox or Mozilla  • Chrome or Google  • Edge or Microsoft   • Opera • Chromium • Brave • Epic • Amigo • Vivaldi • Dragon • Torch  • Comodo • Slimjet • Orbitum • Kometa • 360Browser • Maxthon3 • K-Melon • Sputnik • Nichrome • CocCoc • Uran or uCozMedia • Chromodo • Yandex or YandexBrowser
        /// </param>
        /// <returns>Browser cookie string with domain key-value pairs delimited by semi-colons or empty if not found.</returns>
        public static string Get(string domain, ref string preferredBrowser)
        {
            string cookie = string.Empty;
            bool ffFirst = (!string.IsNullOrEmpty(preferredBrowser) &&
                (preferredBrowser.IndexOf("Mozilla", StringComparison.OrdinalIgnoreCase) != -1 ||
                 preferredBrowser.IndexOf("Firefox", StringComparison.OrdinalIgnoreCase) != -1));

            if (ffFirst)
            {
                cookie = Mozilla.GetCookie(domain);
                if (!string.IsNullOrEmpty(cookie)) preferredBrowser = "FireFox";
            }

            if (string.IsNullOrEmpty(cookie))
                cookie = Chromium.GetCookie(domain, ref preferredBrowser);

            if (!ffFirst && string.IsNullOrEmpty(cookie))
            {
                cookie = Mozilla.GetCookie(domain);
                if (!string.IsNullOrEmpty(cookie)) preferredBrowser = "FireFox";
            }

            preferredBrowser = preferredBrowser ?? string.Empty;
            return cookie;
        }
    }
}
