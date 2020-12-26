using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ChuckHill2.Utilities.Extensions;
using NUnit.Framework;

namespace ChuckHill2.Utilities.UnitTests
{
    [TestFixture]
    public class CastTests
    {
        private enum Numbers { Zero, One, Two, Three, Four, Five, Six, Seven, Eight, Nine, Ten }

        [SetUp] public void Setup() { }

        [Test]
        public void TestCastTo()
        {
            // Special: anything <==> DBNull
            Assert.AreEqual(null, DBNull.Value.CastTo<string>(), string.Empty);
            Assert.AreEqual(null, "Anything".CastTo<DBNull>(), string.Empty);

            // anything ==> string
            Assert.AreEqual(string.Empty, "   ".CastTo<string>(), string.Empty);
            Assert.AreEqual("Hello World", "   Hello World \n   ".CastTo<string>(), string.Empty);
            Assert.AreEqual("1234", 1234.CastTo<string>(), string.Empty);
            Assert.AreEqual("1234", 1234.00000m.CastTo<string>(), string.Empty);
            Assert.AreEqual("1234", 1234.00000d.CastTo<string>(), string.Empty);
            Assert.AreEqual("1234", 1234.00000f.CastTo<string>(), string.Empty);
            Assert.AreEqual("1234.456", 1234.45600000m.CastTo<string>(), string.Empty);
            Assert.AreEqual("1234.456", 1234.45600000d.CastTo<string>(), string.Empty);
            Assert.AreEqual("1234.456", 1234.45600000f.CastTo<string>(), string.Empty);

            // System.Type <==> string
            Assert.AreEqual("System.DateTime, mscorlib", typeof(DateTime).CastTo<string>(), string.Empty);
            Assert.AreEqual(0, typeof(DateTime).CastTo<int>(), string.Empty);
            Assert.AreEqual(null, typeof(DateTime).CastTo<int?>(), string.Empty);
            Assert.AreEqual(typeof(DateTime), "System.DateTime, mscorlib".CastTo<Type>(), string.Empty);
            Assert.AreEqual(null, "System.FooBar, mscorlib".CastTo<Type>(), string.Empty);

            // Simple integer transform
            Assert.AreEqual(null, 4.CastTo<DBNull>(), string.Empty);
            Assert.AreEqual((byte)4, 4.CastTo<byte>(), string.Empty);
            Assert.AreEqual(0, 1000.CastTo<byte>(), string.Empty);
            Assert.AreEqual(0, (-4).CastTo<uint>(), string.Empty);
            Assert.AreEqual("4", 4.CastTo<string>(), string.Empty);
            Assert.AreEqual(0, ((object)null).CastTo<int>(), string.Empty);
            Assert.AreEqual(null, ((object)null).CastTo<int?>(), string.Empty);

            // System.Enum <==> string/integer
            Assert.AreEqual(Numbers.Four, 4.CastTo<Numbers>(), string.Empty);
            Assert.AreEqual(Numbers.Four, "Four".CastTo<Numbers>(), string.Empty);
            Assert.AreEqual(Numbers.Four, "4".CastTo<Numbers>(), string.Empty);
            Assert.AreEqual(Numbers.Four, " 4 ".CastTo<Numbers>(), string.Empty);
            Assert.AreEqual(Numbers.Zero, "xxx".CastTo<Numbers>(), string.Empty);
            Assert.AreEqual(null, "xxx".CastTo<Numbers?>(), string.Empty);

            // string/int (True/False,1/0,Yes/No) ==> Boolean
            Assert.AreEqual("True", true.CastTo<string>(), string.Empty);
            Assert.AreEqual("False", false.CastTo<string>(), string.Empty);
            Assert.AreEqual(1, true.CastTo<int>(), string.Empty);
            Assert.AreEqual(0, false.CastTo<int>(), string.Empty);
            Assert.AreEqual(true, 1.CastTo<bool>(), string.Empty);
            Assert.AreEqual(false, 0.CastTo<bool>(), string.Empty);
            Assert.AreEqual(true, 1d.CastTo<bool>(), string.Empty);
            Assert.AreEqual(false, 0d.CastTo<bool?>(), string.Empty);
            Assert.AreEqual(false, 4.CastTo<bool>(), string.Empty);
            Assert.AreEqual(null, 4.CastTo<bool?>(), string.Empty);
            Assert.AreEqual(true, "TRUE".CastTo<bool>(), string.Empty);
            Assert.AreEqual(true, "si".CastTo<bool>(), string.Empty);
            Assert.AreEqual(true, "Ja".CastTo<bool?>(), string.Empty);
            Assert.AreEqual(false, "No".CastTo<bool>(), string.Empty);
            Assert.AreEqual(false, "JUNK".CastTo<bool>(), string.Empty);
            Assert.AreEqual(null, "JUNK".CastTo<bool?>(), string.Empty);

            // System.Version <==> string
            Assert.AreEqual("1.2.3.4", new Version(1, 2, 3, 4).CastTo<string>(), string.Empty);
            Assert.AreEqual(new Version(1, 2, 3, 4), "1.2.3.4".CastTo<Version>(), string.Empty);
            Assert.AreEqual(null, "1.2.3.4XXX".CastTo<Version>(), string.Empty);
        }

