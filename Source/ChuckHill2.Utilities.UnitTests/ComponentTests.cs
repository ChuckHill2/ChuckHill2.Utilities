using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;
using ChuckHill2.Forms;
using ChuckHill2;

namespace ChuckHill2.UnitTests
{
    [TestFixture]
    public class ComponentTests
    {
        [SetUp] public void Setup() { }

        [Test]
        public void TestImageAttribute()
        {
            Image a;

            var absolutePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"Resources\ImageAttributeTest1.bmp");
            a = new ImageAttribute(absolutePath).Image;
            Assert.IsNotNull(a, "ImageAttribute (file): BMP Absolute File Path");

            a = new ImageAttribute(@"Resources\ImageAttributeTest5.ico").Image;
            Assert.IsNotNull(a, "ImageAttribute (file): ICO Relative File Path: Copy Local");

            a = new ImageAttribute(@"Resources\ImageAttributeTest2.tif").Image;
            Assert.IsNotNull(a, "ImageAttribute (file): TIF Relative File Path: Copy Local");

            a = new ImageAttribute(@"Resources\ImageAttributeTest3.jpg").Image;
            Assert.IsNotNull(a, "ImageAttribute (file): JPG Relative File Path: Copy Local");

            a = new ImageAttribute(@"Resources\ImageAttributeTest4.png").Image;
            Assert.IsNotNull(a, "ImageAttribute (file): PNG Relative File Path: Copy Local");

            a = new ImageAttribute(typeof(TestForm)).Image;
            Assert.IsNotNull(a, "ImageAttribute (manifest): typeof(FormMain) => FormMain.png");

            //Manually adding anything to a Form resource will will be erased by the designer.... However we can access designer-generated resources...
            a = new ImageAttribute(typeof(TestForm), "$this.BackgroundImage").Image;
            Assert.IsNotNull(a, "ImageAttribute (Form resource): typeof(FormMain) => $this.BackgroundImage");

            a = new ImageAttribute(typeof(TestForm), "$this.Icon").Image;
            Assert.IsNotNull(a, "ImageAttribute (Form resource): typeof(FormMain) => $this.Icon");
            
            a = new ImageAttribute(typeof(Panel)).Image;
            Assert.IsNotNull(a, "ImageAttribute (manifest): typeof(Panel) => Panel.bmp");

            a = new ImageAttribute(typeof(Panel), "CheckBox").Image;
            Assert.IsNotNull(a, "ImageAttribute (manifest): typeof(Panel),CheckBox => CheckBox.bmp");

            a = new ImageAttribute(typeof(Panel), "CheckBox.jpg").Image;
            Assert.IsNotNull(a, "ImageAttribute (manifest): typeof(Panel),CheckBox.jpg => CheckBox.bmp");

            a = new ImageAttribute(typeof(Panel), "checkbox").Image;
            Assert.IsNull(a, "ImageAttribute (manifest): typeof(Panel),checkbox => not case-sensitive");

            a = new ImageAttribute(this.GetType(), "ImageAttributeTest5").Image;
            Assert.IsNotNull(a, "ImageAttribute (manifest): typeof(this),ImageAttributeTest5 => ImageAttributeTest5.ico");
            Assert.IsTrue(a.Width == 32 && a.Height == 32, "ImageAttribute (manifest): typeof(this),ImageAttributeTest5 => ImageAttributeTest5.ico not 32x32");
        }

