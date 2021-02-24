using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using LookupDictFrom = System.Collections.Generic.Dictionary<System.Type, System.Collections.Generic.Dictionary<System.Type, System.Func<object, object>>>;
using LookupDictTo = System.Collections.Generic.Dictionary<System.Type, System.Func<object, object>>;

/// <summary>
/// Miscellaneous  Incomplete Work
/// </summary>
namespace ChuckHill2.TBD
{
    /// <summary>
    /// How to handle type conversion to strings
    /// </summary>
    public enum StringProcessing
    {
        /// <summary>
        /// Force all null strings to "".
        /// </summary>
        ToEmpty,
        /// <summary>
        /// No string processing. Leave as-is.
        /// Leading and trailing whitespace is always trimmed off.
        /// </summary>
        None,
        /// <summary>
        /// Force all zero-length strings to null.
        /// (note: leading and trailing whitespace is always trimmed before test)
        /// </summary>
        ToNull
    }

    /// <summary>
    /// Convert value from one type to another.
    /// </summary>
    public static class TypeConverter
    {
        private static readonly object LockObj = new object(); //Used by GetConverter() to support multi-threading.

        /// <summary>
        /// Private cached converter method lookup. Only load upon demand.
        /// </summary>
        private static readonly LookupDictFrom[,] TypeConverters = new LookupDictFrom[3, 2]; //==[stringProcessing, throwOnError];

        /// <summary>
        /// Enable caller to log exceptions upon conversion failure. Only used 
        /// when throwOnError flag is false and a default value is returned.
        /// </summary>
        public static Action<DateTime, Type, Type, object, Exception> Logger = null;

        /// <summary>
        /// One-off Convert value to the destination type.
        /// </summary>
        /// <typeparam name="TIN">Type of input value.</typeparam>
        /// <typeparam name="TOUT">Type of output value</typeparam>
        /// <param name="value">Value to convert.</param>
        /// <param name="stringProcessing">
        ///   Flag to determine how to handle destination strings when TOUT is typeof(string)
        ///   (e.g. force nulls to empty, force empties to null, or do nothing).
        ///   Default is to do nothing.
        /// </param>
        /// <param name="throwOnError">True to throw exception upon conversion error. Default is true.</param>
        /// <returns>Converted value with new type.</returns>
        /// <exception cref="NotSupportedException">Occurs there is no converter to convert from the 'from' type to the 'to' type.</exception>
        public static TOUT Cast<TIN,TOUT>(TIN value, StringProcessing stringProcessing = StringProcessing.None, bool throwOnError = true)
        {
            var tin = typeof(TIN) == typeof(object) && ((object)value) != null ? value.GetType() : typeof(TIN);
            return (TOUT)GetConverter(tin, typeof(TOUT), stringProcessing, throwOnError)(value);
        }

        /// <summary>
        /// Create friendly name from provided type. Intrinsic names are used whereever possible.
        /// </summary>
        /// <param name="t">Type to generate friendly name from.</param>
        /// <returns>Friendly name string</returns>
        public static string GetFriendlyTypeName(Type t)
        {
            if (t == null) return string.Empty;
            string name = t.Name;

            //Convert to intrinsic names
            if ((t.IsPrimitive || t == typeof(string) || t == typeof(decimal) || t == typeof(object)) && t != typeof(IntPtr) && t != typeof(UIntPtr)) name = name.ToLower();
            switch (name)
            {
                case "boolean": name = "bool"; break;
                case "int32": name = "int"; break;
                case "uint32": name = "uint"; break;
                case "int64": name = "long"; break;
                case "uint64": name = "ulong"; break;
                case "int16": name = "short"; break;
                case "uint16": name = "ushort"; break;
            }

            if (t.IsArray) return string.Concat(GetFriendlyTypeName(t.GetElementType()), string.Format("[{0}]", new string(',', t.GetArrayRank() - 1)));

            if (t.IsGenericType)
            {
                if (t.GetGenericTypeDefinition() == typeof(System.Nullable<>)) return string.Concat(GetFriendlyTypeName(t.GenericTypeArguments[0]), "?");

                var sb = new StringBuilder();
                sb.Append(name);
                sb.Length -= 2;
                sb.Append('<');
                if (t.GenericTypeArguments != null)
                {
                    foreach (var typeParam in t.GenericTypeArguments)
                    {
                        sb.Append(GetFriendlyTypeName(typeParam));
                        sb.Append(',');
                    }
                    if (t.GenericTypeArguments.Length > 0) sb.Length -= 1;
                }
                sb.Append('>');
                return sb.ToString();
            }

            return name;
        }

        /// <summary>
        /// Get converter method for converting a value from the source type to the destination type.
        /// For efficiency, all methods are cached for reuse.
        /// This is useful for getting all the converters up-front for converting an array of data classes.
        /// </summary>
        /// <param name="from">Source type to convert value from.</param>
        /// <param name="to">Destination type to convert value to.</param>
        /// <param name="stringProcessing">
        ///   Flag to determine how to handle destination strings when TOUT is typeof(string)
        ///   (e.g. force nulls to empty, force empties to null, or do nothing).
        ///   Default is to do nothing.
        /// </param>
        /// <param name="throwOnError">True to throw exception upon conversion error. Default is true.</param>
        /// <returns></returns>
        public static Func<object, object> GetConverter(Type from, Type to, StringProcessing stringProcessing = StringProcessing.None, bool throwOnError = true)
        {
            int stringProcessingIndex = 0;
            switch (stringProcessing)
            {
                case StringProcessing.None: stringProcessingIndex = 0; break;
                case StringProcessing.ToEmpty: stringProcessingIndex = 1; break;
                case StringProcessing.ToNull: stringProcessingIndex = 2; break;
            }
            int throwOnErrorIndex = throwOnError ? 1 : 0;

            lock(LockObj)
            {
                LookupDictFrom fromDict = TypeConverters[stringProcessingIndex, throwOnErrorIndex];
                if (fromDict == null)
                {
                    fromDict = new LookupDictFrom();
                    TypeConverters[stringProcessingIndex, throwOnErrorIndex] = fromDict;
                }

                LookupDictTo toDict = null;
                if (!fromDict.TryGetValue(from, out toDict))
                {
                    toDict = new LookupDictTo();
                    fromDict.Add(from, toDict);
                }

                Func<object, object> converter = null;
                Exception savedEx = null;
                if (!toDict.TryGetValue(to, out converter))
                {
                    try
                    {
                        converter = InternalGetConverter(from, to, stringProcessing, throwOnError);
                    }
                    catch(Exception ex)
                    {
                        savedEx = ex;
                    }
                    toDict.Add(to, converter);
                }

                if (converter == null)
                {
                  throw savedEx != null ? savedEx : new NotSupportedException(string.Format("Cannot convert from {0} to {1}.", GetFriendlyTypeName(from), GetFriendlyTypeName(to)));
                }

                return converter;
            }
        }

        #region Private Helper Methods

        /// <summary>
        /// Get converter method for converting a value from the source type to the destination type.
        /// </summary>
        /// <param name="from">Source type to convert value from.</param>
        /// <param name="to">Destination type to convert value to.</param>
        /// <param name="stringProcessing">Flag to determine how to handle destination strings (e.g. force nulls to empty, force emptys to null, or do nothing).</param>
        /// <param name="throwOnError">True to throw exception upon conversion error.</param>
        /// <returns></returns>
        private static Func<object, object> InternalGetConverter(Type from, Type to, StringProcessing stringProcessing = StringProcessing.None, bool throwOnError = true)
        {
            Func<object, object> conv = InternalGetKnownConverter(from, to, stringProcessing, throwOnError);

            #region Use .NET TypeDescriptor Converters
            if (conv == null)
            {
                var converter = TypeDescriptor.GetConverter(from);
                if (converter.CanConvertTo(to)) conv = (value) => converter.ConvertTo(value, to);
            }

            if (conv == null)
            {
                var converter = TypeDescriptor.GetConverter(to);
                if (converter.CanConvertFrom(from)) conv = (value) => converter.ConvertFrom(value);
            }
            #endregion

            #region Default Converter
            if (conv == null)
            {
                var emsg = string.Format("Cannot convert from {0} to {1}.", GetFriendlyTypeName(from), GetFriendlyTypeName(to));
                if (throwOnError) throw new NotSupportedException(emsg);
                conv = (value) => { throw new NotSupportedException(emsg); };
            }
            #endregion

            #region throwOnError==false Wrapper
            if (conv != null && !throwOnError)
            {
                conv = ThrowOnErrorWrapper(conv, from, to);
            }
            #endregion

            #region StringProcessing Wrapper
            if (conv != null && stringProcessing != StringProcessing.None && to == typeof(string))
            {
                conv = StringProcessingWrapper(conv, stringProcessing);
            }
            #endregion

            return conv;
        }

