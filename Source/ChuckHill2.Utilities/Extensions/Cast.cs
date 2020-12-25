//--------------------------------------------------------------------------
// <summary>
// Simple import/export of array of classes to Excel or CSV.
// </summary>
// <copyright file="Cast.cs" company="Chuck Hill">
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
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace ChuckHill2.Utilities.Extensions
{
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
        /// type's default value instead. Null if they are nullable types.
        /// </summary>
        /// <typeparam name="T">Type of object to convert to</typeparam>
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
        public static T CastTo<T>(this object value)
        {
            return (T)Cast.To(typeof(T), value);
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

        /// <summary>
        /// Read raw 2-D json stream into enumerable sequence of classes. Multiple calls using the  
        /// same TextReader may be used to retrieve multiple dissimilar sequence of classes.
        /// </summary>
        /// <typeparam name="T">Data model class</typeparam>
        /// <param name="textReader">Open text stream to read</param>
        /// <returns>Enumerable sequence of classes. Not instantiated until evaluated.</returns>
        /// <remarks>
        ///   • Does not support nested data.<br />
        ///   • Class values must be read/writable properties that do not have any attribute with 'Ignore' in the type name.<br />
        /// </remarks>
        public static IEnumerable<T> JsonToModels<T>(TextReader textReader) where T : class, new()
        {
            var properties = GetProperties(typeof(T));

            const int EOF = -1;
            const int BEGINWORD = -2;
            const int ENDWORD = -3;
            bool quoted = false;
            Func<int> readChar = () =>
            {
                int cc;
                bool literal = false;
                while ((cc = textReader.Read()) != -1)
                {
                    if (cc == '\\')
                    {
                        literal = true;
                        continue;
                    }

                    if (literal && quoted)
                    {
                        literal = false;
                        return cc;
                    }

                    literal = false;

                    if (quoted)
                    {
                        if (cc == '"')
                        {
                            quoted = false;
                            return ENDWORD;
                        }

                        return cc;
                    }

                    if (cc != ' ' && cc != '\t' && cc != '\r' && cc != '\n')
                    {
                        if (cc == '"')
                        {
                            quoted = true;
                            return BEGINWORD;
                        }

                        return cc;
                    }
                }

                return cc;
            };

            var sb = new StringBuilder();
            Func<string> quotedWord = () =>
            {
                sb.Length = 0;
                int cw;
                while ((cw = readChar()) != EOF)
                {
                    if (cw == ENDWORD)
                    {
                        var w = sb.ToString();
                        sb.Length = 0;
                        return w;
                    }

                    sb.Append((char)cw);
                }

                return null;
            };

            bool headerRow = true;
            var headers = new List<string>();
            int indent = 0;
            int currentIndent = -1;
            int c;
            T obj = null;
            int propIndex = 0;
            while ((c = readChar()) != EOF)
            {
                if (c == '[')
                {
                    sb.Length = 0;
                    indent++;
                    continue;
                }

                if (c == ']')
                {
                    sb.Length = 0;
                    if (currentIndent - 1 == indent)
                    {
                        readChar(); // read past following comma
                        yield break; // ready to read the next 2-D array
                    }

                    if (currentIndent == indent)
                    {
                        if (headerRow)
                        {
                            headerRow = false;
                            properties = SyncWithHeaders(properties, headers);
                            continue;
                        }

                        if (sb.Length > 0)
                        {
                            var w = sb.ToString();
                            sb.Length = 0;
                            if (w != "null")
                            {
                                properties[propIndex++].SetValue(obj, w);
                            }
                        }

                        indent--;
                        if (obj != null)
                        {
                            yield return obj;
                            propIndex = 0;
                            obj = null;
                        }

                        continue;
                    }
                }

                if (c == BEGINWORD)
                {
                    currentIndent = indent;
                    var w = quotedWord();
                    if (headerRow)
                    {
                        headers.Add(w);
                        continue;
                    }

                    if (obj == null)
                    {
                        obj = new T();
                    }

                    properties[propIndex++].SetValue(obj, w);
                }
                else
                {
                    if (c == ',')
                    {
                        if (sb.Length == 0)
                        {
                            continue;
                        }

                        var w = sb.ToString();
                        sb.Length = 0;
                        if (w == "null")
                        {
                            propIndex++;
                        }
                        else
                        {
                            properties[propIndex++].SetValue(obj, w);
                        }

                        continue;
                    }

                    sb.Append((char)c);
                }
            }

            if (obj != null)
            {
                yield return obj;
            }

            yield break;
        }

        /// <summary>
        /// Transform 2-dimensional string array into enumerable sequence of classes.
        /// </summary>
        /// <typeparam name="T">Class type to transform into. Properties must be read/writable. Properties may be nullable types.</typeparam>
        /// <param name="array">2-Dimensional array to parse</param>
        /// <param name="hasHeader">
        ///   True if first row is a header. Header names must match class property names. The order is not important. Mismatched columns are ignored.
        ///   False if there is no header. Number of class Properties must exactly match number of columns in 2-D array.
        /// </param>
        /// <returns>Enumerable sequence of class T</returns>
        /// <remarks>
        ///   • Does not support nested data classes.<br />
        ///   • Class values must be read/writable properties that do not have any attribute with 'Ignore' in the type name.<br />
        /// </remarks>
        public static IEnumerable<T> ToModels<T>(this string[,] array, bool hasHeader) where T : class, new()
        {
            var properties = GetProperties(typeof(T));

            T obj = null;
            int c = 0;
            IEnumerator enumerator = array.GetEnumerator();

            if (hasHeader)
            {
                var list = new List<string>(properties.Count);
                while (c++ < array.GetLength(1) && enumerator.MoveNext())
                {
                    list.Add((string)enumerator.Current);
                }

                properties = SyncWithHeaders(properties, list);
                c = 0;
            }
            else
            {
                // No header, so we assume a 1-to-1 match of properties with 2-D array columns.
                if (properties.Count != array.GetLength(1))
                {
                    throw new DataMisalignedException($"Number of valid properties ({properties.Count}) in {typeof(T).Name} does not match the number of columns ({array.GetLength(1)}) in 2-D array.");
                }
            }

            // Finally, populate the array of T with data from 2-D array;

            while (enumerator.MoveNext())
            {
                if (c == 0)
                {
                    obj = new T();
                }

                var s = (string)enumerator.Current;
                var p = properties[c];
                if (!string.IsNullOrEmpty(s))
                {
                    p.SetValue(obj, s);
                }

                c = ++c % properties.Count;

                if (c == 0)
                {
                    yield return obj;
                }
            }

            yield break;
        }

        /// <summary>
        /// Transform enumerable sequence of simple data classes into a 2-dimensional string array.
        /// </summary>
        /// <param name="array">Enumerable sequence of data model classes.</param>
        /// <param name="hasHeader">
        ///   True to prefix a header row consisting of the property names.
        ///   False to have no header row.
        /// </param>
        /// <returns>2-D string array.</returns>
        /// <remarks>
        ///   • Does not support nested data classes.<br />
        ///   • Class values must be read/writable properties that do not have any attribute with 'Ignore' in the type name.<br />
        ///   • Columns are in the same order as defined in the data class.
        /// </remarks>
        public static string[,] To2dArray(this IEnumerable array, bool hasHeader)
        {
            var properties = GetProperties(GetElementType(array));

            var list = new List<string[]>();
            string[] row;

            if (hasHeader)
            {
                row = new string[properties.Count];
                for (int i = 0; i < row.Length; i++)
                {
                    row[i] = properties[i].Name;
                }

                list.Add(row);
            }

            foreach (var obj in array)
            {
                row = new string[properties.Count];
                for (int i = 0; i < row.Length; i++)
                {
                    row[i] = properties[i].GetValue(obj).CastTo<string>();
                }

                list.Add(row);
            }

            var result = new string[list.Count, properties.Count];
            for (int r = 0; r < list.Count; r++)
            {
                row = list[r];
                for (int c = 0; c < properties.Count; c++)
                {
                    result[r, c] = row[c];
                }
            }

            return result;
        }

        /// <summary>
        /// Insert null records into a sequence where the specified value changes.
        /// Data should already be sorted by the specified key.
        /// Used for adding a delimiter between changes.
        /// </summary>
        /// <typeparam name="TSource">Data/Model class</typeparam>
        /// <typeparam name="TKey">Property value</typeparam>
        /// <param name="list">Enumerable sequence</param>
        /// <param name="keySelector">Property value to compare</param>
        /// <returns>New enumerable sequence</returns>
        public static IEnumerable<TSource> SplitBy<TSource, TKey>(this IEnumerable<TSource> list, Func<TSource, TKey> keySelector) where TSource : class
        {
            var enumerator = list.GetEnumerator();

            bool firstRow = true;
            TKey prevKey = default(TKey);

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == null)
                {
                    // Just in case someone called SplitBy twice.
                    yield return enumerator.Current;
                    continue;
                }

                if (firstRow)
                {
                    prevKey = keySelector(enumerator.Current);
                    firstRow = false;
                    yield return enumerator.Current;
                    continue;
                }

                TKey value = keySelector(enumerator.Current);
                if (!value.Equals(prevKey))
                {
                    prevKey = value;
                    yield return null;
                }

                yield return enumerator.Current;
            }

            enumerator.Dispose();
            yield break;
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

        /// <summary>
        /// Get list of valid properties in the specified class type.
        /// </summary>
        /// <param name="t">Class type to retrieve properties from</param>
        /// <returns>Array of zero or move valid properties in class</returns>
        private static IList<ModelProperty> GetProperties(Type t)
        {
            // https://stackoverflow.com/questions/9062235/get-properties-in-order-of-declaration-using-reflection/17998371
            var props = t.GetProperties()
                .Where(p => p.CanRead && p.CanWrite &&
                            !p.GetMethod.IsPrivate && !p.SetMethod.IsPrivate &&
                            !p.PropertyType.IsArray &&
                            ((p.PropertyType.IsGenericType && p.PropertyType.GetGenericTypeDefinition() == typeof(System.Nullable<>)) ||
                             !p.PropertyType.IsGenericType) &&
                            !p.CustomAttributes.Any(m => m.AttributeType.Name.Contains("Ignore")))
                .OrderBy(x => x.MetadataToken)
                .Select(p => new ModelProperty
                {
                    Name = p.Name,
                    Type = p.PropertyType.IsGenericType ? p.PropertyType.GenericTypeArguments[0] : p.PropertyType,
                    GetValue = p.GetValue,
                    SetValue = (o, v) => p.SetValue(o, Cast.To(p.PropertyType, v))
                })
                .ToArray();

            if (props.Length == 0)
                throw new ArgumentOutOfRangeException($"Type {t.FullName} has no valid properties.");

            return props;
        }

        private static Type GetElementType(IEnumerable enumerable)
        {
            Type[] interfaces = enumerable.GetType().GetInterfaces();
            Type elementType = (from i in interfaces
                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                select i.GetGenericArguments()[0]).FirstOrDefault();

            // Peek at the first element in the list if we couldn't determine the element type.
            if (elementType == null || elementType == typeof(object))
            {
                throw new InvalidDataException($"Cannot determine underlying type of enumerable object.");
                // First element will be lost if element is returned via 'yield return'.
                // object firstElement = enumerable.Cast<object>().FirstOrDefault();
                // if (firstElement != null) elementType = firstElement.GetType();
            }

            return elementType;
        }

        /// <summary>
        /// Synchronize properties names with header names 
        /// </summary>
        /// <param name="srcProps">Properties to match headers from.</param>
        /// <param name="headers">Header names to synchronize properties to.</param>
        /// <returns>Returns sub/superset of properties to get/set values</returns>
        private static IList<ModelProperty> SyncWithHeaders(IList<ModelProperty> srcProps, IList<string> headers)
        {
            var dstProps = new List<ModelProperty>(srcProps.Count);
            int dummyCount = 0;
            foreach (var h in headers)
            {
                var prop = srcProps.FirstOrDefault(p => p.Name.Equals(h, StringComparison.OrdinalIgnoreCase));
                if (prop == null)
                {
                    prop = new ModelProperty
                    {
                        Name = h,
                        Type = typeof(DBNull),
                        GetValue = (o) => null,
                        SetValue = (o, v) => { }
                    };
                    dummyCount++;
                }

                dstProps.Add(prop);
            }

            if (dstProps.Count == 0 || dstProps.Count == dummyCount)
                throw new ArrayTypeMismatchException("There are no matching headers in property list.");

            return dstProps;
        }

        private class ModelProperty
        {
            public string Name { get; set; }
            public Type Type { get; set; }
            public Func<object, object> GetValue { get; set; }
            public Action<object, object> SetValue { get; set; }

            public override string ToString() => $"{Name}, {Type.Name}";
        }
    }
}
