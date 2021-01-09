using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ChuckHill2.Utilities.Extensions;
using ChuckHill2.Utilities.Extensions.Reflection;
using NUnit.Framework;

namespace ChuckHill2.Utilities.UnitTests
{
    [TestFixture]
    public class NewTests
    {
        [SetUp] public void Setup() { }

        [Test]
        public void TestNew()
        {
            object obj = New<System.Globalization.RegionInfo>.Create();
            Assert.IsNotNull(obj, "");
            Assert.AreEqual(typeof(System.Globalization.RegionInfo), obj.GetType(), "");

            obj = New<string>.Create();
            Assert.IsNotNull(obj, "");
            Assert.AreEqual(typeof(string), obj.GetType(), "");

            obj = New<Guid>.Create();
            Assert.IsNotNull(obj, "");
            Assert.AreEqual(typeof(Guid), obj.GetType(), "");

            obj = New<Exception>.Create();
            Assert.IsNotNull(obj, "");
            Assert.AreEqual(typeof(Exception), obj.GetType(), "");
        }
    }
}
