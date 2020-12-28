using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ChuckHill2.Utilities.Extensions;
using NUnit.Framework;

namespace ChuckHill2.Utilities.UnitTests
{
    [TestFixture]
    public class CommonExtensionsTests
    {
        [SetUp] public void Setup() { }

        [Test]
        public void TestTypeExtensions()
        {
            var s = typeof(Panel).GetManifestResourceStream("CheckBox.bmp");
            Assert.IsNotNull(s, "CheckBox.bmp does not exist.");
            s = typeof(Panel).GetManifestResourceStream("CheckBox.jpg");
            Assert.IsNull(s, "CheckBox.jpg exists.");
        }

        [Test]
        public void TestIntExtensions()
        {
            var s = (-650).ToCapacityString(2);
            Assert.AreEqual(s, "-650 B", "Byte formatting failed.");
            s = 4000.ToCapacityString(2);
            Assert.AreEqual(s, "3.91 KB", "KiloByte formatting failed.");
            s = (1024 * 1024 + ((1024 * 1024)/2)).ToCapacityString(2);
            Assert.AreEqual(s, "1.5 MB" , "MegaByte formatting failed.");
            s = (1024 * 1024 * 1024).ToCapacityString(2);
            Assert.AreEqual(s, "1 GB", "GigaByte formatting failed.");
            s = (1024.0m * 1024.0m * 1024.0m * 1024.0m).ToCapacityString(2);
            Assert.AreEqual(s, "1 TB", "TeraByte formatting failed.");
            s = (1024.0m * 1024.0m * 1024.0m * 1024.0m * 1024.0m).ToCapacityString(2);
            Assert.AreEqual(s, "1 PB", "PetaByte formatting failed.");
            s = (1024.0m * 1024.0m * 1024.0m * 1024.0m * 1024.0m * 1024.0m).ToCapacityString(2);
            Assert.AreEqual(s, "1 EB", "ExaByte formatting failed.");
        }

