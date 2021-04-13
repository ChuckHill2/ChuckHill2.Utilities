//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="NewTests.cs" company="Chuck Hill">
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
