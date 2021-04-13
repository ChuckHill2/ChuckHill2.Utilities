//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="TranslatorTests.cs" company="Chuck Hill">
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
using ChuckHill2.Translators;
using NUnit.Framework;

namespace ChuckHill2.UnitTests
{
    [TestFixture]
    public class TranslatorTests
    {
        private const string Source = "This is a test.";
        private const string Language = "de-DE";

        [SetUp] public void Setup() { }

        [Test, Order(1)]
        public void TestMockTranslate()
        {
            string result = Translator.MockString(Source);
            Assert.AreEqual("†hìš ìš å ‡èš‡. ÌñþÙ8Ò", result, "MockString");
        }

        [Test, Order(2)]
        public void TestDefaultTranslate()
        {
            string result = Translator.Default(Source);
            Assert.AreEqual("\x00A7" + Source, result, "DefaultTranslate");
        }

        //Disabled because an Azure authentication key is required.
        //[Test, Order(3)]
        //public void TestMicrosoftTranslate()
        //{
        //    string result;
        //    ITranslator translate;

        //    translate = new MicrosoftTranslate();
        //    result = translate.TranslateText(Source, Language).Result;
        //    Assert.AreEqual("Dies ist ein Test.", result, translate.GetType().Name);

        //    result = translate.TranslateText(Source, "xx").Result;
        //    Assert.IsNull(result, translate.GetType().Name);
        //}

        [Test, Order(4)]
        public void TestMicrosoftTranslateScrape()
        {
            ITranslator translate = new MicrosoftTranslateScrape();
            string result = translate.TranslateText(Source, Language).Result;
            Assert.AreEqual("Dies ist ein Test.", result, translate.GetType().Name);

            result = translate.TranslateText(Source, "xx").Result;
            Assert.IsNull(result, translate.GetType().Name);
        }

        [Test, Order(5)]
        public void TestGoogleTranslateScrape()
        {
            ITranslator translate = new GoogleTranslateScrape();
            string result = translate.TranslateText(Source, Language).Result;
            Assert.AreEqual("Das ist ein Test.", result, translate.GetType().Name);

            result = translate.TranslateText(Source, "xx").Result;
            Assert.IsNull(result, translate.GetType().Name);
        }

        [Test, Order(6)]
        public void TestTranslate_com_Scrape()
        {
            ITranslator translate = new Translate_com_Scrape();
            string result = translate.TranslateText(Source, Language).Result;
            Assert.AreEqual("Dies ist ein Test.", result, translate.GetType().Name);

            result = translate.TranslateText(Source, "xx").Result;
            Assert.IsNull(result, translate.GetType().Name);
        }

        [Test, Order(7)]
        public void TestMyMemoryTranslate()
        {
            ITranslator translate = new MyMemoryTranslate();
            string result = translate.TranslateText(Source, Language).Result;
            Assert.AreEqual("Dies ist ein Test.", result, translate.GetType().Name);

            result = translate.TranslateText(Source, "xx").Result;
            Assert.IsNull(result, translate.GetType().Name);
        }

        [Test, Order(8)]
        public void TestTranslate()
        {
            string result = Translator.Translate(Source, Language);
            Assert.AreEqual(Translator.Default(Source), result, "Top-level Translate(enabled=false)");

            Translator.Enable = true;
            result = Translator.Translate(Source, Language);
            Assert.AreEqual("Dies ist ein Test.", result, "Top-level Translate(enabled=true)");

            result = Translator.Translate(Source, "XX");
            Assert.AreEqual(Translator.Default(Source), result, "Top-level Translate(unknown lang)");
        }
    }
}
