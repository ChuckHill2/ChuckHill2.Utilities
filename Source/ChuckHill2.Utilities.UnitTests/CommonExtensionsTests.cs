using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ChuckHill2.Extensions;
using ChuckHill2.Extensions.Reflection;
using NUnit.Framework;

namespace ChuckHill2.UnitTests
{
    [TestFixture]
    public class CommonExtensionsTests
    {
        [SetUp] public void Setup() { }

        [Test]
        public void TestAppDomainExtensions()
        {
            var ad = AppDomain.CurrentDomain;
            var currentName = ad.SetFriendlyName("Unit Test");
            Assert.AreEqual("Unit Test", ad.FriendlyName, "ad.SetFriendlyName(newname)");
            ad.SetFriendlyName(currentName);
        }

        [Test]
        public void TestAssemblyExtensions()
        {
            var asm = Assembly.GetExecutingAssembly();

            Assert.AreEqual("ChuckHill2.UnitTests", asm.Attribute<AssemblyProductAttribute>(), "Attribute<> Constructor");
            Assert.AreEqual("True", asm.Attribute<System.Runtime.CompilerServices.RuntimeCompatibilityAttribute>(), "Attribute<> Named");
            Assert.IsNull(asm.Attribute<System.Runtime.InteropServices.TypeLibVersionAttribute>(), "Attribute<> Missing");
            Assert.AreEqual("", asm.Attribute<UnitTestAttribute>(), "Attribute<> Named");

            Assert.IsFalse(asm.AttributeExists<System.Runtime.InteropServices.TypeLibVersionAttribute>(), "AttributeExists<> Missing");
            Assert.IsTrue(asm.AttributeExists<AssemblyProductAttribute>(), "AttributeExists<> Exists");


            var timestamp = ((Assembly)null).PeTimeStamp();
            Assert.IsTrue(timestamp > new DateTime(2000, 1, 1) && timestamp <= DateTime.Now, "null.PEtimestamp()");

            timestamp = asm.PeTimeStamp();
            Assert.IsTrue(timestamp > new DateTime(2000, 1, 1) && timestamp <= DateTime.Now, "Fake PEtimestamp()");

            //Project deterministic flag == false for release and true for debug 
            var location = Assembly.GetExecutingAssembly().Location;
            var fn = Path.Combine(Path.GetDirectoryName(Path.GetDirectoryName(location)), "Release", Path.GetFileName(location));
            if (File.Exists(fn))
            {
                timestamp = (DateTime)typeof(AssemblyExtensions).InvokeReflectedMethod("PeTimeStamp", fn);
                Assert.IsTrue(timestamp > new DateTime(2000, 1, 1) && timestamp <= DateTime.Now, "True PEtimestamp(filename)");
            }

            timestamp = (DateTime)typeof(AssemblyExtensions).InvokeReflectedMethod("PeTimeStamp", ((string)null));
            Assert.IsTrue(timestamp > new DateTime(2000, 1, 1) && timestamp <= DateTime.Now, "Fake PEtimestamp()");

            fn = Path.GetRandomFileName();
            Assert.Catch<System.IO.FileNotFoundException>(() => typeof(AssemblyExtensions).InvokeReflectedMethod("PeTimeStamp", fn), $"Could not find file '{fn}'.");

            fn = Path.GetTempFileName();
            Assert.Catch<BadImageFormatException>(() => typeof(AssemblyExtensions).InvokeReflectedMethod("PeTimeStamp", fn), "Not a PE file.File too small.");
            File.Delete(fn);

            fn = Path.ChangeExtension(location,"pdb");
            Assert.Catch<BadImageFormatException>(() => typeof(AssemblyExtensions).InvokeReflectedMethod("PeTimeStamp", fn), "Not a PE file. DOS Signature not found.");
        }

        [Test]
        public void TestDataSetExtensions()
        {

        }

