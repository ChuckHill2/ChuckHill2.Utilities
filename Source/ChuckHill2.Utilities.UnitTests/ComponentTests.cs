using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using NUnit.Framework;

namespace ChuckHill2.Utilities.UnitTests
{
    [TestFixture]
    public class ComponentTests
    {
        [SetUp]
        public void Setup()
        {
        }

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

        }

        [Test]
        public void TestHSLColor()
        {

        }

        [Test]
        public void TestHSVColor()
        {

        }

        [Test]
        public void TestColorEx()
        {

        }
    }
}
