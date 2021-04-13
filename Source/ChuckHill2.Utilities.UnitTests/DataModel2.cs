//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="DataModel2.cs" company="Chuck Hill">
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
using System.Collections.Generic;

namespace ChuckHill2.UnitTests
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