        [Test]
        public void TestDateTimeExtensions()
        {
            const int time_t = 1582230020;
            var dt = new DateTime(2020, 2, 20, 20, 20, 20, 20);

            Assert.AreEqual(time_t, dt.ToUnixTime(), "ToUnixTime()");
            Assert.AreEqual(new DateTime(2020, 2, 20, 20, 20, 20, 0), time_t.FromUnixTime(), "FromUnixTime()");

            Assert.AreEqual(new DateTime(2020, 2, 20, 20, 20, 20, 0), dt.ToSecond(), "");
            Assert.AreEqual(new DateTime(2020, 2, 20, 20, 20, 0, 0), dt.ToMinute(), "");
            Assert.AreEqual(new DateTime(2020, 2, 20, 20, 0, 0, 0), dt.ToHour(), "");
            Assert.AreEqual(new DateTime(2020, 2, 21, 0, 0, 0, 0), dt.ToDay(), "");

            dt = new DateTime(2020, 2, 20, 12, 35, 35, 555);
            Assert.AreEqual(new DateTime(2020, 2, 20, 12, 35, 36, 0), dt.ToSecond(), "");
            Assert.AreEqual(new DateTime(2020, 2, 20, 12, 36, 0, 0), dt.ToMinute(), "");
            Assert.AreEqual(new DateTime(2020, 2, 20, 13, 0, 0, 0), dt.ToHour(), "");
            Assert.AreEqual(new DateTime(2020, 2, 21, 0, 0, 0, 0), dt.ToDay(), "");
            Assert.AreEqual(new DateTime(2020, 2, 20, 0, 0, 0, 0), new DateTime(2020, 2, 20, 11, 0, 0).ToDay(), "");
        }

        [Test]
        public void TestDictionaryExtensions()
        {
            string source = "1=A, 2 ==B=X, 3 = C, , 4=D, 5=E, 6=, 7, ";
            Assert.AreEqual(7, source.ToDictionary().Count, "ToDictionary<string,string>()");
            Assert.AreEqual(7, source.ToDictionary(',', '=', k => int.Parse(k), v => v).Count, "ToDictionary<int,string>()");

            var dictionary = source.ToDictionary(',', '=');
            Assert.AreEqual("C", dictionary.GetValue("3"), "dictionary.GetValue(validvalue)");
            Assert.AreEqual(null, dictionary.GetValue("XXX"), "dictionary.GetValue(invalidvalue)");
            Assert.AreEqual(null, dictionary.GetValue(null), "dictionary.GetValue(null)");
            dictionary = null;
            Assert.AreEqual(null, dictionary.GetValue("3"), "null.GetValue(\"3\")");

            Assert.AreEqual(0, " ".ToDictionary().Count, "");
            Assert.AreEqual(0, ((string)null).ToDictionary().Count, "");
            Assert.AreEqual(1, "  ABC ".ToDictionary().Count, "");
        }

        private enum TestEnum
        {
            //Warning: NUnit has a DescriptionAttribute too!
            [System.ComponentModel.DescriptionAttribute("Zero (0)")] Zero,
            [System.ComponentModel.DescriptionAttribute("One (1)")] One,
            [System.ComponentModel.DescriptionAttribute("Two (2)")] Two,
            [System.ComponentModel.DescriptionAttribute("Three (3)")] Three,
            [System.ComponentModel.DescriptionAttribute("Four (4)")] Four,
            [System.ComponentModel.DescriptionAttribute("Five (5)")] Five,
            [System.ComponentModel.DescriptionAttribute("Six (6)")] Six,
            [System.ComponentModel.DescriptionAttribute("Seven (7)")] Seven,
            [System.ComponentModel.DescriptionAttribute("Eight (8)")] Eight,
            [System.ComponentModel.DescriptionAttribute("Nine (9)")] Nine,
            /*[System.ComponentModel.DescriptionAttribute("Ten (10)")]*/ Ten
        }

