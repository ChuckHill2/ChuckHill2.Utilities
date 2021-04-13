//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="SqlClient.cs" company="Chuck Hill">
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
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ChuckHill2.Extensions;
using ChuckHill2.Extensions.Reflection;

namespace ChuckHill2
{
    /// <summary>
    /// Sql.Net Utilities
    /// </summary>
    public static class SqlClient
    {
        private const bool STRICT = false; //True to ensure that type properties exactly match sql query results or an exception is thrown. False to ignore mismatches and coerce results into the specified types or set to the type's default value.

        [ThreadStatic] private static SqlConnection Connection = null;
        [ThreadStatic] private static bool _KeepConnection = false;
        [ThreadStatic] private static int _CommandTimeout = 30; //default == 30 seconds

        /// <summary>
        /// Keep connection to DB open for multiple operations that depend upon on the previous sql call
        /// </summary>
        public static bool KeepConnection
        {
            get
            {
                return _KeepConnection;
            }
            set
            {
                if (!value && Connection != null)
                {
                    Connection.Dispose();
                    Connection = null;
                }
                _KeepConnection = value;
                if (value && Connection != null)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Set time to wait for a SQL command to execute. If the command exceeds this value a timeout exception will occur.
        /// </summary>
        public static int CommandTimeout
        {
            get
            {
                return _CommandTimeout;
            }
            set
            {
                _CommandTimeout = value < 30 ? 30 : value;
            }
        }

        /// <summary>
        /// Retrieve a single value from a SQL query.
        /// Will throw formatted exception upon error.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="query">parameterized query format string (see string.Format()) or stored procedure</param>
        /// <param name="args">format replacement args or stored procedure parameters</param>
        /// <returns>The result. May be null.</returns>
        public static Object ExecuteScalar(string connectionString, string query, params object[] args)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateSqlCmd(connectionString, query, args);
                object v = cmd.ExecuteScalar();
                return (v is DBNull ? null : v);
            }
            catch (Exception ex)
            {
                throw ex.AppendMessage(string.Format("ExecuteScalar(\"{0}\",\"{1}\",{{{2}}})", HideCSPwd(connectionString), QueryForException(query), ParamsToString(args)));
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Retrieve a single value from a SQL query.
        /// Will throw formatted exception upon error.
        /// </summary>
        /// <typeparam name="T">Type to cast the return value into.</typeparam>
        /// <param name="connectionString"></param>
        /// <param name="query">parameterized query format string (see string.Format()) or stored procedure</param>
        /// <param name="args">format replacement args or stored procedure parameters</param>
        /// <returns>The result. null values get converted into their associated default value (aka Int32 type returns 0, strings into "")</returns>
        public static T ExecuteScalar<T>(string connectionString, string query, params object[] args)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateSqlCmd(connectionString, query, args);
                object v = cmd.ExecuteScalar();
                //null return values are not allowed.
                return (T)ConvertTo(typeof(T), v);
            }
            catch(Exception ex)
            {
                throw ex.AppendMessage(string.Format("ExecuteScalar<{0}>(\"{1}\",\"{2}\",{{{3}}})", typeof(T).Name, HideCSPwd(connectionString), QueryForException(query), ParamsToString(args)));
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Retrieve a table of data from a Sql query statement.
        /// Will throw formatted exception upon error.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="query">parameterized query format string (see string.Format()) or stored procedure</param>
        /// <param name="args">format replacement args or stored procedure parameters</param>
        /// <returns>Filled dataTable of results.</returns>
        public static DataTable ExecuteQuery(string connectionString, string query, params object[] args)
        {
            SqlCommand cmd = null;
            SqlDataAdapter da = null;
            try
            {
                cmd = CreateSqlCmd(connectionString, query, args);
                da = new SqlDataAdapter(cmd);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;
            }
            catch(Exception ex)
            {
                throw ex.AppendMessage(string.Format("ExecuteQuery(\"{0}\",\"{1}\",{{{2}}})", HideCSPwd(connectionString), QueryForException(query), ParamsToString(args)));
            }
            finally
            {
                if (da != null) da.Dispose();
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Retrieve a table of data from a Sql query statement.
        /// Will throw formatted exception upon error.
        /// Note: field/property name must be the same as the database column name (case-insensitive).
        /// </summary>
        /// <typeparam name="T">
        ///   Typeof array element. If the array element is a primitive type, DateTime, or Guid 
        ///   but not string, only the first column in the query is used. String does not have a 
        ///   default constructor!
        /// </typeparam>
        /// <param name="connectionString"></param>
        /// <param name="query">parameterized query format string (see string.Format()) or stored procedure</param>
        /// <param name="args">format replacement args or stored procedure parameters</param>
        /// <returns>Generic list of typed class objects containing the retrieved results. null valuetype values get converted into their associated default value (aka Int32 type == 0, strings == "")</returns>
        public static List<T> ExecuteQuery<T>(string connectionString, string query, params object[] args)
        {
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            List<T> list = new List<T>();
            Type t = typeof(T);

            //Is the return type a simple list of values?
            bool isPrimitive = t.IsPrimitive || t == typeof(string) || t == typeof(Guid) || t == typeof(Enum);

            try
            {
                cmd = CreateSqlCmd(connectionString, query, args);
                reader = cmd.ExecuteReader(CommandBehavior.SingleResult);
                if (isPrimitive)
                {
                    while (reader.Read())
                    {
                        list.Add((T)ConvertTo(t, reader.GetValue(0))); //we ignore all the rest of the columns.
                    }

                    return list;
                }

                var fields = FieldProp.GetProperties(reader, typeof(T), STRICT);
                
                //Find typeparam parameterless instance constructor. It may be public or private.
                var ci = t.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, Type.EmptyTypes, null);
                var constructorArgs = new object[] { }; //for efficiency

                while (reader.Read())
                {
                    var item = (T)ci.Invoke(constructorArgs);
                    for(int i=0; i<fields.Length; i++)
                    {
                        if (fields[i].Type == null) continue; //matching field not in query
                        fields[i].SetValue(item, ConvertTo(fields[i].Type, reader.GetValue(i)));
                    }
                    list.Add(item);
                }
            }
            catch(Exception ex)
            {
                throw ex.AppendMessage(string.Format("ExecuteQuery<{0}>(\"{1}\",\"{2}\",{{{3}}})", typeof(T).Name, HideCSPwd(connectionString), QueryForException(query), ParamsToString(args)));
            }
            finally
            {
                if (reader != null) reader.Dispose();
                if (cmd != null) cmd.Dispose();
            }
            return list;
        }

        /// <summary>
        /// Retrieve a table of data from a Sql query statement.
        /// Will throw formatted exception upon error.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="query">parameterized query format string (see string.Format()) or stored procedure</param>
        /// <param name="args">format replacement args or stored procedure parameters</param>
        /// <returns>SqlDatataReader object</returns>
        public static SqlDataReader ExecuteReader(string connectionString, string query, params object[] args)
        {
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
                cmd = CreateSqlCmd(connectionString, query, args);
                reader = cmd.ExecuteReader(CommandBehavior.SingleResult|CommandBehavior.CloseConnection);
                return reader;
            }
            catch (Exception ex)
            {
                if (reader != null) reader.Dispose();
                if (cmd != null) cmd.Dispose();
                throw ex.AppendMessage(string.Format("ExecuteReader(\"{0}\",\"{1}\",{{{2}}})", HideCSPwd(connectionString), QueryForException(query), ParamsToString(args)));
            }
        }

        /// <summary>
        /// Retrieve a single record from a DB table. Any subsequent rows are ignored.
        /// Will throw formatted exception upon error.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="query">parameterized query format string (see string.Format()) or stored procedure</param>
        /// <param name="args">format replacement args or stored procedure parameters</param>
        /// <returns>A dictionary of results. null valuetype values get converted into their associated default value (aka Int32 type == 0, strings == "")</returns>
        public static Dictionary<string, object> ExecuteQuerySingleRow(string connectionString, string query, params object[] args)
        {
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
                cmd = CreateSqlCmd(connectionString, query, args);
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                if (reader.Read())
                {
                    Dictionary<string, object> record = new Dictionary<string, object>(reader.FieldCount, StringComparer.InvariantCultureIgnoreCase); //re-create with actual capacity.
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        record.Add(reader.GetName(i), ConvertTo(reader.GetFieldType(i), reader.GetValue(i)));
                    }
                    return record;
                }
                else return new Dictionary<string, object>(0, StringComparer.InvariantCultureIgnoreCase);
            }
            catch (Exception ex)
            {
                throw ex.AppendMessage(string.Format("ExecuteQuerySingleRow(\"{0}\",\"{1}\",{{{2}}})", HideCSPwd(connectionString), QueryForException(query), ParamsToString(args)));
            }
            finally
            {
                if (reader != null) reader.Dispose();
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Retrieve a single record from a DB table. Any subsequent rows are ignored.
        /// Will throw formatted exception upon error.
        /// Note: field/property name must be the same as the database column name (case-insensitive).
        /// </summary>
        /// <typeparam name="T">Typeof class object to return.</typeparam>
        /// <param name="connectionString"></param>
        /// <param name="query">parameterized query format string (see string.Format()) or stored procedure</param>
        /// <param name="args">format replacement args or stored procedure parameters</param>
        /// <returns>Typed class containing the retrieved results. null valuetype values get converted into their associated default value (aka Int32 type == 0, strings == "")</returns>
        public static T ExecuteQuerySingleRow<T>(string connectionString, string query, params object[] args) where T : class, new()
        {
            SqlCommand cmd = null;
            SqlDataReader reader = null;
            try
            {
                cmd = CreateSqlCmd(connectionString, query, args);
                reader = cmd.ExecuteReader(CommandBehavior.SingleRow);
                var fields = FieldProp.GetProperties(reader, typeof(T), STRICT);
                if (reader.Read())
                {
                    T item = new T();
                    for (int i = 0; i < fields.Length; i++)
                    {
                        if (fields[i].Type == null) continue; //matching field not in query
                        fields[i].SetValue(item, ConvertTo(fields[i].Type, reader.GetValue(i)));
                    }
                    return item;
                }
            }
            catch (Exception ex)
            {
                throw ex.AppendMessage(string.Format("ExecuteQuerySingleRow<{0}>(\"{1}\",\"{2}\",{{{3}}})", typeof(T).Name, HideCSPwd(connectionString), (query ?? string.Empty).Trim(), ParamsToString(args)));
            }
            finally
            {
                if (reader != null) reader.Dispose();
                if (cmd != null) cmd.Dispose();
            }
            return null;
        }

        /// <summary>
        /// Execute a Sql statement with no regard for any output values.
        /// Will throw formatted exception upon error.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <param name="query">parameterized query format string (see string.Format()) or stored procedure</param>
        /// <param name="args">format replacement args or stored procedure parameters</param>
        /// <returns>The count of DB rows changed or -1 for non-table oriented sql statements</returns>
        public static int ExecuteNonQuery(string connectionString, string query, params object[] args)
        {
            SqlCommand cmd = null;
            try
            {
                cmd = CreateSqlCmd(connectionString, query, args);
                return cmd.ExecuteNonQuery(); //for non-table oriented sql statements, the return value is -1
            }
            catch (Exception ex)
            {
                throw ex.AppendMessage(string.Format("ExecuteNonQuery(\"{0}\",\"{1}\",{{{2}}})", HideCSPwd(connectionString), QueryForException(query), ParamsToString(args)));
            }
            finally
            {
                if (cmd != null) cmd.Dispose();
            }
        }

        /// <summary>
        /// Handy helper function to hide the connection string password (if any) for logging purposes.
        /// This is needed for logging so as not to reveal the password.
        /// The resulting connection string is NOT usable! For logging only.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>modified connection string with password replaced with '*'s.</returns>
        public static string HideCSPwd(string connectionString)
        {
            if (connectionString.IsNullOrEmpty()) return string.Empty;
            try
            {
                //'Data Source=(local);Initial Catalog=MyDataBase;Integrated Security=SSPI'
                //'Data Source=(local);Initial Catalog=MyDataBase;User ID=user2;Password=myuserpass'
                //'Data Source=(local);Initial Catalog=MyDataBase;User ID=user3;Password="Wild pass2"'
                if (!connectionString.ContainsI("Password=")) return connectionString;
                SqlConnectionStringBuilder sb = new SqlConnectionStringBuilder(connectionString);
                if (sb.Password.IsNullOrEmpty()) return connectionString;
                sb.Password = string.Empty.PadRight(sb.Password.Length, '*');
                return sb.ConnectionString;
            }
            catch { return connectionString; }
        }

        /// <summary>
        /// Extension for formatting a SQL command query for logging.
        /// Handles both plain text queries and parameterized stored procedures.
        /// </summary>
        /// <param name="cmd"></param>
        /// <returns></returns>
        public static string ToStringEx(this DbCommand cmd)
        {
            var sb = new StringBuilder();
            string command = cmd.CommandText.Trim();
            if (command.Contains("\n")) command = command.Trim();
            sb.AppendFormat("SqlExec [{0}].[{1}] ", cmd.Connection.DataSource, cmd.Connection.Database);
            sb.Append(command);
            foreach (DbParameter p in cmd.Parameters)
            {
                DbType t = p.DbType;
                sb.Append(", ");
                sb.Append(p.ParameterName);
                sb.Append('=');
                string value;
                if (p.Value is DateTime) value = ((DateTime)p.Value).ToString("s");
                else if (p.Value is DateTimeOffset) value = ((DateTimeOffset)p.Value).ToString("s");
                else if (p.Value is String) value = string.Concat('"', p.Value, '"');
                else value = p.Value.ToString();
                sb.Append(value);
            }
            return sb.ToString();
        }

        /// <summary>
        /// Reader may hang on close if the reader has not completed. This allows the caller to cancel execution of the underlying command.
        /// </summary>
        /// <param name="reader"></param>
        public static void Cancel(SqlDataReader reader)
        {
            if (reader.IsClosed) return;
            SqlCommand cmd = reader.GetReflectedValue("Command") as SqlCommand; // reader.Command is internal!
            cmd.Cancel();
        }

        /// <summary>
        /// Close and dispose the entire reader, command, and connection, given the SqlDataReader object;
        /// </summary>
        /// <param name="reader"></param>
        public static void DisposeConnection(SqlDataReader reader)
        {
            SqlCommand cmd = reader.GetReflectedValue("Command") as SqlCommand; // reader.Command is internal!
            if (!reader.IsClosed)
            {
                cmd.Cancel();
                reader.Dispose();
            }
            if (cmd != null) cmd.Dispose();
        }

        #region [Private helper utilities]
        /// <summary>
        /// Robust data conversion. Never throws an exception. Returns the type's default value instead.
        /// Note: The default value for a string is "". Not null. Also, string's leading and trailing whitespace is trimmed.
        /// </summary>
        /// <param name="t">Type of object to convert to</param>
        /// <param name="value">Object to convert</param>
        /// <returns>Converted result</returns>
        private static object ConvertTo(Type t, object value)
        {
            var isNullable = false;

            try
            {
                if (value is DBNull)
                {
                    value = null;
                }

                if (t == typeof(string))
                {
                    if (value == null)
                    {
                        return string.Empty;
                    }

                    return value.ToString().Trim();
                }

                if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(System.Nullable<>) && t.GenericTypeArguments.Length > 0)
                {
                    t = t.GenericTypeArguments[0];
                    isNullable = true;
                }

                if (value == null)
                {
                    return t.IsValueType && !isNullable ? Activator.CreateInstance(t) : null;
                }

                if (t.IsEnum)
                {
                    if (t.IsPrimitive)
                    {
                        return Enum.ToObject(t, value);
                    }

                    try
                    {
                        return Enum.Parse(t, value.ToString(), true);
                    }
                    catch
                    {
                        return Enum.ToObject(t, 0);
                    }
                }

                //Convert.ChangeType does not understand anything but true/false, so we have to handle it...
                if (t == typeof(bool))
                {
                    if (value is bool)
                    {
                        return value;
                    }

                    string s = value.ToString().Trim();

                    if (s.Length == 0 || s.Length > 5)
                    {
                        return false;
                    }

                    s = s.ToUpper(CultureInfo.InvariantCulture);

                    var ch = s[0];
                    if ((s.Length == 1 && "TFYN10".Any(m => m == ch)) || (s.Length > 1 && "|TRUE|FALSE|YES|NO|".Contains(string.Concat('|', s, '|'))))
                    {
                        return ch == 'Y' || ch == 'T' || ch == '1';
                    }

                    return false;
                }

                //Convert.ChangeType does not understand Guid's
                if (t == typeof(Guid))
                {
                    if (value is Guid)
                    {
                        return value;
                    }

                    Guid g;
                    if (Guid.TryParse(value.ToString(), out g))
                    {
                        return g;
                    }

                    return Guid.Empty;
                }

                //Convert.ChangeType does not a numeric string datetime format.
                if (t == typeof(DateTime))
                {
                    string s = value.ToString().Trim();
                    if (s.Length == 16 && s.All(c => c >= '0' && c <= '9'))
                    {
                        return DateTime.ParseExact(s, "yyyyMMddHHmmssff", null, DateTimeStyles.None);
                    }
                }

                if (t == typeof(DateTimeOffset))
                {
                    string s = value.ToString().Trim();
                    if (s.Length == 16 && s.All(c => c >= '0' && c <= '9'))
                    {
                        return DateTimeOffset.ParseExact(s, "yyyyMMddHHmmssff", null, DateTimeStyles.None);
                    }
                }

                return Convert.ChangeType(value, t);
            }
            catch
            {
                //do nothing. Use default values below...
            }

            if (t == typeof(string))
            {
                return string.Empty;
            }

            return t.IsValueType && !isNullable ? Activator.CreateInstance(t) : null;
        }

        private struct FieldProp
        {
            public string Name;
            public Type Type;
            public Action<object, object> SetValue;
            public Func<object, object> GetValue;
            //Cache previous calls for performance, but make them weak so GC can cleanup if it really needs the memory.
            private static readonly Dictionary<string, WeakReference<FieldProp[]>> ExistingFieldArrays = new Dictionary<string, WeakReference<FieldProp[]>>();

            public override string ToString()
            {
                return string.Concat(Name ?? string.Empty, ", ", (Type ?? typeof(DBNull)).Name);
            }

            /// <summary>
            /// Get CACHED array of target class field/property properties that match IDataReader names to class
            /// field/property names or alias (see System.ComponentModel.DataAnnotations.Schema.ColumnAttribute).
            /// NULL type or SetValue objects means that IDataReader field has no matching class field/property.
            /// Class fields/properties may be private and/or readonly. 
            /// Properties must have public or private setters or they are ignored. 
            /// Static fields/properties are ignored.
            /// Note: field/property name must be the same as the database column name (case-insensitive).
            /// Exceptions are: 
            /// (1) Database column name has an alias (e.g. Sql statement= ...,xoxoxo [MyResult],...)
            /// (2) Data class read into has fields or properties with the [Column("MyResult")] attribute.
            /// </summary>
            /// <param name="reader">Datareader to read properties from</param>
            /// <param name="t">NET Type to read into</param>
            /// <param name="strict">True to ensure that type properties exactly match sql query results or an exception is thrown. False to ignore mismatches and coerce results into the specified types or set to the type's default value.</param>
            /// <returns>Array of class member properties that match IDataReader fields</returns>
            public static FieldProp[] GetProperties(IDataReader reader, Type t, bool strict)
            {
                var getInternalProperties = strict ? (Func<IDataReader, Type, FieldProp[]>)InternalGetPropertiesStrict : (Func<IDataReader, Type, FieldProp[]>)InternalGetPropertiesForgiving;
                var sb = new StringBuilder(t.FullName);
                sb.Append('|');
                sb.Append(strict);
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    sb.Append('|');
                    sb.Append(reader.GetName(i)); //Just run everything together. It's for unique key only.
                }

                var key = sb.ToString();

                FieldProp[] fieldprops = null;
                WeakReference<FieldProp[]> weakprops;
                if (ExistingFieldArrays.TryGetValue(key, out weakprops))
                {
                    if (weakprops.TryGetTarget(out fieldprops))
                    {
                        return fieldprops;
                    }

                    fieldprops = getInternalProperties(reader, t);
                    weakprops.SetTarget(fieldprops);
                    return fieldprops;
                }

                fieldprops = getInternalProperties(reader, t);
                ExistingFieldArrays[key] = new WeakReference<FieldProp[]>(fieldprops);
                return fieldprops;
            }

            /// <summary>
            /// Strict like EF. Data model properties must match the Sql query column properties exactly.
            /// Data model properties may have the [NotMapped] or [Column("queryColumnName")] attributes 
            /// to match the EF logic.
            /// </summary>
            /// <param name="reader">Datareader to read properties from</param>
            /// <param name="t">NET Type to read into</param>
            /// <returns>Array of class member properties that match IDataReader fields</returns>
            private static FieldProp[] InternalGetPropertiesStrict(IDataReader reader, Type t)
            {
                List<string> orphanedQueryColumns = null;
                List<string> typeMismatch = null;

                //Create a clean and smaller memberinfo search list.
                var members = new Dictionary<string, PropertyInfo>(StringComparer.InvariantCulture);
                foreach (var m in t.GetProperties().Where(x => x.CanRead && x.CanWrite))
                {
                    var name = m.Attribute<NotMappedAttribute>();
                    if (name != null)
                    {
                        continue;
                    }

                    name = m.Attribute<ColumnAttribute>();
                    members[!name.IsNullOrEmpty() ? name : m.Name] = m;
                }

                FieldProp[] fields = new FieldProp[reader.FieldCount];
                for (int i = 0; i < fields.Length; i++)
                {
                    string name = reader.GetName(i);
                    PropertyInfo pi = null;
                    members.TryGetValue(name, out pi);

                    //Matching property does not exist for query column
                    if (pi == null)
                    {
                        if (orphanedQueryColumns == null)
                        {
                            orphanedQueryColumns = new List<string>();
                        }

                        orphanedQueryColumns.Add(name);
                        continue;
                    }

                    //Property type does not match query column type
                    var tt = pi.PropertyType;
                    if (tt.IsGenericType && tt.GetGenericTypeDefinition() == typeof(System.Nullable<>) && tt.GenericTypeArguments.Length > 0)
                    {
                        tt = tt.GenericTypeArguments[0];
                    }

                    if (reader.GetFieldType(i) != tt)
                    {
                        if (typeMismatch == null)
                        {
                            typeMismatch = new List<string>();
                        }

                        typeMismatch.Add(name);
                        members.Remove(name);
                        continue;
                    }

                    //Property and query column match!
                    fields[i].Name = name;
                    fields[i].Type = pi.PropertyType; //leave as nullable type so ConvertTo() knows how to handle it properly.
                    fields[i].GetValue = pi.GetValue;
                    fields[i].SetValue = pi.SetValue;
                    members.Remove(name);
                }

                //Get bad columns from strict matching. Format nice message and throw

                StringBuilder sb = null;
                if (orphanedQueryColumns != null)
                {
                    sb = new StringBuilder();
                    sb.Append("Query columns: ");
                    sb.Append(string.Join(", ", orphanedQueryColumns));
                    sb.Append(", do not have a matching case-sensitive, read/writeable '");
                    sb.Append(t.Name);
                    sb.Append("' properties.");
                }

                if (typeMismatch != null)
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder();
                    }
                    else
                    {
                        sb.AppendLine();
                    }

                    sb.Append("Data model '");
                    sb.Append(t.Name);
                    sb.Append("' properties: ");
                    sb.Append(string.Join(", ", typeMismatch));
                    sb.Append(", do not have the same value type as the matching Sql query columns.");
                }