        /// <summary>
        /// Wrap the converter with a try/catch block.
        /// Note: This cannot be performed in-line within InternalGetConverter() because recursion will occur!
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        private static Func<object, object> ThrowOnErrorWrapper(Func<object, object> conv, Type from, Type to)
        {
            int arrayLen;
            if (from == typeof(Rectangle) || from == typeof(RectangleF) || from == typeof(Version) || from == typeof(IPAddress)) arrayLen = 4;
            else if (from == typeof(Point) || from == typeof(PointF) || from == typeof(Size) || from == typeof(SizeF)) arrayLen = 2;
            else arrayLen = 0;
            object defalt = GetDefaultValue(to, arrayLen);
            return (value) =>
            {
                try
                {
                    return conv(value);
                }
                catch (Exception ex)
                {
                    if (Logger != null) LogMsg(from, to, value, ex);
                    return defalt;
                }
            };
        }

        /// <summary>
        /// Wrap the toString converter with a string handler.
        /// Note: This cannot be performed in-line within InternalGetConverter() because recursion will occur!
        /// </summary>
        /// <param name="conv"></param>
        /// <param name="stringProcessing"></param>
        /// <returns></returns>
        private static Func<object, object> StringProcessingWrapper(Func<object, object> conv, StringProcessing stringProcessing)
        {
            if (stringProcessing == StringProcessing.ToEmpty) return (value) =>
            {
                var s = conv(value) as string;
                if (s == null) return string.Empty;
                return s;
            };

            if (stringProcessing == StringProcessing.ToNull) return (value) =>
            {
                var s = conv(value) as string;
                if (s == null || s.Length == 0) return null;
                return s;
            };

            return conv;
        }

        /// <summary>
        /// This is a non-public type that value.GetType() may return, so we handle it as if it were 'Type'.
        /// Used for converting Type <==> string.
        /// Used exclusively by InternalGetKnownConverter().
        /// </summary>
        private static readonly Type RuntimeType = Type.GetType("System.RuntimeType", false); 

