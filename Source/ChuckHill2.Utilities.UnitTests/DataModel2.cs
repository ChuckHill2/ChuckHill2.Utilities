using System;
using System.Collections.Generic;

namespace ChuckHill2.Utilities.UnitTests
{
    [Serializable]
    public class DataModel2 : IEquatable<DataModel2>, IEqualityComparer<DataModel2>
    {
        public bool? MyBool { get; set; }
        public char MyChar { get; set; }
        public DateTime MyDate { get; set; }
        public DateTime MyDateTime { get; set; }
        public DateTime MyTime { get; set; }
        public DateTimeOffset? MyDateTimeOffset { get; set; }
        public decimal MyDecimal { get; set; }
        public double MyDouble { get; set; }
        public Guid MyGuid { get; set; }
        public int MyInt { get; set; }
        public string MyString { get; set; }
        public TimeSpan MyTimeSpan { get; set; }
        public Version MyVersion { get; set; }

        public DataModel2() { } // for deserialization

        public DataModel2(DataModel dm)
        {
            MyBool = dm.MyBool;
            MyChar = dm.MyChar;
            MyDate = dm.MyDate;
            MyDateTime = dm.MyDateTime;
            MyTime = dm.MyTime;
            MyDateTimeOffset = dm.MyDateTimeOffset;
            MyDecimal = dm.MyDecimal;
            MyDouble = dm.MyDouble;
            MyGuid = dm.MyGuid;
            MyInt = dm.MyInt;
            MyString = dm.MyString;
            MyTimeSpan = dm.MyTimeSpan;
            MyVersion = dm.MyVersion;
        }

        public bool Equals(DataModel2 o) => this.Equals(this, o);
        public override bool Equals(object obj) => this.Equals((DataModel2)obj);
        public override int GetHashCode() => this.GetHashCode(this);
        public int GetHashCode(DataModel2 obj) => throw new NotImplementedException();
        public override string ToString() => $"{MyInt}, {MyDouble}, {MyDecimal}, {MyChar}, {(MyBool.HasValue ? MyBool.Value.ToString() : "(null)")}";

        public bool Equals(DataModel2 x, DataModel2 y)
        {
            if (x == null && y == null) return true;
            if (x == null || y == null) return false;

            var v00 = x.MyInt == y.MyInt;
            var v01 = Math.Round(x.MyDouble, 9).Equals(Math.Round(y.MyDouble, 9));
            var v02 = x.MyDecimal == y.MyDecimal;
            var v03 = x.MyChar == y.MyChar;
            var v04 = x.MyString == y.MyString;
            var v05 = x.MyDateTime == y.MyDateTime;
            var v06 = x.MyDateTimeOffset?.Ticks == y.MyDateTimeOffset?.Ticks;
            var v07 = x.MyDate == y.MyDate;
            var v08 = x.MyTime == y.MyTime;
            var v09 = x.MyGuid == y.MyGuid;
            var v10 = x.MyTimeSpan == y.MyTimeSpan;
            var v11 = x.MyVersion == y.MyVersion;

            bool vx = v00 && v01 && v02 && v03 && v04 && v05 && v06 && v07 && v08 && v09 && v10 && v11;
            return vx;
        }
    }
}
