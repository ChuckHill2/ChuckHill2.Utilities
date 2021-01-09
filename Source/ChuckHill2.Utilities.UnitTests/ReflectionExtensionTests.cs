using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using ChuckHill2.Utilities.Extensions.Reflection;
using NUnit.Framework;

namespace ChuckHill2.Utilities.UnitTests
{
    [TestFixture]
    public class ReflectionExtensionsTests
    {
        [SetUp] public void Setup() { }

        [Test]
        public void TestObjectExtensions()
        {
            var data = DataModel.GenerateData(1).ToArray()[0];
            Type t = Type.GetType("System.Windows.Forms.Layout.TableLayout+ContainerInfo, " + typeof(TableLayoutPanel).Assembly.FullName, false, false);

            Assert.IsNull(((DataModel)null).DeepClone());
            Assert.AreEqual(data, data.DeepClone(), "DeepClone()");

            Assert.IsNull(((DataModel)null).ShallowClone());
            Assert.AreEqual(data, data.ShallowClone(), "ShallowClone()");

            Assert.IsTrue(3.IsImplementationOf(typeof(IFormattable)));
            Assert.IsTrue(typeof(int).IsImplementationOf(typeof(IFormattable)));
            Assert.IsTrue(3.IsImplementationOf(typeof(int)));
            Assert.IsTrue((new List<int>()).IsImplementationOf(typeof(List<>)));

            object intvalue = 3;
            Assert.AreEqual(3, intvalue.GetReflectedValue("m_value"));
            Assert.AreEqual(null, intvalue.GetReflectedValue("m_dummy"));
            intvalue.SetReflectedValue("m_value", 4);
            Assert.AreEqual(4, intvalue);

            var intarray = new int[] { 0,1,2,3,4,5 };
            Assert.AreEqual(6, intarray.GetReflectedValue("Length"));
            Assert.IsFalse(intarray.SetReflectedValue("Length",3));

            Assert.AreEqual(3, intarray.GetReflectedValue("Item", 3));

            Assert.IsNull(intarray.GetReflectedValue("Length", 3));
            Assert.IsNull(intarray.GetReflectedValue("Item", "3"));
            Assert.IsNull(intarray.GetReflectedValue("Item", 3, 4));
            Assert.IsTrue(intarray.SetReflectedValue("Item", 9, 3));
            Assert.AreEqual(9, intarray[3]);
            Assert.IsFalse(intarray.SetReflectedValue("Item", "X"));
            Assert.IsFalse(intarray.SetReflectedValue("Dummy", "X"));

            Assert.AreEqual("System.Windows.Forms.Layout.TableLayout+ContainerInfo", ReflectionExtensions.GetReflectedType("System.Windows.Forms.Layout.TableLayout+ContainerInfo", typeof(TableLayoutPanel)).FullName, "GetReflectedType()");
            Assert.AreEqual("System.Windows.Forms.Layout.TableLayout+ContainerInfo", ReflectionExtensions.GetReflectedType("System.Windows.Forms.Layout.TableLayout+ContainerInfo").FullName, "GetReflectedType()");
            Assert.AreEqual("System.Int32", ReflectionExtensions.GetReflectedType("System.Int32").FullName, "GetReflectedType()");

            object value = new Exception("This is a Test");
            Assert.AreEqual("This is a Test", value.GetReflectedValue("_message"), "GetReflectedValue()");
            value.SetReflectedValue("_message", "This is another Test");
            Assert.AreEqual("System.Exception: This is another Test", value.ToString(), "SetReflectedValue()");

            Assert.IsNotNull(typeof(DataModel).InvokeReflectedMethod(null) as DataModel, "InvokeReflectedMethod(constructor)");
            Assert.AreEqual(data.MyDouble, data.InvokeReflectedMethod("get_MyDouble"), "InvokeReflectedMethod(method)");

            var tt = typeof(IEquatable<DataModel>);
            Assert.IsTrue(data.MemberIs(tt.FullName), "value.MemberIs(typestring)");
            Assert.IsTrue(data.MemberIs(tt), "value.MemberIs(type)");
            Assert.IsTrue(typeof(DataModel).MemberIs(tt.FullName), "type.MemberIs(typestring)");
            Assert.IsTrue(typeof(DataModel).MemberIs(tt), "type.MemberIs(type)");
        }

    }
}
