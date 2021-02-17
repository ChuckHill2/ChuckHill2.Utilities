using System;
using System.Globalization;
using System.Linq;
using ChuckHill2.Extensions;

namespace ChuckHill2
{
    /// <summary>
    /// Robust data type conversion. Never throws an exception. System.Convert on steroids.
    /// </summary>
    public static class Cast
    {
        private static readonly DateTime Epoch = new DateTime(1970, 1, 1); // Unix Epoch
        private static readonly string[] Bools = new string[]
        {
            "true", "false",
            "1", "0",
            "t", "f",
            "yes", "no",          // "" = Invariant Language (Invariant Country)
            "\x662F", "\x5426",   // zh-Hant = Chinese (Traditional); "是","否"
            "ja", "nee",          // nl = Dutch
            "oui", "non",         // fr = French
            "ja", "nein",         // de = German
            "sì", "no",           // it = Italian
            "sí", "no",           // es = Spanish
            "si", "no",
            "\x306F\x3044", "\x3044\x3044\x3048",  // ja = Japanese; "はい","いいえ"
            "\xC608", "\xC544\xB2C8\xC694",        // ko = Korean; "예","아니요"
            "sim", "não",         // pt = Portuguese
            "ja", "nej",          // sv = Swedish
            "evet", "hay\x0131r", // tr = Turkish; "evet","hayır"
            "ÿèš", "ñò"           // sq = Mock Albanian
        };

        /// <summary>
        /// Determine how to convert DateTimes to/from DatetimeOffset. The life of this value is only within the context of this current thread.<br />
        ///  • DateTimeKind.Unspecified == treat all DateTimes as local ignoring its Kind flag. DateTimeOffset offset-parts are stripped.<br />
        ///  • DateTimeKind.Local == all DateTimes are converted to Local as determined by its Kind flag. DateTimeOffsets are converted to local TZ values.<br />
        ///  • DateTimeKind.Utc == all DateTimes are converted to UTC as determined by its Kind flag. All DateTimeOffsets are converted to UTC values, TZ==0.
        /// </summary>
        [ThreadStatic] public static DateTimeKind DateTimeKind;

        /// <summary>
        /// Robust data conversion. Never throws an exception. Returns the 
        /// specified default value instead.
        /// </summary>
        /// <param name="dstType">Type of object to convert to</param>
        /// <param name="value">Object to convert</param>
        /// <param name="defalt">The default value to use if the conversion fails.</param>
        /// <returns>Converted result</returns>
        /// <remarks>
        /// Handles everything System.Convert.ChangeType() does plus:<br />
        ///  • Anything ==&gt; trimmed string<br />
        ///  • string/number (True/False, t/f, 1/0, 1.0/0.0, Yes/No (in many languages)) ==&gt; Boolean<br />
        ///  • Office Automation Double/float/decimal &lt;==&gt; DateTime/DateTimeOffset<br />
        ///  • int(seconds since 1/1/1970) &lt;==&gt; DateTime/DateTimeOffset/Timespan<br />
        ///  • long(ticks) &lt;==&gt; DateTime/DateTimeOffset/Timespan<br />
        ///  • Numeric string (yyyyMMddhhmmssfff) ==&gt; DateTime/DateTimeOffset<br />
        ///  • System.Type &lt;==&gt; string<br />
        ///  • System.Guid &lt;==&gt; string<br />
        ///  • System.Version &lt;==&gt; string<br />
        ///  • [Flags] System.Enum &lt;==&gt; string/integer
        /// </remarks>
        public static object To(Type dstType, object value, object defalt)
        {
            var v = To(dstType, value);
            object d = dstType.IsClass ? null : Activator.CreateInstance(dstType);
            if (MyEquals(d,v) && dstType.IsAssignableFrom(defalt?.GetType()))
            {
                return defalt;
            }

            return v;
        }