        [Test]
        public void TestEnumExtensions()
        {
            Assert.Catch<ArgumentException>(() => 42.Description(), "T must be an enumerated type");
            Assert.Catch<ArgumentException>(() => 42.AllDescriptions(), "T must be an enumerated type");
            Assert.AreEqual("Ten", TestEnum.Ten.Description(), "enum.Description() //no description");

            Assert.AreEqual("Eight (8)", TestEnum.Eight.Description(), "enum.Description()");

            var desc = TestEnum.Four.AllDescriptions();
            Assert.AreEqual(11, desc.Count, "enum.AllDescriptions().Count");
            Assert.AreEqual("Eight (8)", desc[TestEnum.Eight], "enum.AllDescriptions()[value]");
        }

        [Test]
        public void TestExceptionExtensions()
        {
            Exception exception = new Exception("Test Exception");

            Assert.AreEqual("Test Exception\r\nSuffix", exception.AppendMessage("Suffix").Message, "AppendMessage()");
            Assert.AreEqual("Prefix\r\nTest Exception\r\nSuffix", exception.PrefixMessage("Prefix").Message, "PrefixMessage()");
            Assert.AreEqual("Replaced Message", exception.ReplaceMessage("Replaced Message").Message, "ReplaceMessage()");
            Assert.IsTrue(exception.WithStackTrace().WithStackTrace().StackTrace.Length > 10, "WithStackTrace()");
            exception.AppendInnerException(new Exception("InnerException"));
            exception.AppendInnerException(new Exception("InnerException 2"));
            Assert.AreEqual("Exception: Replaced Message / InnerException / InnerException 2", exception.FullMessage(), "FullMessage()");
        }

        [Test]
        public void TestGDI()
        {
            //This project has an icon associated with this assembly
            var ico = GDI.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location, 32);
            Assert.AreEqual(32, ico.Width);

            ico = GDI.ExtractAssociatedIcon(Assembly.GetCallingAssembly().Location, 32);
            Assert.IsNull(ico);
        }

        [Test]
        public void TestListExtensions()
        {
            string source = "A,B,C, D  ,E,F, G";
            var list = source.ToEnumerableList().ToList();

            Assert.AreEqual(7, list.Count, "ToList<string>()");

            string source2 = "A,B,C,E,D,F,G";

            Assert.IsTrue(source.ListEquals(source2), "source.ListEquals(source2)");
            Assert.IsFalse("A,B,C,E".ListEquals(source2), "\"A,B,C,E\".ListEquals(source2)");
            Assert.IsFalse(" ".ListEquals(source2), "\"\".ListEquals(source2)");
            Assert.IsFalse(source.ListEquals(""), "source.ListEquals(\"\")");
            Assert.IsTrue("".ListEquals(""), "\"\".ListEquals(\"\")");

            Assert.IsFalse(source.ListEquals(source2, false), "source.ListEquals(source2,false)");

            Assert.AreEqual(4, list.IndexOf(m => m == "E"), "list.IndexOf()");
            Assert.AreEqual(-1, list.IndexOf(m => m == "Z"), "list.IndexOf()");
            Assert.AreEqual(4, list.LastIndexOf(m => m == "E"), "list.LastIndexOf()");
            Assert.AreEqual(-1, list.LastIndexOf(m => m == "Z"), "list.LastIndexOf()");

            list.MoveToIndex("E", 2);
            Assert.IsTrue((new string[] { "A","E","B","C","D","F","G" }).SequenceEqual(list), "MoveToIndex()");

            string[] list2 = list.ToArray(); //ForEach is builtin to List<> but not in array[]. 
            list2.ForEach(m => m.ToLower());
            Assert.IsTrue((new string[] { "a", "e", "b", "c", "d", "f", "g" }).SequenceEqual(list2), "ForEach()");
            list.ForEachReverse(m => m.ToUpper());
            Assert.IsTrue((new string[] { "A", "E", "B", "C", "D", "F", "G" }).SequenceEqual(list2), "ForEachReverse()");
        }

