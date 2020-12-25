using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using ChuckHill2.Utilities.Extensions;

namespace ChuckHill2.Utilities.UnitTests
{
    public static class TestData
    {
        public static readonly IList<IEnumerable> ArrayOfModels = new IEnumerable[]
        {
            DataModel.GenerateData(5),
            DataModel.GenerateData(10).Select(m => new DataModel2(m)).ToArray()
        };

        public static readonly string[][,] ArrayOf2DArrays = new string[][,]
        {
            new string[,] {
                { "MyInt", "MyDouble", "MyDecimal", "MyChar", "MyString", "MyDateTime", "MyDateTimeOffset", "MyDate", "MyTime", "MyGuid", "MyTimeSpan", "MyVersion", "MyEnum", "MyBool" },
                { "8", "-39.8733546444556", "14.2455555332105", "1", "dolore magna aliqua.", "2/14/1967 6:39:25 PM", "2/14/1967 6:39:25 PM +05:00", "2/14/1967 12:00:00 AM", "1/1/1900 6:39:25 PM", "9062594f-b9af-4e43-ae8a-3ee7babebfbd", "2.18:39:25", "18.39.25.0", "Seven", "False" },
                { "6", "38.6469540599021", "5.63023158145614", "6", "velit esse cillum dolore eu", "2/11/1956 7:10:47 PM", "2/11/1956 7:10:47 PM +04:00", "2/11/1956 12:00:00 AM", "1/1/1900 7:10:47 PM", "9062594f-b9af-4e43-ae8a-3ee7babebfbd", "3.19:10:47", "19.10.47.0", "Nine", null },
                { "-6", "-14.6515889394337", "2.62526462442487", "2", "Ut enim ad minim veniam", "9/11/2020 10:40:40 AM", null, "9/11/2020 12:00:00 AM", "1/1/1900 10:40:40 AM", "ce064b69-ca35-4a7d-9299-d339512c90e8", "10:40:40", "10.40.40.0", "Ten", "True" },
                { "7", "29.6144287938319", "-27.9774100882827", "1", "incididunt ut labore et", "3/3/1958 7:32:18 AM", "3/3/1958 7:32:18 AM +14:00", "3/3/1958 12:00:00 AM", "1/1/1900 7:32:18 AM", "9062594f-b9af-4e43-ae8a-3ee7babebfbd", "1.07:32:18", "7.32.18.0", "Eight", null },
                { "6", "35.6464813396551", "27.3703996918026", "1", "Lorem ipsum dolor sit amet", "6/11/1954 9:22:00 AM", null, "6/11/1954 12:00:00 AM", "1/1/1900 9:22:00 AM", "11537a9a-6396-4927-8e8f-34289a8a827e", "3.09:22:00", "9.22.0.0", "Five", "True" }
            },
            new string[,] {
                { "MyBool", "MyChar", "MyDate", "MyDateTime", "MyTime", "MyDateTimeOffset", "MyDecimal", "MyDouble", "MyGuid", "MyInt", "MyString", "MyTimeSpan", "MyVersion" },
                { "False", "1", "2/14/1967 12:00:00 AM", "2/14/1967 6:39:25 PM", "1/1/1900 6:39:25 PM", "2/14/1967 6:39:25 PM +05:00", "14.2455555332105", "-39.8733546444556", "9062594f-b9af-4e43-ae8a-3ee7babebfbd", "8", "dolore magna aliqua.", "2.18:39:25", "18.39.25.0" },
                { null, "6", "2/11/1956 12:00:00 AM", "2/11/1956 7:10:47 PM", "1/1/1900 7:10:47 PM", "2/11/1956 7:10:47 PM +04:00", "5.63023158145614", "38.6469540599021", "9062594f-b9af-4e43-ae8a-3ee7babebfbd", "6", "velit esse cillum dolore eu", "3.19:10:47", "19.10.47.0" },
                { "True", "2", "9/11/2020 12:00:00 AM", "9/11/2020 10:40:40 AM", "1/1/1900 10:40:40 AM", null, "2.62526462442487", "-14.6515889394337", "ce064b69-ca35-4a7d-9299-d339512c90e8", "-6", "Ut enim ad minim veniam", "10:40:40", "10.40.40.0" },
                { null, "1", "3/3/1958 12:00:00 AM", "3/3/1958 7:32:18 AM", "1/1/1900 7:32:18 AM", "3/3/1958 7:32:18 AM +14:00", "-27.9774100882827", "29.6144287938319", "9062594f-b9af-4e43-ae8a-3ee7babebfbd", "7", "incididunt ut labore et", "1.07:32:18", "7.32.18.0" },
                { "True", "1", "6/11/1954 12:00:00 AM", "6/11/1954 9:22:00 AM", "1/1/1900 9:22:00 AM", null, "27.3703996918026", "35.6464813396551", "11537a9a-6396-4927-8e8f-34289a8a827e", "6", "Lorem ipsum dolor sit amet", "3.09:22:00", "9.22.0.0" },
                { "True", "S", "7/9/1976 12:00:00 AM", "7/9/1976 1:10:15 AM", "1/1/1900 1:10:15 AM", "7/9/1976 1:10:15 AM +06:00", "-36.2597547873202", "-32.4454910040114", "9062594f-b9af-4e43-ae8a-3ee7babebfbd", "5", "deserunt mollit anim id est laborum.", "2.01:10:15", "1.10.15.0" },
                { null, "D", "1/4/1951 12:00:00 AM", "1/4/1951 1:17:12 AM", "1/1/1900 1:17:12 AM", null, "32.5206027750487", "-2.61705431743388", "16a101dd-936e-48e7-bfd5-469dca1c57ba", "8", "sed do eiusmod tempor", "3.01:17:12", "1.17.12.0" },
                { null, "2", "9/18/1955 12:00:00 AM", "9/18/1955 5:01:34 AM", "1/1/1900 5:01:34 AM", null, "10.1184034068689", "12.1727496209427", "ce064b69-ca35-4a7d-9299-d339512c90e8", "-6", "Duis aute irure dolor in", "1.05:01:34", "5.1.34.0" },
                { "False", "4", "10/20/1975 12:00:00 AM", "10/20/1975 7:51:52 AM", "1/1/1900 7:51:52 AM", null, "30.0389871373954", "23.9085029223508", "6c2204d6-3b36-486d-a3e9-1639179adc54", "-5", "fugiat nulla pariatur.", "07:51:52", "7.51.52.0" },
                { null, "3", "4/25/2011 12:00:00 AM", "4/25/2011 3:24:26 PM", "1/1/1900 3:24:26 PM", null, "37.9542399141724", "-12.2695034659791", "e95a293f-5cee-4e44-8434-056b2fdf8f64", "-4", "Ut enim ad minim veniam", "1.15:24:26", "15.24.26.0" }
            }
        };

