//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="CookieTests.cs" company="Chuck Hill">
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
using System.IO;
using NUnit.Framework;

namespace ChuckHill2.UnitTests
{
    [TestFixture]
    public class CookieTests
    {
        [SetUp] public void Setup() { }

        [Test]
        public void TestFirefoxCookie()
        {
            string bdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
            if (Directory.Exists(bdir)) //Firefox browser may not be installed in this user's environment.
            {
                var CookieDomain = ".google.com";
                var cookie = GetCookie.Helper.Mozilla.GetCookie(CookieDomain);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Firefox Browser {CookieDomain} cookie");
            }
        }

        [Test]
        public void TestEdgeCookie()
        {
            var CookieDomain = ".google.com";
            var browser = "Edge";
            var cookie = GetCookie.Helper.Chromium.GetCookie(CookieDomain, ref browser);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Edge Browser {CookieDomain} cookie");
        }

        [Test]
        public void TestChromeCookie()
        {
            var CookieDomain = ".google.com";
            var browser = "Chrome";
            var cookie = GetCookie.Helper.Chromium.GetCookie(CookieDomain, ref browser);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Chrome Browser {CookieDomain} cookie");
        }

        [Test]
        public void TestAnyCookie()
        {
            var CookieDomain = ".google.com";
            var cookie = ChuckHill2.Cookie.Get(CookieDomain);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Any Browser {CookieDomain} cookie");
        }

        [Test]
        public void TestSuggestedEdgeCookie()
        {
            var CookieDomain = ".google.com";
            var browser = "Edge";
            var cookie = ChuckHill2.Cookie.Get(CookieDomain, ref browser);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Edge Browser {CookieDomain} cookie");
        }

        [Test]
        public void TestUnknownCookie()
        {
            var cookie = ChuckHill2.Cookie.Get(".thisIsAnUnknownDomain.com");
            Assert.IsEmpty(cookie, $"Any Browser unknown cookie");
        }
    }
}
