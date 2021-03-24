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
        public void TestCookie()
        {
            // We assume everyone has used Google.com at some time.
            const string CookieDomain = ".google.com";

            string cookie = null;
            string bdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Mozilla\\Firefox\\Profiles");
            if (Directory.Exists(bdir)) //Firefox browser may not be installed in this user's environment.
            {
                cookie = GetCookie.Helper.Mozilla.GetCookie(CookieDomain);
                Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Firefox Browser {CookieDomain} cookie");
            }

            string browser = "Edge";
            cookie = GetCookie.Helper.Chromium.GetCookie(CookieDomain, ref browser);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Edge Browser {CookieDomain} cookie");

            browser = "Chrome";
            cookie = GetCookie.Helper.Chromium.GetCookie(CookieDomain, ref browser);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Chrome Browser {CookieDomain} cookie");

            cookie = ChuckHill2.Cookie.Get(CookieDomain);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Any Browser {CookieDomain} cookie");

            browser = "Edge";
            cookie = ChuckHill2.Cookie.Get(CookieDomain, ref browser);
            Assert.IsTrue(!string.IsNullOrWhiteSpace(cookie), $"Edge Browser {CookieDomain} cookie");

            cookie = ChuckHill2.Cookie.Get(".thisIsAnUnknownDomain.com");
            Assert.IsEmpty(cookie, $"Any Browser unknown cookie");
        }
    }
}
