// --------------------------------------------------------------------------
// <copyright file="TranslatorBase.cs" company="Chuck Hill">
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
// <author>Chuck Hill</author>
// --------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using ChuckHill2.Extensions;

namespace ChuckHill2.Translators
{
    internal interface ITranslator
    {
        /// <summary>
        /// True if non-recoverable error occured during translation. Disables
        /// all further language translations for the life of this instance.
        /// </summary>
        bool ErrorFault { get; }

        /// <summary>
        /// Translate a single word/sentence/paragraph.
        /// </summary>
        /// <param name="input">Source string to be translated translated</param>
        /// <param name="toLanguage">2-letter language code (e.g. en, fr, de, etc).</param>
        /// <returns>Translated string or null if translation fails.</returns>
        Task<string> TranslateText(string input, string toLanguage);
    }

    /// Resources:
    /// https://rapidapi.com/collection/google-translate-api-alternatives
    /// https://stackoverflow.com/questions/57580445/problems-with-scraping-bing-translations
    /// https://resxtranslatorbot.codeplex.com/SourceControl/latest
    /// https://archive.codeplex.com/?p=resxtranslatorbot
    /// http://api.mymemory.translated.net/get?q={0}&langpair=en|{1}&de=insertyouremail@gmail.com
    /// https://www.site24x7.com/tools/json-to-csharp.html --Create classes from JSON example

    /// <summary>
    /// Translatator base that handles all the boilerplate stuff.
    /// </summary>
    internal abstract class TranslatorBase : ITranslator
    {
        //Common User-Agent used by all the translators.
        protected const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:86.0) Gecko/20100101 Firefox/86.0";

        protected readonly HashSet<string> UnsupportedLanguages = new HashSet<string>(StringComparer.Ordinal);

        /// <summary>
        /// True if non-recoverable error occured during translation. Disables
        /// all further language translations for the life of this instance.
        /// </summary>
        public bool ErrorFault { get; private set; }

        /// <summary>
        /// Translate a single word/sentence/paragraph.
        /// </summary>
        /// <param name="input">Source string to be translated translated</param>
        /// <param name="toLanguage">2-letter language code (e.g. en, fr, de, etc).</param>
        /// <returns>Translated string or null if translation fails.</returns>
        public async Task<string> TranslateText(string input, string toLanguage)
        {
            //We normally don't care about the regioninfo except for Chinese.
            //Chinese 'zh' language codes are completely non-standard and must be adjusted differently within each translator.
            toLanguage = toLanguage.ToLowerInvariant();
            var lang = toLanguage.Split('-')[0];
            toLanguage = lang.EqualsI("zh") ? toLanguage : lang;

            //There are no differences between en-US and en-GB, so we do nothing. 
            if (string.IsNullOrWhiteSpace(input) || string.IsNullOrWhiteSpace(toLanguage) || toLanguage=="en") return input;

            if (ErrorFault || input.Length >= 5000) return null;

            if (UnsupportedLanguages.Contains(toLanguage)) return null;

            try
            {
                return await Translate(input, toLanguage);
            }
            catch (Exception ex)
            {
                ErrorFault = true; //Flag to disable all further translations from this application instance.
                Translator.LogDebug("Translate Error: " + ex.FullMessage());
                return null;
            }
        }

        /// <summary>
        /// Core method that performs the translation. Exception handling is performed by caller.
        /// </summary>
        /// <param name="input">Source string to be translated translated</param>
        /// <param name="toLanguage">2-letter language code (e.g. en, fr, de, etc).</param>
        /// <returns>Translated string or null if translation fails.</returns>
        //protected virtual async Task<string> Translate(string input, string toLanguage)
        //{
        //    return await Task.Run(() => (string)null);
        //}

        protected abstract Task<string> Translate(string input, string toLanguage);
    }

    internal static class TranslatorBaseExtensions
    {
        /// <summary>
        /// Handy workaround for setting/overriding header properties.
        /// Microsoft does not make it easy.
        /// </summary>
        /// <param name="headers">Header object</param>
        /// <param name="name">Name of header property</param>
        /// <param name="value">Value to set</param>
        internal static void Set(this HttpRequestHeaders headers, string name, string value)
        {
            if (headers.Contains(name)) headers.Remove(name);
            headers.Add(name, value);
        }
    }
}