        [Test]
        public async Task TestCastToDateTimeUnspecified()
        {
            await TestCastToDateTime(DateTimeKind.Unspecified);
        }

        [Test]
        public async Task TestCastToDateTimeLocal()
        {
            await TestCastToDateTime(DateTimeKind.Local);
        }

        [Test]
        public async Task TestCastToDateTimeUTC()
        {
            await TestCastToDateTime(DateTimeKind.Utc);
        }

        [Test]
        public void TestJsonToModels()
        {
            using (TextReader tr = new StringReader(TestData.Json))
            {
                var page1 = Cast.JsonToModels<DataModel>(tr);
                var page2 = Cast.JsonToModels<DataModel2>(tr);

                Assert.IsTrue(TestData.ArrayOfModels[0].OfType<DataModel>().ToArray().SequenceEqual(page1));
                Assert.IsTrue(TestData.ArrayOfModels[1].OfType<DataModel2>().ToArray().SequenceEqual(page2));
            }
        }

        [Test]
        public void TestIndentedJsonToModels()
        {
            using (TextReader tr = new StringReader(TestData.JsonIndented))
            {
                var page1 = Cast.JsonToModels<DataModel>(tr);
                var page2 = Cast.JsonToModels<DataModel2>(tr);

                Assert.IsTrue(TestData.ArrayOfModels[0].OfType<DataModel>().ToArray().SequenceEqual(page1));
                Assert.IsTrue(TestData.ArrayOfModels[1].OfType<DataModel2>().ToArray().SequenceEqual(page2));
            }
        }

        [Test]
        public void TestToModels()
        {
            var page1 = TestData.ArrayOf2DArrays[0].ToModels<DataModel>(true);
            var page2 = TestData.ArrayOf2DArrays[1].ToModels<DataModel2>(true);

            Assert.IsTrue(TestData.ArrayOfModels[0].OfType<DataModel>().ToArray().SequenceEqual(page1));
            Assert.IsTrue(TestData.ArrayOfModels[1].OfType<DataModel2>().ToArray().SequenceEqual(page2));
        }

        [Test]
        public void TestTo2dArray()
        {
            var page1 = TestData.ArrayOfModels[0].To2dArray(true);
            var page2 = TestData.ArrayOfModels[1].To2dArray(true);

            Assert.IsTrue(IsSequenceEqual(TestData.ArrayOf2DArrays[0], page1));
            Assert.IsTrue(IsSequenceEqual(TestData.ArrayOf2DArrays[1], page2));
        }

        [Test]
        public void TestSplitBy()
        {
            var page = TestData.ArrayOfModels[1].OfType<DataModel>().OrderBy(m => m.MyInt).SplitBy(m => m.MyInt);

            int prevInt = int.MinValue;
            int prevRowNull = 0;
            foreach (var row in page)
            {
                if (row == null)
                {
                    prevRowNull++;
                    Assert.IsTrue(prevRowNull <= 1, "Adjacent null records not allowed.");
                    continue;
                }

                Assert.IsTrue(prevRowNull == 0 || prevInt == int.MinValue || prevInt != row.MyInt, "Records with 2 different keys in the same grouping.");
                prevInt = row.MyInt;
                prevRowNull = 0;
            }
        }

