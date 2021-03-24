// --------------------------------------------------------------------------
// <copyright file="MyMemoryTranslate.cs" company="Chuck Hill">
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
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChuckHill2.Extensions;

namespace ChuckHill2.Translators
{
    /// <summary>
    /// Translate strings using MyMemory Translation API
    /// </summary>
    internal class MyMemoryTranslate : TranslatorBase
    {
        //private class TranslationResult
        //{
        //    public class ResponseDataX
        //    {
        //        public string TranslatedText { get; set; }
        //    }

        //    public ResponseDataX ResponseData { get; set; }
        //    public bool? QuotaFinished { get; set; }
        //    public string ResponseDetails { get; set; }
        //    public int ResponseStatus { get; set; }
        //    //[JsonProperty("exception_code")]
        //    public int? ExceptionCode { get; set; }
        //}

        protected override async Task<string> Translate(string input, string toLanguage)
        {
            switch (toLanguage)
            {
                case "zh-chs": toLanguage = "zh-CN"; break;
                case "zh-tw": toLanguage = "zh-TW"; break;
            }

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"http://api.mymemory.translated.net/get?q={WebUtility.UrlEncode(input)}&langpair=en|{toLanguage}&de=insertyouremail@gmail.com");

                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var value = SimpleJSON.JSON.Parse(result);
                    if (value["responseStatus"].AsInt == 200) return value["responseData"]["translatedText"].Value;
                    if (value["responseData"]["translatedText"].Value.ContainsI("INVALID TARGET LANGUAGE"))
                    {
                        UnsupportedLanguages.Add(toLanguage);
                        Translator.LogDebug($"Translate Warning {value["responseStatus"].Value}: ({toLanguage}) Invalid target language.");
                        return null;
                    }
                    if (value["quotaFinished"].AsBool == true) throw new Exception($"{value["ResponseStatus"].Value}: Usage quota exceeded.");
                    throw new Exception($"{value["responseStatus"].Value}: {value["responseDetails"].Value}");
                }
                else
                {
                    throw new Exception(response.StatusCode.ToString());
                }
            }
        }
    }
}
