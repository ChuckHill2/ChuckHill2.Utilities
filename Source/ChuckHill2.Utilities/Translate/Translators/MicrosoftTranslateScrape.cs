//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="MicrosoftTranslateScrape.cs" company="Chuck Hill">
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
using System.Text;
using System.Threading.Tasks;

namespace ChuckHill2.Translators
{
    /// <summary>
    /// Translate strings using Microsoft 
    /// </summary>
    internal class MicrosoftTranslateScrape : TranslatorBase
    {
        // private class TranslationResult
        // {
        //    public class DetectedLanguage
        //    {
        //        public string language { get; set; }
        //        public double score { get; set; }
        //    }
        //    public class Translation
        //    {
        //        public string text { get; set; }
        //        public string to { get; set; }
        //    }
        //    public DetectedLanguage detectedLanguage { get; set; }
        //    public IList<Translation> translations { get; set; }
        //}
        //private class TranslationError
        //{
        //    public int statusCode { get; set; }
        //}

        protected override async Task<string> Translate(string input, string toLanguage)
        {
            switch (toLanguage)
            {
                case "zh-cn": toLanguage = "zh-Hans"; break;
                case "zh-chs": toLanguage = "zh-Hans"; break;
                case "zh-tw": toLanguage = "zh-Hant"; break;
                case "no": toLanguage = "nb"; break;
            }

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.Headers.Host = "www.bing.com";
                request.Headers.Set("User-Agent", UserAgent);
                request.Headers.Set("Accept", "*/*");
                request.Headers.Set("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Set("Referer", "https://www.bing.com/");
                request.Headers.Set("Connection", "keep-alive");
                request.RequestUri = new Uri("https://www.bing.com/ttranslatev3");
                request.Content = new StringContent($"&fromLang=en&text={WebUtility.UrlEncode(input)}&to={toLanguage}", Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var value = SimpleJSON.JSON.Parse(result);

                    if (result.Contains("statusCode"))
                    {
                        if (value["statusCode"].AsInt == 400)
                        {
                            base.UnsupportedLanguages.Add(toLanguage);
                            Translator.LogDebug($"Translate Warning 400: ({toLanguage}) Bad Request.");
                            return null;
                        }
                    }
                    return value?[0]?["translations"]?[0]?["text"]?.Value;
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