        private async Task TestCastToDateTime(DateTimeKind kind)
        {
            await Task.Run(() =>
            {
                Cast.DateTimeKind = kind;  // ThreadStatic so run this test in a separate thread

                DateTime dt = new DateTime(2020, 7, 25, 13, 25, 30, 0, Cast.DateTimeKind);
                DateTimeOffset dto = new DateTimeOffset(dt, Cast.DateTimeKind == DateTimeKind.Local ? TimeZoneInfo.Local.GetUtcOffset(dt) : TimeSpan.Zero);
                TimeSpan ts = new TimeSpan(44037, 13, 25, 30, 0);
                double dtoa = dt.ToOADate();
                string dtsz = "20200725132530";

                // double ==> DateTime/DateTimeOffset/TimeSpan
                Assert.AreEqual(dt, dtoa.CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(dto, dtoa.CastTo<DateTimeOffset>(), string.Empty);
                Assert.AreEqual(ts, dtoa.CastTo<TimeSpan>(), string.Empty);

                // DateTime ==> float/double/decimal
                Assert.AreEqual(dtoa, dt.CastTo<double>(), string.Empty);
                Assert.AreEqual((float)dtoa, dt.CastTo<float>(), string.Empty);
                Assert.AreEqual((decimal)dtoa, dt.CastTo<decimal>(), string.Empty);

                // DateTimeOffset ==> float/double/decimal
                Assert.AreEqual(dtoa, dto.CastTo<double>(), string.Empty);
                Assert.AreEqual((float)dtoa, dto.CastTo<float>(), string.Empty);
                Assert.AreEqual((decimal)dtoa, dto.CastTo<decimal>(), string.Empty);

                // int <==> DateTime
                Assert.AreEqual(1595683530, dt.CastTo<int>(), string.Empty); // seconds from 1/1/1970
                Assert.AreEqual(dt, 1595683530.CastTo<DateTime>(), string.Empty);

                // long <==> DateTime
                Assert.AreEqual(637312803300000000L, dt.CastTo<long>(), string.Empty); // Ticks
                Assert.AreEqual(dt, 637312803300000000L.CastTo<DateTime>(), string.Empty);

                // int <==> DateTimeOffset
                Assert.AreEqual(1595683530, dto.CastTo<int>(), string.Empty); // seconds from 1/1/1970
                Assert.AreEqual(dto, 1595683530.CastTo<DateTimeOffset>(), string.Empty);

                // long <==> DateTimeOffset
                Assert.AreEqual(637312803300000000L, dto.CastTo<long>(), string.Empty); // Ticks
                Assert.AreEqual(dto, 637312803300000000L.CastTo<DateTimeOffset>(), string.Empty);

                // DateTime <==> DateTimeOffset
                Assert.AreEqual(dt, dto.CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(dto, dt.CastTo<DateTimeOffset>(), string.Empty);

                // Numeric string (yyyyMMddhhmmssfff) ==> DateTime/DateTimeOffset
                Assert.AreEqual(dt, dtsz.CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(dto, dtsz.CastTo<DateTimeOffset>(), string.Empty);

                Assert.AreEqual(new DateTime(2020, 7, 25, 13, 25, 30, 555), "202007251325305559999999".CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(new DateTime(2020, 7, 25, 13, 25, 30, 555), "20200725132530555".CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(new DateTime(2020, 7, 25, 13, 25, 30, 550), "2020072513253055".CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(new DateTime(2020, 7, 25, 13, 25, 30, 500), "202007251325305".CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(new DateTime(2020, 7, 25, 13, 25, 30, 50), "2020072513253005".CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(new DateTime(2020, 7, 25, 13, 25, 30, 5), "20200725132530005".CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(new DateTime(2020, 7, 25, 13, 25, 0, 0), "202007251325".CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(new DateTime(2020, 7, 25, 13, 0, 0, 0), "2020072513".CastTo<DateTime>(), string.Empty);
                Assert.AreEqual(new DateTime(2020, 7, 25, 0, 0, 0, 0), "20200725".CastTo<DateTime>(), string.Empty);
            });
        }

        private static bool IsSequenceEqual(string[,] a1, string[,] a2)
        {
            Func<object, object, bool> Equals = (x, y) =>
            {
                if (x == null && y == null) return true;
                if (x == null || y == null) return false;
                return x.Equals(y);
            };

            if (a1.Length != a2.Length) return false;

            var enumerator = a1.GetEnumerator();

            foreach (var s0 in a2)
            {
                enumerator.MoveNext();
                if (!Equals(s0, enumerator.Current)) return false;
            }

            return true;
        }
    }
}
