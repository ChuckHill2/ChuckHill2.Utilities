//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="DataReader.cs" company="Chuck Hill">
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
using System.Data;

namespace ChuckHill2
{
    /// <summary>
    /// Generic Base class for ALL custom DataReader's.
    /// </summary>
    [Serializable]
    public abstract class DataReaderBase<T> : IDataReader where T : class
    {
        private T m_datasource = null;

        public DataReaderBase() { }
        public DataReaderBase(T datasource) { m_datasource = datasource; }
        public T DataSource { get { return m_datasource; } protected set { m_datasource = value; } }

        #region IDataReader Members
        public abstract void Close();
        public abstract bool Read();

        public bool IsClosed { get { return (m_datasource == null); } }

        public virtual int Depth { get { return 0; } }
        public virtual DataTable GetSchemaTable() { return null; }
        public virtual bool NextResult() { return false; }
        public virtual int RecordsAffected { get { return -1; } }
        #endregion IDataReader Members

        #region IDataRecord Members
        public abstract int FieldCount { get; }
        public abstract string GetName(int i);
        public abstract object GetValue(int i);

        public virtual int GetOrdinal(string name) { return -1; } //needed exclusively for this[string name], above
        public virtual Type GetFieldType(int i) { return null; }

        public string GetDataTypeName(int i) { Type t = GetFieldType(i); return t == null ? null : t.Name; }
        public bool IsDBNull(int i) { Object v = GetValue(i); return (v == null || v is System.DBNull); }

        public object this[string name] { get { int i = this.GetOrdinal(name); if (i < 0) return null; return this.GetValue(i); } }
        public object this[int i] { get { if (i < 0) return null; return this.GetValue(i); } }

        public IDataReader GetData(int i) { throw new NotImplementedException(); }

        #region Get and Convert API (implemented)
        public bool GetBoolean(int i) 
        {
            Object o = GetValue(i);
            if (o == null) return false;
            if (o is Boolean) return (Boolean)o;
            string v = o.ToString();
            if (v.Length==0) return false;
            char c = v[0];
            return (c == '1' || c == 't' || c == 'T' || c == 'y' || c == 'Y'); // == 1/0, true/false, yes/no
        }
        public byte GetByte(int i)
        {
            Object o = GetValue(i);
            if (o == null) return 0;
            if (o is Byte) return (Byte)o;
            Byte v;
            if (Byte.TryParse(o.ToString(), out v)) return v;
            return 0;
        }
        public long GetBytes(int i, long fieldOffset, byte[] buffer, int bufferoffset, int length)
        {
            if (bufferoffset >= buffer.Length) return 0;
            Object o = GetValue(i);
            if (o == null) return 0;
            Byte[] v;
            if (o is Byte[]) v = (Byte[])o;
            try { v = (Byte[])Convert.ChangeType(o, typeof(Byte[])); }
            catch { return 0; }
            if (fieldOffset >= v.Length) return 0;
            int len = 0;
            for (; bufferoffset >= buffer.Length; bufferoffset++)
            {
                buffer[bufferoffset] = v[fieldOffset++];
                if (fieldOffset >= v.Length) return 0;
                len++;
            }
            return len;
        }
        public char GetChar(int i)
        {
            Object o = GetValue(i);
            if (o == null) return '\0';
            if (o is Char) return (Char)o;
            string v = o.ToString();
            if (v.Length == 0) return '\0';
            return v[0];
        }
        public long GetChars(int i, long fieldOffset, Char[] buffer, int bufferoffset, int length)
        {
            if (bufferoffset >= buffer.Length) return 0;
            Object o = GetValue(i);
            if (o == null) return 0;
            Char[] v;
            if (o is Char[]) v = (Char[])o;
            try { v = (Char[])Convert.ChangeType(o, typeof(Char[])); }
            catch { return 0; }
            if (fieldOffset >= v.Length) return 0;
            int len = 0;
            for (; bufferoffset >= buffer.Length; bufferoffset++)
            {
                buffer[bufferoffset] = v[fieldOffset++];
                if (fieldOffset >= v.Length) return 0;
                len++;
            }
            return len;
        }
        public DateTime GetDateTime(int i)
        {
            Object o = GetValue(i);
            if (o == null) return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
            if (o is DateTime) return (DateTime)o;
            DateTime v;
            if (DateTime.TryParse(o.ToString(), out v)) return v;
            return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
        }
        public decimal GetDecimal(int i)
        {
            Object o = GetValue(i);
            if (o == null) return 0;
            if (o is decimal) return (decimal)o;
            decimal v;
            if (decimal.TryParse(o.ToString(), out v)) return v;
            return 0;
        }
        public double GetDouble(int i)
        {
            Object o = GetValue(i);
            if (o == null) return 0.0;
            if (o is double) return (double)o;
            double v;
            if (double.TryParse(o.ToString(), out v)) return v;
            return 0.0;
        }
        public float GetFloat(int i)
        {
            Object o = GetValue(i);
            if (o == null) return 0.0f;
            if (o is float) return (float)o;
            float v;
            if (float.TryParse(o.ToString(), out v)) return v;
            return 0.0f;
        }
        public Guid GetGuid(int i)
        {
            Object o = GetValue(i);
            if (o == null) return Guid.Empty;
            if (o is Guid) return (Guid)o;
            Guid v;
            if (Guid.TryParse(o.ToString(), out v)) return v;
            return Guid.Empty;
        }
        public short GetInt16(int i)
        {
            Object o = GetValue(i);
            if (o == null) return 0;
            if (o is short) return (short)o;
            short v;
            if (short.TryParse(o.ToString(), out v)) return v;
            return 0;
        }
        public int GetInt32(int i)
        {
            Object o = GetValue(i);
            if (o == null) return 0;
            if (o is int) return (int)o;
            int v;
            if (int.TryParse(o.ToString(), out v)) return v;
            return 0;
        }
        public long GetInt64(int i)
        {
            Object o = GetValue(i);
            if (o == null) return 0;
            if (o is long) return (long)o;
            long v;
            if (long.TryParse(o.ToString(), out v)) return v;
            return 0;
        }
        public string GetString(int i)
        {
            Object o = GetValue(i);
            if (o == null) return String.Empty;
            if (o is String) return (String)o;
            return o.ToString();
        }
        public int GetValues(object[] values)
        {
            if (values==null) return 0;
            int i = 0;
            for (; i<values.Length && i<this.FieldCount; i++)
            {
                values[i] = GetValue(i);
            }
            return i;
        }
        #endregion
        #endregion IDataRecord Members

        #region IDisposable Members (implemented)
        public void Dispose() { this.Close(); }
        #endregion IDisposable Members
    }
}