        public static readonly string Csv = @"MyInt,MyDouble,MyDecimal,MyChar,MyString,MyDateTime,MyDateTimeOffset,MyDate,MyTime,MyGuid,MyTimeSpan,MyVersion,MyEnum,MyBool
8,-39.8733546444556,14.2455555332105,1,dolore magna aliqua.,1967-02-14 18:39:25,1967-02-14 18:39:25,1967-02-14,1900-01-01 18:39:25,9062594f-b9af-4e43-ae8a-3ee7babebfbd,(02.18:39:25),18.39.25.0,Seven,False
6,38.6469540599021,5.63023158145614,6,velit esse cillum dolore eu,1956-02-11 19:10:47,1956-02-11 19:10:47,1956-02-11,1900-01-01 19:10:47,9062594f-b9af-4e43-ae8a-3ee7babebfbd,(03.19:10:47),19.10.47.0,Nine,""""
-6,-14.6515889394337,2.62526462442487,2,Ut enim ad minim veniam,2020-09-11 10:40:40,,2020-09-11,1900-01-01 10:40:40,ce064b69-ca35-4a7d-9299-d339512c90e8,(00.10:40:40),10.40.40.0,Ten,True
7,29.6144287938319,-27.9774100882827,1,incididunt ut labore et,1958-03-03 07:32:18,1958-03-03 07:32:18,1958-03-03,1900-01-01 07:32:18,9062594f-b9af-4e43-ae8a-3ee7babebfbd,(01.07:32:18),7.32.18.0,Eight,""""
6,35.6464813396551,27.3703996918026,1,Lorem ipsum dolor sit amet,1954-06-11 09:22,,1954-06-11,1900-01-01 09:22,11537a9a-6396-4927-8e8f-34289a8a827e,(03.09:22),9.22.0.0,Five,True

MyBool,MyChar,MyDate,MyDateTime,MyTime,MyDateTimeOffset,MyDecimal,MyDouble,MyGuid,MyInt,MyString,MyTimeSpan,MyVersion
False,1,1967-02-14,1967-02-14 18:39:25,1900-01-01 18:39:25,1967-02-14 18:39:25,14.2455555332105,-39.8733546444556,9062594f-b9af-4e43-ae8a-3ee7babebfbd,8,dolore magna aliqua.,(02.18:39:25),18.39.25.0
,6,1956-02-11,1956-02-11 19:10:47,1900-01-01 19:10:47,1956-02-11 19:10:47,5.63023158145614,38.6469540599021,9062594f-b9af-4e43-ae8a-3ee7babebfbd,6,velit esse cillum dolore eu,(03.19:10:47),19.10.47.0
True,2,2020-09-11,2020-09-11 10:40:40,1900-01-01 10:40:40,,2.62526462442487,-14.6515889394337,ce064b69-ca35-4a7d-9299-d339512c90e8,-6,Ut enim ad minim veniam,(00.10:40:40),10.40.40.0
,1,1958-03-03,1958-03-03 07:32:18,1900-01-01 07:32:18,1958-03-03 07:32:18,-27.9774100882827,29.6144287938319,9062594f-b9af-4e43-ae8a-3ee7babebfbd,7,incididunt ut labore et,(01.07:32:18),7.32.18.0
True,1,1954-06-11,1954-06-11 09:22,1900-01-01 09:22,,27.3703996918026,35.6464813396551,11537a9a-6396-4927-8e8f-34289a8a827e,6,Lorem ipsum dolor sit amet,(03.09:22),9.22.0.0
True,S,1976-07-09,1976-07-09 01:10:15,1900-01-01 01:10:15,1976-07-09 01:10:15,-36.2597547873202,-32.4454910040114,9062594f-b9af-4e43-ae8a-3ee7babebfbd,5,deserunt mollit anim id est laborum.,(02.01:10:15),1.10.15.0
,D,1951-01-04,1951-01-04 01:17:12,1900-01-01 01:17:12,,32.5206027750487,-2.61705431743388,16a101dd-936e-48e7-bfd5-469dca1c57ba,8,sed do eiusmod tempor,(03.01:17:12),1.17.12.0
,2,1955-09-18,1955-09-18 05:01:34,1900-01-01 05:01:34,,10.1184034068689,12.1727496209427,ce064b69-ca35-4a7d-9299-d339512c90e8,-6,Duis aute irure dolor in,(01.05:01:34),5.1.34.0
False,4,1975-10-20,1975-10-20 07:51:52,1900-01-01 07:51:52,,30.0389871373954,23.9085029223508,6c2204d6-3b36-486d-a3e9-1639179adc54,-5,fugiat nulla pariatur.,(00.07:51:52),7.51.52.0
,3,2011-04-25,2011-04-25 15:24:26,1900-01-01 15:24:26,,37.9542399141724,-12.2695034659791,e95a293f-5cee-4e44-8434-056b2fdf8f64,-4,Ut enim ad minim veniam,(01.15:24:26),15.24.26.0
";

