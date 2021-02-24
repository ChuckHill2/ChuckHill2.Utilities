using System;
using GetCookie.Helper;

namespace GetCookie
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
