using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace ChuckHill2.Utilities.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Handy way to get an manifest resource stream from an assembly that 'type' resides in.
        /// This will not access Project or Form resources (e.g. *.resources).
        /// </summary>
        /// <param name="t">Type whose assembly contains the manifest resources to search.</param>
        /// <param name="name">The unique trailing part of resource name to search. Generally the filename.ext part.</param>
        /// <returns>Found resource stream or null if not found. It's up to the caller to load it into the appropriate object. Generally Image.FromStream(s)</returns>
        /// <remarks>
        /// See ImageAttribute regarding access of any image resource from anywhere.
        /// </remarks>
        public static Stream GetManifestResourceStream(this Type t, string name) => t.Assembly.GetManifestResourceStream(t.Assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(name, StringComparison.OrdinalIgnoreCase)) ?? "NULL");
    }

    public static class IntExtensions
    {
        /// <summary>
        /// Convert a byte count numeric value into a formatted string with units. 
        /// AKA formatted as bytes, kilobytes, megabytes, gigabytes, terabytes.
        /// </summary>
        /// <param name="i">positive or negative numeric value to convert. Any digits after the decimal are rounded off.</param>
        /// <param name="precision">Number of digits after the decimal place. Default==0</param>
        /// <returns>formatted string</returns>
        public static string ToCapacityString<T>(this T i, int precision = 0) where T: struct, IComparable, IFormattable, IConvertible
        {
            var d = Math.Round(Convert.ToDecimal(i)); //may be negative
            var v = Math.Abs(d);

            if (v < 1024) return d.ToString("0 B");
            string szPrecision = precision <= 0 ? "0" : "0.".PadRight(precision + 2, '#');
            if (v < (1024 * 1024)) return (d / (1024.0m)).ToString(szPrecision + " KB");
            if (v < (1024 * 1024 * 1024)) return (d / (1024 * 1024.0m)).ToString(szPrecision + " MB");
            if (v < (1024 * 1024 * 1024 * 1024m)) return (d / (1024 * 1024 * 1024.0m)).ToString(szPrecision + " GB");
            if (v < (1024 * 1024 * 1024 * 1024m * 1024m)) return (d / (1024 * 1024 * 1024.0m * 1024.0m)).ToString(szPrecision + " TB");
            return v.ToString();  //shouldn't get here.
        }
    }

    public static class StringExtensions
    {
        /// <summary>
        /// For SQL, C# and C++ strings, perform the following:
        /// <list type="bullit">
        /// <item>Replace all tabs with 2 spaces</item>
        /// <item>Remove trailing whitespace on each line</item>
        /// <item>Squeeze out multiple newlines</item>
        /// </list>
        /// </summary>
        /// <returns>fixed up string</returns>
        public static string Beautify(this string s) { return Beautify(s, false, null); }
        /// <summary>
        /// For SQL, C# and C++ strings, perform the following:
        /// <list type="bullit">
        /// <item>Replace all tabs with 2 spaces</item>
        /// <item>Remove trailing whitespace on each line</item>
        /// <item>Squeeze out multiple newlines</item>
        /// </list>
        /// Optionally perform the following:
        /// <list type="bullit">
        /// <item>Indent all lines with this string (usually whitespace chars)</item>
        /// </list>
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <param name="indent">string to indent each line with</param>
        /// <returns>fixed up string</returns>
        public static string Beautify(this string s, string indent) { return Beautify(s, false, indent); }
        /// <summary>
        /// For SQL, C# and C++ strings, perform the following:
        /// <list type="bullit">
        /// <item>Replace all tabs with 2 spaces</item>
        /// <item>Remove trailing whitespace on each line</item>
        /// <item>Squeeze out multiple newlines</item>
        /// </list>
        /// Optionally perform the following:
        /// <list type="bullit">
        /// <item>Strip SQL (e.g. --) and/or C comments (e.g. /**/)</item>
        /// <item>Indent all lines with this string (usually whitespace chars)</item>
        /// </list>
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <param name="stripComments">True to strip comments</param>
        /// <param name="indent">string to indent each line with</param>
        /// <returns>fixed up string</returns>
        public static string Beautify(this string s, bool stripComments, string indent)
        {
            if (stripComments)
            {
                s = Regex.Replace(s, @"^[ \t]*(--|//).*?\r\n", "", RegexOptions.Multiline); //remove whole line sql or c++ comments
                s = Regex.Replace(s, @"[ \t]*(--|//).*?$", "", RegexOptions.Multiline); //remove trailing sql or c++ comments
                s = Regex.Replace(s, @"\r\n([ \t]*/\*.*?\*/[ \t]*\r\n)+", "\r\n", RegexOptions.Singleline); //remove whole line c-like comments
                s = Regex.Replace(s, @"[ \t]*/\*.*?\*/[ \t]*", "", RegexOptions.Singleline); //remove trailing c-like comments
            }

            s = s.Trim().Replace("\t", "  "); //replace tabs with 2 spaces
            s = Regex.Replace(s, @" +$", "", RegexOptions.Multiline); //remove trailing whitespace
            s = Regex.Replace(s, "(\r\n){2,}", "\r\n"); //squeeze out multiple newlines
            if (!string.IsNullOrEmpty(indent)) s = Regex.Replace(s, @"^(.*)$", indent + "$1", RegexOptions.Multiline);  //indent
            return s;
        }

        /// <summary>
        /// Strip one or more whitspace chars (including newlines) and replace with a single space char.
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <returns>The squeezed single-line string</returns>
        public static string Squeeze(this string s)
        {
            if (s.IsNullOrEmpty()) return string.Empty;
            //This is 2.6x faster than ""return Regex.Replace(s.Trim(), "[\r\n \t]+", " ");""
            StringBuilder sb = new StringBuilder(s.Length);
            char prev = ' ';
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c > 0 && c < 32) c = ' ';
                if (prev == ' ' && prev == c) continue;
                prev = c;
                sb.Append(c);
            }
            if (prev == ' ') sb.Length = sb.Length - 1;
            return sb.ToString();
        }
        /// <summary>
        /// Squeeze out multiple adjcent specified chars and replace with a single replacement char.
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <param name="chars2Remove">Array of chars to remove</param>
        /// <param name="replaceChar">The single replacement char to use in place of the multiply removed chars. If undefined, the removed chars are squeezed out completely.</param>
        /// <returns>The squeezed string</returns>
        public static string Squeeze(this string s, IList<char> chars2Remove, char replaceChar = '\0')
        {
            if (s.IsNullOrEmpty()) return string.Empty;
            StringBuilder sb = new StringBuilder(s.Length);
            char prev = replaceChar;
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (chars2Remove.Any(m => m == c)) c = replaceChar;
                if (prev == replaceChar && prev == c) continue;
                prev = c;
                if (sb.Length == 0 && c == replaceChar) continue;
                if (c != '\0') sb.Append(c);
            }
            if (prev != '\0' && prev == replaceChar) sb.Length = sb.Length - 1;
            return sb.ToString();
        }

        /// <summary>
        /// Test if a string variable is null or string value is a zero length string after all leading and trailing whitespace has been removed.
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <returns>True or False</returns>
        public static bool IsNullOrEmpty(this string s) { return string.IsNullOrWhiteSpace(s); }

        /// <summary>
        /// List of all known whitespace characters
        /// </summary>
        public static readonly char[] WhiteSpace = new char[]{
            '\xFEFF', '\xFFFE', //UTF-8 Byte order marks. .NET 3.5 used to consider these whitespace, but 4.0 does not!
            '\0', '\a', '\b', '\f', '\n', '\r', '\t', '\v', ' ', '\xA0', //ASCII whitespace
            '\x1680', '\x2000', '\x2001', '\x2002', '\x2003', '\x2004',  //Unicode Whitespace
            '\x2005', '\x2006', '\x2007', '\x2008', '\x2009', '\x200A',
            '\x202F', '\x205F', '\x3000' };

        /// <summary>
        /// Safe string formatter. Similar to String.Format() except it will NEVER throw an error. Internally if an 
        /// ArgumentNullException or FormatException exception occurs, the returned string will simply be a 
        /// comma-delimited list of the format string and its arguments.
        /// </summary>
        /// <param name="format">A composite format string.</param>
        /// <param name="args">An System.Object array containing zero or more objects to format.</param>
        /// <returns>Resulting formatted string</returns>
        public static string Format(this string format, params object[] args)
        {
            //This is needed if the caller screws up the format string.
            try
            {
                //nothing to do!
                if (format.IsNullOrEmpty()) return format;
                //if string contains "{0}", but arg list is empty, just return the string.
                if (args == null || args.Length == 0) return format;
                //If the format arg is null, say so!
                for (int i = 0; i < args.Length; i++) { if (args[i] == null) args[i] = "null"; }
                return string.Format(format, args);
            }
            catch
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("(string format error) \"{0}\"", format);
                for (int i = 0; i < args.Length; i++)
                {
                    sb.AppendFormat(", \"{0}\"", args[i].ToString());
                }
                return sb.ToString();
            }
        }

        /// <summary>
        ///  Safely removes all leading and trailing occurrences of a set of characters specified
        ///  in an array from the current System.String object. Will not throw an error, even if the string is null.
        /// </summary>
        /// <param name="s">String to trim</param>
        /// <param name="trimChars">An array of Unicode characters to remove.</param>
        /// <returns>
        /// The string that remains after all occurrences of the characters in the trimChars
        /// parameter are removed from the start and end of the current System.String
        /// object. If trimChars is null, standard white-space characters are removed instead.
        /// </returns>
        /// <remarks>
        /// String is automatically truncated at '\0' char within the string AFTER the leading and trailing trimChars are removed.
        /// This may occur when using strings via pInvoke.
        /// </remarks>
        public static string TrimEx(this string s, params char[] trimChars)
        {
            if (s == null) return s;
            if (s.Length == 0) return s;
            var sb = new StringBuilder(s.Length); //string will always be <= s.Length
            foreach (char c in s)
            {
                if (c == '\0') break;
                sb.Append(c);
            }
            return sb.ToString().Trim(trimChars);
        }
        /// <summary>
        ///  Safely removes all leading and trailing white-space characters from the current System.String object.
        ///  Will not throw an error, even if the string is null.
        /// </summary>
        /// <param name="s">String to trim</param>
        /// <returns>
        /// The string that remains after all white-space characters are removed from the start and end of the current System.String object.
        /// </returns>
        /// <remarks>
        /// String is automatically truncated at '\0' char within the string AFTER the leading and trailing trimChars are removed.
        /// This may occur when using strings via pInvoke.
        /// </remarks>
        public static string TrimEx(this string s) { return s.TrimEx(StringExtensions.WhiteSpace); }

        /// <summary>
        ///  Safely removes all leading white-space characters from the current System.String object.
        ///  Will not throw an error, even if the string is null.
        /// </summary>
        /// <param name="s">String to trim</param>
        /// <returns>
        /// The string that remains after all white-space characters are removed from the start of the current System.String object.
        /// </returns>
        public static string TrimStartEx(this string s) { return (s == null ? s : s.TrimStart(StringExtensions.WhiteSpace)); }

        /// <summary>
        ///  Safely removes all trailing white-space characters from the current System.String object.
        ///  Will not throw an error, even if the string is null.
        /// </summary>
        /// <param name="s">String to trim</param>
        /// <returns>
        /// The string that remains after all white-space characters are removed from the end of the current System.String object.
        /// </returns>
        public static string TrimEndEx(this string s) { return (s == null ? s : s.TrimEnd(StringExtensions.WhiteSpace)); }

        /// <summary>
        /// Returns a value indicating whether the specified string fragment occurs within this string. Safely handles null values
        /// </summary>
        /// <param name="s">String to search.</param>
        /// <param name="value">The string fragment to seek.</param>
        /// <param name="ignoreCase">True for case-insensitive search.</param>
        /// <returns>True if the value parameter occurs within this string or if both the string to search and the value to seek are both null.</returns>
        public static bool Contains(this string s, string value, bool ignoreCase)
        {
            if (s == null && value == null) return true;
            if (!ignoreCase) return (s != null && value != null && s.Contains(value));
            return (s != null && value != null && s.IndexOf(value, 0, StringComparison.OrdinalIgnoreCase) != -1);
        }

        /// <summary>
        /// Returns a value indicating whether the specified case-insensitive string fragment occurs within this string. Safely handles null values.
        /// </summary>
        /// <param name="s">String to search.</param>
        /// <param name="value">The case-insensitive System.String object to seek.</param>
        /// <returns>true if the value parameter occurs within this string or if both the string to search and the value to seek are both null.</returns>
        /// <remarks>Created for symmetry with EqualsI() below.</remarks>
        public static bool ContainsI(this string s, string value) => Contains(s, value, true);

        /// <summary>
        /// Returns a value indicating whether the specified case-insensitive string is equal to this string. Safely handles null values
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <param name="value">The case-insensitive string to compare.</param>
        /// <returns>true if the value parameter equals this string or if both the string to search and the value to seek are both null.</returns>
        public static bool EqualsI(this string s, string value)
        {
            if (s == null && value == null) return true;
            return (s != null && value != null && s.Equals(value, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        /// Remove any of the listed chars from the string.
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <param name="removeChars">list of characters to remove from this string. If undefined, defaults to whitespace.</param>
        /// <returns>resultant string. If source string is null or empty, it is just returned.</returns>
        public static string Remove(this string s, params char[] removeChars)
        {
            if (s == null || s.Length == 0) return s;
            if (removeChars == null || removeChars.Length == 0) removeChars = new char[] { ' ', '\t', '\r', '\n', '\b', '\f', '\v', '\a' };
            var sb = new StringBuilder(s.Length);
            int i, len = removeChars.Length;
            bool found;
            foreach (char c in s)
            {
                for (found = false, i = 0; i < len; i++)
                {
                    if (removeChars[i] == c) { found = true; break; }
                }
                if (found) continue;
                sb.Append(c);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Convert any string into a C# identifier.
        /// </summary>
        /// <param name="s">String to convert</param>
        /// <returns>String that conforms to the C# identifier rules using camel-case. Null or empty strings are converted to "__".</returns>
        public static string ToIdentifier(this string s)
        {
            if (s.IsNullOrEmpty()) return "__";
            //Compliant with item 2.4.2 of the C# specification
            //squeeze out illegal chars and convert to camel-case
            s = Regex.Replace(s, @"[^\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Nd}\p{Nl}\p{Mn}\p{Mc}\p{Cf}\p{Pc}\p{Lm}]+.?",
                delegate (Match m) { return m.Index + m.Value.Length == s.Length ? string.Empty : char.ToUpperInvariant(m.Value[m.Value.Length - 1]).ToString(); });
            if (!char.IsLetter(s, 0)) //identifier must start with a letter or underscore.
                s = string.Concat("_", s);
            if (!Microsoft.CSharp.CSharpCodeProvider.CreateProvider("C#").IsValidIdentifier(s)) //identifier must not be a C# keyword
                s = string.Concat("@", s);
            if (s.Length > 511) s = s.Substring(0, 511); //error CS0645: Identifier too long
            return s;
        }

        /// <summary>
        /// Convert any enumerable object into an item-delimited string.
        /// </summary>
        /// <param name="list">Enumerable object to convert.</param>
        /// <param name="delimiter">String delimiter between items.</param>
        /// <param name="predicate">Optional method to convert enumerated item to a string. The default is item.ToString().</param>
        /// <returns>The generated string.</returns>
        public static string ToString(this IEnumerable list, string delimiter, Func<object, string> predicate = null)
        {
            delimiter = delimiter ?? ", ";
            if (predicate == null) predicate = (o) => o.ToString();
            var sb = new StringBuilder();
            foreach (var item in list)
            {
                sb.Append(predicate(item));
                sb.Append(delimiter);
            }

            if (sb.Length > 0) sb.Length -= delimiter.Length;
            return sb.ToString();
        }

        /// <summary>
        /// Convert a byte array of any encoding into a string.
        /// </summary>
        /// <param name="byteBuffer">Value to convert</param>
        /// <returns>Resulting string</returns>
        public static string ToStringEx(this byte[] byteBuffer)
        {
            if (byteBuffer == null) return null;
            int byteLen = byteBuffer.Length;
            if (byteLen == 0) return string.Empty;
            if (byteLen == 1) return Convert.ToString(byteBuffer[0]);
            Encoding encoding;
            int skipBOM = 0;

            if (byteBuffer[0] == 0xFE && byteBuffer[1] == 0xFF)
            {
                encoding = new UnicodeEncoding(true, true);
                skipBOM = 2;
            }
            else if (byteBuffer[0] == 0xFF && byteBuffer[1] == 0xFE)
            {
                if (byteLen < 4 || byteBuffer[2] != 0x00 || byteBuffer[3] != 0x00)
                {
                    encoding = new UnicodeEncoding(false, true);
                    skipBOM = 2;
                }
                else
                {
                    encoding = new UTF32Encoding(false, true);
                    skipBOM = 4;
                }
            }
            else if (byteLen >= 3 && byteBuffer[0] == 0xEF && (byteBuffer[1] == 0xBB && byteBuffer[2] == 0xBF))
            {
                encoding = Encoding.UTF8;
                skipBOM = 3;
            }
            else if (byteLen >= 4 && byteBuffer[0] == 0x00 && (byteBuffer[1] == 0x00 && byteBuffer[2] == 0xFE) && byteBuffer[3] == 0xFF)
            {
                encoding = new UTF32Encoding(true, true);
                skipBOM = 4;
            }
            else if (byteBuffer.All(b=>b < 0x80))
            {
                encoding = Encoding.ASCII;
                skipBOM = 0;
            }
            else
            {
                encoding = Encoding.UTF8;
                skipBOM = 0;
            }

            return encoding.GetString(byteBuffer, skipBOM, byteBuffer.Length - skipBOM);
        }

        /// <summary>
        /// Convert string into UTF8 encoded array of bytes.
        /// </summary>
        /// <param name="s">String to encode</param>
        /// <returns>Array of bytes.</returns>
        public static byte[] ToBytes(this string s)
        {
            if (s == null) return null;
            if (s == string.Empty) return new byte[0];
            return Encoding.UTF8.GetBytes(s);
        }

        /// <summary>
        /// Convert a stream encoded as UTF8 into a string
        /// </summary>
        /// <param name="stream">Stream to read. If possible, the stream position is always reset to zero</param>
        /// <returns>resulting string</returns>
        public static string ToStringEx(this Stream stream)
        {
            if (stream == null) return null;
            if (!stream.CanRead) return string.Empty;
            if (stream.CanSeek) stream.Seek(0, SeekOrigin.Begin);
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                int c = reader.Peek();
                if (c == '\xFEFF' || c == '\xFFFE') reader.Read(); //skip byte order marks
                return reader.ReadToEnd();
            }
        }

        /// <summary>
        /// Convert string into a MemoryStream.
        /// </summary>
        /// <param name="s">String to read</param>
        /// <returns>Open MemoryStream</returns>
        public static Stream ToStream(this string s)
        {
            if (s == null) return null;
            return new MemoryStream(Encoding.UTF8.GetBytes(s));
        }

        /// <summary>
        /// Detect if string is an XML string.
        /// </summary>
        /// <param name="s">String to inspect.</param>
        /// <returns>True if string is an XML string.</returns>
        public static bool IsXml(this string s)
        {
            if (s == null) return false;
            //Ignore UTF-8 BOM (byte order mark)
            int i = (s.Length > 0 && (s[0] == '\xFEFF' || s[0] == '\xFFFE')) ? 1 : 0;
            if (s.Length < (5 + i)) return false;
            // same as if (s.StartsWith(@"﻿<?xml")) but doesn't always work!
            return (s[i + 0] == '<' && s[i + 1] == '?' && s[i + 2] == 'x' && s[i + 3] == 'm' && s[i + 4] == 'l');
        }

        /// <summary>
        /// Detect if string is a filename. Filename does not have to exist.
        /// Useful for detecting if an overloaded striing could be a filename or something else.
        /// </summary>
        /// <param name="path">String to inspect,</param>
        /// <returns>True if string resembles a valid filename,</returns>
        public static bool IsFileName(this string path)
        {
            if (path.IsNullOrEmpty()) return false;
            if (path.Length > 260) return false; //260 is the max filename length in Microsoft Windows.
            if (path.IndexOfAny(System.IO.Path.GetInvalidPathChars()) >= 0) return false;
            try { Path.GetFullPath(path); } catch { return false; }
            return true;
        }

        /// <summary>
        /// Detect if string is a base64 encoded string.
        /// </summary>
        /// <param name="s">String to inspect.</param>
        /// <returns>True if string is a base64 string.</returns>
        public static bool IsBase64(this string s)
        {
            if (s == null || s.Length < 2 || ((s.Length * 3) % 4) == 3) return false;
            return (s.Length > 0 && s.All(c => ((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '+' || c == '/' || c == '=')));
        }

        /// <summary>
        /// Detect if string is a hexadecimal encoded string.
        /// </summary>
        /// <param name="s">String to inspect.</param>
        /// <returns>True if string is a hexadecimal string.</returns>
        public static bool IsHex(this string s)
        {
            if (s == null || s.Length < 2 || (s.Length % 2) == 1) return false; //length must be an even multiple of 2 but not zero.
            return (s.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f') || (c >= 'A' && c <= 'F')));
        }

        /// <summary>
        /// Detect if string is a numeric string (e.g. containing only numbers).
        /// </summary>
        /// <param name="s">String to inspect.</param>
        /// <returns>True if string is a numeric string.</returns>
        public static bool IsNumeric(this string s)
        {
            if (s == null) return false;
            return (s.Length > 0 && s.All(c => (c >= '0' && c <= '9')));
        }

        /// <summary>
        /// Detect if file content or string is CSV format.
        /// </summary>
        /// <param name="s">Filename path or string literal</param>
        /// <returns>True if string or filename is in CSV format</returns>
        public static bool IsCSV(this string s)
        {
            if (s.IsFileName())
            {
                if (!File.Exists(s)) return false;
                using (var fs = File.Open(s, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
                {
                    return fs.IsCSV();
                }
            }
            return s.ToStream().IsCSV();
        }

        /// <summary>
        /// Detect if stream content is CSV format.
        /// </summary>
        /// <param name="s">Stream to inspect.l</param>
        /// <returns>True if streame is in CSV format</returns>
        public static bool IsCSV(this Stream s)
        {
            if (s == null) return false;
            if (!s.CanSeek) return false;  //we can't seek!

            if (s.Length < 4) return false; //too small!
            long pos = s.Position;
            byte[] bytes = new byte[4096]; //4096==Win32 native minimum memory allocation block size.
            s.Read(bytes, 0, bytes.Length);
            s.Position = pos; //restore stream pointer position
            string line = null;
            int fieldCount;
            string[] lines = bytes.ToStringEx().Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < lines.Length; i++)
            {
                line = lines[i];
                //Note: Official CSV does not support any sort of comment delimiter. 
                //Although '#' comments are sometimes used in custom parsers, but comments are NOT supported by Excel.
                if (line.IsNullOrEmpty()) continue; //skip empty lines
                fieldCount = line.Split(',').Length;
                if (fieldCount > 1) return true; //must have at least 2 header fields
                //A valid CSV may contain only the header line.
                //if (fieldCount > 1) //must have at least 2 header fields
                //{
                //    for (int j = i + 1; i < lines.Length; j++) //data line fieldcount must equal header line fieldcount
                //    {
                //        line = lines[j];
                //        if (line.IsNullOrEmpty()) continue; //skip empty lines
                //        int fieldCount2 = line.Split(',').Length;
                //        if (fieldCount == fieldCount2) return true;
                //        break;
                //    }
                //}
                break;
            }
            return false;
        }

        /// <summary>
        /// Compute unique MD5 hash. Useful for creating hash dictionary.
        /// DO NOT USE for security encryption.
        /// </summary>
        /// <param name="str">Filename path or string literal</param>
        /// <returns>Guid</returns>
        public static Guid ToHash(this string str)
        {
            if (str.IsNullOrEmpty()) return Guid.Empty;
            Stream stream = null;
            try
            {
                if (str.IsFileName())
                {
                    if (!File.Exists(str)) return Guid.Empty;
                    stream = File.Open(str, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                }
                else
                {
                    stream = str.ToStream();
                }
                return stream.ToHash();
            }
            finally
            {
                if (stream != null) stream.Dispose();
            }
        }

        /// <summary>
        /// Compute unique MD5 hash. Useful for creating hash dictionary.
        /// DO NOT USE for security encryption.
        /// </summary>
        /// <param name="bytes">Source byte array</param>
        /// <returns>Guid</returns>
        public static Guid ToHash(this byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes))
            {
                return stream.ToHash();
            }
        }

        /// <summary>
        /// Compute unique MD5 hash. Useful for creating hash dictionary.
        /// DO NOT USE for security encryption.
        /// </summary>
        /// <param name="stream">Stream content to generate hash from.</param>
        /// <returns>Guid</returns>
        public static Guid ToHash(this Stream stream)
        {
            Guid hash = Guid.Empty;
            if (stream == null) return hash;
            if (!stream.CanRead) return hash;
            bool fipsCompliance = FIPSCompliance;
            using (var provider = new MD5CryptoServiceProvider()) { hash = new Guid(provider.ComputeHash(stream)); }
            if (fipsCompliance) FIPSCompliance = fipsCompliance;
            return hash;
        }

        /// <summary>
        /// Get or set FIPS compliance flag.
        /// A hacky way to allow non-FIPS compliant algorthms to run.
        /// Non-FIPS compliant algorthims are:
        ///     MD5CryptoServiceProvider,
        ///     RC2CryptoServiceProvider,
        ///     RijndaelManaged,
        ///     RIPEMD160Managed,
        ///     SHA1Managed,
        ///     SHA256Managed,
        ///     SHA384Managed,
        ///     SHA512Managed,
        ///     AesManaged,
        ///     MD5Cng. 
        /// In particular, enables use of fast MD5 hash to create unique identifiers for internal use.
        /// </summary>
        private static bool FIPSCompliance
        {
            get { return CryptoConfig.AllowOnlyFipsAlgorithms; }
            set
            {
                FieldInfo fi;
                fi = typeof(CryptoConfig).GetField("s_fipsAlgorithmPolicy", BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(null, value);
                fi = typeof(CryptoConfig).GetField("s_haveFipsAlgorithmPolicy", BindingFlags.Static | BindingFlags.NonPublic);
                if (fi != null) fi.SetValue(null, true);
            }
        }

        /// <summary>
        /// Convert byte array to a hexidecimal string
        /// </summary>
        /// <param name="bytes">Byte array to convert</param>
        /// <returns>converted string</returns>
        public static string ToHex(this byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return string.Empty;

            StringBuilder result = new StringBuilder(bytes.Length * 2);
            string hexAlphabet = "0123456789ABCDEF";

            foreach (byte b in bytes)
            {
                result.Append(hexAlphabet[(int)(b >> 4)]);
                result.Append(hexAlphabet[(int)(b & 0xF)]);
            }

            return result.ToString();
        }
        /// <summary>
        /// Convert hexidecimal string to byte array.
        /// Will throw an error if any character in string is not in range of [0-9-A-F].
        /// </summary>
        /// <param name="hex">Hexidecimal string to decode</param>
        /// <returns>byte array</returns>
        public static byte[] FromHex(this string hex)
        {
            if (hex == null || hex.Length < 2) return new byte[0];

            byte[] bytes = new byte[hex.Length / 2];
            int[] hexValue = new int[] { 0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F };

            for (int x = 0, i = 0; i < hex.Length; i += 2, x += 1)
            {
                bytes[x] = (byte)(hexValue[Char.ToUpper(hex[i + 0]) - '0'] << 4 |
                                  hexValue[Char.ToUpper(hex[i + 1]) - '0']);
            }
            return bytes;
        }
    }

    public static class DictionaryExtensions
    {
        /// <summary>
        /// Convert any IEnumerable source into a dictionary.
        /// Note: System.Linq.ToDictionary(...) only supports generic IEnumerable. This also supports old-style non-generic IEnumerable.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector">delegate to return the dictionary key from the enumerated element</param>
        /// <param name="capacity">Set the initial size of the resulting dictionary. Should be greater than or equal to the number of elements in the source otherwise it will auto-expand to support all elements.</param>
        /// <param name="comparer">Key equality comparer</param>
        /// <returns></returns>
        public static Dictionary<TKey, TSource> ToDictionary<TKey, TSource>(this IEnumerable source, Func<TSource, TKey> keySelector, int capacity, IEqualityComparer<TKey> comparer)
        {
            Dictionary<TKey, TSource> d = new Dictionary<TKey, TSource>(capacity, comparer);
            foreach (TSource p in source) { d.Add(keySelector(p), p); }
            return d;
        }
        /// <summary>
        /// Convert any IEnumerable source into a dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector">delegate to return the dictionary key from the enumerated element</param>
        /// <returns></returns>
        public static Dictionary<TKey, TSource> ToDictionary<TKey, TSource>(this IEnumerable source, Func<TSource, TKey> keySelector)
        {
            return ToDictionary<TKey, TSource>(source, keySelector, 0, null);
        }
        /// <summary>
        /// Convert any IEnumerable source into a dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector">delegate to return the dictionary key from the enumerated element</param>
        /// <param name="capacity">Set the initial size of the resulting dictionary. Should be greater than or equal to the number of elements in the source otherwise it will auto-expand to support all elements.</param>
        /// <returns></returns>
        public static Dictionary<TKey, TSource> ToDictionary<TKey, TSource>(this IEnumerable source, Func<TSource, TKey> keySelector, int capacity)
        {
            return ToDictionary<TKey, TSource>(source, keySelector, capacity, null);
        }
        /// <summary>
        /// Convert any IEnumerable source into a dictionary.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TSource"></typeparam>
        /// <param name="source"></param>
        /// <param name="keySelector">delegate to return the dictionary key from the enumerated element</param>
        /// <param name="comparer">Key equality comparer</param>
        /// <returns></returns>
        public static Dictionary<TKey, TSource> ToDictionary<TKey, TSource>(this IEnumerable source, Func<TSource, TKey> keySelector, IEqualityComparer<TKey> comparer)
        {
            return ToDictionary<TKey, TSource>(source, keySelector, 0, comparer);
        }

        /// <summary>
        /// Deserialize a formatted string into a string dictionary. Must be of the form "key=value,key=value,...".
        /// Note that the string keys are case-insensitive.
        /// Warning: If a key or value contains one of the delimiters, the results are undefined.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this string s)
        {
            return ToDictionary<string, string>(s, ',', '=', k => k, v => v, StringComparer.InvariantCultureIgnoreCase);
        }
        /// <summary>
        /// Deserialize a formatted string into a string dictionary.
        /// Note that the string keys are case-insensitive.
        /// Warning: If a key or value contains one of the delimiters, the results are undefined.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="elementDelimiter">character used between each keyvalue element.</param>
        /// <param name="kvDelimiter">character used between the key and value pairs.</param>
        /// <returns></returns>
        public static Dictionary<string, string> ToDictionary(this string s, char elementDelimiter, char kvDelimiter)
        {
            return ToDictionary<string, string>(s, elementDelimiter, kvDelimiter, k => k, v => v, StringComparer.InvariantCultureIgnoreCase);
        }
        /// <summary>
        /// Deserialize a formatted string into a typed dictionary.
        /// Warning: If a key or value contains one of the delimiters, the results are undefined.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="s"></param>
        /// <param name="elementDelimiter">character used between each keyvalue element.</param>
        /// <param name="kvDelimiter">character used between the key and value pairs.</param>
        /// <param name="keyConverter">delegate used to deserialize key string into the TKey type</param>
        /// <param name="valueConverter">delegate used to deserialize value string into the TValue type</param>
        /// <param name="comparer">Key equality comparer</param>
        /// <returns></returns>
        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this string s, char elementDelimiter, char kvDelimiter, Func<string, TKey> keyConverter, Func<string, TValue> valueConverter, IEqualityComparer<TKey> comparer)
        {
            if (s == null || s.Length == 0) return new Dictionary<TKey, TValue>(0, comparer);
            string[] array = s.Split(new Char[] { elementDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            Dictionary<TKey, TValue> d = new Dictionary<TKey, TValue>(array.Length, comparer);
            TValue defalt = default(TValue);

            foreach (string item in array)
            {
                string[] kv = item.Split(new Char[] { kvDelimiter }, StringSplitOptions.RemoveEmptyEntries);
                d[keyConverter(kv[0].Trim())] = kv.Length > 1 ? valueConverter(kv[1].Trim()) : defalt;
            }
            return d;
        }

        /// <summary>
        /// Safely gets the value associated with the specified key. 
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="dict"></param>
        /// <param name="key">The key whose value to get.</param>
        /// <returns>the value for the associated key or the default value if it doesn't exist (null for reference types, the default for value types)</returns>
        public static TValue GetValue<TKey, TValue>(this IDictionary<TKey, TValue> dict, TKey key)
        {
            lock (dict)
            {
                TValue value;
                if (dict == null || key == null) return default(TValue); //to avoid exception when key==null
                if (dict.TryGetValue(key, out value)) return value;
                return default(TValue);
            }
        }
    }

    public static class ListExtensions
    {
        /// <summary>
        /// Deserializes a formatted comma-delimited string into a list of strings.
        /// Warning: If a value a delimiter, the results are undefined.
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static List<string> ToList(this string s) { return ToList<string>(s, ',', v => v); }
        /// <summary>
        /// Deserializes a formatted delimited string into a list of strings.
        /// Warning: If a value a delimiter, the results are undefined.
        /// </summary>
        /// <param name="s"></param>
        /// <param name="elementDelimiter">character used between each element.</param>
        /// <returns></returns>
        public static List<string> ToList(this string s, char elementDelimiter) { return ToList<string>(s, elementDelimiter, v => v); }
        /// <summary>
        /// Deserializes a formatted delimited string into a typed list.
        /// Warning: If a value a delimiter, the results are undefined.
        /// </summary>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="s"></param>
        /// <param name="elementDelimiter">character used between each element.</param>
        /// <param name="valueConverter">delegate used to deserialize the value element into the TValue type</param>
        /// <returns></returns>
        public static List<TValue> ToList<TValue>(this string s, char elementDelimiter, Func<string, TValue> valueConverter)
        {
            string[] array = s.Split(new Char[] { elementDelimiter }, StringSplitOptions.RemoveEmptyEntries);
            List<TValue> list = new List<TValue>(array.Length);
            foreach (string item in array) { list.Add(valueConverter(item.Trim())); }
            return list;
        }

        public static bool ListEquals(this string s1, string s2, bool ignoreOrder = true, char elementDelimiter = ',', bool ignoreCase = true)
        {
            if (s1.IsNullOrEmpty() && s2.IsNullOrEmpty()) return true;
            if (s1.IsNullOrEmpty() || s2.IsNullOrEmpty()) return false;

            var L1 = s1.ToList(elementDelimiter);
            var L2 = s2.ToList(elementDelimiter);
            if (L1.Count != L2.Count) return false;
            if (ignoreOrder)
            {
                StringComparer comparer = ignoreCase ? StringComparer.InvariantCultureIgnoreCase : StringComparer.InvariantCulture;
                L1.Sort(comparer);
                L2.Sort(comparer);
            }
            StringComparison comparison = ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture;
            for (int i = 0; i < L1.Count; i++)
            {
                if (!L1[i].Equals(L2[i], comparison)) return false;
            }
            return true;
        }

        /// <summary>
        /// Get index of the first class array element based upon a matching delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="match">delegate to determine if there is a match</param>
        /// <returns>index of matched element or -1 if not found</returns>
        public static int IndexOf<T>(this IList<T> list, Func<T, bool> match) where T : class
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (match(list[i])) return i;
            }
            return -1;
        }
        /// <summary>
        /// Get index of the last class array element based upon a matching delegate.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <param name="match">delegate to determine if there is a match</param>
        /// <returns>index of matched element or -1 if not found</returns>
        public static int LastIndexOf<T>(this IList<T> list, Func<T, bool> match) where T : class
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (match(list[i])) return i;
            }
            return -1;
        }
    }

    public static class ExceptionExtensions
    {
        //HACK! Exception.Message is read only! We create extension methods to correct this.
        private static readonly FieldInfo _message = typeof(Exception).GetField("_message", BindingFlags.NonPublic | BindingFlags.Instance);

        /// <summary>
        /// Replace the existing exception message string
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="msg"></param>
        public static Exception ReplaceMessage(this Exception ex, string msg) { _message.SetValue(ex, msg); return ex; }
        /// <summary>
        /// Append string to the existing exception message string. A newline is automatically inserted.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="msg"></param>
        public static Exception AppendMessage(this Exception ex, string msg) { _message.SetValue(ex, ex.Message + Environment.NewLine + msg); return ex; }
        /// <summary>
        /// Prefix string to the existing exception message string. A newline is automatically inserted.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="msg"></param>
        public static Exception PrefixMessage(this Exception ex, string msg) { _message.SetValue(ex, msg + Environment.NewLine + ex.Message); return ex; }

        /// <summary>
        /// Append child exception as inner exception to this exception.
        /// </summary>
        /// <param name="ex"></param>
        /// <param name="childEx"></param>
        /// <returns></returns>
        public static Exception AppendInnerException(this Exception ex, Exception childEx)
        {
            if (ex.InnerException != null) AppendInnerException(ex.InnerException, childEx);
            else
            {
                //parentEx.InnerException = childEx; aka private Exception _innerException
                FieldInfo _innerException = typeof(Exception).GetField("_innerException", BindingFlags.Instance | BindingFlags.NonPublic);
                if (_innerException != null) _innerException.SetValue(ex, childEx);
            }
            return ex;
        }

        /// <summary>
        /// Add a stack trace to this exception if it does not already have one. 
        /// An uninitalized stack trace occurs when the caller instantiates a new 
        /// Exception(). Normally, when the exception gets thrown, the stack trace
        /// is automatically added. This extension method is only needed when the 
        /// newly created exception is NOT going to be thrown. This extension method
        /// will NOT overwrite a pre-existing stack trace.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Exception WithStackTrace(this Exception ex)
        {
            if (ex.StackTrace != null) return ex;

            string st = Environment.StackTrace;
            //Need to unwind stack trace to remove our private internal calls:
            //   at System.Environment.get_StackTrace()
            //   at Pandora.Diagnostics.ExceptionUtils.WithStackTrace(Exception ex) in C:\SourceCode\PandoraMain\Pandora.Diagnostics\Diagnostics.cs:line 291
            //Can't just search for st.IndexOf("WithStackTrace"), because the function name may not exist in obfuscated code!

            string name = System.Reflection.MethodBase.GetCurrentMethod().Name;
            int i = st.IndexOf(name);
            if (i > 0) i = st.IndexOf("   at ", i);
            if (i > 0) st = st.Substring(i);
            FieldInfo _stackTraceString = typeof(Exception).GetField("_stackTraceString", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_stackTraceString != null) _stackTraceString.SetValue(ex, st);
            return ex;
        }

        /// <summary>
        /// Retrieve exception message with all inner exception messages, combined.
        /// </summary>
        /// <param name="ex"></param>
        /// <returns></returns>
        public static string FullMessage(this Exception ex)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(ex.GetType().Name);
            sb.Append(": ");
            do
            {
                sb.Append(ex.Message.Trim());
                ex = ex.InnerException;
                if (ex != null && !string.IsNullOrEmpty(ex.Message)) sb.Append(" / ");
                else break;
            } while (ex != null);
            return sb.ToString();
        }
    }

    public static class ObjectExtensions
    {
        /// <summary>
        /// Duplicate the entire graph of any class object. Duplicates nested objects as well. Properly handles recursion. 
        /// Class and all nested classes <b>must</b> be marked as [Serializable] or an exception will occur.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Object to copy</param>
        /// <returns>independent copy of object</returns>
        public static T DeepClone<T>(this T obj) where T : class
        {
            if (obj == null) return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)bf.Deserialize(ms);
            }
        }

        /// <summary>
        /// Create a shallow copy of any object. Nested class objects
        /// are not duplicated. They are just referenced again.
        /// Object does not need to be marked as [Serializable].
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">Object to copy</param>
        /// <returns>copy of object</returns>
        public static T ShallowClone<T>(this T obj)
        {
            if (obj == null) return default(T);
            MethodInfo mi = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
            return (T)mi.Invoke(obj, new object[0]);
        }

        /// <summary>
        /// Get specified public or private type.
        /// </summary>
        /// <param name="typename">
        ///    Case-sensitive full name of the type to get.<br />
        ///    example: "System.Windows.Forms.Layout.TableLayout+ContainerInfo"
        /// </param>
        /// <param name="relatedType">Optional known public type that is in the same assembly as 'typename'.</param>
        /// <returns>Found type or null if not found</returns>
        /// <remarks>
        ///    When 'relatedType' is defined, this method is effectively the same as:<br />
        ///    Type t = Type.GetType("System.Windows.Forms.Layout.TableLayout+ContainerInfo, " + typeof(TableLayoutPanel).Assembly.FullName, false, false);
        ///    If 'relatedType' is undefined, this method will search the currently loaded assemblies for a match.
        /// </remarks>
        private static Type GetReflectedType(string typename, Type relatedType=null)
        {
            if (typename.IsNullOrEmpty()) return null;

            Type t = Type.GetType(typename, false, false);

            if (t==null && relatedType !=null)
            {
                t = Type.GetType($"{typename}, {relatedType.Assembly.FullName}", false, false);
            }

            if (t == null) //Ok. Hunt for it the hard way, assuming the assembly is already loaded.
            {
                var elements = typename.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (elements.Length > 1) return null;
                typename = elements[0];
                t = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(m => m.FullName.EqualsI(typename));
                if (t == null) return null;
            }

            return t;
        }

        /// <summary>
        /// Handy utility for getting array of arg types for invoking methods
        /// </summary>
        /// <param name="args">array of args</param>
        /// <returns>Always returns a Type array</returns>
        private static Type[] GetReflectedArgTypes(params object[] args)
        {
            Type[] types;
            if (args == null || args.Length == 0) return new Type[0];
            types = new Type[args.Length];
            for (int i = 0; i < types.Length; i++) { types[i] = (args[i] == null ? null : args[i].GetType()); }
            return types;
        }

        /// <summary>
        /// Get value by reflection from an object.
        /// It may be a field or property, public or private, instance or static.
        /// This function may be chained together to get a nested value.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="obj">Object to retrieve value from. Must not be null.</param>
        /// <param name="membername">Case-sensitive field or property name.</param>
        /// <param name="index">Optional index for indexed properties.</param>
        /// <returns>Retrieved value or null if field or property not found and readable.</returns>
        public static object GetReflectedValue(this Object obj, string membername, params object[] indices)
        {
            try
            {
                MemberInfo[] mis = obj.GetType().GetMember(membername, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (mis.Length == 0) return null;
                foreach (var mi in mis)
                {
                    if (mi is FieldInfo)
                    {
                        FieldInfo fi = mi as FieldInfo;
                        return fi.GetValue(obj);
                    }
                    else if (mi is PropertyInfo)
                    {
                        PropertyInfo pi = mi as PropertyInfo;
                        if (!pi.CanRead) continue;
                        var iparams = pi.GetIndexParameters();
                        if (iparams.Length != indices.Length) continue;
                        if (iparams.Length == 0) return pi.GetValue(obj);
                        bool match = true;
                        for (int i = 0; i < iparams.Length; i++) { if (iparams[i].ParameterType != indices[i].GetType()) { match = false; break; } }
                        if (!match) continue;
                        return pi.GetValue(obj, indices);
                    }
                }
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Set value by reflection to an object.
        /// It may be a field or property, public or private, instance or static.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="obj">Object to retrieve value from. Must not be null.</param>
        /// <param name="membername">Case-sensitive field or property name.</param>
        /// <param name="value">value to set</param>
        /// <param name="index">Optional index for indexed properties.</param>
        /// <returns>True if value successfully set or false if field or property not found or writeable.</returns>
        public static bool SetReflectedValue(this Object obj, string membername, object value, params object[] indices)
        {
            try
            {
                MemberInfo[] mis = obj.GetType().GetMember(membername, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
                if (mis.Length == 0) return false;
                foreach (var mi in mis)
                {
                    if (mi is FieldInfo)
                    {
                        FieldInfo fi = mi as FieldInfo;
                        fi.SetValue(obj, value);
                        return true;
                    }
                    else if (mi is PropertyInfo)
                    {
                        PropertyInfo pi = mi as PropertyInfo;
                        if (!pi.CanWrite) continue;
                        var iparams = pi.GetIndexParameters();
                        if (iparams.Length != indices.Length) continue;
                        if (iparams.Length == 0) { pi.SetValue(obj, value); return true; }
                        bool match = true;
                        for (int i = 0; i < iparams.Length; i++) { if (iparams[i].ParameterType != indices[i].GetType()) { match = false; break; } }
                        if (!match) continue;
                        pi.SetValue(obj, value, indices);
                        return true;
                    }
                }
                return false;
            }
            catch { return false; }
        }

        /// <summary>
        /// Invoke a static method or invoke a constructor to return a constructed object or invoke a static method.
        /// It may be a static method or non-static constructor, public or private.
        /// Does not handle 'ref' or 'out' arguments.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="typename">Case-sensitive full type name of the object to invoke OR fully qualified assembly name just in case the assembly has not already been loaded into the domain.</param>
        /// <param name="args">arguments to pass to method or constructor.</param>
        /// <returns>the constructed object or method return value.</returns>
        public static object InvokeReflectedMethod(string typename, string membername, params object[] args)
        {
            try
            {
                Type t = GetReflectedType(typename);
                if (t == null) return null;
                return InvokeReflectedMethod(t, membername, args);
            }
            catch { return null; }
        }

        /// <summary>
        /// Invoke a public or private static method or constructor.
        /// Does not handle 'ref' or 'out' arguments.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="t">Type execute static method on. Must not be null.</param>
        /// <param name="membername">Case-sensitive static method name. If null, must be a constructor.</param>
        /// <param name="args">arguments to pass to method</param>
        /// <returns>value returned from method or created object if a constructor</returns>
        public static object InvokeReflectedMethod(this Type t, string membername, params object[] args)
        {
            if (t == null) return null;
            try
            {
                if (membername.IsNullOrEmpty())
                {
                    ConstructorInfo ci = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, GetReflectedArgTypes(args), null);
                    if (ci != null) return ci.Invoke(args);
                    return null;
                }
                MethodInfo mi = t.GetMethod(membername, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, GetReflectedArgTypes(args), null);
                if (mi != null) return mi.Invoke(null, args);
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Invoke a public or private instance method.
        /// Does not handle 'ref' or 'out' arguments.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="obj">Object to execute method on. Must not be null.</param>
        /// <param name="membername">Case-sensitive method name.</param>
        /// <param name="args">arguments to pass to method</param>
        /// <returns>value returned from method</returns>
        public static object InvokeReflectedMethod(this Object obj, string membername, params object[] args)
        {
            try
            {
                Type t = obj.GetType();
                MethodInfo mi = t.GetMethod(membername, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, GetReflectedArgTypes(args), null);
                if (mi != null) return mi.Invoke(obj, args);
                return null;
            }
            catch { return null; }
        }

        /// <summary>
        /// Determine if the object is of the specified type by string name. Same as 
        /// (myobject is System.String) except type does not have be in an assembly reference.
        /// </summary>
        /// <param name="obj">Object to test.</param>
        /// <param name="typename">Assembly-qualified type name.
        ///    Case-insensitive, full type name of the object to invoke OR fully 
        ///    qualified assembly name just in case the assembly has not already 
        ///    been loaded into the domain.
        ///    Example: "Infragistics.Win.Misc.UltraButton, Infragistics2.Win.Misc.v10.2" 
        ///       where the first part is the Type fullname and the second part is the 
        ///       assembly to which it belogs. The 2nd part of typename is not required 
        ///       for "System" types.
        /// </param>
        /// <returns>true if typename is the class or contains the base class or interface of. False if not, or typename cannot be found</returns>
        public static bool ReflectedIs(this Object obj, string typename)
        {
            if (obj == null) return false;
            Type t = GetReflectedType(typename);
            if (t == null) return false;
            return (t.IsAssignableFrom(obj.GetType()));
        }

        /// <summary>
        /// Determine if the object is of the specified type. Same as 
        /// (myobject is System.String) except type is determined at 
        /// runtime, not compile type.
        /// </summary>
        /// <param name="obj">Object to test.</param>
        /// <param name="isType">Type of object it is.</param>
        /// <returns>true if typename is the class or contains the base class or interface of. False if not, or typename cannot be found</returns>
        public static bool ReflectedIs(this Object obj, Type isType)
        {
            if (obj == null) return false;
            return (isType.IsAssignableFrom(obj.GetType()));
        }

        #region Debugging tool: public static string[] GetReflectedObjectMembers(this Object obj)
        public static string[] GetReflectedObjectMembers(string typename)
        {
            Type t = GetReflectedType(typename);
            if (t == null) return null;
            return GetReflectedObjectMembers(t);
        }
        public static string[] GetReflectedObjectMembers(this Object obj)
        {
            return GetReflectedObjectMembers(obj.GetType(), obj);
        }
        public static string[] GetReflectedObjectMembers(this Type t, Object obj = null)
        {
            MemberInfo[] mis = t.GetMembers(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            List<string> members = new List<string>(mis.Length);
            foreach (var mi in mis)
            {
                switch (mi.MemberType)
                {
                    case MemberTypes.Constructor: members.Add(GetConstructorDeclaration(mi)); break;
                    case MemberTypes.Event: members.Add(GetEventDeclaration(mi)); break;
                    case MemberTypes.Field: members.Add(GetFieldDeclaration(mi, obj)); break;
                    case MemberTypes.Method: members.Add(GetMethodDeclaration(mi)); break;
                    case MemberTypes.Property: members.Add(GetPropertyDeclaration(mi, obj)); break;
                    case MemberTypes.TypeInfo: members.Add(GetTypeDeclaration(mi)); break;
                    default: members.Add(GetUnknownDeclaration(mi)); break;
                }
            }
            return members.ToArray();
        }
        #region GetReflectedObjectMembers(Type) private methods
        private static string GetConstructorDeclaration(MemberInfo mi)
        {
            var m = mi as ConstructorInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            //if (m.IsSpecialName) sb.Append("[special] ");
            if (m.IsPublic) sb.Append("public ");
            else if (m.IsAssembly) sb.Append("internal ");
            else if (m.IsFamily) sb.Append("protected ");
            else if (m.IsPrivate) sb.Append("private ");
            if (m.IsStatic) sb.Append("static ");
            if (m.IsVirtual && !m.IsFinal) sb.Append("virtual ");
            if (m.IsVirtual && m.IsFinal) sb.Append("override ");
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.DeclaringType.Name); //sb.Append(m.Name);
            sb.Append('(');
            string comma = "";
            foreach (ParameterInfo p in m.GetParameters())
            {
                sb.Append(comma);
                if (p.IsIn && p.IsOut) sb.Append("ref ");
                if (!p.IsIn && p.IsOut) sb.Append("out ");
                sb.Append(MakeName(p.ParameterType));
                if (p.HasDefaultValue)
                {
                    sb.Append(" = ");
                    sb.Append(p.DefaultValue.GetType() == typeof(string) ? "\"" + p.DefaultValue.ToString() + "\"" : p.DefaultValue.ToString());
                }
                comma = ", ";
            }
            sb.Append(')');
            return sb.ToString();
        }
        private static string GetEventDeclaration(MemberInfo mi)
        {
            var m = mi as EventInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            MethodInfo am = m.AddMethod;
            if (am.IsPublic) sb.Append("public ");
            else if (am.IsAssembly) sb.Append("internal ");
            else if (am.IsFamily) sb.Append("protected ");
            else if (am.IsPrivate) sb.Append("private ");
            if (am.IsStatic) sb.Append("static ");
            if (am.IsVirtual && !am.IsFinal) sb.Append("virtual ");
            if (am.IsVirtual && am.IsFinal) sb.Append("override ");
            sb.Append("event ");
            sb.Append(MakeName(m.EventHandlerType));
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.Name);
            return sb.ToString();
        }
        private static string GetFieldDeclaration(MemberInfo mi, Object obj = null)
        {
            var m = mi as FieldInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            if (m.IsSpecialName) sb.Append("[special] ");
            if (m.IsPublic) sb.Append("public ");
            else if (m.IsAssembly) sb.Append("internal ");
            else if (m.IsFamily) sb.Append("protected ");
            else if (m.IsPrivate) sb.Append("private ");
            if (m.IsStatic) sb.Append("static ");
            //if (m.IsVirtual && !m.IsFinal) sb.Append("virtual ");
            //if (m.IsVirtual && m.IsFinal) sb.Append("override ");
            sb.Append(MakeName(m.FieldType));
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.Name);
            if (obj != null)
            {
                sb.Append(" = ");
                object value = m.GetValue(obj);
                if (value == null) sb.Append("null");
                else
                {
                    string quote = (m.FieldType == typeof(string) ? "\"" : "");
                    sb.Append(quote);
                    try { sb.Append(m.GetValue(obj).ToString()); } catch { }
                    sb.Append(quote);
                }
            }
            return sb.ToString();
        }
        private static string GetMethodDeclaration(MemberInfo mi)
        {
            var m = mi as MethodInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            if (m.IsSpecialName) sb.Append("[special] ");
            if (m.IsPublic) sb.Append("public ");
            else if (m.IsAssembly) sb.Append("internal ");
            else if (m.IsFamily) sb.Append("protected ");
            else if (m.IsPrivate) sb.Append("private ");
            if (m.IsStatic) sb.Append("static ");
            if (m.IsVirtual && !m.IsFinal) sb.Append("virtual ");
            if (m.IsVirtual && m.IsFinal) sb.Append("override ");
            sb.Append(MakeName(m.ReturnType));
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.Name);
            sb.Append('(');
            string comma = "";
            foreach (ParameterInfo p in m.GetParameters())
            {
                sb.Append(comma);
                if (p.IsIn && p.IsOut) sb.Append("ref ");
                if (!p.IsIn && p.IsOut) sb.Append("out ");
                sb.Append(MakeName(p.ParameterType));
                if (p.HasDefaultValue)
                {
                    sb.Append(" = ");
                    sb.Append(p.DefaultValue.GetType() == typeof(string) ? "\"" + p.DefaultValue.ToString() + "\"" : p.DefaultValue.ToString());
                }
                comma = ", ";
            }
            sb.Append(')');
            return sb.ToString();
        }
        private static string GetPropertyDeclaration(MemberInfo mi, Object obj = null)
        {
            var m = mi as PropertyInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            MethodInfo ac1 = m.GetMethod;
            MethodInfo ac2 = m.SetMethod;
            if (ac1 == null) ac1 = ac2;
            if (ac2 == null) ac2 = ac1;
            if (ac1.IsPublic && ac2.IsPublic) sb.Append("public ");
            else if (ac1.IsAssembly && ac2.IsAssembly) sb.Append("internal ");
            else if (ac1.IsFamily && ac2.IsFamily) sb.Append("protected ");
            else if (ac1.IsPrivate && ac2.IsPrivate) sb.Append("private ");
            if (ac1.IsStatic && ac2.IsStatic) sb.Append("static ");
            if ((ac1.IsVirtual && !ac1.IsFinal) || (ac2.IsVirtual && !ac2.IsFinal)) sb.Append("virtual ");
            if ((ac1.IsVirtual && ac1.IsFinal) || (ac2.IsVirtual && ac2.IsFinal)) sb.Append("override ");

            sb.Append(MakeName(m.PropertyType));
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.Name);

            ParameterInfo[] parameters = m.GetIndexParameters();
            if (parameters.Length > 0)
            {
                sb.Append("[");
                string comma = "";
                foreach (ParameterInfo p in m.GetIndexParameters())
                {
                    sb.Append(comma);
                    if (p.IsIn && p.IsOut) sb.Append("ref ");
                    if (!p.IsIn && p.IsOut) sb.Append("out ");
                    sb.Append(MakeName(p.ParameterType));
                    if (p.HasDefaultValue)
                    {
                        sb.Append(" = ");
                        sb.Append(p.DefaultValue.GetType() == typeof(string) ? "\"" + p.DefaultValue.ToString() + "\"" : p.DefaultValue.ToString());
                    }
                    comma = ", ";
                }
                sb.Append(']');
            }

            sb.Append(" { ");
            if (m.CanRead) sb.Append("get; ");
            if (m.CanWrite) sb.Append("set; ");
            sb.Append("}");

            if (obj != null && m.CanRead && parameters.Length == 0)
            {
                sb.Append(" = ");

                object value = null;
                try { value = m.GetValue(obj); } catch { }
                if (value == null) sb.Append("null");
                else
                {
                    string quote = (m.PropertyType == typeof(string) ? "\"" : "");
                    sb.Append(quote);
                    try { sb.Append(m.GetValue(obj).ToString()); } catch { }
                    sb.Append(quote);
                }
            }

            return sb.ToString();
        }
        private static string GetTypeDeclaration(MemberInfo mi)
        {
            var m = mi as TypeInfo;
            return GetUnknownDeclaration(mi);
        }
        private static string GetUnknownDeclaration(MemberInfo mi)
        {
            return string.Format("[{0}] {1}.{2}", mi.MemberType, mi.DeclaringType.Name, mi.Name);
        }
        private static string MakeName(Type t)
        {
            if (t == null) return "void";
            var sb = new StringBuilder();
            int i = t.Name.IndexOf('`');
            sb.Append(i >= 0 ? t.Name.Substring(0, i) : t.Name);
            if (t.IsGenericType)
            {
                sb.Append('<');
                string comma = "";
                foreach (Type arg in t.GetGenericArguments())
                {
                    sb.Append(comma);
                    sb.Append(MakeName(arg));
                    comma = ", ";
                }
                sb.Append('>');
            }
            return sb.ToString();
        }
        #endregion
        #endregion
    }

    public static class EnumExtensions
    {
        /// <summary>
        /// Retrieve the DescriptionAttribute string associated with an Enum value.
        /// Example: 
        /// enum MyEnum {
        ///     [Description("My Value #1")] MyValue1,
        ///     [Description("My Value #2")] MyValue2
        ///  }
        /// Useful for Data Binding an Enum with human-friendly Descriptions to ComboBoxes
        /// http://www.codeproject.com/KB/cs/enumdatabinding.aspx
        /// combo.DataSource = typeof(MyEnum).Descriptions();
        /// combo.DisplayMember = "Value";
        /// combo.ValueMember = "Key";
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <param name="value">Enum value to retrieve description string from.</param>
        /// <returns>Associated description string or the enum value string itself</returns>
        public static string Description<T>(this T value) where T : struct, IComparable, IConvertible, IFormattable
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            FieldInfo fi = typeof(T).GetField(value.ToString());
            DescriptionAttribute[] attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0) return attributes[0].Description;
            else return value.ToString();
        }

        /// <summary>
        /// Retrieve a list of all the DescriptionAttribute strings associated with an Enum type.
        /// Example: 
        /// enum MyEnum {
        ///     [Description("My Value #1")] MyValue1,
        ///     [Description("My Value #2")] MyValue2
        ///  }
        /// Useful for Data Binding an Enum with human-friendly Descriptions to ComboBoxes
        /// http://www.codeproject.com/KB/cs/enumdatabinding.aspx
        /// combo.DataSource = typeof(MyEnum).Descriptions();
        /// combo.DisplayMember = "Value";
        /// combo.ValueMember = "Key";
        /// </summary>
        /// <typeparam name="T">Enum type</typeparam>
        /// <returns>Associated list of description strings for the enum type</returns>
        public static IDictionary<T, string> Descriptions<T>() where T : struct, IComparable, IConvertible, IFormattable
        {
            if (!typeof(T).IsEnum) throw new ArgumentException("T must be an enumerated type");
            T[] enumValues = Enum.GetValues(typeof(T)) as T[];
            Dictionary<T, string> d = new Dictionary<T, string>(enumValues.Length);
            foreach (T value in enumValues) { d.Add(value, Description(value)); }
            return d;
        }
    }

    public static class AssemblyExtensions
    {
        /// <summary>
        /// Get the first value (as string) for the first attribute of the given type.
        /// </summary>
        /// <typeparam name="T">Attribute to get value for</typeparam>
        /// <param name="asm">Assembly to check</param>
        /// <returns>Value as string or "" if no arguments, or null if attribute not found.</returns>
        public static string Attribute<T>(this Assembly asm) where T : Attribute
        {
            foreach (CustomAttributeData data in asm.CustomAttributes)
            {
                if (typeof(T) != data.AttributeType) continue;
                if (data.ConstructorArguments.Count > 0) return data.ConstructorArguments[0].Value.ToString();
                if (data.NamedArguments.Count > 0) return data.NamedArguments[0].TypedValue.Value.ToString();
                return string.Empty;
            }
            return null;
        }

        /// <summary>
        ///  Detect if assembly attribute exists.
        /// </summary>
        /// <typeparam name="T">Attribute to search for</typeparam>
        /// <param name="asm">Assembly to search in</param>
        /// <returns>True if found</returns>
        public static bool AttributeExists<T>(this Assembly asm) where T : Attribute
        {
            return asm.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(T)) != null;
        }

        /// <summary>
        /// Gets the build/link timestamp from the specified executable file header.
        /// </summary>
        /// <param name="filePath">PE file to retrieve build date from</param>
        /// <returns>The local DateTime that the specified assembly was built.</returns>
        /// <remarks>
        /// WARNING: When compiled in a .netcore application/library, the PE timestamp 
        /// is NOT set with the the application link time. It contains some other non-
        /// timestamp (hash?) value. To force the .netcore linker to embed the true 
        /// timestamp as previously, add the csproj property 
        /// "<Deterministic>False</Deterministic>".
        /// </remarks>
        private static DateTime PEtimestamp(string filePath)
        {
            uint TimeDateStamp = 0;
            using (FileStream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                //Minimum possible executable file size.
                if (stream.Length < 268) throw new BadImageFormatException("Not a PE file. File too small.", filePath);
                //The first 2 bytes in file == IMAGE_DOS_SIGNATURE, 0x5A4D, or MZ.
                if (stream.ReadByte() != 'M' || stream.ReadByte() != 'Z') throw new BadImageFormatException("Not a PE file. DOS Signature not found.", filePath);
                stream.Position = 60; //offset of IMAGE_DOS_HEADER.e_lfanew
                stream.Position = ReadUInt32(stream); // e_lfanew = 128
                uint ntHeadersSignature = ReadUInt32(stream); // ntHeadersSignature == 17744 aka "PE\0\0"
                if (ntHeadersSignature != 17744) throw new BadImageFormatException("Not a PE file. NT Signature not found.", filePath);
                stream.Position += 4; //offset of IMAGE_FILE_HEADER.TimeDateStamp
                TimeDateStamp = ReadUInt32(stream); //unix-style time_t value
            }

            DateTime returnValue = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(TimeDateStamp);
            returnValue = returnValue.ToLocalTime();

            if (returnValue < new DateTime(2000, 1, 1) || returnValue > DateTime.Now)
            {
                //PEHeader link timestamp field is random junk because csproj property "Deterministic" == true
                //so we just return the 2nd best "build" time (iffy, unreliable).
                return File.GetCreationTime(filePath);
            }

            return returnValue;
        }

        /// <summary>
        /// Gets the build/link timestamp from the specified assembly file header.
        /// </summary>
        /// <param name="asm">Assembly to retrieve build date from</param>
        /// <returns>The local DateTime that the specified assembly was built.</returns>
        /// <remarks>
        /// WARNING: When compiled in a .netcore application/library, the PE timestamp 
        /// is NOT set with the the application link time. It contains some other non-
        /// timestamp (hash?) value. To force the .netcore linker to embed the true 
        /// timestamp as previously, add the csproj property 
        /// "<Deterministic>False</Deterministic>".
        /// </remarks>
        public static DateTime PEtimestamp(this Assembly asm)
        {
            if (asm.IsDynamic)
            {
                //The assembly was dynamically built in-memory so the build date is Now. Besides, 
                //accessing the location of a dynamically built assembly will throw an exception!
                return DateTime.Now;
            }

            return PEtimestamp(asm.Location);
        }

        /// <summary>
        /// Support utility exclusively for PEtimestamp()
        /// </summary>
        /// <param name="fs">File stream</param>
        /// <returns>32-bit unsigned int at current offset</returns>
        private static uint ReadUInt32(FileStream fs)
        {
            byte[] bytes = new byte[4];
            fs.Read(bytes, 0, 4);
            return BitConverter.ToUInt32(bytes, 0);
        }
    }

    public static class MemberInfoExtensions
    {
        /// <summary>
        /// Get the first value (as string) for the first attribute of the given type
        /// </summary>
        /// <typeparam name="T">Attribute to get value for</typeparam>
        /// <param name="mi">Object member info</param>
        /// <returns>Value as string or "" if no arguments, or null if attribute not found.</returns>
        public static string Attribute<T>(this MemberInfo mi) where T : Attribute
        {
            foreach (CustomAttributeData data in mi.CustomAttributes)
            {
                if (typeof(T) != data.AttributeType) continue;
                if (data.ConstructorArguments.Count > 0) return data.ConstructorArguments[0].Value.ToString();
                if (data.NamedArguments.Count > 0) return data.NamedArguments[0].TypedValue.Value.ToString();
                return string.Empty;
            }
            return null;
        }

        /// <summary>
        ///  Detect if assembly attribute exists.
        /// </summary>
        /// <typeparam name="T">Attribute to search for</typeparam>
        /// <param name="mi">Object member info</param>
        /// <returns>True if found</returns>
        public static bool AttributeExists<T>(this MemberInfo mi) where T : Attribute
        {
            return mi.CustomAttributes.FirstOrDefault(ca => ca.AttributeType == typeof(T)) != null;
        }
    }

    public static class AppDomainExtensions
    {
        /// <summary>
        /// Normally, once the AppDomain has been created, the AppDomain.FriendlyName 
        /// cannot be changed. This allows it to be changed.
        /// </summary>
        /// <param name="ad">AppDomain to change</param>
        /// <param name="newname">new "FriendlyName"</param>
        public static void SetFriendlyName(this AppDomain ad, string newname)
        {
            MethodInfo mi = typeof(AppDomain).GetMethod("nSetupFriendlyName", BindingFlags.Instance | BindingFlags.NonPublic);
            if (mi == null) return;
            mi.Invoke(ad, new object[] { newname });
        }
    }

    public static class DateTimeExtensions
    {
        /// <summary>
        /// Convert local time to UTC DateTimeOffset.
        /// </summary>
        /// <param name="local">Local datetime</param>
        /// <returns>UTC DateTimeOffset</returns>
        public static DateTimeOffset ToUTC(this DateTime local) => new DateTimeOffset(local.ToUniversalTime(), new TimeSpan());

        /// <summary>
        /// Convert DateTimeOffset to local datetime.
        /// </summary>
        /// <param name="utc">DateTimeOffset to convert.</param>
        /// <returns>Local datetime</returns>
        public static DateTime ToLocal(this DateTimeOffset utc) => utc.LocalDateTime;

        /// <summary>
        /// Convert DateTime to local DateTime as determined by the DateTime.Kind property.
        /// </summary>
        /// <param name="utc">DateTime to convert.</param>
        /// <returns>Local datetime</returns>
        /// <remarks>
        /// If dt.Kind is DateTimeKind.Utc, the conversion is performed.<br />
        /// If dt.Kind is DateTimeKind.Local, the conversion is not performed.<br />
        /// If dt.Kind is DateTimeKind.Unspecified, the conversion is performed as if dt was universal time.
        /// </remarks>
        public static DateTime ToLocal(this DateTime utc) => utc.ToLocalTime();

        /// <summary>
        /// Round datetime to the nearest minute. 
        /// </summary>
        /// <param name="dt">Datetime to round</param>
        /// <returns>Rounded datetime. dt.Kind is preserved.</returns>
        public static DateTime ToMinute(this DateTime dt) => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute + (dt.Second > 30 ? 1 : 0), 0, dt.Kind);

        /// <summary>
        /// Round datetime to the nearest second. 
        /// </summary>
        /// <param name="dt">Datetime to round</param>
        /// <returns>Rounded datetime. dt.Kind is preserved.</returns>
        public static DateTime ToSecond(this DateTime dt) => new DateTime(dt.Year, dt.Month, dt.Day, dt.Hour, dt.Minute, dt.Second + (dt.Millisecond > 500 ? 1 : 0), 0, dt.Kind);

        private static readonly DateTime UnixEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);

        /// <summary>
        /// Convert datetime to unix time_t integer in seconds.
        /// </summary>
        /// <param name="dt">Datetime to convert.</param>
        /// <returns>time_t integer representing seconds from 1/1/1970.</returns>
        public static int ToUnixTime(this DateTime dt) => (int)(dt - UnixEpoch).TotalSeconds;

        /// <summary>
        /// Convert time_t seconds from 1/1/1970 to DateTime
        /// </summary>
        /// <param name="time_t">Seconds from 1/1/1970</param>
        /// <returns>Datetime equivalant.</returns>
        public static DateTime FromUnixTime(this int time_t) => UnixEpoch.AddSeconds(time_t);
    }

    public static class EnumerableExtensions
    {
        /// <summary>
        /// Perform action upon each item in enumerable loop, ascending.
        /// </summary>
        /// <typeparam name="T">Type of item in enumeration</typeparam>
        /// <param name="source">Enumerable array</param>
        /// <param name="action">Action to perform on each item in enumeration. Return true to continue to next item or false to break enumeration</param>
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) return;

            if (source is IList<T> list)
            {
                var k = list.Count;
                for (int i = 0; i < k; i++)
                {
                    action(list[i]);
                    k = list.Count;
                }
            }
            else
            {
                foreach (var v in source) action(v);
            }
        }

        /// <summary>
        /// Perform action upon each item in enumerable loop, descending (reverse order).
        /// Useful when the count of items may change.
        /// </summary>
        /// <typeparam name="T">Type of item in enumeration</typeparam>
        /// <param name="source">Enumerable array</param>
        /// <param name="action">Action to perform on each item in enumeration. Return true to continue to next item or false to break enumeration</param>
        public static void ForEachDesc<T>(this IEnumerable<T> source, Func<T, bool> action)
        {
            //Action on sequence items is performed in descending order just in case elements are removed by the action.
            //Equivalant to: foreach (var v in source.Reverse()) action(v); but more efficient.

            if (source is IList<T> list)
            {
                for (int i = list.Count - 1; i >= 0; i--)
                    if (!action(list[i])) break;
            }
            else if (source is ICollection<T> collection)
            {
                var length = collection.Count;
                if (length == 0) return;
                T[] array = new T[length];
                collection.CopyTo(array, 0);

                for (int i = length - 1; i >= 0; i--)
                    if (!action(array[i])) break;
            }
            else
            {
                T[] array = null;
                int length = 0;
                foreach (T element in source)
                {
                    if (array == null) array = new T[4];
                    else if (array.Length == length)
                    {
                        T[] elementArray = new T[checked(length * 2)];
                        Array.Copy((Array)array, 0, (Array)elementArray, 0, length);
                        array = elementArray;
                    }
                    array[length] = element;
                    ++length;
                }

                for (int i = length - 1; i >= 0; i--)
                    if (!action(array[i])) break;
            }
        }
    }
}