        [Test]
        public void TestStringExtensions()
        {
            string source = @"
                Strip one or more whitspace chars (including newlines)
                and replace with a single space char.    ";
            string result;

            result = source.Squeeze();
            Assert.AreEqual("Strip one or more whitspace chars (including newlines) and replace with a single space char.", result, "Squeeze(whitespace)");
            result = source.Squeeze(new char[] { '(', ')','\r','\n' }, ' ');
            Assert.AreEqual("Strip one or more whitspace chars including newlines and replace with a single space char.", result, "Squeeze(whitespace+'(',')', ' ')");
            result = source.Squeeze(StringExtensions.WhiteSpace.Concat(new char[] { '(', ')' }).ToList());
            Assert.AreEqual("Striponeormorewhitspacecharsincludingnewlinesandreplacewithasinglespacechar.", result, "Squeeze(whitespace+'(',')')");

            Assert.IsTrue(((string)null).IsNullOrEmpty(), "null.IsNullOrEmpty()");
            Assert.IsTrue("".IsNullOrEmpty(), "\"\".IsNullOrEmpty()");
            Assert.IsTrue(" \r\n   ".IsNullOrEmpty(), "\"  \".IsNullOrEmpty()");
            Assert.IsFalse(" ABC ".IsNullOrEmpty(), "\"ABC\".IsNullOrEmpty()");

            source = null;
            Assert.IsNull(source.TrimEx(), "null.TrimEx()");
            source = "  \0  ABC\r\n  \0DEF  ";
            Assert.AreEqual("", source.TrimEx(), "TrimEx(), Source contains char '\\0'");
            source = "  ABC\r\n  \0DEF  ";
            Assert.AreEqual("ABC", source.TrimEx(), "TrimEx(), Source contains char '\\0'");
            Assert.AreEqual("ABC\r\n  ", source.TrimStartEx(), "TrimStartEx(), Source contains char '\\0'");
            Assert.AreEqual("  ABC", source.TrimEndEx(), "TrimEndEx(), Source contains char '\\0'");

            source = null;
            Assert.IsTrue(source.Contains(null, true), "null.Contains(null,true)");
            Assert.IsFalse(source.Contains("def", true), "null.Contains(value,true)");
            source = "ABCDEFGHI";
            Assert.IsFalse(source.Contains(null, true), "value.Contains(null,true)");
            Assert.IsFalse(source.Contains("def",false), "value.Contains(value,false)");
            Assert.IsTrue(source.Contains("def", true), "value.Contains(value,true)");

            source = null;
            Assert.IsTrue(source.EqualsI(null), "null.EqualsI(null)");
            Assert.IsFalse(source.EqualsI("def"), "null.EqualsI(value)");
            source = "ABCDEFGHI";
            Assert.IsFalse(source.EqualsI(null), "value.EqualsI(null)");
            Assert.IsFalse(source.EqualsI("def"), "value.EqualsI(value)");
            Assert.IsTrue(source.EqualsI("AbcDefGhi"), "value.EqualsI(value)");

            Assert.IsNull(((string)null).Remove(), "null.Remove()");
            Assert.AreEqual("ABCDEF",     "    ABC\r\n  DEF  ".Remove(), "value.Remove()");
            Assert.AreEqual("ABC\r\nDEF", "    ABC\r\n  DEF  ".Remove(new char[] { ' ', 'b' }), "value.Remove(' ','b')");

            Assert.AreEqual("__", ((string)null).ToIdentifier(), "null.ToIdentifier()");
            Assert.AreEqual("__", "".ToIdentifier(), "\"\".ToIdentifier()");
            Assert.AreEqual("HelloWorld", " hello-\r\nworld! ".ToIdentifier(), "value.ToIdentifier()");
            Assert.AreEqual("HelloWoRld", " hello-\r\nwoRld! ".ToIdentifier(), "value.ToIdentifier()");

            Assert.IsNull(((string)null).ToString(", "), "null.ToString(\", \")");
            Assert.AreEqual("H|e|l|l|o|W|o|r|l|d", "HelloWorld".ToString("|"), "value.ToString(\"|\")");

            var dict = new Dictionary<int, Version>()
            {
                {1, new Version(1,2,3,4) },
                {2, new Version(5,6,7,8) },
                {3, new Version(9,0,1,2) }
            };
            result = dict.ToString(", ", kv => $"{kv.Key}={kv.Value}");
            Assert.AreEqual("1=1.2.3.4, 2=5.6.7.8, 3=9.0.1.2", result, "value.ToString(\", \")");

            Assert.IsNull(((byte[])null).ToStringEx(), "((byte[])null).ToStringEx()");
            Assert.AreEqual("", (new byte[0]).ToStringEx(), "((new byte[0]).ToStringEx()");

            var bytes = Encoding.ASCII.GetBytes("Hello World");
            Assert.AreEqual("Hello World", bytes.ToStringEx(), "ASCII bytes.ToStringEx()");
            bytes = Encoding.UTF7.GetBytes("Hello World");
            Assert.AreEqual("Hello World", bytes.ToStringEx(), "UTF7 bytes.ToStringEx()");

            bytes = Encoding.UTF8.GetBytes("Hello \x2022 World"); // '\x2022' == '•' == bullit
            Assert.AreEqual("Hello \x2022 World", bytes.ToStringEx(), "UTF8(no preamble) bytes.ToStringEx()");

            bytes = Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("Hello \x2022 World")).ToArray();
            Assert.AreEqual("Hello \x2022 World", bytes.ToStringEx(), "UTF8 bytes.ToStringEx()");
            bytes = Encoding.UTF32.GetPreamble().Concat(Encoding.UTF32.GetBytes("Hello \x2022 World")).ToArray();
            Assert.AreEqual("Hello \x2022 World", bytes.ToStringEx(), "UTF32 bytes.ToStringEx()");

