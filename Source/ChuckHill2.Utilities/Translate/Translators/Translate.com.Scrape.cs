//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Translate.com.Scrape.cs" company="Chuck Hill">
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
using ChuckHill2.Extensions;

namespace ChuckHill2.Translators
{
    /// <summary>
    /// Translate strings using Microsoft 
    /// </summary>
    internal class Translate_com_Scrape : TranslatorBase
    {
        // public class TranslationResult
        // {
        //    public class Human_translation_details
        //    {
        //        public int free_words_remaining_after_order { get; set; }
        //        public int free_words_used_in_order { get; set; }
        //        public int plan_monthly_words_used_in_order { get; set; }
        //        public int plan_previous_plan_words_used { get; set; }
        //        public int free_words_used { get; set; }
        //        public string total_cost { get; set; }
        //        public string translation_language { get; set; }
        //        public string total_words { get; set; }
        //        public string translation_text_preview { get; set; }
        //    }
        //    public string result { get; set; }
        //    public string message { get; set; }
        //    public string original_text { get; set; }
        //    public string translated_text { get; set; }
        //    public long translation_id { get; set; }
        //    public string uri_slug { get; set; }
        //    public string seo_directory_url { get; set; }
        //    public string translation_source { get; set; }
        //    public string request_source { get; set; }
        //    public bool is_favorite { get; set; }
        //    public bool human_translaton_possible { get; set; }
        //    public Human_translation_details human_translation_details { get; set; }
        // }

        protected override async Task<string> Translate(string input, string toLanguage)
        {
            switch (toLanguage)
            {
                case "zh-chs": toLanguage = "zh"; break;
                case "zh-tw": toLanguage = "zh-TW"; break;
            }

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.Headers.Host = "www.translate.com";
                request.Headers.Set("User-Agent", UserAgent);
                request.Headers.Set("Accept", "*/*");
                request.Headers.Set("Accept-Language", "en-US,en;q=0.5");
                request.Headers.Set("Referer", "https://www.translate.com/");
                request.Headers.Set("Connection", "keep-alive");
                request.RequestUri = new Uri("https://www.translate.com/translator/ajax_translate");
                request.Content = new StringContent($"&text_to_translate={WebUtility.UrlEncode(input)}&source_lang=en&translated_lang={toLanguage}&use_cache_only=false", Encoding.UTF8, "application/x-www-form-urlencoded");
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var value = SimpleJSON.JSON.Parse(result);

                    if (value["result"].Value.EqualsI("error"))
                    {
                        base.UnsupportedLanguages.Add(toLanguage);
                        return null;
                    }

                    return value["translated_text"].Value;
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
