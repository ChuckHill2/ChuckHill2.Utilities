// --------------------------------------------------------------------------
// <copyright file="MicrosoftTranslate.cs" company="Chuck Hill">
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
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SimpleJSON;
using ChuckHill2.Extensions;

namespace ChuckHill2.Translators
{
    /// <summary>
    /// Translate strings using Microsoft Azure Cognitive Services
    /// </summary>
    internal class MicrosoftTranslate : TranslatorBase
    {
        //https://docs.microsoft.com/en-us/azure/cognitive-services/translator/translator-text-how-to-signup to get subscription key.
        //https://portal.azure.com/#home
        //https://docs.microsoft.com/en-us/azure/cognitive-services/translator/reference/v3-0-translate
        //Example: https://github.com/MicrosoftTranslator/Text-Translation-API-V3-C-Sharp

        private class TranslationResult
        {
            public class DetectedLanguageX
            {
                public string Language { get; set; }
                public float Score { get; set; }
            }
            public class TextResult
            {
                public string Text { get; set; }
                public string Script { get; set; }
            }
            public class Translation
            {
                public class AlignmentX
                {
                    public string Proj { get; set; }
                }
                public class SentenceLength
                {
                    public int[] SrcSentLen { get; set; }
                    public int[] TransSentLen { get; set; }
                }

                public string Text { get; set; }
                public TextResult Transliteration { get; set; }
                public string To { get; set; }
                public AlignmentX Alignment { get; set; }
                public SentenceLength SentLen { get; set; }
            }

            public DetectedLanguageX DetectedLanguage { get; set; }
            public TextResult SourceText { get; set; }
            public Translation[] Translations { get; set; }
        }

        private class TranslateError
        {
            public class ErrorX
            {
                public int Code { get; set; }
                public string Message { get; set; }
            }

            public ErrorX Error { get; set; }
        }

        private const string SubscriptionKey = "1234567890abcdef01234567890abcde";  //bogus azure authentication key. 

        protected override async Task<string> Translate(string input, string toLanguage)
        {
            switch (toLanguage)
            {
                case "zh-chs": toLanguage = "zh-Hans"; break;
                case "zh-tw":  toLanguage = "zh-Hant"; break;
            }

            var body = new JSONArray();
            body.Add("Text", input);
            var requestBody = body.ToString(0);

            using (HttpClient client = new HttpClient())
            using (HttpRequestMessage request = new HttpRequestMessage())
            {
                // Build the request.
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri($"https://api.cognitive.microsofttranslator.com/translate?api-version=3.0&from=en&to={toLanguage}");
                request.Content = new StringContent(requestBody, Encoding.UTF8, "application/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", SubscriptionKey);
                HttpResponseMessage response = await client.SendAsync(request).ConfigureAwait(false);
                string result = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var value = SimpleJSON.JSON.Parse(result);
                    return value?[0]?["Translations"]?[0]?["Text"]?.Value;
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(result))
                        throw new Exception(response.StatusCode.ToString());
                    if (result.ContainsI("error"))
                    {
                        var value = SimpleJSON.JSON.Parse(result);
                        if (value["Error"]["Code"].AsInt == 400036) //The target language is not valid.
                        {
                            UnsupportedLanguages.Add(toLanguage);
                            Translator.LogDebug($"Translate Warning 400036: ({toLanguage}) {value["Error"]["Message"].Value}");
                            return null;
                        }
                        throw new Exception($"({value["Error"]["Code"].Value}) {value["Error"]["Message"].Value}");
                    }
                    throw new Exception(result);
                }
            }
        }
    }
}