        /// <summary>
        /// Generate and return the type converter that *we* handle. 
        /// InternalGetConverter() will handle the types we don't handle.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="stringProcessing"></param>
        /// <param name="throwOnError"></param>
        /// <returns></returns>
        public static Func<object, object> InternalGetKnownConverter(Type from, Type to, StringProcessing stringProcessing, bool throwOnError)
        {
            //Constant Suffixes: long = L, ulong = UL, decimal = M, uint = U, float = F, double = D
            //Primitive Types:   Boolean, Byte, SByte, Char, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Single, Double.
            //Value Types:       (primitive types), Nullable, Enum, Decimal, Guid, DateTime, DateTimeOffset, TimeSpan, Point, PointF, Size, SizeF, Rectangle, RectangleF
            //Reference Types:   String, Type, Version, IPAddress
            const long UnixEpoch = 621355968000000000L; //#Ticks to 1/1/1970 == pre-evaluated new DateTime(1970,1,1).Ticks;

            #region Identity: T==>T (e.g. pass-thru)
            if (from == to) return (value) => value;
            #endregion

            #region Nullable<> conversion
            if (to.IsValueType && IsNullable(from))
            {
                var convert = GetConverter(GetNullableType(from), to, stringProcessing, throwOnError);
                var defalt = GetDefaultValue(to,0);
                return (value) => value==null ? defalt : convert(value);
            }
            if (from.IsValueType && IsNullable(to))
            {
                var convert = GetConverter(from, GetNullableType(to), stringProcessing, throwOnError);
                return (value) => convert(value);
            }
            if (IsNullable(from) && IsNullable(to))
            {
                var convert = GetConverter(GetNullableType(from), GetNullableType(to), stringProcessing, throwOnError);
                return (value) => value==null ? null : convert(value);
            }
            #endregion

            #region Primitive or String ==> Bool
            if (to == typeof(bool))
            {
                if (from.IsPrimitive || from == typeof(string)) return (value) => "|false|no|f|n|0|".IndexOf(string.Concat("|", value.ToString(), "|"), StringComparison.OrdinalIgnoreCase) == -1;
                if (from == typeof(decimal)) return (value) => "|false|no|f|n|0|".IndexOf(string.Concat("|", ((decimal)value).ToString("0.#################################", CultureInfo.InvariantCulture), "|"), StringComparison.OrdinalIgnoreCase) == -1;
            }
            #endregion
            #region Bool ==> Primitive or String
            if (from == typeof(bool))
            {
                if (to == typeof(byte)) return value => (bool)value ? (byte)1 : (byte)0;
                if (to == typeof(sbyte)) return value => (bool)value ? (sbyte)1 : (sbyte)0;
                if (to == typeof(char)) return value => (bool)value ? 'T' : 'F';
                if (to == typeof(decimal)) return value => (bool)value ? (decimal)1 : (decimal)0;
                if (to == typeof(double)) return value => (bool)value ? (double)1 : (double)0;
                if (to == typeof(float)) return value => (bool)value ? (float)1 : (float)0;
                if (to == typeof(int)) return value => (bool)value ? (int)1 : (int)0;
                if (to == typeof(uint)) return value => (bool)value ? (uint)1 : (uint)0;
                if (to == typeof(long)) return value => (bool)value ? (long)1 : (long)0;
                if (to == typeof(ulong)) return value => (bool)value ? (ulong)1 : (ulong)0;
                if (to == typeof(short)) return value => (bool)value ? (short)1 : (short)0;
                if (to == typeof(ushort)) return value => (bool)value ? (ushort)1 : (ushort)0;
                //if (to == typeof(IntPtr)) return value => new IntPtr((bool)value ? 1 : 0);
                //if (to == typeof(UIntPtr)) return value => new UIntPtr((bool)value ? 1u : 0u);

                if (to == typeof(bool[])) return value => new bool[] { (bool)value };
                if (to == typeof(byte[])) return value => new byte[] { (bool)value ? (byte)1 : (byte)0 };
                if (to == typeof(sbyte[])) return value => new sbyte[] { (bool)value ? (sbyte)1 : (sbyte)0 };
                if (to == typeof(char[])) return value => new char[] { (bool)value ? 'T' : 'F' };
                if (to == typeof(decimal[])) return value => new decimal[] { (bool)value ? (decimal)1 : (decimal)0 };
                if (to == typeof(double[])) return value => new double[] { (bool)value ? (double)1 : (double)0 };
                if (to == typeof(float[])) return value => new float[] { (bool)value ? (float)1 : (float)0 };
                if (to == typeof(int[])) return value => new int[] { (bool)value ? (int)1 : (int)0 };
                if (to == typeof(uint[])) return value => new uint[] { (bool)value ? (uint)1 : (uint)0 };
                if (to == typeof(long[])) return value => new long[] { (bool)value ? (long)1 : (long)0 };
                if (to == typeof(ulong[])) return value => new ulong[] { (bool)value ? (ulong)1 : (ulong)0 };
                if (to == typeof(short[])) return value => new short[] { (bool)value ? (short)1 : (short)0 };
                if (to == typeof(ushort[])) return value => new ushort[] { (bool)value ? (ushort)1 : (ushort)0 };
                if (to == typeof(string[])) return value => new string[] { value.ToString() };

                if (to == typeof(List<bool>)) return value => new List<bool>() { (bool)value };
                if (to == typeof(List<byte>)) return value => new List<byte>() { (bool)value ? (byte)1 : (byte)0 };
                if (to == typeof(List<sbyte>)) return value => new List<sbyte>() { (bool)value ? (sbyte)1 : (sbyte)0 };
                if (to == typeof(List<char>)) return value => new List<char>() { (bool)value ? (char)1 : (char)0 };
                if (to == typeof(List<decimal>)) return value => new List<decimal>() { (bool)value ? (decimal)1 : (decimal)0 };
                if (to == typeof(List<double>)) return value => new List<double>() { (bool)value ? (double)1 : (double)0 };
                if (to == typeof(List<float>)) return value => new List<float>() { (bool)value ? (float)1 : (float)0 };
                if (to == typeof(List<int>)) return value => new List<int>() { (bool)value ? (int)1 : (int)0 };
                if (to == typeof(List<uint>)) return value => new List<uint>() { (bool)value ? (uint)1 : (uint)0 };
                if (to == typeof(List<long>)) return value => new List<long>() { (bool)value ? (long)1 : (long)0 };
                if (to == typeof(List<ulong>)) return value => new List<ulong>() { (bool)value ? (ulong)1 : (ulong)0 };
                if (to == typeof(List<short>)) return value => new List<short>() { (bool)value ? (short)1 : (short)0 };
                if (to == typeof(List<ushort>)) return value => new List<ushort>() { (bool)value ? (ushort)1 : (ushort)0 };
                if (to == typeof(List<string>)) return value => new List<string>() { value.ToString() };
            }
            #endregion

            #region Primitive or String <==> Enum
            if (to.IsEnum)
            {
                if (from.IsPrimitive) return (value) => Enum.ToObject(to, (long)value);
                if (from == typeof(decimal)) return (value) =>  Enum.ToObject(to, decimal.ToInt64((decimal)value));
                if (from == typeof(string)) return (value) => Enum.Parse(to, (string)value, true);
                if (from.IsEnum)
                {
                    var convert = GetConverter(Enum.GetUnderlyingType(from), Enum.GetUnderlyingType(to), stringProcessing, throwOnError);
                    return (value) => Enum.ToObject(to, convert(value));
                }
            }
            if (from.IsEnum)
            {
                if (to.IsPrimitive)
                {
                    var convert = GetConverter(Enum.GetUnderlyingType(from), to, stringProcessing, throwOnError);
                    return (value) => convert(value);
                }
                if (to == typeof(string)) return (value) => value.ToString();
            }
            #endregion

            #region String ==> Type, Non-Primitive, or Array
            if (from == typeof(string))
            {
                if (to == typeof(Type)) return (value) => string.IsNullOrWhiteSpace((string)value) ? null : Type.GetType((string)value, true, true);
                else if (to == RuntimeType) return (value) => string.IsNullOrWhiteSpace((string)value) ? null : Type.GetType((string)value, true, true);
                else if (to.IsPrimitive)
                {
                    if (to == typeof(char)) return (value) => string.IsNullOrEmpty((string)value)?'\0':((string)value)[0];
                    else if (to == typeof(IntPtr)) return (value) => new IntPtr(long.Parse(string.IsNullOrWhiteSpace((string)value)?"0":(string)value));
                    else if (to == typeof(UIntPtr)) return (value) => new UIntPtr(ulong.Parse(string.IsNullOrWhiteSpace((string)value) ? "0" : (string)value));
                }
                else if (to.IsValueType && !to.IsPrimitive)
                {
                    if (to == typeof(DateTime)) return (value) => ToDateTime((string)value);
                    else if (to == typeof(DateTimeOffset)) return (value) => new DateTimeOffset(ToDateTime((string)value));
                    else if (to == typeof(TimeSpan)) return (value) => TimeSpan.Parse((string)value);
                    else if (to == typeof(Point)) return (value) =>
                    {
                        var fields = Regex.Split((string)value, @"[^0-9\.-]+", RegexOptions.Compiled)
                                .Where(p => p.Length > 0)
                                .Select(p => int.Parse(p))
                                .ToList();
                        for (int i = fields.Count; i < 2; i++) fields.Add(0);
                        return new Point(fields[0], fields[1]);
                    };
                    else if (to == typeof(PointF)) return (value) =>
                    {
                        var fields = Regex.Split((string)value, @"[^0-9\.-]+", RegexOptions.Compiled)
                                .Where(p => p.Length > 0)
                                .Select(p => float.Parse(p))
                                .ToList();
                        for (int i = fields.Count; i < 2; i++) fields.Add(0f);
                        return new PointF(fields[0], fields[1]);
                    };
                    else if (to == typeof(Rectangle)) return (value) =>
                    {
                        var fields = Regex.Split((string)value, @"[^0-9\.-]+", RegexOptions.Compiled)
                                .Where(p => p.Length > 0)
                                .Select(p => int.Parse(p))
                                .ToList();
                        for (int i = fields.Count; i < 4; i++) fields.Add(0);
                        return new Rectangle(fields[0], fields[1], fields[2], fields[3]);
                    };
                    else if (to == typeof(RectangleF)) return (value) =>
                    {
                        var fields = Regex.Split((string)value, @"[^0-9\.-]+", RegexOptions.Compiled)
                                .Where(p => p.Length > 0)
                                .Select(p => float.Parse(p))
                                .ToList();
                        for (int i = fields.Count; i < 4; i++) fields.Add(0f);
                        return new RectangleF(fields[0], fields[1], fields[2], fields[3]);
                    };
                    else if (to == typeof(Size)) return (value) =>
                    {
                        var fields = Regex.Split((string)value, @"[^0-9\.-]+", RegexOptions.Compiled)
                                .Where(p => p.Length > 0)
                                .Select(p => int.Parse(p))
                                .ToList();
                        for (int i = fields.Count; i < 2; i++) fields.Add(0);
                        return new Size(fields[0], fields[1]);
                    };
                    else if (to == typeof(SizeF)) return (value) =>
                    {
                        var fields = Regex.Split((string)value, @"[^0-9\.-]+", RegexOptions.Compiled)
                                .Where(p => p.Length > 0)
                                .Select(p => float.Parse(p))
                                .ToList();
                        for (int i = fields.Count; i < 2; i++) fields.Add(0f);
                        return new SizeF(fields[0], fields[1]);
                    };
                }
                else if (!to.IsValueType)
                {
                    if (to == typeof(Version)) return (value) => Version.Parse((string)value);
                    else if (to == typeof(IPAddress)) return (value) => IPAddress.Parse((string)value);
                    else if (IsArray(to))
                    {
                        var convert = GetConverter(from, GetArrayElementType(to), stringProcessing, throwOnError);
                        return (value) =>
                        {
                            var result = QuotedSplit((string)value);
                            IList list = (IList)GetDefaultValue(to, result.Length);
                            for (int i = 0; i < result.Length; i++) list[i] = convert(result[i]);
                            return list;
                        };
                    }
                }
            }
            #endregion

            #region Type, Non-Primitive, or Array ==> String
            if (to == typeof(string))
            {
                if (from == typeof(Type)) return (value) => value == null ? string.Empty : string.Join(", ", ((Type)value).AssemblyQualifiedName.Split(','), 0, 2);
                else if (from == RuntimeType) return (value) => value == null ? string.Empty : string.Join(", ", ((Type)value).AssemblyQualifiedName.Split(','), 0, 2);
                else if (from.IsPrimitive)
                {
                    if (from == typeof(char)) return (value) => ((char)value).ToString();
                    else if (from == typeof(IntPtr)) return (value) => ((IntPtr)value).ToString();
                    else if (from == typeof(UIntPtr)) return (value) => ((UIntPtr)value).ToString();
                }
                else if (from.IsValueType && !from.IsPrimitive)
                {
                    if (from == typeof(decimal)) return (value) => ((decimal)value).ToString("0.#################################", CultureInfo.InvariantCulture);
                    else if (from == typeof(Guid)) return (value) => ((Guid)value).ToString(null, CultureInfo.InvariantCulture);
                    else if (from == typeof(DateTime)) return (value) => ToString((DateTime)value);
                    else if (from == typeof(DateTimeOffset)) return (value) => ToString(((DateTimeOffset)value).DateTime);
                    else if (from == typeof(TimeSpan)) return (value) => ((TimeSpan)value).ToString("c", CultureInfo.InvariantCulture);
                    else if (from == typeof(Point)) return (value) => string.Concat(((Point)value).X, ",", ((Point)value).Y);
                    else if (from == typeof(PointF)) return (value) => string.Concat(((PointF)value).X, ",", ((PointF)value).Y);
                    else if (from == typeof(Rectangle)) return (value) => string.Concat(((Rectangle)value).X, ",", ((Rectangle)value).Y, ",", ((Rectangle)value).Width, ",", ((Rectangle)value).Height);
                    else if (from == typeof(RectangleF)) return (value) => string.Concat(((RectangleF)value).X, ",", ((RectangleF)value).Y, ",", ((RectangleF)value).Width, ",", ((RectangleF)value).Height);
                    else if (from == typeof(Size)) return (value) => string.Concat(((Size)value).Width, ",", ((Size)value).Height);
                    else if (from == typeof(SizeF)) return (value) => string.Concat(((SizeF)value).Width, ",", ((SizeF)value).Height);
                }
                else if (!from.IsValueType) //reference types (e.g. classes, not structs)
                {
                    if (from == typeof(Version)) return (value) => ((Version)value).ToString();
                    else if (from == typeof(IPAddress)) return (value) => ((IPAddress)value).ToString();
                    else if (IsArray(from))
                    {
                        var convert = GetConverter(GetArrayElementType(from), to, stringProcessing, throwOnError);
                        return (value) =>
                        {
                            var sb = new StringBuilder();
                            foreach (var o in (IList)value)
                            {
                                var s = (string)convert(o);
                                bool needsQuote = s.Contains(',');
                                if (needsQuote) sb.Append('"');
                                sb.Append(s);
                                if (needsQuote) sb.Append('"');
                                sb.Append(',');
                            }
                            if (sb.Length > 0) sb.Length -= 1;
                            return sb.ToString();
                        };
                    }
                }
                else if (typeof(IFormattable).IsAssignableFrom(from))
                {
                    return (value) => ((IFormattable)value).ToString(null, CultureInfo.InvariantCulture);
                }
            }
            #endregion

            #region Decimal ==> Primitive
            if ((from == typeof(decimal)) && to.IsPrimitive)
            {
                if (to == typeof(bool)) return (value) => (decimal)value != 0m;
                else if (to == typeof(byte)) return (value) => decimal.ToByte((decimal)value);
                else if (to == typeof(sbyte)) return (value) => decimal.ToSByte((decimal)value);
                else if (to == typeof(short)) return (value) => decimal.ToInt16((decimal)value);
                else if (to == typeof(ushort)) return (value) => decimal.ToUInt16((decimal)value);
                else if (to == typeof(int)) return (value) => decimal.ToInt32((decimal)value);
                else if (to == typeof(uint)) return (value) => decimal.ToUInt32((decimal)value);
                else if (to == typeof(long)) return (value) => decimal.ToInt64((decimal)value);
                else if (to == typeof(ulong)) return (value) => decimal.ToUInt64((decimal)value);
                else if (to == typeof(char)) return (value) => (char)decimal.ToUInt16((decimal)value);
                else if (to == typeof(double)) return (value) => decimal.ToDouble((decimal)value);
                else if (to == typeof(float)) return (value) => decimal.ToSingle((decimal)value);
                else if (to == typeof(IntPtr)) return (value) => new IntPtr(decimal.ToInt64((decimal)value));
                else if (to == typeof(UIntPtr)) return (value) => new UIntPtr(decimal.ToUInt64((decimal)value));
            }
            #endregion

            #region Primitive ==> Decimal
            if ((to == typeof(decimal)) && from.IsPrimitive)
            {
                if (from == typeof(bool)) return (value) => (bool)value ? 1m : 0m;
                else if (from == typeof(byte)) return (value) => new decimal((int)value);
                else if (from == typeof(sbyte)) return (value) => new decimal((int)value);
                else if (from == typeof(short)) return (value) => new decimal((int)value);
                else if (from == typeof(ushort)) return (value) => new decimal((uint)value);
                else if (from == typeof(int)) return (value) => new decimal((int)value);
                else if (from == typeof(uint)) return (value) => new decimal((uint)value);
                else if (from == typeof(long)) return (value) => new decimal((long)value);
                else if (from == typeof(ulong)) return (value) => new decimal((ulong)value);
                else if (from == typeof(char)) return (value) => new decimal((uint)value);
                else if (from == typeof(double)) return (value) => new decimal((double)value);
                else if (from == typeof(float)) return (value) => new decimal((float)value);
                else if (from == typeof(IntPtr)) return (value) => new decimal(((IntPtr)value).ToInt64());
                else if (from == typeof(UIntPtr)) return (value) => new decimal(((UIntPtr)value).ToUInt64());
            }
            #endregion
            
            #region Primitive ==> Non-Primitive
            if (from.IsPrimitive && !to.IsPrimitive)
            {
                if (to==typeof(DateTime))
                {
                    if (from == typeof(decimal)) return (value) => new DateTime(decimal.ToInt64((decimal)value));
                    if (from == typeof(long)) return (value) => new DateTime((long)value);
                    if (from == typeof(ulong)) return (value) => new DateTime((long)value);
                    if (from == typeof(int)) return (value) => new DateTime(UnixEpoch).AddSeconds((double)value);
                    if (from == typeof(uint)) return (value) => new DateTime(UnixEpoch).AddSeconds((double)value);
                    if (from == typeof(short)) return (value) => new DateTime(UnixEpoch).AddDays((double)value);
                    if (from == typeof(ushort)) return (value) => new DateTime(UnixEpoch).AddDays((double)value);
                    if (from == typeof(float)) return (value) => DateTime.FromOADate((double)value);
                    if (from == typeof(double)) return (value) => DateTime.FromOADate((double)value);
                }
                if (to == typeof(DateTimeOffset))
                {
                    if (from == typeof(decimal)) return (value) => new DateTimeOffset(decimal.ToInt64((decimal)value),TimeSpan.Zero);
                    if (from == typeof(long)) return (value) => new DateTimeOffset((long)value, TimeSpan.Zero);
                    if (from == typeof(ulong)) return (value) => new DateTimeOffset((long)value, TimeSpan.Zero);
                    if (from == typeof(int)) return (value) => new DateTimeOffset(UnixEpoch, TimeSpan.Zero).AddSeconds((double)value);
                    if (from == typeof(uint)) return (value) => new DateTimeOffset(UnixEpoch, TimeSpan.Zero).AddSeconds((double)value);
                    if (from == typeof(short)) return (value) => new DateTimeOffset(UnixEpoch, TimeSpan.Zero).AddDays((double)value);
                    if (from == typeof(ushort)) return (value) => new DateTimeOffset(UnixEpoch, TimeSpan.Zero).AddDays((double)value);
                    if (from == typeof(float)) return (value) => new DateTimeOffset(DateTime.FromOADate((double)value));
                    if (from == typeof(double)) return (value) => new DateTimeOffset(DateTime.FromOADate((double)value));
                }
                if (to == typeof(TimeSpan))
                {
                    if (from == typeof(decimal)) return (value) => TimeSpan.FromTicks(decimal.ToInt64((decimal)value));
                    if (from == typeof(long)) return (value) => TimeSpan.FromTicks((long)value);
                    if (from == typeof(ulong)) return (value) => TimeSpan.FromTicks((long)value);
                    if (from == typeof(int)) return (value) => TimeSpan.FromTicks(UnixEpoch + ((int)value * TimeSpan.TicksPerSecond));
                    if (from == typeof(uint)) return (value) => TimeSpan.FromTicks(UnixEpoch + ((uint)value * TimeSpan.TicksPerSecond));
                    if (from == typeof(short)) return (value) => TimeSpan.FromTicks(UnixEpoch + ((uint)value * TimeSpan.TicksPerDay));
                    if (from == typeof(ushort)) return (value) => TimeSpan.FromTicks(UnixEpoch + ((ushort)value * TimeSpan.TicksPerDay));
                    if (from == typeof(float)) return (value) => TimeSpan.FromTicks((long)((double)value * TimeSpan.TicksPerDay));
                    if (from == typeof(double)) return (value) => TimeSpan.FromTicks((long)((double)value * TimeSpan.TicksPerDay));
                }
                if (to == typeof(Point))
                {
                    if (from == typeof(decimal)) { var conv = GetSplitMethod(typeof(long), 2); return (value) => { var v = conv(decimal.ToInt64((decimal)value)); return new Point(v[0], v[1]); }; }
                    if (from == typeof(long)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Point(v[0], v[1]); }; }
                    if (from == typeof(ulong)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Point(v[0], v[1]); }; }
                    if (from == typeof(int)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Point(v[0], v[1]); }; }
                    if (from == typeof(uint)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Point(v[0], v[1]); }; }
                    if (from == typeof(short)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Point(v[0], v[1]); }; }
                    if (from == typeof(ushort)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Point(v[0], v[1]); }; }
                }
                if (to == typeof(PointF))
                {
                    if (from == typeof(decimal)) { var conv = GetSplitMethod(typeof(long), 2); return (value) => { var v = conv(decimal.ToInt64((decimal)value)); return new PointF(v[0], v[1]); }; }
                    if (from == typeof(long)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new PointF(v[0], v[1]); }; }
                    if (from == typeof(ulong)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new PointF(v[0], v[1]); }; }
                    if (from == typeof(int)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new PointF(v[0], v[1]); }; }
                    if (from == typeof(uint)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new PointF(v[0], v[1]); }; }
                    if (from == typeof(short)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new PointF(v[0], v[1]); }; }
                    if (from == typeof(ushort)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new PointF(v[0], v[1]); }; }
                }
                if (to == typeof(Size))
                {
                    if (from == typeof(decimal)) { var conv = GetSplitMethod(typeof(long), 2); return (value) => { var v = conv(decimal.ToInt64((decimal)value)); return new Size(v[0], v[1]); }; }
                    if (from == typeof(long)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Size(v[0], v[1]); }; }
                    if (from == typeof(ulong)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Size(v[0], v[1]); }; }
                    if (from == typeof(int)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Size(v[0], v[1]); }; }
                    if (from == typeof(uint)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Size(v[0], v[1]); }; }
                    if (from == typeof(short)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Size(v[0], v[1]); }; }
                    if (from == typeof(ushort)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new Size(v[0], v[1]); }; }
                }
                if (to == typeof(SizeF))
                {
                    if (from == typeof(decimal)) { var conv = GetSplitMethod(typeof(long), 2); return (value) => { var v = conv(decimal.ToInt64((decimal)value)); return new SizeF(v[0], v[1]); }; }
                    if (from == typeof(long)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new SizeF(v[0], v[1]); }; }
                    if (from == typeof(ulong)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new SizeF(v[0], v[1]); }; }
                    if (from == typeof(int)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new SizeF(v[0], v[1]); }; }
                    if (from == typeof(uint)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new SizeF(v[0], v[1]); }; }
                    if (from == typeof(short)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new SizeF(v[0], v[1]); }; }
                    if (from == typeof(ushort)) { var conv = GetSplitMethod(from, 2); return (value) => { var v = conv(value); return new SizeF(v[0], v[1]); }; }
                }
                if (to == typeof(Rectangle))
                {
                    if (from == typeof(decimal)) { var conv = GetSplitMethod(typeof(long), 4); return (value) => { var v = conv(decimal.ToInt64((decimal)value)); return new Rectangle(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(long)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new Rectangle(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(ulong)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new Rectangle(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(int)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new Rectangle(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(uint)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new Rectangle(v[0], v[1], v[2], v[3]); }; }
                }
                if (to == typeof(RectangleF))
                {
                    if (from == typeof(decimal)) { var conv = GetSplitMethod(typeof(long), 4); return (value) => { var v = conv(decimal.ToInt64((decimal)value)); return new RectangleF(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(long)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new RectangleF(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(ulong)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new RectangleF(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(int)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new RectangleF(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(uint)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new RectangleF(v[0], v[1], v[2], v[3]); }; }
                }
                if (to == typeof(Version))
                {
                    if (from == typeof(decimal)) { var conv = GetSplitMethod(typeof(long), 4); return (value) => { var v = conv(decimal.ToInt64((decimal)value)); return new Version(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(long)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new Version(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(ulong)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new Version(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(int)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new Version(v[0], v[1], v[2], v[3]); }; }
                    if (from == typeof(uint)) { var conv = GetSplitMethod(from, 4); return (value) => { var v = conv(value); return new Version(v[0], v[1], v[2], v[3]); }; }
                }
                if (to == typeof(IPAddress))
                {
                    if (from == typeof(decimal)) return (value) => new IPAddress(decimal.ToInt64((decimal)value) & 0xFFFFFFFFL);
                    if (from == typeof(long)) return (value) => new IPAddress((long)value & 0xFFFFFFFFL);
                    if (from == typeof(ulong)) return (value) => new IPAddress(unchecked((long)value & 0xFFFFFFFFL));
                    if (from == typeof(int)) return (value) => new IPAddress((long)value & 0xFFFFFFFF);
                    if (from == typeof(uint)) return (value) => new IPAddress(unchecked((long)value & 0xFFFFFFFFL));
                }
            }
            #endregion

            #region Primitive ==> Primitive
            if (to.IsPrimitive && from.IsPrimitive)
            {
                if (from == typeof(char))
                {
                    //if (to == typeof(bool)) return (value) => (char)value != '\0' && (char)value != 'N' && (char)value != 'n' && (char)value != 'F' && (char)value != 'f' && (char)value != '0';
                    if (to == typeof(byte)) return (value) => (byte)(char)value;
                    if (to == typeof(sbyte)) return (value) => (sbyte)(char)value;
                    if (to == typeof(short)) return (value) => (short)(char)value;
                    if (to == typeof(ushort)) return (value) => (ushort)(char)value;
                    if (to == typeof(int)) return (value) => (int)(char)value;
                    if (to == typeof(uint)) return (value) => (uint)(char)value;
                    if (to == typeof(long)) return (value) => (long)(char)value;
                    if (to == typeof(ulong)) return (value) => (ulong)(char)value;
                    //if (to == typeof(char)) return (value) => (char)value;
                    if (to == typeof(double)) return (value) => (double)(char)value;
                    if (to == typeof(float)) return (value) => (float)(char)value;
                    if (to == typeof(IntPtr)) return (value) => new IntPtr((int)(char)value);
                    if (to == typeof(UIntPtr)) return (value) => new IntPtr((uint)(char)value);

                }
                if (from == typeof(IntPtr))
                {
                    //if (to == typeof(bool)) return (value) => (char)value != '\0' && (char)value != 'N' && (char)value != 'n' && (char)value != 'F' && (char)value != 'f' && (char)value != '0';
                    if (to == typeof(byte)) return (value) => (byte)((IntPtr)value).ToInt32();
                    if (to == typeof(sbyte)) return (value) => (sbyte)((IntPtr)value).ToInt32();
                    if (to == typeof(short)) return (value) => (short)((IntPtr)value).ToInt32();
                    if (to == typeof(ushort)) return (value) => (ushort)((IntPtr)value).ToInt32();
                    if (to == typeof(int)) return (value) => (int)((IntPtr)value).ToInt32();
                    if (to == typeof(uint)) return (value) => (uint)((IntPtr)value).ToInt32();
                    if (to == typeof(long)) return (value) => (long)((IntPtr)value).ToInt64();
                    if (to == typeof(ulong)) return (value) => (ulong)((IntPtr)value).ToInt64();
                    if (to == typeof(char)) return (value) => (char)((IntPtr)value).ToInt32();
                    if (to == typeof(double)) return (value) => (double)((IntPtr)value).ToInt32();
                    if (to == typeof(float)) return (value) => (float)((IntPtr)value).ToInt32();
                    //if (to == typeof(IntPtr)) return (value) => new IntPtr(IntPtr)value).ToInt64());
                    if (to == typeof(UIntPtr)) return (value) => new IntPtr(((IntPtr)value).ToInt64());
                }
                if (from == typeof(UIntPtr))
                {
                    //if (to == typeof(bool)) return (value) => (char)value != '\0' && (char)value != 'N' && (char)value != 'n' && (char)value != 'F' && (char)value != 'f' && (char)value != '0';
                    if (to == typeof(byte)) return (value) => (byte)((UIntPtr)value).ToUInt32();
                    if (to == typeof(sbyte)) return (value) => (sbyte)((UIntPtr)value).ToUInt32();
                    if (to == typeof(short)) return (value) => (short)((UIntPtr)value).ToUInt32();
                    if (to == typeof(ushort)) return (value) => (ushort)((UIntPtr)value).ToUInt32();
                    if (to == typeof(int)) return (value) => (int)((UIntPtr)value).ToUInt32();
                    if (to == typeof(uint)) return (value) => (uint)((UIntPtr)value).ToUInt32();
                    if (to == typeof(long)) return (value) => (long)((UIntPtr)value).ToUInt64();
                    if (to == typeof(ulong)) return (value) => (ulong)((UIntPtr)value).ToUInt64();
                    if (to == typeof(char)) return (value) => (char)((UIntPtr)value).ToUInt32();
                    if (to == typeof(double)) return (value) => (double)((UIntPtr)value).ToUInt32();
                    if (to == typeof(float)) return (value) => (float)((UIntPtr)value).ToUInt32();
                    if (to == typeof(IntPtr)) return (value) => new IntPtr((long)((UIntPtr)value).ToUInt64());
                    //if (to == typeof(UIntPtr)) return (value) => new UIntPtr(((UIntPtr)value).ToUInt64());
                }
            }
            #endregion

            #region Non-Primitive ==> Primitive
            if (to.IsPrimitive && !from.IsPrimitive)
            {
                if (from == typeof(DateTime))
                {
                    if (to == typeof(decimal)) return (value) => (decimal)((DateTime)value).Ticks;
                    if (to == typeof(long))    return (value) => ((DateTime)value).Ticks;
                    if (to == typeof(ulong))   return (value) => (ulong)((DateTime)value).Ticks;
                    if (to == typeof(int))     return (value) => (int)((((DateTime)value).Ticks - UnixEpoch) / TimeSpan.TicksPerSecond);  //seconds from 1/1/1970
                    if (to == typeof(uint))    return (value) => (uint)((((DateTime)value).Ticks - UnixEpoch) / TimeSpan.TicksPerSecond); //seconds from 1/1/1970
                    if (to == typeof(short))   return (value) => (short)((((DateTime)value).Ticks - UnixEpoch) / TimeSpan.TicksPerDay);   //days from 1/1/1970
                    if (to == typeof(ushort))  return (value) => (ushort)((((DateTime)value).Ticks - UnixEpoch) / TimeSpan.TicksPerDay);  //days from 1/1/1970
                    if (to == typeof(float))   return (value) => (float)((DateTime)value).ToOADate(); //days + fractional day
                    if (to == typeof(double))  return (value) => ((DateTime)value).ToOADate(); //days + fractional day
                }
                if (from == typeof(DateTimeOffset))
                {
                    if (to == typeof(decimal)) return (value) => (decimal)((DateTimeOffset)value).Ticks;
                    if (to == typeof(long))    return (value) => ((DateTimeOffset)value).Ticks;
                    if (to == typeof(ulong))   return (value) => (ulong)((DateTimeOffset)value).Ticks;
                    if (to == typeof(int))     return (value) => (int)((((DateTimeOffset)value).Ticks - UnixEpoch) / TimeSpan.TicksPerSecond);  //seconds from 1/1/1970
                    if (to == typeof(uint))    return (value) => (uint)((((DateTimeOffset)value).Ticks - UnixEpoch) / TimeSpan.TicksPerSecond); //seconds from 1/1/1970
                    if (to == typeof(short))   return (value) => (short)((((DateTimeOffset)value).Ticks - UnixEpoch) / TimeSpan.TicksPerDay);   //days from 1/1/1970
                    if (to == typeof(ushort))  return (value) => (ushort)((((DateTimeOffset)value).Ticks - UnixEpoch) / TimeSpan.TicksPerDay);  //days from 1/1/1970
                    if (to == typeof(float))   return (value) => (float)((DateTimeOffset)value).DateTime.ToOADate(); //days + fractional day
                    if (to == typeof(double))  return (value) => ((DateTimeOffset)value).DateTime.ToOADate(); //days + fractional day
                }
                if (from == typeof(TimeSpan))
                {
                    if (to == typeof(decimal)) return (value) => (decimal)((TimeSpan)value).Ticks;
                    if (to == typeof(long))    return (value) => ((TimeSpan)value).Ticks;
                    if (to == typeof(ulong))   return (value) => (ulong)((TimeSpan)value).Ticks;
                    if (to == typeof(int))     return (value) => (int)((((TimeSpan)value).Ticks - UnixEpoch) / TimeSpan.TicksPerSecond);  //seconds from 1/1/1970
                    if (to == typeof(uint))    return (value) => (uint)((((TimeSpan)value).Ticks - UnixEpoch) / TimeSpan.TicksPerSecond); //seconds from 1/1/1970
                    if (to == typeof(short))   return (value) => (short)((((TimeSpan)value).Ticks - UnixEpoch) / TimeSpan.TicksPerDay);   //days from 1/1/1970
                    if (to == typeof(ushort))  return (value) => (ushort)((((TimeSpan)value).Ticks - UnixEpoch) / TimeSpan.TicksPerDay);  //days from 1/1/1970
                    if (to == typeof(float))   return (value) => (float)((TimeSpan)value).TotalDays; //days + fractional day
                    if (to == typeof(double))  return (value) => ((TimeSpan)value).TotalDays; //days + fractional day
                }
                if (from == typeof(Point))
                {
                    if (to == typeof(decimal)) { var conv = Get2ElementJoinMethod(typeof(long)); return (value) => (decimal)conv(((Point)value).X, ((Point)value).Y); }
                    if (to == typeof(long))    { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Point)value).X, ((Point)value).Y); }
                    if (to == typeof(ulong))   { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Point)value).X, ((Point)value).Y); }
                    if (to == typeof(int))     { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Point)value).X, ((Point)value).Y); }
                    if (to == typeof(uint))    { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Point)value).X, ((Point)value).Y); }
                    if (to == typeof(short))   { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Point)value).X, ((Point)value).Y); }
                    if (to == typeof(ushort))  { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Point)value).X, ((Point)value).Y); }
                }
                if (from == typeof(PointF))
                {
                    if (to == typeof(decimal)) { var conv = Get2ElementJoinMethod(typeof(long)); return (value) => (decimal)conv((int)((PointF)value).X, (int)((PointF)value).Y); }
                    if (to == typeof(long))    { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((PointF)value).X, (int)((PointF)value).Y); }
                    if (to == typeof(ulong))   { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((PointF)value).X, (int)((PointF)value).Y); }
                    if (to == typeof(int))     { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((PointF)value).X, (int)((PointF)value).Y); }
                    if (to == typeof(uint))    { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((PointF)value).X, (int)((PointF)value).Y); }
                    if (to == typeof(short))   { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((PointF)value).X, (int)((PointF)value).Y); }
                    if (to == typeof(ushort))  { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((PointF)value).X, (int)((PointF)value).Y); }
                }
                if (from == typeof(Size))
                {
                    if (to == typeof(decimal)) { var conv = Get2ElementJoinMethod(typeof(long)); return (value) => (decimal)conv(((Size)value).Width, ((Size)value).Height); }
                    if (to == typeof(long))    { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Size)value).Width, ((Size)value).Height); }
                    if (to == typeof(ulong))   { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Size)value).Width, ((Size)value).Height); }
                    if (to == typeof(int))     { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Size)value).Width, ((Size)value).Height); }
                    if (to == typeof(uint))    { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Size)value).Width, ((Size)value).Height); }
                    if (to == typeof(short))   { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Size)value).Width, ((Size)value).Height); }
                    if (to == typeof(ushort))  { var conv = Get2ElementJoinMethod(to); return (value) => conv(((Size)value).Width, ((Size)value).Height); }
                }
                if (from == typeof(SizeF))
                {
                    if (to == typeof(decimal)) { var conv = Get2ElementJoinMethod(typeof(long)); return (value) => (decimal)conv((int)((SizeF)value).Width, (int)((SizeF)value).Height); }
                    if (to == typeof(long))    { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((SizeF)value).Width, (int)((SizeF)value).Height); }
                    if (to == typeof(ulong))   { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((SizeF)value).Width, (int)((SizeF)value).Height); }
                    if (to == typeof(int))     { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((SizeF)value).Width, (int)((SizeF)value).Height); }
                    if (to == typeof(uint))    { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((SizeF)value).Width, (int)((SizeF)value).Height); }
                    if (to == typeof(short))   { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((SizeF)value).Width, (int)((SizeF)value).Height); }
                    if (to == typeof(ushort))  { var conv = Get2ElementJoinMethod(to); return (value) => conv((int)((SizeF)value).Width, (int)((SizeF)value).Height); }
                }
                if (from == typeof(Rectangle))
                {
                    if (to == typeof(decimal)) { var conv = Get4ElementJoinMethod(typeof(long)); return (value) => (decimal)conv(((Rectangle)value).X, ((Rectangle)value).Y, ((Rectangle)value).Width, ((Rectangle)value).Height); }
                    if (to == typeof(long))    { var conv = Get4ElementJoinMethod(to); return (value) => conv(((Rectangle)value).X, ((Rectangle)value).Y, ((Rectangle)value).Width, ((Rectangle)value).Height); }
                    if (to == typeof(ulong))   { var conv = Get4ElementJoinMethod(to); return (value) => conv(((Rectangle)value).X, ((Rectangle)value).Y, ((Rectangle)value).Width, ((Rectangle)value).Height); }
                    if (to == typeof(int))     { var conv = Get4ElementJoinMethod(to); return (value) => conv(((Rectangle)value).X, ((Rectangle)value).Y, ((Rectangle)value).Width, ((Rectangle)value).Height); }
                    if (to == typeof(uint))    { var conv = Get4ElementJoinMethod(to); return (value) => conv(((Rectangle)value).X, ((Rectangle)value).Y, ((Rectangle)value).Width, ((Rectangle)value).Height); }
                }
                if (from == typeof(RectangleF))
                {
                    if (to == typeof(decimal)) { var conv = Get4ElementJoinMethod(typeof(long)); return (value) => (decimal)conv((int)((RectangleF)value).X, (int)((RectangleF)value).Y, (int)((RectangleF)value).Width, (int)((RectangleF)value).Height); }
                    if (to == typeof(long))    { var conv = Get4ElementJoinMethod(to); return (value) => conv((int)((RectangleF)value).X, (int)((RectangleF)value).Y, (int)((RectangleF)value).Width, (int)((RectangleF)value).Height); }
                    if (to == typeof(ulong))   { var conv = Get4ElementJoinMethod(to); return (value) => conv((int)((RectangleF)value).X, (int)((RectangleF)value).Y, (int)((RectangleF)value).Width, (int)((RectangleF)value).Height); }
                    if (to == typeof(int))     { var conv = Get4ElementJoinMethod(to); return (value) => conv((int)((RectangleF)value).X, (int)((RectangleF)value).Y, (int)((RectangleF)value).Width, (int)((RectangleF)value).Height); }
                    if (to == typeof(uint))    { var conv = Get4ElementJoinMethod(to); return (value) => conv((int)((RectangleF)value).X, (int)((RectangleF)value).Y, (int)((RectangleF)value).Width, (int)((RectangleF)value).Height); }
                }
                if (from == typeof(Version))
                {
                    if (to == typeof(decimal)) { var conv = Get4ElementJoinMethod(typeof(long)); return (value) => (decimal)conv(((Version)value).Major, ((Version)value).Minor, ((Version)value).Revision, ((Version)value).Build); }
                    if (to == typeof(long))    { var conv = Get4ElementJoinMethod(to); return (value) => conv(((Version)value).Major, ((Version)value).Minor, ((Version)value).Revision, ((Version)value).Build); }
                    if (to == typeof(ulong))   { var conv = Get4ElementJoinMethod(to); return (value) => conv(((Version)value).Major, ((Version)value).Minor, ((Version)value).Revision, ((Version)value).Build); }
                    if (to == typeof(int))     { var conv = Get4ElementJoinMethod(to); return (value) => conv(((Version)value).Major, ((Version)value).Minor, ((Version)value).Revision, ((Version)value).Build); }
                    if (to == typeof(uint))    { var conv = Get4ElementJoinMethod(to); return (value) => conv(((Version)value).Major, ((Version)value).Minor, ((Version)value).Revision, ((Version)value).Build); }
                }
                if (from == typeof(IPAddress))
                {
                    #pragma warning disable 618 //warning CS0618: 'System.Net.IPAddress.Address' is obsolete: 'This property has been deprecated. It is address family dependent. Please use IPAddress.Equals method to perform comparisons.
                    if (to == typeof(decimal)) return (value) => (decimal)(((IPAddress)value).Address & 0xFFFFFFFFL);
                    if (to == typeof(long))    return (value) => (long)(((IPAddress)value).Address & 0xFFFFFFFFL);
                    if (to == typeof(ulong))   return (value) => (ulong)(((IPAddress)value).Address & 0xFFFFFFFFL);
                    if (to == typeof(int))     return (value) => (int)(((IPAddress)value).Address & 0xFFFFFFFFL);
                    if (to == typeof(uint))    return (value) => (uint)(((IPAddress)value).Address & 0xFFFFFFFFL);
                    #pragma warning restore 618
                }
            }
            #endregion

            #region Array <-> conversion
            if (IsArray(to))
            {
                if (IsArray(from))
                {
                    var convert = GetConverter(GetArrayElementType(from), GetArrayElementType(to), stringProcessing, throwOnError);
                    return (value) =>
                    {
                        IList srcList = (IList)value;
                        IList dstList = (IList)GetDefaultValue(to, srcList.Count);
                        for (int i = 0; i < srcList.Count; i++) dstList[i] = convert(srcList[i]);
                        return dstList;
                    };
                }
                else
                {
                    var convert = GetConverter(from, GetArrayElementType(to), stringProcessing, throwOnError);
                    return (value) =>
                    {
                        IList dstList = (IList)GetDefaultValue(to, 1);
                        dstList[0] = convert(value);
                        return dstList;
                    };
                }
            }

            if (IsArray(from))
            {
                var convert = GetConverter(GetArrayElementType(from), to, stringProcessing, throwOnError);
                return (value) => convert(((IList)value)[0]);
            }
            #endregion

            return null; //we didn't provide any special casting handlers. The caller will have to handle it.
        }

        /// <summary>
        /// Get the array parameter type for any IList type.
        /// </summary>
        /// <param name="t">The type to search</param>
        /// <returns>Array parameter type or null if not an array.</returns>
        private static Type GetArrayElementType(Type t)
        {
            return t.IsArray
                       ? t.GetElementType()
                       : t.IsGenericType && t.GenericTypeArguments != null && t.GenericTypeArguments.Length > 0
                           ? t.GenericTypeArguments[0]
                           : null;
        }

        /// <summary>
        /// Get the generic Nullable`1 type parameter.
        /// </summary>
        /// <param name="t">Type to check</param>
        /// <returns>The type parameter or null if not Nullable</returns>
        private static Type GetNullableType(Type t)
        {
            return t.IsGenericType && 
                t.GetGenericTypeDefinition() == typeof(System.Nullable<>) && 
                t.GenericTypeArguments.Length > 0 
                ? t.GenericTypeArguments[0] 
                : null;
        }

        /// <summary>
        /// Determine if specified type is a nullable type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static bool IsNullable(Type t)
        {
            return t.IsGenericType && t.GetGenericTypeDefinition() == typeof(System.Nullable<>);
        }

        /// <summary>
        /// Check if type is of IList type.
        /// </summary>
        /// <param name="t">Type to check</param>
        /// <returns>True if array type or false if not</returns>
        private static bool IsArray(Type t)
        {
            return !t.IsValueType && typeof(IList).IsAssignableFrom(t);
        }

        /// <summary>
        /// Robust helper method for converting a string into a datetime. 
        /// Also includes support for D16 format.
        /// Will throw FormatException upon error.
        /// </summary>
        /// <param name="s">string to parse into datetime</param>
        /// <returns>DateTime object</returns>
        private static DateTime ToDateTime(string s)
        {
            DateTime dt;
            if (DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out dt)) return dt;
            if (CultureInfo.CurrentCulture.LCID != 0x0409 && DateTime.TryParse(s, CultureInfo.GetCultureInfo(0x0409), DateTimeStyles.AssumeLocal | DateTimeStyles.AllowWhiteSpaces, out dt)) return dt;
            if (s.Length >= 8 && s.All(c => (c >= '0' && c <= '9'))) //special case: D16 format
            {
                int year = 0, month = 0, day = 0, hour = 0, minute = 0, second = 0, millisecond = 0;
                year = Int32.Parse(s.Substring(0, 4));
                month = Int32.Parse(s.Substring(4, 2));
                day = Int32.Parse(s.Substring(6, 2));
                if (s.Length >= 12) hour = Int32.Parse(s.Substring(8, 2));
                if (s.Length >= 12) minute = Int32.Parse(s.Substring(10, 2));
                if (s.Length >= 14) second = Int32.Parse(s.Substring(12, 2));
                if (s.Length >= 16 && s.Length < 17) millisecond = Int32.Parse(s.Substring(14, 1)) * 100;
                if (s.Length >= 16 && s.Length < 18) millisecond = Int32.Parse(s.Substring(14, 2)) * 10;
                if (s.Length >= 16 && s.Length < 19) millisecond = Int32.Parse(s.Substring(14, 3));

                if (year < 1970 || year > 3000) throw new FormatException("D16 year part must be between 1970 and 3000.");
                if (month < 1 || month > 12) throw new FormatException("D16 month part must be between 01 and 12.");
                if (day < 1 || day > DateTime.DaysInMonth(year, month)) throw new FormatException("D16 day part must be between 01 and 31.");
                if (hour > 23) throw new FormatException("D16 hour part must be between 00 and 23.");
                if (minute > 59) throw new FormatException("D16 minute part must be between 00 and 59.");
                if (second > 59) throw new FormatException("D16 second part must be between 00 and 59.");

                return new DateTime(year, month, day, hour, minute, second, millisecond);
            }

            throw new FormatException(string.Format("Don't know how to convert \"{0}\" to DateTime", s));
        }

        /// <summary>
        /// Convert datetime to string. For brevity, trailing hours,minutes,
        /// seconds,milliseconds that are zero are ignored in the output. 
        /// </summary>
        /// <param name="dt"></param>
        /// <returns></returns>
        private static string ToString(DateTime dt)
        {
            if (dt.Millisecond != 0) return dt.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
            if (dt.Second != 0) return dt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
            if (dt.Hour != 0 || dt.Minute != 0) return dt.ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
            return dt.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        /// <summary>
        /// Safely split quoted comma-delimited strings. Two adjcent double-quotes 
        /// is considered a single literal double-quote that is part of the string.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        private static string[] QuotedSplit(string s)
        {
            var sb = new StringBuilder();
            var e = new List<string>();
            bool quoted = false;
            int i=0;
            Func<char> Peek = () => i < (s.Length-1) ? s[i+1] : ',';

            for(i=0; i<s.Length; i++)
            {
                var c = s[i];
                if (c == '"' && Peek() == '"') { sb.Append(c); i++; continue; }
                if (c == '"') { quoted = !quoted; continue; }
                if (c == ',' && quoted) { sb.Append(c); continue; }
                if (c==',')
                {
                    e.Add(sb.ToString().Trim());
                    sb.Length = 0;
                    continue;
                }

                sb.Append(c); 
            }
            e.Add(sb.ToString().Trim());
            sb.Length = 0;
            return e.ToArray();
        }

        /// <summary>
        /// For brevity, remove trailing zeros after the decimal point.
        /// </summary>
        /// <param name="s">String representation of decimal, double, or float</param>
        /// <returns>Trimmed numeric string</returns>
        private static string TrimEndZeros(string s)
        {
            int dot = -1;
            int zero = -1;
            for (int i = s.Length - 1; i >= 0; i--)
            {
                char c = s[i];
                if (c == '0') { zero = i; continue; }
                if (c == '.') { dot = i; break; }
                if (c == 'e' || c == 'E') return s;
            }
            if (dot == -1) return s;
            if (zero == dot + 1) zero = dot;
            return s.Substring(0, zero);
        }

        /// <summary>
        /// Get the default value for all our known types
        /// </summary>
        /// <param name="t">Type to get the default value for</param>
        /// <param name="arrayLen">If type is an array or List, this will be the length/count of the array populated with the default values.</param>
        /// <returns>Instianted value</returns>
        private static object GetDefaultValue(Type t, int arrayLen = 0)
        {
            if (t.IsEnum) return 0;
            if (t == typeof(string)) return string.Empty;
            if (t == typeof(DateTime)) return new DateTime(1900, 1, 1);
            if (t == typeof(DateTimeOffset)) return new DateTimeOffset(1900, 1, 1, 0, 0, 0, TimeSpan.Zero);
            if (t == typeof(Version)) return new Version(0, 0, 0, 0);
            if (t == typeof(IPAddress)) return new IPAddress(0);
            if (t.IsValueType) return Activator.CreateInstance(t);  //All value types have default constructors. Reference types may or may not.
            if (t.IsArray) return Array.CreateInstance(t.GetElementType(), arrayLen);
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(List<>))
            {
                var list = (IList)Activator.CreateInstance(t, new object[] { arrayLen });
                if (arrayLen == 0) return list;
                var defalt = GetDefaultValue(t.GenericTypeArguments[0], 0);
                for (int i = 0; i < arrayLen; i++) list.Add(defalt);
                return list;
            }

            return t.IsAbstract ? null : t.GetConstructor(Type.EmptyTypes) == null ? FormatterServices.GetUninitializedObject(t) : Activator.CreateInstance(t);
        }

        /// <summary>
        /// Gets method for splitting an integer into 2 or 4 parts based upon source integer type.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="len">Must be 2 or 4</param>
        /// <returns></returns>
        private static Func<object, int[]> GetSplitMethod(Type t, int len)
        {
            if (t == typeof(long))
            {
                if (len == 2) return (v) => new[]
                {
                    (int)((long)v & 0xFFFF),
                    (int)((long)v>>16 & 0xFFFF)
                };
                if (len == 4) return (v) => new[]
                {
                    (int)((long)v & 0xFFFF),
                    (int)((long)v>>16 & 0xFFFF),
                    (int)((long)v>>32 & 0xFFFF),
                    (int)((long)v>>48)
                };
                return null;
            }
            if (t == typeof(ulong))
            {
                if (len == 2) return (v) => new[]
                {
                    (int)((ulong)v & 0xFFFF),
                    (int)((ulong)v>>16 & 0xFFFF)
                };
                if (len == 4) return (v) => new[]
                {
                    (int)((ulong)v & 0xFFFF),
                    (int)((ulong)v>>16 & 0xFFFF),
                    (int)((ulong)v>>32 & 0xFFFF),
                    (int)((ulong)v>>48)
                };
                return null;
            }

            if (t == typeof(int))
            {
                if (len == 2) return (v) => new[]
                {
                    (int)((int)v & 0xFFFF),
                    (int)((int)v >> 16)
                };
                if (len == 4) return (v) => new[]
                {
                    (int)((int)v & 0xFF),
                    (int)((int)v>>8 & 0xFF),
                    (int)((int)v>>16 & 0xFF),
                    (int)((int)v>>24)
                };
                return null;
            }
            if (t == typeof(uint))
            {
                if (len == 2) return (v) => new[]
                {
                    (int)((uint)v & 0xFFFF),
                    (int)((uint)v >> 16)
                };
                if (len == 4) return (v) => new[]
                {
                    (int)((uint)v & 0xFF),
                    (int)((uint)v>>8 & 0xFF),
                    (int)((uint)v>>16 & 0xFF),
                    (int)((uint)v>>24)
                };
                return null;
            }

            if (t == typeof(short))
            {
                if (len == 2) return (v) => new[]
                {
                    (int)((short)v & 0xFF),
                    (int)((short)v >> 8)
                };
                return null;
            }
            if (t == typeof(ushort))
            {
                if (len == 2) return (v) => new[]
                {
                    (int)((ushort)v & 0xFF),
                    (int)((ushort)v >> 8)
                };
                return null;
            }
            return null;
        }

        /// <summary>
        /// Gets method for joining 2 integers into a single long, long, int, uint, short, or ushort integer.
        /// High bytes of source integers may be truncated in order to fit into target integer.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Func<int, int, object> Get2ElementJoinMethod(Type t)
        {
            if (t == typeof(long))
            {
                return (v1, v2) => (long)((v1 & 0xFFFFL) | ((v2 & 0xFFFFL) << 16));
            }
            if (t == typeof(ulong))
            {
                return (v1, v2) => (ulong)((v1 & 0xFFFFL) | ((v2 & 0xFFFFL) << 16));
            }

            if (t == typeof(int))
            {
                return (v1, v2) => (int)((v1 & 0xFFFF) | ((v2 & 0xFFFF) << 16));
            }
            if (t == typeof(uint))
            {
                return (v1, v2) => (uint)((v1 & 0xFFFF) | ((v2 & 0xFFFF) << 16));
            }

            if (t == typeof(short))
            {
                return (v1, v2) => (short)((v1 & 0xFF) | ((v2 & 0xFF) << 8));
            }
            if (t == typeof(ushort))
            {
                return (v1, v2) => (ushort)((v1 & 0xFF) | ((v2 & 0xFF) << 8));
            }
            return null;
        }

        /// <summary>
        /// Gets method for joining 4 integers into a single long, long, int, or uint integer.
        /// High bytes of source integers may be truncated in order to fit into target integer.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private static Func<int, int, int, int, object> Get4ElementJoinMethod(Type t)
        {
            if (t == typeof(long))
            {
                return (v1, v2, v3, v4) => (long)(((long)v1 & 0xFFFFL) | (((long)v2 & 0xFFFFL) << 16) | (((long)v3 & 0xFFFFL) << 32) | ((long)v4 << 48));
            }
            if (t == typeof(ulong))
            {
                return (v1, v2, v3, v4) => (ulong)(((long)v1 & 0xFFFFL) | (((long)v2 & 0xFFFFL) << 16) | (((long)v3 & 0xFFFFL) << 32) | ((long)v4 << 48));
            }

            if (t == typeof(int))
            {
                return (v1, v2, v3, v4) => (int)((v1 & 0xFF) | ((v2 & 0xFF) << 8) | ((v3 & 0xFF) << 16) | (v4 << 24));
            }
            if (t == typeof(uint))
            {
                return (v1, v2, v3, v4) => (uint)((v1 & 0xFF) | ((v2 & 0xFF) << 8) | ((v3 & 0xFF) << 16) | (v4 << 24));
            }
            return null;
        }

        /// <summary>
        /// Exception logger when throwOnError is false and default values are returned.
        /// The caller's logger runs asynchronously, in order to not block the converter.
        /// Use TypeDescriptor.GetFriendlyTypeName(Type) to translate type into readable name.
        /// </summary>
        /// <param name="from">Source type to convert value from.</param>
        /// <param name="to">Destination type to convert value to.</param>
        /// <param name="value">Value that failed to be converted.</param>
        /// <param name="ex">Exception that was caught.</param>
        private static void LogMsg(Type from, Type to, object value, Exception ex)
        {
            if (Logger == null) return; //No log handler was provided by user, so no logging...
            try
            {
                System.Threading.ThreadPool.QueueUserWorkItem(p=>Logger(DateTime.Now, from, to, value, ex));
            }
            catch
            {
                Logger = null; //Logger itself threw an error, so disable it.
            }
        }

        #endregion //Private Helper Methods
    }

    public class ConverterTesting
    {
        //constant suffixes: long = L, ulong = UL, decimal = M, uint = U, float = F, double = D
        //primitive types: Boolean, Byte, SByte, Char, Int16, UInt16, Int32, UInt32, Int64, UInt64, IntPtr, UIntPtr, Double, and Single.
        //value types: (primitive types), Nullable<>, Decimal, DateTime, DateTimeOffset, TimeSpan, Point, PointF, Rectangle, RectangleF, Size, SizeF, Enum
        //ref types: Array[], List<>, String, Version, IPAddress
        #region private static readonly List<Type> KnownTypes = new List<Type>(){ typeof(...), ... };
        public static readonly List<Type> KnownTypes = new List<Type>()
        {
            //t.IsValueType == true && t.IsPrimitive == true
            typeof(bool),
            typeof(byte),
            typeof(sbyte),
            typeof(char),
            typeof(decimal),
            typeof(double),
            typeof(float),
            typeof(int),
            typeof(uint),
            typeof(long),
            typeof(ulong),
            typeof(short),
            typeof(ushort),
            typeof(string),
            typeof(IntPtr),
            typeof(UIntPtr),

            //t.IsValueType == true && t.IsPrimitive == false
            typeof(bool?),
            typeof(byte?),
            typeof(sbyte?),
            typeof(char?),
            typeof(decimal?),
            typeof(double?),
            typeof(float?),
            typeof(int?),
            typeof(uint?),
            typeof(long?),
            typeof(ulong?),
            typeof(short?),
            typeof(ushort?),

            //t.IsValueType == false && t.IsArray == true
            typeof(bool[]),
            typeof(byte[]),
            typeof(sbyte[]),
            typeof(char[]),
            typeof(decimal[]),
            typeof(double[]),
            typeof(float[]),
            typeof(int[]),
            typeof(uint[]),
            typeof(long[]),
            typeof(ulong[]),
            typeof(short[]),
            typeof(ushort[]),
            typeof(string[]),

            //t.IsValueType == false && t.IsArray == false
            typeof(List<bool>),
            typeof(List<byte>),
            typeof(List<sbyte>),
            typeof(List<char>),
            typeof(List<decimal>),
            typeof(List<double>),
            typeof(List<float>),
            typeof(List<int>),
            typeof(List<uint>),
            typeof(List<long>),
            typeof(List<ulong>),
            typeof(List<short>),
            typeof(List<ushort>),
            typeof(List<string>),

            //t.IsValueType == false
            typeof(Type),
            typeof(Version),
            typeof(IPAddress),
            //typeof(Enum),  --Not a real castable type. use type.IsEnum;
            //typeof(System.RunTimeType), --Non-public type treated identically as 'Type'

            //t.IsValueType == true && t.IsPrimitive = false
            typeof(DateTime),
            typeof(DateTimeOffset),
            typeof(TimeSpan),
            typeof(DBNull),
            typeof(Point),
            typeof(PointF),
            typeof(Rectangle),
            typeof(RectangleF),
            typeof(Size),
            typeof(SizeF)
        };
        #endregion

        /// <summary>
        /// Returns table matrix, in CSV format, of all the possible type conversions.
        /// Where the stringized bit-wise flags are:
        ///      1 == identity, aka conversion to itself.
        ///     10 == our own converter
        ///    100 == System.ComponentModel.TypeDescriptor 'from'
        ///   1000 == System.ComponentModel.TypeDescriptor 'to'
        /// </summary>
        /// <returns></returns>
        public static string GetConverterMatrix()
        {
            var sb = new StringBuilder();
            sb.Append("               DEST - TO\nSOURCE - FROM,");
            foreach (var srcType in KnownTypes)
            {
                sb.Append(TypeConverter.GetFriendlyTypeName(srcType));
                sb.Append(',');
            }
            sb.Length--;
            sb.AppendLine();


            //Each row is Source-From and each column is Destination-To
            foreach (var srcType in KnownTypes)
            {
                sb.Append(TypeConverter.GetFriendlyTypeName(srcType));
                sb.Append(',');
                foreach (var dstType in KnownTypes)
                {
                    sb.Append(GetConverterMatrix(srcType, dstType));
                    sb.Append(',');
                }
                sb.Length--;
                sb.AppendLine();
            }

            return sb.ToString();
        }

        private static int GetConverterMatrix(Type from, Type to)
        {
            if (from == to) return 1;
            int status = 0;

            try
            {
                if (TypeConverter.InternalGetKnownConverter(from, to, StringProcessing.None, true) != null) status += 10;
            }
            catch { }

            var converter = TypeDescriptor.GetConverter(from);
            if (converter.CanConvertTo(to)) status += 100;

            converter = TypeDescriptor.GetConverter(to);
            if (converter.CanConvertFrom(from)) status += 1000;

            return status;
        }
    }
}