        [Test]
        public void TestFontMetrics()
        {
            using (Font f = new Font("Segoe UI", 9.0f, FontStyle.Regular))
            {
                var fm1 = new NetFontMetrics(f);
                Assert.IsTrue(
                    fm1.EmHeightPixels == 9.0f &&
                    fm1.AscentPixels == 9.711914f &&
                    fm1.DescentPixels == 2.258789f &&
                    fm1.CellHeightPixels == 11.9707031f &&
                    fm1.InternalLeadingPixels == 2.97070313f &&
                    fm1.LineSpacingPixels == 11.9707031f &&
                    fm1.ExternalLeadingPixels == 0f
                    , "NetFontMetrics values are incorrect.");

                var fm2 = new Win32FontMetrics(f);
                Assert.IsTrue(
                    fm2.EmHeightPixels == 15 &&
                    fm2.AscentPixels == 12 &&
                    fm2.DescentPixels == 3 &&
                    fm2.CellHeightPixels == 15 &&
                    fm2.InternalLeadingPixels == 3 &&
                    fm2.LineSpacingPixels == 15 &&
                    fm2.ExternalLeadingPixels == 0
                    , "Win32FontMetrics values are incorrect.");

                var fm3 = new ImageFontMetrics(f);
                Assert.IsTrue(
                    fm3.EmHeightPixels == 12 &&
                    //fm3.AscentPixels == 0 &&
                    //fm3.DescentPixels == 0 &&
                    fm3.CellHeightPixels == 12 &&
                    fm3.InternalLeadingPixels == 4 &&
                    fm3.LineSpacingPixels == 1
                    //fm3.ExternalLeadingPixels == 0
                    , "ImageFontMetrics values are incorrect.");
            }
        }

        [Test]
        public void TestHSLColor()
        {
            //RGB=154,205,50  HSL=79.74°, 0.61, 0.50  HSV=79.74°, 0.76, 0.80
            HSLColor hsl = Color.YellowGreen;
            Assert.IsTrue(
                Math.Round(hsl.Hue, 2) == 79.74 &&
                Math.Round(hsl.Saturation, 2) == 0.61 &&
                Math.Round(hsl.Luminosity, 2) == 0.50,
                "Color converted to HSL is incorrect");

            Color c = hsl;
            Assert.IsTrue(
                c.R == Color.YellowGreen.R &&
                c.G == Color.YellowGreen.G &&
                c.B == Color.YellowGreen.B,
                "HSL converted to Color is incorrect");
        }

        [Test]
        public void TestHSVColor()
        {
            //RGB=154,205,50  HSL=79.74°, 0.61, 0.50  HSV=79.74°, 0.76, 0.80
            HSVColor hsv = Color.YellowGreen;
            Assert.IsTrue(
                Math.Round(hsv.Hue, 2) == 79.74 &&
                Math.Round(hsv.Saturation, 2) == 0.76 &&
                Math.Round(hsv.Value, 2) == 0.80,
                "Color converted to HSV is incorrect");

            Color c = hsv;
            Assert.IsTrue(
                c.R == Color.YellowGreen.R &&
                c.G == Color.YellowGreen.G &&
                c.B == Color.YellowGreen.B,
                "HSV converted to Color is incorrect");

        }

        [Test]
        public void TestColorEx()
        {
            Color c = Color.FromArgb(216, 39, 187);
            Assert.AreEqual("(216,39,187)", c.GetName(), "GetName() failed.");

            Color c1 = c.MakeNamed("AlmostMediumVioletRed");
            Assert.IsTrue(c1.Name == "AlmostMediumVioletRed" && c1.IsNamedColor && !c1.IsKnownColor && !c1.IsSystemColor && c.A == 255 && c.R == 216 && c.B == 187, "MakeNamed() failed.");

            Color c2 = c1.NearestKnownColor();
            Assert.AreEqual("MediumVioletRed", c2.Name, "NearestKnownColor() failed");

            Color c3 = Color.FromArgb(128, 216, 39, 187);
            Assert.AreEqual("(128,216,39,187)", c3.GetName(), "Translucent GetName() failed.");

            Color c4 = c3.MakeNamed("TranslucentAlmostMediumVioletRed");
            Assert.IsTrue(c4.Name == "TranslucentAlmostMediumVioletRed" && c4.IsNamedColor && !c4.IsKnownColor && !c4.IsSystemColor && c4.A == 128 && c4.R == 216 && c4.B == 187, "MakeNamed() failed.");

            Color c5 = Color.FromArgb(128, c3.NearestKnownColor());
            Assert.AreEqual("(128,MediumVioletRed)", c5.GetName(), "Translucent GetName() failed.");
        }
    }
}
