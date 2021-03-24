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