        /// <summary>
        /// Robust data conversion. Never throws an exception. Returns the 
        /// type's default value instead. Null if they are nullable types. 
        /// </summary>
        /// <param name="dstType">Type of object to convert to</param>
        /// <param name="value">Object to convert</param>
        /// <returns>Converted result</returns>
        /// <remarks>
        /// Handles everything System.Convert.ChangeType() does plus:<br />
        ///  • Anything ==&gt; trimmed string<br />
        ///  • string/number (True/False, t/f, 1/0, 1.0/0.0, Yes/No (in many languages)) ==&gt; Boolean<br />
        ///  • Office Automation Double/float/decimal &lt;==&gt; DateTime/DateTimeOffset<br />
        ///  • int(seconds since 1/1/1970) &lt;==&gt; DateTime/DateTimeOffset/Timespan<br />
        ///  • long(ticks) &lt;==&gt; DateTime/DateTimeOffset/Timespan<br />
        ///  • Numeric string (yyyyMMddhhmmssfff) ==&gt; DateTime/DateTimeOffset<br />
        ///  • System.Type &lt;==&gt; string<br />
        ///  • System.Guid &lt;==&gt; string<br />
        ///  • System.Version &lt;==&gt; string<br />
        ///  • [Flags] System.Enum &lt;==&gt; string/integer
        /// </remarks>
        public static object To(Type dstType, object value)
        {
            var isNullable = false;

