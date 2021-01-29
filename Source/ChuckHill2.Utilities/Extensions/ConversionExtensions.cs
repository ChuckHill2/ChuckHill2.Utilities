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
using System.IO;
using System.Linq;
using System.Text;

namespace ChuckHill2.Extensions
{
    public static class ConversionExtensions
    {
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
        /// specified default value instead.
        /// </summary>
        /// <typeparam name="T">Type of object to convert to</typeparam>
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
        public static T CastTo<T>(this object value, T defalt)
        {
            return (T)Cast.To(typeof(T), value, defalt);
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
        public static IEnumerable<T> JsonToModels<T>(this TextReader textReader) where T : class, new()
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
        /// Convert enumerable array of data models into a multi-line CSV string.
        /// </summary>
        /// <param name="array">Enumerable array of data models</param>
        /// <returns>A multi-line CSV string</returns>
        /// <seealso cref="ToCSV(this IEnumerable items, TextWriter textwriter)"/>
        public static string ToCSV(this IEnumerable array)
        {
            using (var ms = new MemoryStream())
            {
                using (var sw = new StreamWriter(ms))
                {
                    ToCSV(array, sw);
                    sw.Flush();
                    ms.Position = 0;
                    using (var sr = new StreamReader(ms))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
        }

        /// <summary>
        /// Serializes a single enumerable object into a single CSV document within the 
        /// specified Stream. There are NO limits to the number of records written to the 
        /// stream. There is no caching or buffering. Individual values are formatted and 
        /// written to the stream immediately.
        /// </summary>
        /// <param name="items">Enumerable list of items to write.</param>
        /// <param name="textwriter">
        ///   Open stream to write to. Note: stream is not closed and stream pointer is not
        ///   reset to beginning in order to potentially perform further processing.
        /// </param>
        /// <remarks>
        /// • Writes columns in the same order as declared in the data model.
        /// • Does not perform any custom formatting or language translation.
        /// • <see cref="https://github.com/ChuckHill2/CsvExcelExportImport"/> for a mor comprehensive conversion.
        /// </remarks>
        public static void ToCSV(this IEnumerable items, TextWriter textwriter)
        {
            var properties = GetProperties(GetElementType(items));

            using (var writer = new CsvWriter(textwriter))
            {
                // Write header record
                foreach (var p in properties)
                {
                    writer.WriteField(p.Name);
                }

                writer.WriteEOL();

                // Write records
                foreach (var item in items)
                {
                    foreach (var p in properties)
                    {
                        writer.WriteField(p.GetValue(item));
                    }

                    writer.WriteEOL();
                }
            }
        }

        /// <summary>
        /// Get enumerable text objects beginning at the current position in the stream.
        /// </summary>
        /// <typeparam name="T">Type of class objects to read into.</typeparam>
        /// <param name="textReader">Stream to read. Must contain at least 2 rows and 2 columns.</param>
        /// <returns>
        ///   Enumerable list of class object. DO NOT close the stream until after the
        ///   enumerable list has been evaluated.
        /// </returns>
        /// <remarks>
        /// • The column headings in the CSV stream must be of the same names as the model property name. Mismatched names are ignored.
        /// • The CSV column order is not important.
        /// • Does not perform any language translation.
        /// • CSV values that cannot be converted to the model column type will default to the default value for that data type..
        /// • <see cref="https://github.com/ChuckHill2/CsvExcelExportImport"/> for a mor comprehensive conversion.
        /// </remarks>
        public static IEnumerable<T> CsvToModels<T>(this TextReader textReader)
        {
            var t = typeof(T);
            var properties = GetProperties(t);
            using (var reader = new CsvReader(textReader))
            {
                var headers = reader.ReadRecord();
                properties = SyncWithHeaders(properties, headers);
                while (!reader.EndOfFile)
                {
                    var index = 0;
                    var nuClass = (T)Activator.CreateInstance(t);
                    foreach (var field in reader.ReadField())
                    {
                        var pa = properties[index];
                        // CSV cannot detect difference between "" and null, so we opt for null to support nullable types.
                        pa.SetValue(nuClass, field.Length == 0 ? null : field);
                        index++;
                    }

                    if (index > 0)
                    {
                        yield return nuClass;
                    }
                }

                yield break;
            }
        }

        /// <summary>
        /// Convert CSV formatted string into an enumerable array of data models.
        /// </summary>
        /// <typeparam name="T">Type of class objects to  write into.</typeparam>
        /// <param name="csvString">A multi-line CSV string. Must have at least 2 rows and 2 columns.</param>
        /// <returns>
        ///  An enumerable list of class objects. 
        /// </returns>
        /// <remarks>
        /// • The column headings in the CSV stream must be of the same names as the model property name. Mismatched names are ignored.
        /// • The CSV column order is not important.
        /// • Does not perform any language translation.
        /// • CSV values that cannot be converted to the model column type will default to the default value for that data type..
        /// • <see cref="https://github.com/ChuckHill2/CsvExcelExportImport"/> for a mor comprehensive conversion.
        /// </remarks>
        public static IEnumerable<T> CsvToModels<T>(this string csvString)
        {
            using (var sr = new StringReader(csvString))
            {
                foreach(T v in sr.CsvToModels<T>())
                {
                    yield return v;
                }
            }
            yield break;
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
                    firstRow = true;
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

        /// <summary>
        /// Infer element type from an anonymous non-generic Enumerable object WITHOUT evaluating the enumerable object.
        /// </summary>
        /// <param name="enumerable">An anonymous non-generic Enumerable object</param>
        /// <returns>Type of items in enumerable array.</returns>
        /// <exception cref="System.InvalidDataException">Cannot determine underlying type of the enumerable object.</exception>
        private static Type GetElementType(IEnumerable enumerable)
        {
            Type[] interfaces = enumerable.GetType().GetInterfaces();
            Type elementType = (from i in interfaces
                where i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                select i.GetGenericArguments()[0]).FirstOrDefault();

            // Peek at the first element in the list if we couldn't determine the element type.
            if (elementType == null || elementType == typeof(object))
            {
                throw new InvalidDataException($"Cannot determine underlying type of the enumerable object.");
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
            //The minimum necessary member property properties needed to read and write the values.
            public string Name { get; set; }
            public Type Type { get; set; }
            public Func<object, object> GetValue { get; set; }
            public Action<object, object> SetValue { get; set; }

            public override string ToString() => $"{Name}, {Type.Name}"; //for debugging
        }
    }
}
