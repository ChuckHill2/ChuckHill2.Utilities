//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="CsvReader.cs" company="Chuck Hill">
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
using System.Collections.Specialized;
using System.IO;
using System.Text;

namespace ChuckHill2
{
    /// <summary>
    /// Parse a file, a string, or open stream of CSV formatted text data.
    /// </summary>
    public class CsvReader : IDisposable
    {
        /// <summary>
        /// Marks the end of the csv stream. Not necessary if the entire stream contains only a single CSV object.
        /// </summary>
        public const char EndOfCSV = '\f'; // FF (formfeed) End of CSV data

        #region -= Private Variables =-
        private bool EOL = false;  // end of line/record flag
        private bool EOF = false;  // end of file flag
        private CharCache Cache = null;
        private StringBuilder sb = new StringBuilder(256);
        private int TrailingWhitespaceIndex = -1;  // index to last whitespace char in field
        private bool HasTrailingComma = false;     // flag that CSV record continues on next line
        private bool LineHasFields = false;        // For detection of zero fields in a line
        private int fieldCount = 0;                // Useful for error handling
        private int recordCount = 0;               // Useful for error handling
        #endregion -= Private Variables =-

        #region -= Properties =-
        /// <summary>
        /// Returns true when end of file is reached. WARNING: Trailing whitespace or 
        /// illegal chars in file are ignored but EOF is not true until these chars are
        /// evaluated, thus the last instantiation of this enumerable object will cause 
        /// a foreach loop to exit immediately because there are no fields left to return.
        /// </summary>
        public bool EndOfFile { get { return EOF; } }
        public bool EndOfLine { get { return EOL; } }
        public int FieldCount { get { return fieldCount; } }
        public int RecordCount { get { return recordCount; } }
        #endregion -= Properties =-

        #region -= Static Utilities =-
        /// <summary>
        /// Robust CSV string parse. Properly handles quoted/escaped fields.
        /// Only returns the first CSV record (record may span multiple lines).
        /// Calls CSVReader.SplitItem()
        /// </summary>
        /// <param name="csvReader">CSV string to parse</param>
        /// <returns>Array of fields from first record of CSV string</returns>
        public static string[] Split(string csvReader)
        {
            using (CsvReader csv = new CsvReader(csvReader))
            {
                IEnumerable<string> items = csv.ReadField();
                List<string> array = new List<string>();
                foreach (string item in items)
                {
                    array.Add(item);
                }
                return array.ToArray();
            }
        }

        /// <summary>
        /// Compares 2 string arrays to see if they are identical (case-insensitive)
        /// </summary>
        /// <param name="s1">Array of strings #1</param>
        /// <param name="s2">Array of strings #2</param>
        /// <returns>True if identical</returns>
        public static bool Match(string[] s1, string[] s2)
        {
            if (s1.Length != s2.Length) return false;
            for (int i = 0; i < s1.Length; i++)
            {
                if (string.Compare(s1[i], s2[i], true) != 0) return false;
            }
            return true;
        }

        /// <summary>
        /// Converts array of strings into a properly quoted/escaped CSV string.
        /// </summary>
        /// <param name="szArray">Array of strings to convert</param>
        /// <returns>Quoted CSV string</returns>
        public static string ToString(string[] szArray)
        {
            StringBuilder sb = new StringBuilder();
            bool starting = true;
            foreach (string s in szArray)
            {
                if (!starting) sb.Append(',');
                sb.Append('"');
                sb.Append(s.Replace("\"", "\"\""));
                sb.Append('"');
                starting = false;
            }
            return sb.ToString();
        }

        /// <summary>
        /// Converts array of strings into simple comma-delimited string. 
        /// Does not handle commas or quotes or newlines embedded in string elements.
        /// </summary>
        /// <param name="szArray">Array of strings to convert</param>
        /// <returns>Comma-delimited string</returns>
        public static string ToSimpleString(string[] szArray)
        {
            return string.Join(",", szArray);
        }
        public static string ToSimpleString(StringCollection szArray)
        {
            StringBuilder sb = new StringBuilder();
            bool appending = false;
            foreach (string s in szArray)
            {
                if (appending) sb.Append(',');
                sb.Append(s);
                appending = true;
            }
            return sb.ToString();
        }
        #endregion -= Static Utilities =-

