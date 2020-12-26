using System;
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
