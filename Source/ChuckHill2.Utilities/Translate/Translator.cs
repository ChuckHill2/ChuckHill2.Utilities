//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Translator.cs" company="Chuck Hill">
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
using System.Diagnostics;
using System.Text;
using ChuckHill2.Translators;

namespace ChuckHill2
{
    /// <summary>
    /// Translate an English word or phrase into another language.
    /// </summary>
    public static class Translator
    {
        /// <summary>
        /// Flag to enable string translations. False returns always returns the value of Default(). This is the default.
        /// </summary>
        public static bool Enable { get; set; }

        //List of known translation services. They may have limited usage so translation falls-back in to the next translation service in the list. 
        private static readonly ITranslator[] Translators = new ITranslator[]
        {
            //new MicrosoftTranslate(),     //Azure suspended my 'free' account. I barely used it! Life of these official API's are no better than scraping!
            new MicrosoftTranslateScrape(),
            new GoogleTranslateScrape(),
            new Translate_com_Scrape(),
            new MyMemoryTranslate()         //translations are not the greatest.
        };

        #region private static readonly Dictionary<char, char> MockCharTranslation = new Dictionary<char, char>()
        private static readonly Dictionary<char, char> MockCharTranslation = new Dictionary<char, char>()
        {
            { 'A', '\x00c5' },
            { 'B', '\x00DF' },
            { 'C', '\x00c7' },
            { 'D', '\x00D0' },
            { 'E', '\x00c8' },
            { 'F', 'F' },
            { 'G', 'G' },
            { 'H', 'H' },
            { 'I', '\x00cc' },
            { 'J', '\x222B' },
            { 'K', 'K' },
            { 'L', '\x0141' },
            { 'M', '\x03C0' },
            { 'N', '\x00d1' },
            { 'O', '\x00d2' },
            { 'P', 'P' },
            { 'Q', '\x2126' },
            { 'R', 'R' },
            { 'S', '\x0160' },
            { 'T', '\x2020' },
            { 'U', '\x00d9' },
            { 'V', 'V' },
            { 'W', 'W' },
            { 'X', 'X' },
            { 'Y', '\x0178' },
            { 'Z', '\x017D' },
            { 'a', '\x00e5' },
            { 'b', 'b' },
            { 'c', '\x00e7' },
            { 'd', 'd' },
            { 'e', '\x00e8' },
            { 'f', 'ƒ' },
            { 'g', 'g' },
            { 'h', 'h' },
            { 'i', '\x00ec' },
            { 'j', 'j' },
            { 'k', 'k' },
            { 'l', '\x222B' },
            { 'm', 'm' },
            { 'n', '\x00f1' },
            { 'o', '\x00f2' },
            { 'p', '\x00FE' },
            { 'q', '\x2202' },
            { 'r', 'r' },
            { 's', '\x0161' },
            { 't', '\x2021' },
            { 'u', '\x00f9' },
            { 'v', '\x221A' },
            { 'w', 'w' },
            { 'x', '\x00D7' },
            { 'y', '\x00FF' },
            { 'z', '\x017E' },
            { '0', '0' },
            { '1', '1' },
            { '2', '2' },
            { '3', '3' },
            { '4', '4' },
            { '5', '5' },
            { '6', '6' },
            { '7', '7' },
            { '8', '8' },
            { '9', '9' }
        };
        #endregion

        /// <summary>
        /// Create pseudo-translated string to look like anglicized language with extra padding to test field sizes.
        /// </summary>
        /// <param name="s">Original english word or phrase</param>
        /// <param name="size">%Size of new anglicized word or phrase. Must be greater than 100%</param>
        /// <returns>New pseudo-translation</returns>
        public static string MockString(string s, int size = 150)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;

            int length = size <= 100 ? s.Length : (int)(s.Length * size / 100.0);

            StringBuilder sb = new StringBuilder(length);
            foreach (char c in s)
            {
                sb.Append(MockCharTranslation.TryGetValue(c, out char C) ? C : c);
            }

            if (length <= sb.Length) return sb.ToString();

            sb.Append(' ');
            Random rand = new Random(s.Length); //for a given string length get repeatable pseudo-random numbers. Unit tests require it!
            string randChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            int k = 0;
            while (sb.Length < length)
            {
                sb.Append(MockCharTranslation[randChars[rand.Next(0, randChars.Length - 1)]]);
                if (++k % 8 == 0) sb.Append(' ');
            }
            if (sb[sb.Length - 1] == ' ') sb.Length--;

            return sb.ToString();
        }

        /// <summary>
        /// Default 'translation' which just prefixes string with a special character. This simply signifies that this string has not yet been manually translated.
        /// </summary>
        /// <param name="s">Original english word or phrase</param>
        /// <returns>'Translated' String.</returns>
        public static string Default(string s)
        {
            //Flag as 'translated'
            return "\x00A7" + s; // '\x00A7'=='§'
        }

        /// <summary>
        /// Translate a single word/sentence/paragraph using machine-language translation services.
        /// </summary>
        /// <param name="input">The string to be translated</param>
        /// <param name="lang">Culture code (e.g. en-US), or 2-letter language code (e.g. en), case-insensitive. </param>
        /// <returns>Translated string or §string if it cannot be translated.</returns>
        public static string Translate(string s, string lang)
        {
            if (Translator.Enable)
            {
                foreach (var translator in Translators)
                {
                    if (translator.ErrorFault) continue;
                    var result = translator.TranslateText(s, lang).Result;
                    if (result != null) return result;
                }

                LogDebug("Using '\x00A7name' for all subsequent translations.");
            }

            return Default(s);
        }

        /// <summary>
        /// Write debug messages to the console. Available in DEBUG mode only.
        /// </summary>
        /// <param name="s">Message to log</param>
        [Conditional("DEBUG")]
        public static void LogDebug(string s)
        {
            Console.WriteLine(s);
        }
    }
}
