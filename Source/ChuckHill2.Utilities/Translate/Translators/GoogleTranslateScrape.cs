//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="GoogleTranslateScrape.cs" company="Chuck Hill">
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

/// <summary>
/// Internal classes exclusively used by Translator.Translate()
/// </summary>
namespace ChuckHill2.Translators
{
    /// <summary>
    /// Translate strings using Google UI 
    /// </summary>
    internal class GoogleTranslateScrape : TranslatorBase
    {
        protected override async Task<string> Translate(string input, string toLanguage)
        {
            switch (toLanguage)
            {
                case "zh-chs": toLanguage = "zh-CN"; break;  //chinese simplified
                case "zh-hans": toLanguage = "zh-CN"; break; //chinese simplified
                case "zh-hant": toLanguage = "zh-TW"; break; //chinese traditional
                case "fil": toLanguage = "tl"; break; //Filipino
                case "he": toLanguage = "iw"; break;  //Hebrew
                case "nb": toLanguage = "no"; break;  //Norwegian bokmal to nynorsk
            }

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.Headers.Host = "translate.google.com";
                request.Headers.Set("User-Agent", UserAgent);
                request.Headers.Set("Accept", "*/*");
                request.Headers.Set("Accept-Language", "en-US,en;q=0.9");
                request.Headers.Set("Referer", "https://translate.google.com/");
                //request.Headers.Set("Connection", "keep-alive");
                request.RequestUri = new Uri($"https://translate.google.com/translate_a/single?client=gtx&dt=t&ie=utf-8&oe=utf-8&sl=en&tl={toLanguage}&q={WebUtility.UrlEncode(input)}");
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    // Result Example: [[["Qui êtes vous.","Who are You.",null,null,1]],null,"en",null,null,null,null,[]]
                    var value = SimpleJSON.JSON.Parse(result);
                    string v = value[0][0][0].Value;
                    if (input.Equals(v) && toLanguage != "en")
                    {
                        v = null;
                        base.UnsupportedLanguages.Add(toLanguage);
                    }

                    return v;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(result))
                        throw new Exception(response.StatusCode.ToString());
                    throw new Exception(result);
                }
            }
        }
    }
}