        /// <summary>
        /// Construct a new CSV data parser object.
        /// </summary>
        /// <param name="s">CSV text data stream that contains 1 or more pages of CSV data where each unique CSV page is delimited by a form-feed ('\f') character.</param>
        public CsvReader(TextReader s)
        {
            Cache = new CharCache(s);
        }

        /// <summary>
        /// Construct a new CSV data parser object.
        /// </summary>
        /// <param name="s">CSV data stream containing a single page of CSV data.</param>
        public CsvReader(Stream s)
        {
            Cache = new CharCache(s);
        }

        /// <summary>
        /// Construct a new CSV data parser object
        /// </summary>
        /// <param name="filename">Name of file containing a single page of CSV data.</param>
        public CsvReader(string filename)
        {
            Cache = new CharCache(filename);
        }

        /// <summary>
        /// Cleanup and mark for GC cleanup
        /// </summary>
        public void Dispose()
        {
            if (Cache != null) { Cache.Dispose(); Cache = null; }
            EOF = true;  // forces SplitItem() to enumerate nothing in case someone attempts to use this object after Dispose()
        }

        /// <summary>
        /// Get array of fields. Beginning of array truncated if not starting at the beginning of
        /// a CSV record. May also be useful for scanning to the beginning of the next record.
        /// </summary>
        /// <returns>Array of field strings</returns>
        public string[] ReadRecord()
        {
            IEnumerable<string> items = this.ReadField();
            List<string> array = new List<string>(_maxFields); // define capacity for effiency
            foreach (string item in items)
            {
                array.Add(item);
            }
            if (this.FieldCount > _maxFields) _maxFields = ((this.FieldCount / 4) + 1) * 4; // round up to nearest multiple of 4
            return array.ToArray();
        }
        private int _maxFields = 4; // upon the first add, capacity==4

        /// <summary>
        /// **Split CSV formatted file, record by record.**<br/>
        /// Example Usage:
        /// ```csharp
        /// CSVReader csv = new CSVReader(filename);
        /// while(!csv.EndOfFile) // foreach record
        /// {
        ///    IEnumerable<string> items = csv.ReadField();
        ///    foreach (string cell in items) { do...something }
        /// }
        /// csv.Dispose();
        /// ```
        /// 
        /// ##### Features
        /// * A CSV delimiter is a comma.
        /// * Leading and trailing whitespace and quotes from each field are removed.
        /// * Blank lines are ignored unless quoted.
        /// * When quoted, a double-double-quote (e.g. "") is treated as a literal field character.
        /// * A CSV record terminator is a newline '\\n' unless quoted or ends with a delimiter.
        /// * Quoted fields may span multiple lines.
        /// * Trailing delimiter (trailing whitespace is ignored) on end of text line assumes the
        ///    CSV record continues on the next line.
        /// 
        /// ##### Gotchas
        /// * Trailing whitespace or illegal chars in file are ignored but EOF is not true until these
        ///    chars are evaluated, thus the last instantiation of this enumerable object will cause
        ///    a foreach loop to exit immediately because there are no fields to return.
        /// 
        /// ##### Quoting Concepts
        /// A field only needs to be quoted if the field contains a field delimiter (e.g. a comma) or a quote char.
        /// </summary>
        /// <returns>Parsed string field</returns>
        public IEnumerable<string> ReadField()
        {
            if (EOF) { yield break; }  // End of file. We're done...for good.

            bool quoted = false;
            char c;

            EOL = false;  // starting a new line/record
            sb.Length = 0;
            HasTrailingComma = false;
            LineHasFields = false;
            TrailingWhitespaceIndex = -1;
            fieldCount = 0;
 
            while ((c = Cache.GetChar()) != '\0')
            {
                if (quoted)
                {
                    if (c == '"')  // terminate literal string
                    {
                        if (Cache.PeekChar() == '"') // Is the quote a literal char? Note: Any leading & trailing quotes are removed by sbAppend()
                        {
                            Cache.GetChar(); // flush the extra quote
                            SbAppend(c);     // save it here.
                            continue;
                        }
                        quoted = false;
                        continue;
                    }
                    if (c == '\n') sb.Append('\r');  // Cache.GetChar() does not return '\r', So we put it back into Windows text format here.
                    SbAppend(c);
                    continue;
                }

                if (c == '"') // begin quoted literal field
                {
                    quoted = true;
                    HasTrailingComma = false;
                    continue; 
                }
                if (c == ',') // end of field
                {
                    HasTrailingComma = true;
                    yield return DoYield();
                    continue;
                }
                if (c == '\n')  // end of record
                {
                    if ((!LineHasFields || HasTrailingComma) && sb.Length == 0)  // next line must be a continuation of the record
                    {
                        LineHasFields = false;
                        HasTrailingComma = false;
                        EOL = false;
                        continue;
                    }
                    EOL = true;
                    
                    yield return DoYield();  // return last field in record
                    recordCount++;
                    yield break;             // enumeration done
                }
                SbAppend(c);
            }

            EOF = true;  // no more characters in file
            if (HasTrailingComma && sb.Length == 0) yield return string.Empty; // there was a delimiter preceding the trailing whitespace, so this is a valid empty string.
            if (sb.Length == 0) yield break; // there was just trailing whitespace so we were already at EOF
            yield return DoYield();  // return the last field in file.
        }