        [Test]
        public void TestMathEx()
        {
            var s = (-650).ToCapacityString(2);
            Assert.AreEqual(s, "-650 B", "Byte formatting failed.");
            s = 4000.ToCapacityString(2);
            Assert.AreEqual(s, "3.91 KB", "KiloByte formatting failed.");
            s = (1024 * 1024 + ((1024 * 1024) / 2)).ToCapacityString(2);
            Assert.AreEqual(s, "1.5 MB", "MegaByte formatting failed.");
            s = (1024 * 1024 * 1024).ToCapacityString(2);
            Assert.AreEqual(s, "1 GB", "GigaByte formatting failed.");
            s = (1024.0m * 1024.0m * 1024.0m * 1024.0m).ToCapacityString(2);
            Assert.AreEqual(s, "1 TB", "TeraByte formatting failed.");
            s = (1024.0m * 1024.0m * 1024.0m * 1024.0m * 1024.0m).ToCapacityString(2);
            Assert.AreEqual(s, "1 PB", "PetaByte formatting failed.");
            s = (1024.0m * 1024.0m * 1024.0m * 1024.0m * 1024.0m * 1024.0m).ToCapacityString(2);
            Assert.AreEqual(s, "1 EB", "ExaByte formatting failed.");

            Assert.AreEqual(3, MathEx.Max(1, 3, 2), "");
            Assert.AreEqual(1, MathEx.Min(2, 1, 3), "");
            Assert.AreEqual(new Version(9, 0, 1, 2), MathEx.Max(new Version(1, 2, 3, 4), new Version(9, 0, 1, 2), new Version(5, 6, 7, 8)), "");
            Assert.AreEqual(new Version(1, 2, 3, 4), MathEx.Min(new Version(5, 6, 7, 8), new Version(1, 2, 3, 4), new Version(9, 0, 1, 2)), "");
        }

        [Test]
        public void TestMemberInfoExtensions()
        {
            var mi = typeof(DataModel).GetMember("MyInt")[0];
            var mi2 = typeof(DataModel).GetMember("PropIgnored2")[0];

            Assert.AreEqual("This is a Unit Test.", mi.Attribute<System.ComponentModel.DescriptionAttribute>(), "Attribute<> Constructor");
            Assert.AreEqual("2", mi.Attribute<System.ComponentModel.DataAnnotations.Schema.ColumnAttribute>(), "Attribute<> Named");
            Assert.AreEqual("", mi2.Attribute<System.Xml.Serialization.XmlIgnoreAttribute>(), "Attribute<> Empty");
            Assert.IsNull(mi.Attribute<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>(), "Attribute<> Missing");

            Assert.IsFalse(mi.AttributeExists<System.ComponentModel.DataAnnotations.Schema.NotMappedAttribute>(), "AttributeExists<> Missing");
            Assert.IsTrue(mi2.AttributeExists<System.Xml.Serialization.XmlIgnoreAttribute>(), "AttributeExists<> Exists");
        }