            try
            {
                // Handlers for everything that System.Convert.ChangeType() cannot handle (or handles poorly).

                if (value is DBNull) value = null;

                if (dstType == typeof(string))
                {
                    if (value == null) return null;
                    if (value is string) return value.ToString().Trim();
                    if (value is Type)
                    {
                        var n = ((Type)value).AssemblyQualifiedName;
                        return n?.Substring(0, n.IndexOf(',', n.IndexOf(',', 0) + 1));
                    }

                    // Normalize: strip decimal trailing zeros
                    if (value is decimal) return ((decimal)value).ToString("0.#################################");

                    return value.ToString();
                }

                if (dstType.IsGenericType && dstType.GetGenericTypeDefinition() == typeof(System.Nullable<>) && dstType.GenericTypeArguments.Length > 0)
                {
                    dstType = dstType.GenericTypeArguments[0];
                    isNullable = true;
                }

                if (value == null || dstType == typeof(DBNull))
                {
                    return dstType.IsValueType && !isNullable ? Activator.CreateInstance(dstType) : null;
                }

                if (dstType == value.GetType())
                {
                    return value; // no conversion needed.
                }

                if (dstType.IsEnum)
                {
                    if (value.GetType().IsPrimitive)
                    {
                        return Enum.ToObject(dstType, value);
                    }

                    try
                    {
                        var v = value.ToString().Trim();
                        if (v == string.Empty) return isNullable ? null : Enum.ToObject(dstType, 0); // Minimize exceptions. There is no TryParse().
                        return Enum.Parse(dstType, v, true);
                    }
                    catch
                    {
                        if (isNullable) return null;
                        return Enum.ToObject(dstType, 0); // return first enum element
                    }
                }

                if (dstType == typeof(bool))
                {
                    string s = value.ToString().Trim().ToLowerInvariant();
                    var i = Array.FindIndex(Bools, m => m.Equals(s));
                    if (i == -1 && isNullable) return null;
                    return (i & 1) == 0;
                }

                if (dstType == typeof(double) || dstType == typeof(float) || dstType == typeof(decimal))
                {
                    // Warning: .NET beginning of time is 1/1/0001. Office Automation Beginning of time 1/1/1900
                    // Also, OA does not understand DateTimeOffset. So we flatten it into DateTime
                    if (value is DateTime)
                    {
                        var v = ((DateTime)value).ToOADate();
                        if (dstType == typeof(float)) return (float)v;
                        if (dstType == typeof(decimal)) return (decimal)v;
                        return v;
                    }
                    else if (value is DateTimeOffset)
                    {
                        var v = ((DateTimeOffset)value).CastTo<DateTime>().ToOADate();
                        if (dstType == typeof(float)) return (float)v;
                        if (dstType == typeof(decimal)) return (decimal)v;
                        return v;
                    }
                    else if (value is TimeSpan)
                    {
                        var v = new DateTime(1899, 12, 30).Add((TimeSpan)value).ToOADate();
                        if (dstType == typeof(float)) return (float)v;
                        if (dstType == typeof(decimal)) return (decimal)v;
                        return v;
                    }
                }

                if (dstType == typeof(int))
                {
                    if (value is DateTime)
                    {
                        var v = ((DateTime)value - Epoch).TotalSeconds;
                        return (int)v;
                    }

                    if (value is DateTimeOffset)
                    {
                        var v = (((DateTimeOffset)value).CastTo<DateTime>() - Epoch).TotalSeconds;
                        return (int)v;
                    }

                    if (value is TimeSpan)
                    {
                        var v = ((TimeSpan)value).TotalSeconds;
                        return (int)v;
                    }
                }

                if (dstType == typeof(long))
                {
                    if (value is DateTime)
                    {
                        var v = ((DateTime)value).Ticks;
                        return (long)v;
                    }

                    if (value is DateTimeOffset)
                    {
                        var v = ((DateTimeOffset)value).CastTo<DateTime>().Ticks;
                        return (long)v;
                    }

                    if (value is TimeSpan)
                    {
                        var v = ((TimeSpan)value).Ticks;
                        return (long)v;
                    }
                }

                if (dstType == typeof(Guid))
                {
                    if (Guid.TryParse(value.ToString().Trim(), out Guid g))
                    {
                        return g;
                    }
                }

                if (dstType == typeof(TimeSpan))
                {
                    if (value is float || value is double || value is decimal)
                    {
                        return DateTime.FromOADate((double)value) - new DateTime(1899, 12, 30);
                    }
                    else if (value is int)
                    {
                        return new TimeSpan(0, 0, (int)value);
                    }
                    else if (value is long)
                    {
                        return new TimeSpan((long)value);
                    }
                    else
                    {
                        // We include '(' and ')' because the CSV writer puts value in parentheses.
                        // This is done to force the value typed as text when loading into Excel.
                        // Excel does not understand TimeSpan and auto-converts it strangely.
                        var s = value.ToString().Trim(new char[] { ' ', '\t', '\r', '\n', ',', '(', ')' });
                        if (TimeSpan.TryParse(s, out TimeSpan g))
                        {
                            return g;
                        }
                    }
                }

                if (dstType == typeof(DateTime))
                {
                    if (value is float || value is double || value is decimal)
                    {
                        // Warning: .NET beginning of time is 1/1/0001. Office Automation Beginning of time 1/1/1900
                        return DateTime.SpecifyKind(DateTime.FromOADate((double)value), DateTimeKind);
                    }
                    else if (value is int)
                    {
                        return Epoch.AddSeconds((int)value);
                    }
                    else if (value is long)
                    {
                        return DateTime.SpecifyKind(new DateTime().AddTicks((long)value), DateTimeKind);
                    }
                    else if (value is DateTimeOffset)
                    {
                        var dto = (DateTimeOffset)value;
                        switch (DateTimeKind)
                        {
                            case DateTimeKind.Unspecified: return dto.DateTime;
                            case DateTimeKind.Local:       return dto.LocalDateTime;
                            case DateTimeKind.Utc:         return dto.UtcDateTime;
                            default:                       return dto.DateTime;
                        }
                    }
                    else
                    {
                        string s = value.ToString().Trim();
                        DateTime dt;

                        // Try converting from numeric string (yyyyMMddhhmmssfff) format.
                        if (TryNumericStringToDateTime(s, out dt))
                        {
                            return DateTime.SpecifyKind(dt, DateTimeKind);
                        }

                        // Try converting in context of the current culture region.
                        if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out dt))
                        {
                            return DateTime.SpecifyKind(dt, DateTimeKind);
                        }

                        // Try converting in context of en-US culture.
                        if (CultureInfo.CurrentCulture.LCID != 0x0409 && DateTime.TryParse(s, CultureInfo.GetCultureInfo(0x0409), DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out dt))
                        {
                            return DateTime.SpecifyKind(dt, DateTimeKind);
                        }
                    }
                }