        private string DoYield()
        {
            fieldCount++;
            LineHasFields = true;
            SbAppend('\0'); // Remove trailing whitespace to match the old foxpro code
            string field = sb.ToString();
            sb.Length = 0;
            return field;
        }

        private void SbAppend(char c)
        {
            // Handle leading & trailing whitespace and quotes here. Faster than using string.Trim()
            if (c == ' ' || c == '\t' || c == '"')  // '\n' may be a valid character
            {
                if (sb.Length == 0) return;  // ignore leading whitespace
                if (TrailingWhitespaceIndex == -1) TrailingWhitespaceIndex = sb.Length;  // Mark the first trailing whitespace char
            }
            else if (c != '\0') TrailingWhitespaceIndex = -1;  // Oops! Whitespace char is NOT the last char

            if (c == '\0')  // Flag to trim trailing whitespace
            {
                if (TrailingWhitespaceIndex >= 0)
                {
                    sb.Length = TrailingWhitespaceIndex;
                    TrailingWhitespaceIndex = -1;
                }
                return;
            }

            HasTrailingComma = false;  // If we got a valid char then the delimiter is not the last char on the line.
            sb.Append(c);
        }

        /// <summary>
        /// Handy class to efficiently retrieve buffered characters one at a time from file/stream.
        /// Automatically skips illegal chars.
        /// </summary>
        private class CharCache : IDisposable
        {
            private bool ReaderCreatedHere = false;
            // Optimization Note: Tried using my own char cache but it didn't help. It was even slightly slower.
            private TextReader Reader = null;

            public CharCache(TextReader s)
            {
                Reader = s;
            }

            public CharCache(Stream s) : this(new StreamReader(s, Encoding.UTF8, true, 4096, true))
            {
                ReaderCreatedHere = true;
            }

            public CharCache(string filename) : this(File.Open(filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
            }

            public char GetChar()
            {
                int i = Reader.Read();
                while (i < 32 && i != '\n')
                {
                    if (i == EndOfCSV) return '\0'; // FF (formfeed) End of CSV data
                    if (i == -1) return '\0';  // End of stream
                    i = Reader.Read();
                }
                return (char)i;
            }
            public char PeekChar()
            {
                return (char)Reader.Peek();
            }
            public void Dispose()
            {
                if (!ReaderCreatedHere) return;
                if (Reader != null) { Reader.Close(); Reader.Dispose();  Reader = null; }
            }
        }
    }
}