        public static readonly string Json = @"[[[""MyInt"",""MyDouble"",""MyDecimal"",""MyChar"",""MyString"",""MyDateTime"",""MyDateTimeOffset"",""MyDate"",""MyTime"",""MyGuid"",""MyTimeSpan"",""MyVersion"",""MyEnum"",""MyBool""],[""8"",""-39.8733546444556"",""14.2455555332105"",""1"",""dolore magna aliqua."",""2/14/1967 6:39:25 PM"",""2/14/1967 6:39:25 PM +05:00"",""2/14/1967 12:00:00 AM"",""1/1/1900 6:39:25 PM"",""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",""2.18:39:25"",""18.39.25.0"",""Seven"",""False""],[""6"",""38.6469540599021"",""5.63023158145614"",""6"",""velit esse cillum dolore eu"",""2/11/1956 7:10:47 PM"",""2/11/1956 7:10:47 PM +04:00"",""2/11/1956 12:00:00 AM"",""1/1/1900 7:10:47 PM"",""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",""3.19:10:47"",""19.10.47.0"",""Nine"",null],[""-6"",""-14.6515889394337"",""2.62526462442487"",""2"",""Ut enim ad minim veniam"",""9/11/2020 10:40:40 AM"",null,""9/11/2020 12:00:00 AM"",""1/1/1900 10:40:40 AM"",""ce064b69-ca35-4a7d-9299-d339512c90e8"",""10:40:40"",""10.40.40.0"",""Ten"",""True""],[""7"",""29.6144287938319"",""-27.9774100882827"",""1"",""incididunt ut labore et"",""3/3/1958 7:32:18 AM"",""3/3/1958 7:32:18 AM +14:00"",""3/3/1958 12:00:00 AM"",""1/1/1900 7:32:18 AM"",""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",""1.07:32:18"",""7.32.18.0"",""Eight"",null],[""6"",""35.6464813396551"",""27.3703996918026"",""1"",""Lorem ipsum dolor sit amet"",""6/11/1954 9:22:00 AM"",null,""6/11/1954 12:00:00 AM"",""1/1/1900 9:22:00 AM"",""11537a9a-6396-4927-8e8f-34289a8a827e"",""3.09:22:00"",""9.22.0.0"",""Five"",""True""]],[[""MyBool"",""MyChar"",""MyDate"",""MyDateTime"",""MyTime"",""MyDateTimeOffset"",""MyDecimal"",""MyDouble"",""MyGuid"",""MyInt"",""MyString"",""MyTimeSpan"",""MyVersion""],[""False"",""1"",""2/14/1967 12:00:00 AM"",""2/14/1967 6:39:25 PM"",""1/1/1900 6:39:25 PM"",""2/14/1967 6:39:25 PM +05:00"",""14.2455555332105"",""-39.8733546444556"",""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",""8"",""dolore magna aliqua."",""2.18:39:25"",""18.39.25.0""],[null,""6"",""2/11/1956 12:00:00 AM"",""2/11/1956 7:10:47 PM"",""1/1/1900 7:10:47 PM"",""2/11/1956 7:10:47 PM +04:00"",""5.63023158145614"",""38.6469540599021"",""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",""6"",""velit esse cillum dolore eu"",""3.19:10:47"",""19.10.47.0""],[""True"",""2"",""9/11/2020 12:00:00 AM"",""9/11/2020 10:40:40 AM"",""1/1/1900 10:40:40 AM"",null,""2.62526462442487"",""-14.6515889394337"",""ce064b69-ca35-4a7d-9299-d339512c90e8"",""-6"",""Ut enim ad minim veniam"",""10:40:40"",""10.40.40.0""],[null,""1"",""3/3/1958 12:00:00 AM"",""3/3/1958 7:32:18 AM"",""1/1/1900 7:32:18 AM"",""3/3/1958 7:32:18 AM +14:00"",""-27.9774100882827"",""29.6144287938319"",""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",""7"",""incididunt ut labore et"",""1.07:32:18"",""7.32.18.0""],[""True"",""1"",""6/11/1954 12:00:00 AM"",""6/11/1954 9:22:00 AM"",""1/1/1900 9:22:00 AM"",null,""27.3703996918026"",""35.6464813396551"",""11537a9a-6396-4927-8e8f-34289a8a827e"",""6"",""Lorem ipsum dolor sit amet"",""3.09:22:00"",""9.22.0.0""],[""True"",""S"",""7/9/1976 12:00:00 AM"",""7/9/1976 1:10:15 AM"",""1/1/1900 1:10:15 AM"",""7/9/1976 1:10:15 AM +06:00"",""-36.2597547873202"",""-32.4454910040114"",""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",""5"",""deserunt mollit anim id est laborum."",""2.01:10:15"",""1.10.15.0""],[null,""D"",""1/4/1951 12:00:00 AM"",""1/4/1951 1:17:12 AM"",""1/1/1900 1:17:12 AM"",null,""32.5206027750487"",""-2.61705431743388"",""16a101dd-936e-48e7-bfd5-469dca1c57ba"",""8"",""sed do eiusmod tempor"",""3.01:17:12"",""1.17.12.0""],[null,""2"",""9/18/1955 12:00:00 AM"",""9/18/1955 5:01:34 AM"",""1/1/1900 5:01:34 AM"",null,""10.1184034068689"",""12.1727496209427"",""ce064b69-ca35-4a7d-9299-d339512c90e8"",""-6"",""Duis aute irure dolor in"",""1.05:01:34"",""5.1.34.0""],[""False"",""4"",""10/20/1975 12:00:00 AM"",""10/20/1975 7:51:52 AM"",""1/1/1900 7:51:52 AM"",null,""30.0389871373954"",""23.9085029223508"",""6c2204d6-3b36-486d-a3e9-1639179adc54"",""-5"",""fugiat nulla pariatur."",""07:51:52"",""7.51.52.0""],[null,""3"",""4/25/2011 12:00:00 AM"",""4/25/2011 3:24:26 PM"",""1/1/1900 3:24:26 PM"",null,""37.9542399141724"",""-12.2695034659791"",""e95a293f-5cee-4e44-8434-056b2fdf8f64"",""-4"",""Ut enim ad minim veniam"",""1.15:24:26"",""15.24.26.0""]]]";

        public static readonly string JsonIndented = @"[
  [
    [
      ""MyInt"",
      ""MyDouble"",
      ""MyDecimal"",
      ""MyChar"",
      ""MyString"",
      ""MyDateTime"",
      ""MyDateTimeOffset"",
      ""MyDate"",
      ""MyTime"",
      ""MyGuid"",
      ""MyTimeSpan"",
      ""MyVersion"",
      ""MyEnum"",
      ""MyBool""
    ],
    [
      ""8"",
      ""-39.8733546444556"",
      ""14.2455555332105"",
      ""1"",
      ""dolore magna aliqua."",
      ""2/14/1967 6:39:25 PM"",
      ""2/14/1967 6:39:25 PM +05:00"",
      ""2/14/1967 12:00:00 AM"",
      ""1/1/1900 6:39:25 PM"",
      ""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",
      ""2.18:39:25"",
      ""18.39.25.0"",
      ""Seven"",
      ""False""
    ],
    [
      ""6"",
      ""38.6469540599021"",
      ""5.63023158145614"",
      ""6"",
      ""velit esse cillum dolore eu"",
      ""2/11/1956 7:10:47 PM"",
      ""2/11/1956 7:10:47 PM +04:00"",
      ""2/11/1956 12:00:00 AM"",
      ""1/1/1900 7:10:47 PM"",
      ""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",
      ""3.19:10:47"",
      ""19.10.47.0"",
      ""Nine"",
      null
    ],
    [
      ""-6"",
      ""-14.6515889394337"",
      ""2.62526462442487"",
      ""2"",
      ""Ut enim ad minim veniam"",
      ""9/11/2020 10:40:40 AM"",
      null,
      ""9/11/2020 12:00:00 AM"",
      ""1/1/1900 10:40:40 AM"",
      ""ce064b69-ca35-4a7d-9299-d339512c90e8"",
      ""10:40:40"",
      ""10.40.40.0"",
      ""Ten"",
      ""True""
    ],
    [
      ""7"",
      ""29.6144287938319"",
      ""-27.9774100882827"",
      ""1"",
      ""incididunt ut labore et"",
      ""3/3/1958 7:32:18 AM"",
      ""3/3/1958 7:32:18 AM +14:00"",
      ""3/3/1958 12:00:00 AM"",
      ""1/1/1900 7:32:18 AM"",
      ""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",
      ""1.07:32:18"",
      ""7.32.18.0"",
      ""Eight"",
      null
    ],
    [
      ""6"",
      ""35.6464813396551"",
      ""27.3703996918026"",
      ""1"",
      ""Lorem ipsum dolor sit amet"",
      ""6/11/1954 9:22:00 AM"",
      null,
      ""6/11/1954 12:00:00 AM"",
      ""1/1/1900 9:22:00 AM"",
      ""11537a9a-6396-4927-8e8f-34289a8a827e"",
      ""3.09:22:00"",
      ""9.22.0.0"",
      ""Five"",
      ""True""
    ]
  ],
  [
    [
      ""MyBool"",
      ""MyChar"",
      ""MyDate"",
      ""MyDateTime"",
      ""MyTime"",
      ""MyDateTimeOffset"",
      ""MyDecimal"",
      ""MyDouble"",
      ""MyGuid"",
      ""MyInt"",
      ""MyString"",
      ""MyTimeSpan"",
      ""MyVersion""
    ],
    [
      ""False"",
      ""1"",
      ""2/14/1967 12:00:00 AM"",
      ""2/14/1967 6:39:25 PM"",
      ""1/1/1900 6:39:25 PM"",
      ""2/14/1967 6:39:25 PM +05:00"",
      ""14.2455555332105"",
      ""-39.8733546444556"",
      ""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",
      ""8"",
      ""dolore magna aliqua."",
      ""2.18:39:25"",
      ""18.39.25.0""
    ],
    [
      null,
      ""6"",
      ""2/11/1956 12:00:00 AM"",
      ""2/11/1956 7:10:47 PM"",
      ""1/1/1900 7:10:47 PM"",
      ""2/11/1956 7:10:47 PM +04:00"",
      ""5.63023158145614"",
      ""38.6469540599021"",
      ""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",
      ""6"",
      ""velit esse cillum dolore eu"",
      ""3.19:10:47"",
      ""19.10.47.0""
    ],
    [
      ""True"",
      ""2"",
      ""9/11/2020 12:00:00 AM"",
      ""9/11/2020 10:40:40 AM"",
      ""1/1/1900 10:40:40 AM"",
      null,
      ""2.62526462442487"",
      ""-14.6515889394337"",
      ""ce064b69-ca35-4a7d-9299-d339512c90e8"",
      ""-6"",
      ""Ut enim ad minim veniam"",
      ""10:40:40"",
      ""10.40.40.0""
    ],
    [
      null,
      ""1"",
      ""3/3/1958 12:00:00 AM"",
      ""3/3/1958 7:32:18 AM"",
      ""1/1/1900 7:32:18 AM"",
      ""3/3/1958 7:32:18 AM +14:00"",
      ""-27.9774100882827"",
      ""29.6144287938319"",
      ""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",
      ""7"",
      ""incididunt ut labore et"",
      ""1.07:32:18"",
      ""7.32.18.0""
    ],
    [
      ""True"",
      ""1"",
      ""6/11/1954 12:00:00 AM"",
      ""6/11/1954 9:22:00 AM"",
      ""1/1/1900 9:22:00 AM"",
      null,
      ""27.3703996918026"",
      ""35.6464813396551"",
      ""11537a9a-6396-4927-8e8f-34289a8a827e"",
      ""6"",
      ""Lorem ipsum dolor sit amet"",
      ""3.09:22:00"",
      ""9.22.0.0""
    ],
    [
      ""True"",
      ""S"",
      ""7/9/1976 12:00:00 AM"",
      ""7/9/1976 1:10:15 AM"",
      ""1/1/1900 1:10:15 AM"",
      ""7/9/1976 1:10:15 AM +06:00"",
      ""-36.2597547873202"",
      ""-32.4454910040114"",
      ""9062594f-b9af-4e43-ae8a-3ee7babebfbd"",
      ""5"",
      ""deserunt mollit anim id est laborum."",
      ""2.01:10:15"",
      ""1.10.15.0""
    ],
    [
      null,
      ""D"",
      ""1/4/1951 12:00:00 AM"",
      ""1/4/1951 1:17:12 AM"",
      ""1/1/1900 1:17:12 AM"",
      null,
      ""32.5206027750487"",
      ""-2.61705431743388"",
      ""16a101dd-936e-48e7-bfd5-469dca1c57ba"",
      ""8"",
      ""sed do eiusmod tempor"",
      ""3.01:17:12"",
      ""1.17.12.0""
    ],
    [
      null,
      ""2"",
      ""9/18/1955 12:00:00 AM"",
      ""9/18/1955 5:01:34 AM"",
      ""1/1/1900 5:01:34 AM"",
      null,
      ""10.1184034068689"",
      ""12.1727496209427"",
      ""ce064b69-ca35-4a7d-9299-d339512c90e8"",
      ""-6"",
      ""Duis aute irure dolor in"",
      ""1.05:01:34"",
      ""5.1.34.0""
    ],
    [
      ""False"",
      ""4"",
      ""10/20/1975 12:00:00 AM"",
      ""10/20/1975 7:51:52 AM"",
      ""1/1/1900 7:51:52 AM"",
      null,
      ""30.0389871373954"",
      ""23.9085029223508"",
      ""6c2204d6-3b36-486d-a3e9-1639179adc54"",
      ""-5"",
      ""fugiat nulla pariatur."",
      ""07:51:52"",
      ""7.51.52.0""
    ],
    [
      null,
      ""3"",
      ""4/25/2011 12:00:00 AM"",
      ""4/25/2011 3:24:26 PM"",
      ""1/1/1900 3:24:26 PM"",
      null,
      ""37.9542399141724"",
      ""-12.2695034659791"",
      ""e95a293f-5cee-4e44-8434-056b2fdf8f64"",
      ""-4"",
      ""Ut enim ad minim veniam"",
      ""1.15:24:26"",
      ""15.24.26.0""
    ]
  ]
]";

        public static readonly string[,] AssignmentData = new string[,]
        {
            { "UserId", "UserName", "UserType", "Item 1", "Item 2", "Item 3", "Item 4", "Item 5", "Item 6", "Item 7", "Item 8", "Item 9", "Item 10", "Item 11", "Item 12", "Item 13", "Item 14", "Item 15", "Item 16", "Item 17", "Item 18", "Item 19", "Item 20", "Item 21", "Item 22" },
            { "", "", "", "84745A5F-7B66-481D-B5C3-DEA12EB17868", "D0623698-3BEB-4F52-8B03-B59D5E797CFC", "C38C5ADF-A190-4721-A164-8E1A24376CFC", "80690954-4B4E-4DA9-9473-DECF90B26D58", "88A2D199-3E91-4016-ACFB-3A8E70CD5431", "A3AC64F5-99AB-46C6-96C3-128A6C1E02E5", "7A11B17C-66C2-4314-98D9-71BD6FF54394", "29EE4D7E-F8B2-48B0-A788-0433DB4718B0", "7F1D7638-98CD-44E6-8B4A-84439A6D6169", "9B40E426-601B-4D36-B1B7-AB5D7E4F6401", "DB47BD60-3296-4F13-ADB4-76635A6AFDAC", "6ECCF125-D3AD-4286-8B6B-949D9397EF63", "FCA67C0B-F86D-48D3-A57C-A1E6955914D2", "52BF8619-231C-42DF-8E1F-0BC00A4615C2", "18DD5006-96BA-4D78-B2ED-66D5A486E8B2", "2970BC7C-1012-49DF-B660-F752D486AA70", "DDE583F2-24F8-4865-918D-988618E8848F", "5700B6D9-F059-4472-9FC0-871110BE425E", "78CFF57E-F62F-4B9D-9A51-A0842E852D1C", "0D67F559-8147-40E0-84C3-73E01B1EC206", "D9DDBC7D-A6B0-40CD-83A5-74464760F2D5", "E933D1E6-2035-4C6C-9DEB-02DB98E8BDCC" },
            { "281", "281", "NONE", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "XKUHOZPX", "AHMAD ELDRIGE", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "UCWMWYHG", "ALEJANDRA MCKNIGHT", "UserTypeEngineer", null, null, null, null, null, null, null, null, "X", null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "KBJAT", "ALESHIA RIPLEY", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "RMMGOUNU", "ALLA ORIO", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "BAVMVC", "ANDREAS SCARANO", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "ZYGCMSV", "ANETTE GODSMAN", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "UCWWAVGL", "ANGLA MARN", "UserTypeSupport", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, "X", null, null, null },
            { "AN", "Annie Nanda", "UserTypeMarketing", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "FNGQP", "ANTWAN SEVERY", "UserTypeMarketing2", null, null, null, null, null, null, null, null, null, "X", null, null, null, null, null, null, null, null, "X", null, null, null },
            { "UZWWOEXQQ", "ATHENA LITSEY", "UserTypeManagerB", null, null, null, "X", "X", "X", "X", null, null, null, null, null, null, null, null, null, "X", null, "X", null, null, null },
            { "JXFMXWE", "BEULA HERMEZ", "UserTypeEngineer", null, null, null, null, null, null, null, null, null, null, "X", "X", null, null, null, null, null, null, null, null, null, null },
            { "AGLWJEGH", "BIBI ALVEREST", "UserTypeManagerB", null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null, "X", "X", "X", null, "X" },
            { "ZKIMONN", "BREE MONTALBO", "UserTypeManagerA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "AVNTJLUF", "BRITT FOULCARD", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "DYROQ", "BRODERICK NAVIA", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "YGQJDVYB", "CASSIE SCUNGIO", "UserTypeEngineer", null, null, null, null, null, null, null, null, null, "X", null, null, null, null, null, null, null, null, "X", null, null, null },
            { "EHBVMJ", "CASSY KUREK", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "MYBEJNJX", "CHARITY COONS", "UserTypeTechB", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "PWYHNTVQ", "CHARLEY CULBERSON", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X" },
            { "TRBCBCO", "CHASE FRAGOSA", "UserTypeTechA", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X", null, null, "X" },
            { "AAXTPHHI", "CLARINDA VANNOY", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "IDYKKUAY", "CLEOTILDE OVERBAUGH", "UserTypeTechB", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "NGYNR", "CORDELL FEGARO", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "UIWIPZPB", "CORRINA AARONSON", "UserTypeEngineer", null, null, null, null, null, null, null, null, null, "X", null, null, null, null, null, null, null, null, "X", null, null, null },
            { "KYQHWJVX", "CORRINA DELASBOUR", "UserTypeEngineer", null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null, null, null, "X", null, null, null },
            { "SZKYTJO", "CRISELDA WOODCOCK", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "TDVC", "DANIELLE SCHRIEVER", "UserTypeEngineer", null, null, null, null, null, null, null, null, "X", null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "ZTWCBDJ", "DANUTA MCINNIS", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "FFOHYLOV", "DAWNA SHAKE", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "GCNCLHZ", "DEANGELO DEMARK", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, "X" },
            { "EQDXUCWU", "DEBBY HASAK", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X", null, null, "X" },
            { "TNVBSKL", "DEMETRIA LUNDRIGAN", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X", null, null, null },
            { "LTETTCJP", "DIAN SHORTS", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "DRXTNZ", "DINA DEMICHELIS", "UserTypeManagerA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "QCDZXPOF", "DIONNE TREVOR", "UserTypeEngineer", null, null, null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "FKNATI", "DUNCAN NERY", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "USWYKTUT", "DWANA SELEM", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "FWGUMOO", "EDIE CREESE", "UserTypeManagerB", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "WNXNYSQV", "ELANA SKALSKI", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "SMKUAOT", "ELDA BOLLOM", "UserTypeMarketing", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, "X" },
            { "HFRMAGL", "ELIN HARM", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "AQOVVPBX", "EMILY FICKAS", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X", null, "X" },
            { "YCHMRNDC", "ERNESTINA WATRS", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "PHPSIGAN", "ESSIE LIPHAM", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "VQKVLWIY", "ESTER HOELTER", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "IJLUEWIC", "ETHAN CRYSLER", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X", null, "X" },
            { "GWSYDWJU", "FANNIE AYMAR", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "POKMCAT", "FRANCINE DERER", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "PRWFVUL", "GEORGIE LOLAR", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "TUCYMHWD", "GIOVANNI VERMILLION", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "IHGFMKKJ", "GISELLE PERLA", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X" },
            { "HXWBCTT", "GRACIE SHUKLA", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "FATXOBP", "HARRIS SOLINSKI", "UserTypeManagerA", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null, "X" },
            { "XEYNIGX", "HEIKE SEMPEK", "UserTypeManagerB", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "RZWWZBVJ", "HERSCHEL KEEVER", "UserTypeSupport", null, null, null, "X", "X", "X", "X", null, null, null, null, null, null, null, null, null, "X", null, "X", null, null, null },
            { "LKUSBZEDD", "HILMA MARSALIS", "UserTypeTechA", null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X", null, "X", null, "X" },
            { "XBTQIVCF", "HYMAN OBSTFELD", "UserTypeMarketing2", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null },
            { "UIKXHPV", "ILDA LUDLAM", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "BKPZIWIN", "IMOGENE BLAZINA", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "YAOFZOLO", "ISIAH GREMMINGER", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "QOCKVQUEUZL", "JAMAAL GREENSTEIN", "UserTypeTechB", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "AIZITZOG", "JAMEY RIDDERHOFF", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "HSRETLKM", "JEN ALICE", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "LGRSEVQ", "JENI IBASITAS", "UserTypeTechA", null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null, null, null, null, "X" },
            { "NBKWLEG", "JERAMY EHLE", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "UZQPDBCX", "JERILYN SAILER", "UserTypeManagerB", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "OAOFMLQA", "JERMAINE BENSE", "UserTypeTechA", "X", null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X" },
            { "RZJSVTK", "JI GAGLIO", "UserTypeManagerA", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X", null, null, "X" },
            { "QHWQLGNU", "JO KOSSEN", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "PZVNEQQ", "JOLIE HEINECKE", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "AOABTXVD", "JOLYN MARKEVICH", "UserTypeManagerA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X", null, "X" },
            { "GGTXRISD", "JOYCELYN SEATS", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "KTZWHLPW", "JULIEANN WHISTLEHUNT", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "CDVTYOR", "KACEY VONGPHAKDY", "UserTypeManagerB", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "NAFNHUZP", "KACY FRACIER", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "DPTULHCQ", "KANDRA PHARIS", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "GYNEOEJP", "KAROLE TANON", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "XKSHKKS", "KATHERINE BELTRAME", "UserTypeManagerB", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "QPEGTKKY", "LALA COHN", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "JFZAUNHR", "LARONDA SCHAK", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "UGOLERM", "LAVONDA TOBE", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "YSOAPCJL", "LEANDRA MOCCO", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "YHCVFSR", "LEON REZEK", "UserTypeAnesthesiaTech", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null },
            { "QUWIPB", "LILA SOCHAN", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "CWTHF", "LISANDRA AJOKU", "UserTypeTechA", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null, null, null, "X" },
            { "RHYFBOEY", "LUBA GORRI", "UserTypeManagerB", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "SMMUEK", "MABEL WENIG", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "DQEJDS", "MADALENE ROBERTI", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "VDELJTLN", "MARCI SODARO", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "OUAYGOOK", "MARGY BATT", "UserTypeTechB", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "UGOXGSZA", "MARIANA CASTANEDA", "UserTypeManagerB", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "ZFZNXZM", "MARLIN RAATZ", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "CHBFQ", "MEAGHAN DETERLINE", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "SQALLU", "MI JESKIE", "UserTypeNurseManager", null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null, null, null, "X", null, null, null },
            { "KRRDWCGX", "MILES CAMPANY", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X" },
            { "KOTHUEH", "MIRIAN SOSEBEE", "UserTypeEngineer", null, null, null, null, null, null, null, null, null, null, "X", "X", null, null, null, null, null, null, null, null, null, null },
            { "ZTWMWVGC", "MYLES GAFFNEY", "UserTypeEngineer", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "YAEC", "NADINE MULKEY", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "XQRIPSCP", "NELIA BATTENHOUSE", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X", null, null, null, null },
            { "BVAKW", "NIDA BEEBE", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", "X", "X", null, "X" },
            { "LKOXP", "ONIE DOMINA", "UserTypeEngineer", null, null, null, null, null, null, null, null, null, null, "X", "X", null, null, null, null, null, null, null, null, null, null },
            { "PAM1", "Pam one", "UserTypeSupport", null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "ULIHUQT", "PERRY EDE", "UserTypeEngineer", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "ELVOGLRE", "PHOEBE MOEVAO", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "QQ", "qq", "NONE", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "PJLTTC", "RAEANN PEAGLER", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "GZZMBDGD", "REAGAN ARCIZO", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "MVZYPRPC", "REID BENDY", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "REVATHI", "revathi s", "NONE", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "UYXMSIJQ", "RILEY ZINDEL", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, "X", "X", null, null, "X", null, "X", null, "X" },
            { "PMMHKZBE", "RIMA GIAMICHAEL", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "MSPSJGUV", "SALVADOR THORNES", "UserTypeSupport", null, null, "X", "X", "X", "X", "X", null, null, null, null, null, null, null, null, null, "X", "X", "X", null, null, "X" },
            { "SBCSXDB", "SAM ACHTERHOF", "UserTypeSupport", null, null, "X", "X", "X", "X", "X", null, null, null, null, null, null, null, null, null, "X", null, "X", null, null, "X" },
            { "BQTDOJWK", "SETSUKO TOWLER", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "FZLWDJ", "SHENNA CITARELLA", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X" },
            { "FRVEOKI", "SHIRLEEN MASON", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, "X", null },
            { "OEXAZYBW", "SHON DECOMO", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "BENQCDR", "SHONA HAUER", "UserTypeMarketing2", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null },
            { "QBMOQLQ", "SOFIA MASLYN", "UserTypeManagerA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, null },
            { "VYPXJN", "SYLVIA BOSIO", "UserTypeEngineer", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "T1", "T1", "NONE", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "T10", "T10", "NONE", "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null },
            { "STUTEZ", "TRACY WYLAND", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X" },
            { "HEODCKB", "TRUDI FALLEN", "UserTypeSupport", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X" },
            { "JWWZZTOI", "VALERIE VAROS", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X" },
            { "XQKNDSIY", "WAYNE STILLIONS", "UserTypeTechA", null, null, "X", null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, "X", null, null, "X" },
            { "YXGTXKMP", "WINFRED DEFALCO", "UserTypeEngineer", null, null, null, null, null, null, null, "X", null, null, null, null, null, null, null, null, null, null, "X", null, null, null }
        };
    }
}