                if (dstType == typeof(DateTimeOffset))
                {
                    if (value is float || value is double || value is decimal)
                    {
                        // Warning: .NET beginning of time is 1/1/0001. Office Automation Beginning of time 1/1/1900
                        // Also, OA does not understand DateTimeOffset. So we expand it from DateTime.
                        var dt = DateTime.FromOADate((double)value);
                        return new DateTimeOffset(dt, DateTimeKind == DateTimeKind.Local ? TimeZoneInfo.Local.GetUtcOffset(dt) : TimeSpan.Zero);
                    }
                    else if (value is int)
                    {
                        var dt = Epoch.AddSeconds((int)value);
                        return new DateTimeOffset(dt, DateTimeKind == DateTimeKind.Local ? TimeZoneInfo.Local.GetUtcOffset(dt) : TimeSpan.Zero);
                    }
                    else if (value is long)
                    {
                        var dt = new DateTime().AddTicks((long)value);
                        return new DateTimeOffset(dt, DateTimeKind == DateTimeKind.Local ? TimeZoneInfo.Local.GetUtcOffset(dt) : TimeSpan.Zero);
                    }
                    else if (value is DateTime)
                    {
                        var dt = (DateTime)value;
                        switch (DateTimeKind)
                        {
                            case DateTimeKind.Unspecified: return new DateTimeOffset(dt, TimeSpan.Zero);
                            case DateTimeKind.Local:       return new DateTimeOffset(dt.Kind == DateTimeKind.Unspecified ? dt : dt.ToLocalTime(), TimeZoneInfo.Local.GetUtcOffset(dt));
                            case DateTimeKind.Utc:         return new DateTimeOffset(dt.Kind == DateTimeKind.Unspecified ? dt : dt.ToUniversalTime(), TimeSpan.Zero);
                            default:                       return new DateTimeOffset(dt, TimeSpan.Zero);
                        }
                    }
                    else
                    {
                        string s = value.ToString().Trim();
                        DateTimeOffset dto;

                        // Try converting from numeric string (yyyyMMddhhmmssfff) format.
                        if (TryNumericStringToDateTime(s, out DateTime dt))
                        {
                            return new DateTimeOffset(dt, DateTimeKind == DateTimeKind.Local ? TimeZoneInfo.Local.GetUtcOffset(dt) : TimeSpan.Zero);
                        }

                        // Try converting in context of the current culture region.
                        if (DateTimeOffset.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out dto))
                        {
                            return dto;
                        }

                        // Try converting in context of en-US culture.
                        if (CultureInfo.CurrentCulture.LCID != 0x0409 && DateTimeOffset.TryParse(s, CultureInfo.GetCultureInfo(0x0409), DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out dto))
                        {
                            return dto;
                        }
                    }
                }

                if (dstType == typeof(Version))
                {
                    if (Version.TryParse(value.ToString().Trim(), out Version ver))
                    {
                        return ver;
                    }
                }

                if (dstType == typeof(Type) && value is string)
                {
                    return Type.GetType(value.ToString().Trim(), false);
                }

                return System.Convert.ChangeType(value, dstType);  // try to convert everything else
            }
            catch
            {
                return dstType.IsValueType && !isNullable ? Activator.CreateInstance(dstType) : null;
            }
        }

        private static bool MyEquals(object a, object b)
        {
            if (a == null && b == null) return true;
            if (a == null && b != null) return false;
            if (a != null && b == null) return false;
            return a.Equals(b);
        }

        /// <summary>
        /// Try to convert numeric string into Datetime.
        /// Example: \"20050204224530110\" == \"2005-02-04 22:45:30.110\" with optional time, seconds, milliseconds
        /// </summary>
        /// <param name="s">Numeric string to parse</param>
        /// <param name="dt">Resulting datetime</param>
        /// <returns>True if successfully converted.</returns>
        private static bool TryNumericStringToDateTime(string s, out DateTime dt)
        {
            dt = DateTime.MinValue;
            try
            {
                if (s.Length < 8 || !s.All(c => (c >= '0' && c <= '9')))
                {
                    return false;
                }

                int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;
                year = int.Parse(s.Substring(0, 4));
                month = int.Parse(s.Substring(4, 2));
                day = int.Parse(s.Substring(6, 2));
                if (s.Length >= 10)
                {
                    hour = int.Parse(s.Substring(8, 2));
                }

                if (s.Length >= 12)
                {
                    minute = int.Parse(s.Substring(10, 2));
                }

                if (s.Length >= 14)
                {
                    second = int.Parse(s.Substring(12, 2));
                }

                if (s.Length >= 15)
                {
                    millisecond = int.Parse(s.Substring(14).PadRight(3, '0').Substring(0, 3));
                }

                if (year < 1900 || year > 3000)
                {
                    return false;
                }

                if (month < 1 || month > 12)
                {
                    return false;
                }

                if (day < 1 || day > DateTime.DaysInMonth(year, month))
                {
                    return false;
                }

                dt = new DateTime(year, month, day, hour, minute, second, millisecond);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
