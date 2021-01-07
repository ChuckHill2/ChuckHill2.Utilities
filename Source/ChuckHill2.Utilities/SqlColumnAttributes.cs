using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text.RegularExpressions;
using ChuckHill2.Utilities.Extensions;
using SqlColDictionary = System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, ChuckHill2.Utilities.SqlColumnAttributes>>;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Retrieve the column attributes for all the columns of all the tables 
    /// in the SQL database. Because the schema is fixed for at least the duration of 
    /// this process, this method should really be in the ImportManager class. This 
    /// method only needs to be called once and is common to all system importers.
    /// </summary>
    /// <returns>Read-only array of SQL column attributes</returns>
    public class SqlColumnAttributes
    {
        public enum ConstraintType { NONE, PK, FK, UNIQUE }

        private string toStringName = ":";

        private string tableName = string.Empty;
        private string columnName = string.Empty;
        private int ordinal = 0;
        private ConstraintType constraint = ConstraintType.NONE;
        private bool isView = false;
        private Type columnType = null;
        private SqlDbType dbColumnType = (SqlDbType)(-1);
        private object defaultValue = null;
        private bool nullable = true;
        private int maxLength = -1;
        private int precision = -1;
        private int scale = -1;

        public string TableName { get { return tableName; } }
        public string ColumnName { get { return columnName; } }
        public int Ordinal { get { return ordinal; } }
        public ConstraintType Constraint { get { return constraint; } }
        public bool IsView { get { return isView; } }
        public Type ColumnType { get { return columnType; } }
        public SqlDbType DBColumnType { get { return dbColumnType; } }
        public object DefaultValue { get { return defaultValue; } }
        public bool Nullable { get { return nullable; } }
        public int MaxLength { get { return maxLength; } }
        public int Precision { get { return precision; } }
        public int Scale { get { return scale; } }

        /// <summary>
        /// Get column attributes for all tables in the default database.
        /// </summary>
        /// <param name="connectionString"></param>
        /// <returns>2-dimensional dictionary of [tablenames][columnames]</returns>
        public static SqlColDictionary GetSqlColumns(string connectionString)
        {
            //Create a 2-dimensional dictionary of tablenames/columnnames
            var dict = new SqlColDictionary(StringComparer.InvariantCultureIgnoreCase);
            if (connectionString.IsNullOrEmpty()) return dict;
            SqlDataReader reader = null;
            #region string query = @"...
            string query = @"SELECT 
c.TABLE_NAME TableName, 
c.COLUMN_NAME ColumnName, 
c.ORDINAL_POSITION Ordinal, 
tc.CONSTRAINT_TYPE ConstraintType,
t.TABLE_TYPE TableType,
c.DATA_TYPE [Type], 
c.COLUMN_DEFAULT Defalt, 
c.IS_NULLABLE Nullable, 
c.CHARACTER_MAXIMUM_LENGTH [Length], 
c.NUMERIC_PRECISION [Precision],
c.NUMERIC_SCALE [Scale]
FROM INFORMATION_SCHEMA.COLUMNS c
LEFT JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS cu on cu.TABLE_NAME = c.TABLE_NAME and cu.COLUMN_NAME = c.COLUMN_NAME
LEFT JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc on tc.TABLE_NAME = c.TABLE_NAME and tc.CONSTRAINT_NAME = cu.CONSTRAINT_NAME
LEFT JOIN INFORMATION_SCHEMA.TABLES AS t on t.TABLE_NAME = c.TABLE_NAME
ORDER BY TableName, Ordinal";
            #endregion

            try
            {
                reader = SqlClient.ExecuteReader(connectionString, query);
                List<string> list = new List<string>();
                while (reader.Read())
                {
                    var sa = new SqlColumnAttributes(reader);
                    Dictionary<string, SqlColumnAttributes> col;
                    if (!dict.TryGetValue(sa.TableName, out col))
                    {
                        col = new Dictionary<string, SqlColumnAttributes>(StringComparer.InvariantCultureIgnoreCase);
                        dict.Add(sa.TableName, col);
                    }
                    SqlColumnAttributes saTemp;
                    if (col.TryGetValue(sa.ColumnName, out saTemp))
                    {
                        if (saTemp.Constraint == SqlColumnAttributes.ConstraintType.PK) continue;
                        if (sa.Constraint == SqlColumnAttributes.ConstraintType.PK) { col[sa.ColumnName] = sa; continue; }
                        if (saTemp.Constraint == SqlColumnAttributes.ConstraintType.FK) continue;
                        if (sa.Constraint == SqlColumnAttributes.ConstraintType.FK) { col[sa.ColumnName] = sa; continue; }
                        if (saTemp.Constraint == SqlColumnAttributes.ConstraintType.UNIQUE) continue;
                        if (sa.Constraint == SqlColumnAttributes.ConstraintType.UNIQUE) { col[sa.ColumnName] = sa; continue; }
                        continue;
                    }
                    col.Add(sa.ColumnName, sa);
                }
                return dict;
            }
            catch (Exception ex)
            {
                throw ex.PrefixMessage(string.Format("GetSqlColumnAttributes(\"{0}\") failed", connectionString));
            }
            finally
            {
                if (reader != null) { reader.DisposeConnection(); reader = null; }
            }
        }

        private SqlColumnAttributes(SqlDataReader sqlRdr)
        {
            int index = 0;
            tableName = ToString(sqlRdr, index++, string.Empty);
            columnName = ToString(sqlRdr, index++, string.Empty);
            ordinal = ToInt32(sqlRdr, index++, -1);
            constraint = ToConstraintTypeEnum(ToString(sqlRdr, index++, string.Empty));
            isView = (sqlRdr.GetValue(index++).ToString() == "VIEW"); //only valid values are 'VIEW' and 'BASE TABLE'
            columnType = SqlTypeStringToNetType.GetValue(ToString(sqlRdr, index, "Udt"));
            dbColumnType = SqlTypeStringToSqlDbType.GetValue(ToString(sqlRdr, index++, "Udt"));
            defaultValue = ToDefault(sqlRdr, index++, columnType, null);
            nullable = ToBoolean(sqlRdr, index++, true);
            //if field not nullable but the default value is null, we have to come up with a suitable 'empty' value
            if (!nullable && defaultValue == null) defaultValue = getDefaultValue(columnType);
            maxLength = ToInt32(sqlRdr, index++, 0); //only strings have length. -1 == "MAX"
            precision = ToInt32(sqlRdr, index++, -1); //numeric/decimal field size
            scale = ToInt32(sqlRdr, index++, -1); //numeric/decimal digits after the decimal place

            toStringName = tableName + ":" + columnName;  //the ToString() pre-computed return value
        }
        private ConstraintType ToConstraintTypeEnum(string t)
        {
            switch (t)
            {
                case "": return ConstraintType.NONE;
                case "PRIMARY KEY": return ConstraintType.PK;
                case "FOREIGN KEY": return ConstraintType.FK;
                case "UNIQUE": return ConstraintType.UNIQUE;
            }
            return ConstraintType.NONE;
        }
        private object getDefaultValue(Type t)
        {
            if (t == null) return "0";
            switch (t.Name)
            {
                case "DateTime": return new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local);
                case "DateTimeOffset": return new DateTimeOffset(new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Local), TimeSpan.Zero);
                case "String": return string.Empty;
                default: if (t.IsValueType) return Activator.CreateInstance(t); else return null;
            }
        }
        private static String ToString(SqlDataReader sqlRdr, int index, String defalt = null)
        {
            try
            {
                if (sqlRdr.IsDBNull(index)) return defalt;
                return sqlRdr.GetValue(index).ToString();
            }
            catch { return defalt; }
        }
        private static Int32 ToInt32(SqlDataReader sqlRdr, int index, Int32 defalt = 0)
        {
            try
            {
                if (sqlRdr.IsDBNull(index)) return defalt;
                Int32 i;
                if (Int32.TryParse(sqlRdr.GetValue(index).ToString(), out i)) return i;
                return defalt;
            }
            catch { return defalt; }
        }
        private static Boolean ToBoolean(SqlDataReader sqlRdr, int index, Boolean defalt = false)
        {
            try
            {
                if (sqlRdr.IsDBNull(index)) return defalt;
                string s = sqlRdr.GetValue(index).ToString();
                if (s.IsNullOrEmpty()) return defalt;
                char c = s[0];
                return (c == '1' || c == 'T' || c == 't' || c == 'Y' || c == 'y'); // 1/0 or true/false or yes/no
            }
            catch { return defalt; }
        }
        private static Object ToDefault(SqlDataReader sqlRdr, int index, Type t, Object defalt = null)
        {
            //Many default value strings are surrounded by parentheses in addition to single quotes for strings.
            Regex re = new Regex(@"[\(\)']", RegexOptions.None);
            try
            {
                if (sqlRdr.IsDBNull(index)) return defalt;
                string s = sqlRdr.GetValue(index).ToString();
                if (s.IsNullOrEmpty()) return defalt;
                if (s.EndsWith("())")) return null; //DB column autogenerated value...aka '(newid())' 
                s = re.Replace(s, string.Empty);
                if (s.IsNullOrEmpty()) return defalt;
                if (t == typeof(Boolean))
                {
                    char c = s[0];
                    return (c == '1' || c == 'T' || c == 't' || c == 'Y' || c == 'y'); // 1/0 or true/false or yes/no
                }
                return Convert.ChangeType(s, t);
            }
            catch { return defalt; }
        }

        #region Type Translation Dictionaries
        private static readonly Dictionary<string, System.Data.SqlDbType> SqlTypeStringToSqlDbType = new Dictionary<string, SqlDbType>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "bigint", System.Data.SqlDbType.BigInt },  //Int64	
            { "binary", System.Data.SqlDbType.Binary },  //Byte[]	
            { "bit", System.Data.SqlDbType.Bit },  //Boolean	
            { "char", System.Data.SqlDbType.Char },  //Char	
            { "date", System.Data.SqlDbType.Date },  //DateTime	3-bytes, resolution 1 day
            { "datetime", System.Data.SqlDbType.DateTime },  //DateTime	8-bytes
            { "datetime2", System.Data.SqlDbType.DateTime2 },  //DateTime	
            { "datetimeoffset", System.Data.SqlDbType.DateTimeOffset },  //DateTimeOffset	
            { "decimal", System.Data.SqlDbType.Decimal },  //Decimal	
            { "float", System.Data.SqlDbType.Float },  //Double	
            { "image", System.Data.SqlDbType.Image },  //Byte[]	
            { "int", System.Data.SqlDbType.Int },  //Int32	
            { "money", System.Data.SqlDbType.Money },  //Int64	
            { "nchar", System.Data.SqlDbType.NChar },  //String	
            { "ntext", System.Data.SqlDbType.NText },  //String	
            { "numeric", System.Data.SqlDbType.Decimal },  //Decimal	
            { "nvarchar", System.Data.SqlDbType.NVarChar },  //String	
            { "real", System.Data.SqlDbType.Real },  //Single	
            { "smalldatetime", System.Data.SqlDbType.SmallDateTime },  //DateTime	resolution=1 sec
            { "smallint", System.Data.SqlDbType.SmallInt },  //Int16	
            { "smallmoney", System.Data.SqlDbType.SmallMoney },  //Int32	
            { "Structured", System.Data.SqlDbType.Structured },  //Object	
            { "text", System.Data.SqlDbType.Text },  //String	
            { "time", System.Data.SqlDbType.Time },  //TimeSpan	5-bytes, resolution 1 ms
            { "timestamp", System.Data.SqlDbType.Timestamp },  //DateTime	8-bytes
            { "tinyint", System.Data.SqlDbType.TinyInt },  //Byte	
            { "udt", System.Data.SqlDbType.Udt },  //user-defined type	
            { "uniqueidentifier", System.Data.SqlDbType.UniqueIdentifier },  //Guid	
            { "varbinary", System.Data.SqlDbType.VarBinary },  //Byte[]	
            { "varchar", System.Data.SqlDbType.VarChar },  //String	
            { "variant", System.Data.SqlDbType.Variant },  //Object	
            { "xml", System.Data.SqlDbType.Xml }  //String	
        };

        private static readonly Dictionary<string, Type> SqlTypeStringToNetType = new Dictionary<string, Type>(StringComparer.InvariantCultureIgnoreCase)
        {
            { "bigint", typeof(System.Int64) },  //System.Data.SqlDbType.BigInt	
            { "binary", typeof(System.Byte) },  //System.Data.SqlDbType.Binary	
            { "bit", typeof(System.Boolean) },  //System.Data.SqlDbType.Bit	
            { "char", typeof(System.String) },  //System.Data.SqlDbType.Char	
            { "date", typeof(System.DateTime) },  //System.Data.SqlDbType.Date	3-bytes, resolution 1 day
            { "datetime", typeof(System.DateTime) },  //System.Data.SqlDbType.DateTime	8-bytes
            { "datetime2", typeof(System.DateTime) },  //System.Data.SqlDbType.DateTime2	
            { "datetimeoffset", typeof(System.DateTimeOffset) },  //System.Data.SqlDbType.DateTimeOffset	
            { "decimal", typeof(System.Decimal) },  //System.Data.SqlDbType.Decimal	
            { "float", typeof(System.Double) },  //System.Data.SqlDbType.Float	
            { "image", typeof(System.Byte) },  //System.Data.SqlDbType.Image	
            { "int", typeof(System.Int32) },  //System.Data.SqlDbType.Int	
            { "money", typeof(System.Decimal) },  //System.Data.SqlDbType.Money	
            { "nchar", typeof(System.String) },  //System.Data.SqlDbType.NChar	
            { "ntext", typeof(System.String) },  //System.Data.SqlDbType.NText	
            { "numeric", typeof(System.Decimal) },  //System.Data.SqlDbType.Decimal	
            { "nvarchar", typeof(System.String) },  //System.Data.SqlDbType.NVarChar	
            { "real", typeof(System.Single) },  //System.Data.SqlDbType.Real	
            { "smalldatetime", typeof(System.DateTime) },  //System.Data.SqlDbType.SmallDateTime	resolution=1 sec
            { "smallint", typeof(System.Int16) },  //System.Data.SqlDbType.SmallInt	
            { "smallmoney", typeof(System.Decimal) },  //System.Data.SqlDbType.SmallMoney	
            { "Structured", typeof(System.Object) },  //System.Data.SqlDbType.Structured	
            { "text", typeof(System.String) },  //System.Data.SqlDbType.Text	
            { "time", typeof(System.TimeSpan) },  //System.Data.SqlDbType.Time	5-bytes, resolution 1 ms
            { "timestamp", typeof(System.DateTime) },  //System.Data.SqlDbType.Timestamp	8-bytes
            { "tinyint", typeof(System.Byte) },  //System.Data.SqlDbType.TinyInt	
            { "udt", typeof(System.Object) },  //System.Data.SqlDbType.Udt	
            { "uniqueidentifier", typeof(System.Guid) },  //System.Data.SqlDbType.UniqueIdentifier	
            { "varbinary", typeof(System.Byte[]) },  //System.Data.SqlDbType.VarBinary	
            { "varchar", typeof(System.String) },  //System.Data.SqlDbType.VarChar	
            { "variant", typeof(System.Object) },  //System.Data.SqlDbType.Variant	
            { "xml", typeof(System.String) }  //System.Data.SqlDbType.Xml	
        };

        private static readonly Dictionary<Type, string> NetTypeToSqlTypeString = new Dictionary<Type, string>()
        {
            { typeof(System.Boolean), "bit" },  //System.Data.SqlDbType.Bit	
            { typeof(System.Byte), "tinyint" },  //System.Data.SqlDbType.TinyInt	
            { typeof(System.Byte[]), "binary" },  //System.Data.SqlDbType.Binary	
            { typeof(System.Char), "char" },  //System.Data.SqlDbType.Char	
            { typeof(System.DateTime), "datetime" },  //System.Data.SqlDbType.DateTime	8-bytes
            { typeof(System.DateTimeOffset), "datetimeoffset" },  //System.Data.SqlDbType.DateTimeOffset	
            { typeof(System.Decimal), "numeric" },  //System.Data.SqlDbType.Decimal	
            { typeof(System.Double), "float" },  //System.Data.SqlDbType.Float	
            { typeof(System.Guid), "uniqueidentifier" },  //System.Data.SqlDbType.UniqueIdentifier	
            { typeof(System.Int16), "smallint" },  //System.Data.SqlDbType.SmallInt	
            { typeof(System.Int32), "int" },  //System.Data.SqlDbType.Int	
            { typeof(System.Int64), "bigint" },  //System.Data.SqlDbType.BigInt	
            { typeof(System.Object), "variant" },  //System.Data.SqlDbType.Variant	
            { typeof(System.Single), "real" },  //System.Data.SqlDbType.Real	
            { typeof(System.String), "nvarchar" },  //System.Data.SqlDbType.NVarChar	
            { typeof(System.TimeSpan), "time" },  //System.Data.SqlDbType.Time	5-bytes, resolution 1 ms
        };
        #endregion

        public override string ToString()
        {
            //Useful for displaying a nice value during debugging or sorting or searching.
            return toStringName;
        }
    }
}