        [Test]
        public void TestStringExtensions()
        {
            string source;
            string result;

            source = @"
                Strip one or more whitspace chars (including newlines)
                and replace with a single space char.    ";

            Assert.AreEqual("", "".Squeeze());
            Assert.AreEqual("", ((string)null).Squeeze());
            result = source.Squeeze();
            Assert.AreEqual("Strip one or more whitspace chars (including newlines) and replace with a single space char.", result, "Squeeze(whitespace)");

            Assert.AreEqual("", "".Squeeze(new char[] { '(', ')', '\r', '\n' }, ' '));
            Assert.AreEqual("", ((string)null).Squeeze(new char[] { '(', ')', '\r', '\n' }, ' '));
            result = source.Squeeze(new char[] { '(', ')', '\r', '\n' }, ' ');
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
            Assert.AreEqual("ABC", "/{ABC}/".TrimEx('/','{','}'), "TrimEx(trimchars)");

            source = null;
            Assert.IsTrue(source.ContainsI(null), "null.ContainsI(null)");
            Assert.IsFalse(source.ContainsI("def"), "null.Contains(value,true)");
            Assert.IsFalse("abc".ContainsI(null), "null.Contains(value,true)");
            Assert.IsTrue("".ContainsI(""), "null.Contains(value,true)");
            Assert.IsFalse("Abc".ContainsI(""), "null.Contains(value,true)");
            source = "ABCDEFGHI";
            Assert.IsFalse(source.Contains("def", false), "value.Contains(value,false)");
            Assert.IsTrue(source.Contains("def", true), "value.Contains(value,true)");

            source = null;
            Assert.IsTrue(source.EqualsI(null), "null.EqualsI(null)");
            Assert.IsFalse(source.EqualsI("def"), "null.EqualsI(value)");
            source = "ABCDEFGHI";
            Assert.IsFalse(source.EqualsI(null), "value.EqualsI(null)");
            Assert.IsFalse(source.EqualsI("def"), "value.EqualsI(value)");
            Assert.IsTrue(source.EqualsI("AbcDefGhi"), "value.EqualsI(value)");

            Assert.AreEqual(null, ((string)null).ReplaceI(null, null), "");
            Assert.AreEqual("", "".ReplaceI("avc", "def"), "");
            source = "ABCDEFGHI";
            Assert.AreEqual(source, source.ReplaceI(null, "A"), "");
            Assert.AreEqual(source, source.ReplaceI("", "A"), "");
            Assert.AreEqual(source, source.ReplaceI("def", null), "");
            Assert.AreEqual("ABCjklGHI", source.ReplaceI("def","jkl"), "");

            Assert.IsNull(((string)null).Remove(), "null.Remove()");
            Assert.AreEqual("ABCDEF", "    ABC\r\n  DEF  ".Remove(), "value.Remove()");
            Assert.AreEqual("ABC\r\nDEF", "    ABC\r\n  DEF  ".Remove(new char[] { ' ', 'b' }), "value.Remove(' ','b')");

            Assert.AreEqual("__", ((string)null).ToIdentifier(), "null.ToIdentifier()");
            Assert.AreEqual("__", "".ToIdentifier(), "\"\".ToIdentifier()");
            Assert.AreEqual("HelloWorld", " hello-\r\nworld! ".ToIdentifier(), "value.ToIdentifier()");
            Assert.AreEqual("_3HelloWoRld", " 3 hello-\r\nwoRld! ".ToIdentifier(), "value.ToIdentifier()");

            Assert.IsNull(((string)null).ToString(", "), "null.ToString(\", \")");
            Assert.AreEqual("H|e|l|l|o|W|o|r|l|d", "HelloWorld".ToString("|"), "value.ToString(\"|\")");

            var dict = new Dictionary<int, Version>()
            {
                {1, new Version(1,2,3,4) },
                {2, new Version(5,6,7,8) },
                {3, new Version(9,0,1,2) }
            };
            result = dict.ToString(null, kv => $"{kv.Key}={kv.Value}");
            Assert.AreEqual("1=1.2.3.4, 2=5.6.7.8, 3=9.0.1.2", result, "value.ToString(\", \")");
            
            Assert.IsNull(((byte[])null).ToStringEx(), "((byte[])null).ToStringEx()");
            Assert.AreEqual("", (new byte[0]).ToStringEx(), "((new byte[0]).ToStringEx()");
            Assert.AreEqual("H", (new byte[] { 0x48 }).ToStringEx(), "((new byte['H']).ToStringEx()");

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
            Assert.AreEqual(null, ((Stream)null).ToStringEx(), "");

            source = @"<?xml version=""1.0"" encoding=""utf-8""?>";
            Assert.IsFalse(((string)null).IsXml(), "null.IsXml()");
            Assert.IsFalse("<?xm".IsXml(), "\"<?xm\".IsXml()");
            Assert.IsFalse("Hello World".IsXml(), "\"Hello World\".IsXml()");
            Assert.IsTrue(source.IsXml(), "(xml header).IsXml()");
            source = Encoding.UTF8.GetString(Encoding.UTF8.GetPreamble().Concat(source.ToBytes()).ToArray());
            Assert.IsTrue(source.IsXml(), "(xml header w/preamble).IsXml()");

            Assert.IsFalse(((string)null).IsFileName(), "null.IsFileName()");
            Assert.IsFalse("".IsFileName(), "\"\".IsFileName()");
            Assert.IsTrue(@"C:\Program Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe".IsFileName(), "(fullfilepath).IsFileName()");
            Assert.IsTrue(@"\\CLOUD\MyBackups\Backup.bak".IsFileName(), "(UNC filepath).IsFileName()");
            Assert.IsFalse(@"C:\Program | Files (x86)\Microsoft Visual Studio\2019\Enterprise\Common7\IDE\devenv.exe".IsFileName(), "(full/filepath).IsFileName()");
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

            Assert.IsFalse(((string)null).IsHex());
            Assert.IsFalse("A".IsHex());
            Assert.IsFalse("AbC".IsHex());
            Assert.IsTrue("AbCD".IsHex());
            Assert.IsFalse("AByzCD".IsHex());
            Assert.IsTrue("1B CD".IsHex());

            Assert.IsFalse(((string)null).IsNumeric());
            Assert.IsFalse("A".IsNumeric());
            Assert.IsFalse("10.0".IsNumeric());
            Assert.IsTrue("12345".IsNumeric());
            Assert.IsFalse("-12345".IsNumeric());

            Assert.IsFalse(((string)null).IsCSV(), "null.IsCSV()");
            Assert.IsFalse("".IsCSV(), "\"\".IsCSV()");
            Assert.IsFalse("aa,bb,cc".IsCSV(), "\"aa, bb, cc\".IsCSV()");
            Assert.IsTrue(TestData.Csv.IsCSV(), "(csvData).IsCSV()");

            Assert.AreEqual(Guid.Empty, "".ToHash(), "\"Hello World\".ToHash()");
            Assert.AreEqual(new Guid("b18d0ab1-e064-4175-05b7-a99be72e3fe5"), "Hello World".ToBytes().ToHash(), "\"Hello World\".ToHash()");
            Assert.AreEqual(new Guid("b18d0ab1-e064-4175-05b7-a99be72e3fe5"), "Hello World".ToHash(), "\"Hello World\".ToHash()");
            Assert.AreEqual(Guid.Empty, ((Stream)null).ToHash(), "null.ToHash()");
            Assert.AreEqual(new Guid("b18d0ab1-e064-4175-05b7-a99be72e3fe5"), "Hello World".ToHash(), "\"Hello World\".ToHash()");

            Assert.AreEqual("", ((byte[])null).ToHex());
            Assert.AreEqual("", (new byte[0]).ToHex());

            Assert.IsNull(((string)null).FromHex());
            Assert.AreEqual(0, "A".FromHex().Length);

            Assert.AreEqual(null, ((string)null).ToBytes());
            Assert.AreEqual(new byte[0], "".ToBytes());
            Assert.AreEqual(null, ((string)null).ToStream());


            Assert.AreEqual(TestData.Json, TestData.Json.ToBytes().ToHex().FromHex().ToStringEx(), "ToHex().FromHex()");
            Assert.AreEqual(TestData.Json, TestData.Json.ToBytes().ToHex(40).FromHex().ToStringEx(), "ToHex(40).FromHex()");
        }

        [Test]
        public void TestTypeExtensions()
        {
            var s = typeof(Panel).GetManifestResourceStream("CheckBox.bmp");
            Assert.IsNotNull(s, "CheckBox.bmp does not exist.");
            s = typeof(Panel).GetManifestResourceStream("CheckBox.jpg");
            Assert.IsNull(s, "CheckBox.jpg exists.");
        }
    }

    /// <summary>
    /// Added to this unit test assembly via VersionInfo.cs and referenced in TestAssemblyExtensions().
    /// </summary>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public sealed class UnitTestAttribute : Attribute
    {
        public UnitTestAttribute()
        {
        }
    }
}