            var stream = new MemoryStream(Encoding.UTF8.GetBytes("Hello \x2022 World"));
            Assert.AreEqual("Hello \x2022 World", stream.ToStringEx(), "UTF8(no preamble) stream.ToStringEx()");
            stream = new MemoryStream(Encoding.UTF8.GetPreamble().Concat(Encoding.UTF8.GetBytes("Hello \x2022 World")).ToArray());
            Assert.AreEqual("Hello \x2022 World", stream.ToStringEx(), "UTF8 stream.ToStringEx()");

            source = @"<?xml version=""1.0"" encoding=""utf-8""?>";
            Assert.IsFalse(((string)null).IsXml(), "null.IsXml()");
            Assert.IsFalse("Hello World".IsXml(), "\"Hello World\".IsXml()");
            Assert.IsTrue(source.IsXml(), "(xml header).IsXml()");
            source = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble().Concat(source.ToBytes()).ToArray());
            Assert.IsTrue(source.IsXml(), "(xml header w/preamble).IsXml()");

            Assert.IsFalse(((string)null).IsFileName(), "null.IsFileName()");
            Assert.IsFalse("".IsFileName(), "\"\".IsFileName()");
            Assert.IsTrue(@"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe".IsFileName(), "(fullfilepath).IsFileName()");
            Assert.IsTrue(@"\\CLOUD\MyBackups\Backup.bak".IsFileName(), "(UNC filepath).IsFileName()");
            Assert.IsFalse(@"C:\Program / Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe".IsFileName(), "(full/filepath).IsFileName()");
            Assert.IsFalse(@"spyxx.exe".IsFileName(), "(relfilename).IsFileName()");
            Assert.IsFalse((new String('X', 500)).IsFileName(), "(veryverylongstring).IsFileName()");
            Assert.IsFalse(@"D:\abc\".IsFileName(), "(path,nofilename).IsFileName()");

            Assert.IsFalse(((string)null).IsBase64(), "null.IsBase64()");
            Assert.IsFalse("".IsBase64(), "\"\".IsBase64()");
            source = Convert.ToBase64String(Encoding.UTF8.GetBytes("Hello World"));
            Assert.IsFalse("Hello World".IsBase64(), "\"Hello World\".IsBase64()");
            Assert.IsFalse(("A" + source).IsBase64(), "(corrupted value).IsBase64()");
            Assert.IsTrue(" S G k = ".IsBase64(), "\" S G k = \".IsBase64() //'Hi'");
            Assert.IsTrue(source.IsBase64(), "(base64 value).IsBase64() //'Hello World'");

            Assert.IsFalse(((string)null).IsCSV(), "null.IsCSV()");
            Assert.IsFalse("".IsCSV(), "\"\".IsCSV()");
            Assert.IsFalse("aa,bb,cc".IsCSV(), "\"aa, bb, cc\".IsCSV()");
            Assert.IsTrue(TestData.Csv.IsCSV(), "(csvData).IsCSV()");

            Assert.AreEqual(new Guid("b18d0ab1-e064-4175-05b7-a99be72e3fe5"), "Hello World".ToHash(), "\"Hello World\".ToHash()");

            Assert.AreEqual(TestData.Json, TestData.Json.ToBytes().ToHex().FromHex().ToStringEx(), "ToHex().FromHex()");
            Assert.AreEqual(TestData.Json, TestData.Json.ToBytes().ToHex(40).FromHex().ToStringEx(), "ToHex(40).FromHex()");
        }

        [Test]
        public void TestDictionaryExtensions()
        {

        }
        [Test]
        public void TestListExtensions()
        {

        }
        [Test]
        public void TestExceptionExtensions()
        {

        }
        [Test]
        public void TestObjectExtensions()
        {

        }
        [Test]
        public void TestEnumExtensions()
        {

        }
        [Test]
        public void TestAssemblyExtensions()
        {

        }
        [Test]
        public void TestMemberInfoExtensions()
        {

        }
        [Test]
        public void TestAppDomainExtensions()
        {

        }
        [Test]
        public void TestDateTimeExtensions()
        {

        }
        [Test]
        public void TestEnumerableExtensions()
        {

        }
    }
}
