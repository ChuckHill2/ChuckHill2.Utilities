using System;
using System.Collections;
using System.Data.Common;
using System.Data.SqlClient;
using System.Text;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Extends SqlConnectionStringBuilder to normalize the computername in SqlConnectionStringBuilder.DataSource. 
    /// This handles variants "myServerName" and "myServerName\myInstanceName".
    /// It replaces all variants of case-insensitive local computer name with "(local)".
    /// These consist of: the local computer name, "127.0.0.1", "localhost", "(local)", ".", or the local computer ip address.
    /// All of this is necessary in order to compare DataSource's of different connection strings for case-insensitive equality.
    /// </summary>
    public class SqlConnectionStringBuilderEx
    {
        //sealed class! cannot extend it, so we have to clone the interface.
        private SqlConnectionStringBuilder csb = new SqlConnectionStringBuilder();
        public SqlConnectionStringBuilderEx() { }
        public SqlConnectionStringBuilderEx(string connectionString)
        {
            this.ConnectionString = connectionString;
        }
        public SqlConnectionStringBuilderEx(SqlConnectionStringBuilder baseCsb)
        {
            csb = baseCsb;
            FixDataSource();
        }
        public SqlConnectionStringBuilderEx(SqlConnectionStringBuilderEx cloneCsb) //independent copy
        {
            this.ConnectionString = cloneCsb.ConnectionString;
        }

        public string ConnectionString
        {
            get { return csb.ConnectionString; }
            set { csb.ConnectionString = value ?? string.Empty; FixDataSource(); }
        }
        public string DataSource
        {
            get { return csb.DataSource; }
            set { csb.DataSource = value ?? string.Empty; FixDataSource(); }
        }
        public string ComputerName
        {
            get
            {
                var items = csb.DataSource.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (items != null && items.Length > 0) return items[0];
                return null;
            }
            set
            {
                FixDataSource(value, null);
            }
        }
        public string InstanceName
        {
            get
            {
                var items = csb.DataSource.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
                if (items != null && items.Length > 1) return items[1];
                return null;
            }
            set
            {
                FixDataSource(null, value);
            }
        }

        private void FixDataSource(string computer = null, string instance = null)
        {
            if (csb.ConnectionString.IsNullOrEmpty()) return;
            if (csb.DataSource.IsNullOrEmpty() && !computer.IsNullOrEmpty())
            {
                if (instance.IsNullOrEmpty()) csb.DataSource = computer;
                else csb.DataSource = string.Format("{0}\\{1}", computer, instance);
                return;
            }
            if (csb.DataSource.IsNullOrEmpty()) return;
            var items = csb.DataSource.Split(new char[] { '\\', '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (items != null && items.Length > 0 && computer.IsNullOrEmpty()) computer = items[0];
            if (items != null && items.Length > 1 && instance.IsNullOrEmpty()) instance = items[1];

            if (computer == "." || computer.EqualsI("(local)") || computer == "127.0.0.1" || computer.EqualsI("localhost")) computer = "(local)";
            else if (UserAccount.IsLocalIpAddress(computer)) computer = "(local)";

            if (instance.IsNullOrEmpty()) csb.DataSource = computer;
            else csb.DataSource = string.Format("{0}\\{1}", computer, instance);

            if (!csb.IntegratedSecurity && (csb.UserID.IsNullOrEmpty() || csb.Password.IsNullOrEmpty())) csb.IntegratedSecurity = true;
            if (!csb.IntegratedSecurity) csb.Remove("Integrated Security");
            if (csb.IntegratedSecurity) { csb.Remove("User ID"); csb.Remove("Password"); }

        }

        //extra transparency to make this 'look' like SqlConnectionStringBuilder is a base class
        public static implicit operator SqlConnectionStringBuilder(SqlConnectionStringBuilderEx r) { return r.csb; }
        public static implicit operator SqlConnectionStringBuilderEx(SqlConnectionStringBuilder r) { return new SqlConnectionStringBuilderEx(r); }
        public static bool operator ==(SqlConnectionStringBuilderEx r1, SqlConnectionStringBuilderEx r2) { return r1.Equals(r2); }
        public static bool operator !=(SqlConnectionStringBuilderEx r1, SqlConnectionStringBuilderEx r2) { return !r1.Equals(r2); }
        public override bool Equals(object obj)
        {
            SqlConnectionStringBuilder csb2 = null;
            if (obj is SqlConnectionStringBuilderEx) csb2 = ((SqlConnectionStringBuilderEx)obj).csb;
            else if (obj is SqlConnectionStringBuilder) csb2 = (SqlConnectionStringBuilder)obj;
            if (csb2==null) return false;
            if (!csb.DataSource.EqualsI(csb2.DataSource)) return false;
            if (!csb.InitialCatalog.EqualsI(csb2.InitialCatalog)) return false;
            if (csb.IntegratedSecurity != csb2.IntegratedSecurity) return false;
            if (!csb.IntegratedSecurity)
            {
                if (!csb.UserID.EqualsI(csb2.UserID)) return false;
                if (!csb.Password.Equals(csb2.Password)) return false;
            }
            return true;
        }
        public override int GetHashCode() { return csb.GetHashCode(); }
        public override string ToString() { return csb.ToString(); }

        //Everything else is no-change

        #region SqlConnectionStringBuilder
        public ApplicationIntent ApplicationIntent { get { return csb.ApplicationIntent; } set { csb.ApplicationIntent = value; } }
        public string ApplicationName { get { return csb.ApplicationName; } set { csb.ApplicationName = value; } }
        public bool AsynchronousProcessing { get { return csb.AsynchronousProcessing; } set { csb.AsynchronousProcessing = value; } }
        public string AttachDBFilename { get { return csb.AttachDBFilename; } set { csb.AttachDBFilename = value; } }
        //public bool ConnectionReset { get { return csb.ConnectionReset; } set { csb.ConnectionReset = value; } }
        public int ConnectTimeout { get { return csb.ConnectTimeout; } set { csb.ConnectTimeout = value; } }
        public bool ContextConnection { get { return csb.ContextConnection; } set { csb.ContextConnection = value; } }
        public string CurrentLanguage { get { return csb.CurrentLanguage; } set { csb.CurrentLanguage = value; } }
        public bool Encrypt { get { return csb.Encrypt; } set { csb.Encrypt = value; } }
        public bool Enlist { get { return csb.Enlist; } set { csb.Enlist = value; } }
        public string FailoverPartner { get { return csb.FailoverPartner; } set { csb.FailoverPartner = value; } }
        public string InitialCatalog { get { return csb.InitialCatalog; } set { csb.InitialCatalog = value; } }
        public bool IntegratedSecurity { get { return csb.IntegratedSecurity; } set { csb.IntegratedSecurity = value; } }
        public bool IsFixedSize { get { return csb.IsFixedSize; } }
        public ICollection Keys { get { return csb.Keys; } }
        public int LoadBalanceTimeout { get { return csb.LoadBalanceTimeout; } set { csb.LoadBalanceTimeout = value; } }
        public int MaxPoolSize { get { return csb.MaxPoolSize; } set { csb.MaxPoolSize = value; } }
        public int MinPoolSize { get { return csb.MinPoolSize; } set { csb.MinPoolSize = value; } }
        public bool MultipleActiveResultSets { get { return csb.MultipleActiveResultSets; } set { csb.MultipleActiveResultSets = value; } }
        public bool MultiSubnetFailover { get { return csb.MultiSubnetFailover; } set { csb.MultiSubnetFailover = value; } }
        public string NetworkLibrary { get { return csb.NetworkLibrary; } set { csb.NetworkLibrary = value; } }
        public int PacketSize { get { return csb.PacketSize; } set { csb.PacketSize = value; } }
        public string Password { get { return csb.Password; } set { csb.Password = value; } }
        public bool PersistSecurityInfo { get { return csb.PersistSecurityInfo; } set { csb.PersistSecurityInfo = value; } }
        public bool Pooling { get { return csb.Pooling; } set { csb.Pooling = value; } }
        public bool Replication { get { return csb.Replication; } set { csb.Replication = value; } }
        public string TransactionBinding { get { return csb.TransactionBinding; } set { csb.TransactionBinding = value; } }
        public bool TrustServerCertificate { get { return csb.TrustServerCertificate; } set { csb.TrustServerCertificate = value; } }
        public string TypeSystemVersion { get { return csb.TypeSystemVersion; } set { csb.TypeSystemVersion = value; } }
        public string UserID { get { return csb.UserID; } set { csb.UserID = value; } }
        public bool UserInstance { get { return csb.UserInstance; } set { csb.UserInstance = value; } }
        public ICollection Values { get { return csb.Values; } }
        public string WorkstationID { get { return csb.WorkstationID; } set { csb.WorkstationID = value; } }
        public object this[string keyword] { get { return csb[keyword]; } set { csb[keyword] = value; } }
        public bool ContainsKey(string keyword) { return csb.ContainsKey(keyword); }
        public bool Remove(string keyword) { return csb.Remove(keyword); }
        public bool ShouldSerialize(string keyword) { return csb.ShouldSerialize(keyword); }
        public bool TryGetValue(string keyword, out object value) { return csb.TryGetValue(keyword, out value); }
        #endregion

        #region DbConnectionStringBuilder
        public bool BrowsableConnectionString { get { return csb.BrowsableConnectionString; } set { csb.BrowsableConnectionString = value; } }
        public int Count { get { return csb.Count; } }
        public bool IsReadOnly { get { return csb.IsReadOnly; } }
        public void Add(string keyword, object value) { csb.Add(keyword, value); }
        public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value) { DbConnectionStringBuilder.AppendKeyValuePair(builder, keyword, value); }
        public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value, bool useOdbcRules) { DbConnectionStringBuilder.AppendKeyValuePair(builder, keyword, value, useOdbcRules); }
        public void Clear() { csb.Clear(); }
        public bool EquivalentTo(SqlConnectionStringBuilderEx connectionStringBuilder) { return connectionStringBuilder.csb.EquivalentTo(this.csb); }
        #endregion
    }
}