                if (members.Count > 0)
                {
                    if (sb == null)
                    {
                        sb = new StringBuilder();
                    }
                    else
                    {
                        sb.AppendLine();
                    }

                    sb.Append("Data model '");
                    sb.Append(t.Name);
                    sb.Append("' case-sensitive properties: ");
                    sb.Append(string.Join(", ", members.Keys.ToArray()));
                    sb.Append(", do not exist in the Sql query column headings.");
                }

                if (sb != null)
                {
                    throw new MissingMemberException(sb.ToString());
                }

                return fields;
            }

            private static FieldProp[] InternalGetPropertiesForgiving(IDataReader reader, Type t)
            {
                //Create a clean and smaller memberinfo search list.
                var members = new Dictionary<string, MemberInfo>(StringComparer.InvariantCultureIgnoreCase);
                foreach (var m in t.GetMembers(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property)
                    {
                        string name = m.Attribute<ColumnAttribute>();
                        if (!name.IsNullOrEmpty())
                        {
                            members[name] = m;
                        }

                        members[m.Name] = m;
                    }
                }

                FieldProp[] fields = new FieldProp[reader.FieldCount];
                for (int i = 0; i < fields.Length; i++)
                {
                    string name = reader.GetName(i);
                    MemberInfo mi = null;
                    members.TryGetValue(name, out mi);
                    if (mi == null)
                    {
                        continue;
                    }

                    fields[i].Name = name;
                    if (mi.MemberType == MemberTypes.Field)
                    {
                        FieldInfo fi = (FieldInfo)mi;
                        fields[i].Type = fi.FieldType;
                        fields[i].GetValue = fi.GetValue;
                        fields[i].SetValue = fi.SetValue;
                    }
                    else
                    {
                        PropertyInfo pi = (PropertyInfo)mi;
                        fields[i].Type = pi.PropertyType;
                        if (pi.CanRead)
                        {
                            fields[i].GetValue = pi.GetValue;
                        }
                        else
                        {
                            fields[i].GetValue = delegate(object obj) { return ConvertTo(pi.PropertyType, null); };
                        }

                        if (pi.CanWrite)
                        {
                            fields[i].SetValue = pi.SetValue;
                        }
                        else
                        {
                            fields[i].SetValue = delegate(object obj, object value) { };
                        }
                    }
                }

                return fields;
            }
        }

        private static SqlCommand CreateSqlCmd(string connectionString, string query, params object[] args)
        {
            SqlConnection conn = null;
            SqlCommand cmd = null;

            if (connectionString.ContainsI("Provider"))
            {
                var sb = new DbConnectionStringBuilder();
                sb.ConnectionString = connectionString;
                sb.Remove("Provider");
                connectionString = sb.ConnectionString;
            }

            if (connectionString.IsNullOrEmpty()) throw new ArgumentNullException("ConnectionString is Empty");
            if (query.IsNullOrEmpty()) throw new ArgumentNullException("Query string is Empty");
            try
            {
                if (args != null && args.Length > 0 && query.Contains("{0")) query = string.Format(query, args);
                query = query.Trim();

                if (KeepConnection)
                {
                    if (Connection == null) Connection = conn = new SqlConnection(connectionString);
                    else conn = Connection;
                }
                else
                {
                    conn = new SqlConnection(connectionString);
                }

                cmd = conn.CreateCommand();
                cmd.Parameters.AddRange(ToSqlParameters(args));
                cmd.CommandType = query.Contains(' ') ? CommandType.Text : CommandType.StoredProcedure;
                cmd.CommandText = query;
                cmd.CommandTimeout = CommandTimeout;
                if (conn.State != ConnectionState.Open) conn.Open();
                cmd.Disposed += delegate(object sender, EventArgs e)
                {
                    ((SqlCommand)sender).Parameters.Clear(); //Remove parameters from collection so the SqlParameters may be reused in other calls.
                    if (!KeepConnection) ((SqlCommand)sender).Connection.Dispose();
                };
            }
            catch
            {
                if (cmd != null) cmd.Dispose();
                if (conn != null)
                {
                    conn.Dispose();
                    if (KeepConnection) Connection = null;
                }
                throw;
            }
            return cmd;
        }

        /// <summary>
        /// Convert array of sql arguments into an array of SqlParameters.
        /// Note: Sql queries from within the .NET framework absolutely require
        /// parameter names. The names may or may not contain the sql variable 
        /// prefix '@'. It is quietly added if it does not exist.
        /// </summary>
        /// <param name="args">Any array of sql statement parameter values</param>
        /// <returns>Array of SqlParameters</returns>
        private static SqlParameter[] ToSqlParameters(IEnumerable args)
        {
            PropertyInfo pikey = null;
            PropertyInfo pivalue = null;
            List<SqlParameter> parameters = new List<SqlParameter>();

            if (args != null)
            {
                //quietly skips invalid type arguments.
                foreach (var arg in args)
                {
                    if (arg == null)
                    {
                        continue;
                    }

                    if (arg is IEnumerable)
                    {
                        parameters.AddRange(ToSqlParameters((IEnumerable)arg));
                        continue;
                    }

                    if (arg is SqlParameter)
                    {
                        parameters.Add((SqlParameter)arg);
                        continue;
                    }

                    if (arg is IDataParameter)
                    {
                        var p = (IDataParameter)arg;
                        parameters.Add(
                            new SqlParameter()
                            {
                                DbType = p.DbType,
                                Direction = p.Direction,
                                IsNullable = p.IsNullable,
                                ParameterName = p.ParameterName,
                                SourceColumn = p.SourceColumn,
                                SourceVersion = p.SourceVersion,
                                Value = p.Value
                            });
                        continue;
                    }

                    if (arg is DictionaryEntry)
                    {
                        var p = (DictionaryEntry)arg;
                        parameters.Add(new SqlParameter(p.Key.ToString(), p.Value));
                        continue;
                    }

                    if (arg is KeyValuePair<string, object>)
                    {
                        var p = (KeyValuePair<string, object>)arg;
                        parameters.Add(new SqlParameter(p.Key, p.Value));
                        continue;
                    }

                    Type t = arg.GetType(); //The KeyValuePair value type can be anything.
                    if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) && t.GenericTypeArguments[0] == typeof(string))
                    {
                        if (pikey == null)
                        {
                            pikey = t.GetProperty("Key");
                        }

                        if (pivalue == null)
                        {
                            pivalue = t.GetProperty("Value");
                        }

                        parameters.Add(new SqlParameter(pikey.GetValue(arg).ToString(), pivalue.GetValue(arg)));
                        continue;
                    }
                }
            }

            return parameters.ToArray();
        }

        /// <summary>
        /// Convert Sql parameters array into a formatted string for logging purposes
        /// </summary>
        /// <param name="args">Array of parameters</param>
        /// <returns>Formatted string</returns>
        private static string ParamsToString(IEnumerable args)
        {
            if (args == null)
            {
                return string.Empty;
            }

            PropertyInfo pikey = null;
            PropertyInfo pivalue = null;
            var sb = new StringBuilder();

            foreach (object arg in args)
            {
                Type t = (arg ?? typeof(DBNull)).GetType();
                string key = null;
                object value = null;

                if (arg == null)
                {
                    value = "null";
                }
                else if (arg is IEnumerable)
                {
                    sb.Append(ParamsToString((IEnumerable)arg));
                    sb.Append(", ");
                    continue;
                }
                else if (arg is SqlParameter)
                {
                    var p = (SqlParameter)arg;
                    key = p.ParameterName;
                    value = p.Value ?? (p.Direction != ParameterDirection.Input ? "[out]" : "null");
                }
                else if (arg is IDataParameter)
                {
                    var p = (IDataParameter)arg;
                    key = p.ParameterName;
                    value = p.Value;
                }
                else if (arg is DictionaryEntry)
                {
                    var p = (DictionaryEntry)arg;
                    key = p.Key.ToString();
                    value = p.Value;
                }
                else if (arg is KeyValuePair<string, object>)
                {
                    var p = (KeyValuePair<string, object>)arg;
                    key = p.Key;
                    value = p.Value;
                }
                else if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(KeyValuePair<,>) && t.GenericTypeArguments[0] == typeof(string))
                {
                    if (pikey == null)
                    {
                        pikey = t.GetProperty("Key");
                    }

                    if (pivalue == null)
                    {
                        pivalue = t.GetProperty("Value");
                    }

                    key = pikey.GetValue(arg) as string;
                    value = pivalue.GetValue(arg);
                }
                else
                {
                    value = arg;
                }

                if (value is DateTime)
                {
                    value = ((DateTime)value).ToString("s");
                }
                else if (value is DateTimeOffset)
                {
                    value = ((DateTimeOffset)value).ToString("s");
                }
                else if (value is string)
                {
                    value = string.Concat('"', value, '"');
                }
                else
                {
                    value = (value ?? "null").ToString();
                }

                if (key != null)
                {
                    sb.AppendFormat("{0}={1}, ", key, value);
                }
                else
                {
                    sb.Append(value);
                    sb.Append(", ");
                }
            }

            if (sb.Length >= 2)
            {
                sb.Length -= 2; //remove trailing ", "
            }

            return sb.ToString();
        }

        private static string QueryForException(string query)
        {
            var q = (query ?? string.Empty).Trim();
            return q.Length > 160 ? "[query]" : q;
        }
        #endregion
    }
}

