using System;
using System.Collections;
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
            string source = "1=A, 2 ==B=X, 3 = C, , 4=D, 5=E, 6=, 7, ";
            Assert.AreEqual(7, source.ToDictionary().Count, "ToDictionary<string,string>()");
            Assert.AreEqual(7, source.ToDictionary(',', '=', k => int.Parse(k), v => v).Count, "ToDictionary<int,string>()");

            var dictionary = source.ToDictionary(',', '=');
            Assert.AreEqual("C", dictionary.GetValue("3"), "dictionary.GetValue(validvalue)");
            Assert.AreEqual(null, dictionary.GetValue("XXX"), "dictionary.GetValue(invalidvalue)");
        }

        [Test]
        public void TestListExtensions()
        {
            string source = "A,B,C, D  ,E,F, G";
            var list = source.ToEnumerableList().ToList();

            Assert.AreEqual(7, list.Count, "ToList<string>()");

            string source2 = "A,B,C,E,D,F,G";

            Assert.IsTrue(source.ListEquals(source2), "source.ListEquals(source2)");
            Assert.IsFalse(source.ListEquals(source2,false), "source.ListEquals(source2,false)");

            Assert.AreEqual(4, list.IndexOf(m => m == "E"), "list.IndexOf()");
            Assert.AreEqual(4, list.LastIndexOf(m => m == "E"), "list.LastIndexOf()");
        }

        [Test]
        public void TestExceptionExtensions()
        {
            Exception exception = new Exception("Test Exception");

            Assert.AreEqual("Test Exception\r\nSuffix", exception.AppendMessage("Suffix").Message, "AppendMessage()");
            Assert.AreEqual("Prefix\r\nTest Exception\r\nSuffix", exception.PrefixMessage("Prefix").Message, "PrefixMessage()");
            Assert.AreEqual("Replaced Message", exception.ReplaceMessage("Replaced Message").Message, "ReplaceMessage()");
            Assert.IsTrue(exception.WithStackTrace().StackTrace.Length > 10, "WithStackTrace()");
            exception.AppendInnerException(new Exception("InnerException"));
            Assert.AreEqual("Exception: Replaced Message / InnerException", exception.FullMessage(), "FullMessage()");
        }

        [Test]
        public void TestObjectExtensions()
        {
            var data = DataModel.GenerateData(1).ToArray()[0];
            Type t = Type.GetType("System.Windows.Forms.Layout.TableLayout+ContainerInfo, " + typeof(TableLayoutPanel).Assembly.FullName, false, false);

            Assert.AreEqual(data, data.DeepClone(), "DeepClone()");
            Assert.AreEqual(data, data.ShallowClone(), "ShallowClone()");

            Assert.AreEqual("System.Windows.Forms.Layout.TableLayout+ContainerInfo", ObjectExtensions.GetReflectedType("System.Windows.Forms.Layout.TableLayout+ContainerInfo", typeof(TableLayoutPanel)).FullName, "GetReflectedType()");
            Assert.AreEqual("System.Windows.Forms.Layout.TableLayout+ContainerInfo", ObjectExtensions.GetReflectedType("System.Windows.Forms.Layout.TableLayout+ContainerInfo").FullName, "GetReflectedType()");
            Assert.AreEqual("System.Int32", ObjectExtensions.GetReflectedType("System.Int32").FullName, "GetReflectedType()");

            object value = new Exception("This is a Test");
            Assert.AreEqual("This is a Test", value.GetReflectedValue("_message"), "GetReflectedValue()");
            value.SetReflectedValue("_message", "This is another Test");
            Assert.AreEqual("System.Exception: This is another Test", value.ToString(), "SetReflectedValue()");

            Assert.IsNotNull(typeof(DataModel).InvokeReflectedMethod("DataModel") as DataModel, "InvokeReflectedMethod(constructor)");
            Assert.AreEqual(data.MyDouble, data.InvokeReflectedMethod("get_MyDouble"), "InvokeReflectedMethod(method)");

            var tt = typeof(IEquatable<DataModel>);
            Assert.IsTrue(data.MemberIs(tt.FullName), "value.MemberIs(typestring)");
            Assert.IsTrue(data.MemberIs(tt), "value.MemberIs(type)");
            Assert.IsTrue(typeof(DataModel).MemberIs(tt.FullName), "type.MemberIs(typestring)");
            Assert.IsTrue(typeof(DataModel).MemberIs(tt), "type.MemberIs(type)");
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
            [System.ComponentModel.DescriptionAttribute("Ten (10)")] Ten
        }

        [Test]
        public void TestEnumExtensions()
        {
            Assert.AreEqual("Eight (8)", TestEnum.Eight.Description(), "enum.Description()");

            var desc = TestEnum.Four.AllDescriptions();
            Assert.AreEqual(11, desc.Count, "enum.AllDescriptions().Count");
            Assert.AreEqual("Eight (8)", desc[TestEnum.Eight], "enum.AllDescriptions()[value]");
        }

        [Test]
        public void TestAssemblyExtensions()
        {
            var asm = Assembly.GetExecutingAssembly();

            Assert.AreEqual("ChuckHill2.Utilities.UnitTests", asm.Attribute<AssemblyProductAttribute>(), "Attribute<> Constructor");
            Assert.AreEqual("True", asm.Attribute<System.Runtime.CompilerServices.RuntimeCompatibilityAttribute>(), "Attribute<> Named");
            Assert.IsNull(asm.Attribute<System.Runtime.InteropServices.TypeLibVersionAttribute>(), "Attribute<> Missing");

            Assert.IsFalse(asm.AttributeExists<System.Runtime.InteropServices.TypeLibVersionAttribute>(), "AttributeExists<> Missing");
            Assert.IsTrue(asm.AttributeExists<AssemblyProductAttribute>(), "AttributeExists<> Exists");

            var timestamp = asm.PEtimestamp();
            Assert.IsTrue(timestamp > new DateTime(2000, 1, 1) && timestamp <= DateTime.Now, "PEtimestamp()");
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
        public void TestAppDomainExtensions()
        {
            var ad = AppDomain.CurrentDomain;
            var currentName = ad.SetFriendlyName("Unit Test");
            Assert.AreEqual("Unit Test", ad.FriendlyName, "ad.SetFriendlyName(newname)");
            ad.SetFriendlyName(currentName);
        }

        [Test]
        public void TestDateTimeExtensions()
        {
            const int time_t = 1582230020;
            var dt = new DateTime(2020, 2, 20, 20, 20, 20);

            Assert.AreEqual(time_t, dt.ToUnixTime(), "ToUnixTime()");
            Assert.AreEqual(dt, time_t.FromUnixTime(), "FromUnixTime()");
        }
    }
}
