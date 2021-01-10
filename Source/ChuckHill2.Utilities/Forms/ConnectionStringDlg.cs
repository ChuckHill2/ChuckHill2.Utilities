using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using ChuckHill2.Extensions;

namespace ChuckHill2.Forms
{
    public partial class ConnectionStringDlg : Form
    {
        /// <summary>
        /// Simple UI to create a connection string.
        /// </summary>
        /// <param name="owner">Parent/owner of this dialog.</param>
        /// <param name="connectionString">default connection string value or null if there is no default.</param>
        /// <param name="isOleDb">true if this dialog is to create an OleDb connection string. Otherwise create a SqlServer connection string</param>
        /// <returns>The new connection string. If dialog cancelled, the original default.</returns>
        public static string Show(IWin32Window owner, string connectionString=null, bool isOleDb=false)
        {
            using (var dlg = new ConnectionStringDlg(connectionString, isOleDb))
            {
                if (dlg.ShowDialog(owner) == DialogResult.OK)
                    return dlg.ConnectionString;
                else return connectionString;
            }
        }

        //private static string GetDBConnection(string connectionString)
        //{
        //    //Alternatitive method for creating a connection string.
        //    //Some of the problems with this method are:
        //    // (1) Requires additional references that are not in the GAC. In other words, we must manage these additional assemblies ourselves.
        //    // (2) There are many dialog options that will create a connection string that will fail in our environment.
        //    // (3) The result requires additional manipulation in order to actually work in C#.
        //    //
        //    //http://www.codeproject.com/Articles/6080/Using-DataLinks-to-get-or-edit-a-connection-string
        //    //Required References NOT in the GAC:
        //    //    Interop.MSDASC - Microsoft OLE DB Service Component 1.0 Type Library
        //    //    C:\Program Files (x86)\Microsoft.NET\Primary Interop Assemblies\adodb.dll
        //    //    C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\.NETFramework\v4.5\Microsoft.CSharp.dll
        //
        //    MSDASC.DataLinks mydlg = new MSDASC.DataLinks();
        //    mydlg.hWnd = this.Handle.ToInt32();
        //    if (string.IsNullOrWhiteSpace(connectionString))
        //    {
        //        ADODB.Connection ADOcon = (ADODB.Connection)mydlg.PromptNew();
        //        if (ADOcon != null) connectionString = ADOcon.ConnectionString;
        //    }
        //    else
        //    {
        //        ADODB.Connection ADOcon = new ADODB.Connection();
        //        ADOcon.ConnectionString = connectionString;

        //        System.Object connectionObj = ADOcon;
        //        if (mydlg.PromptEdit(ref connectionObj))
        //            connectionString = ((ADODB.Connection)connectionObj).ConnectionString;
        //    }
        //    return connectionString;
        //}

        private bool isOleDb = false;

        private string ConnectionString
        {
            get
            {
                var sb = new SqlConnectionStringBuilder();
                if (m_cmbDataSource.Text.IsNullOrEmpty()) return null;
                sb.DataSource = m_cmbDataSource.Text;
                if (m_cmbDatabase.Text.IsNullOrEmpty()) sb.InitialCatalog = "master";
                else sb.InitialCatalog = m_cmbDatabase.Text;
                if (m_radUserPass.Checked && !m_cmbUsername.Text.IsNullOrEmpty() && !m_txtPassword.Text.IsNullOrEmpty())
                {
                    sb.Remove("Integrated Security"); //Default==false
                    sb.UserID = m_cmbUsername.Text;
                    sb.Password = m_txtPassword.Text;
                }
                else sb.IntegratedSecurity = true;
                if (!isOleDb)
                {
                    sb.MaxPoolSize = 200;
                    sb.ConnectTimeout = 600;
                }
                else sb.ApplicationIntent = ApplicationIntent.ReadOnly;
                string cs = sb.ConnectionString;
                if (isOleDb)
                {
                    //Integrated Security=true throws an exception when used with the
                    //OleDb provider! This works for both OleDb and SQLServer providers.
                    cs = "Provider=SQLOLEDB;" + cs.Replace("Integrated Security=True", "Integrated Security=SSPI");
                }

                return cs;
            }
        }

        private string SqlConnectionString  //For internal use only
        {
            get
            {
                var sb = new SqlConnectionStringBuilder();
                if (m_cmbDataSource.Text.IsNullOrEmpty()) return null;
                sb.DataSource = m_cmbDataSource.Text;
                if (m_cmbDatabase.Text.IsNullOrEmpty()) sb.InitialCatalog = "master";
                else sb.InitialCatalog = m_cmbDatabase.Text;
                if (m_radUserPass.Checked && !m_cmbUsername.Text.IsNullOrEmpty() && !m_txtPassword.Text.IsNullOrEmpty())
                {
                    sb.Remove("Integrated Security"); //Default==false
                    sb.UserID = m_cmbUsername.Text;
                    sb.Password = m_txtPassword.Text;
                }
                else sb.IntegratedSecurity = true;
                sb.ApplicationIntent = ApplicationIntent.ReadOnly;
                return sb.ConnectionString;
            }
        }

        private ConnectionStringDlg(string connectionString, bool isOleDb)
        {
            InitializeComponent();

            this.isOleDb = isOleDb;
            this.Text = isOleDb ? "OleDb Server Connection String" : "Sql Server Connection String";
            var sb = new SqlConnectionStringBuilder();
            try
            {
                if (connectionString.ContainsI("SQLOLEDB"))
                {
                    //Provider=SQLOLEDB;Integrated Security=SSPI;Persist Security Info=False;Initial Catalog=MyDatabase;Data Source=(local)
                    var db = new DbConnectionStringBuilder(false); //OleDbConnectionStringBuilder
                    db.ConnectionString = connectionString;
                    db.Remove("Provider");
                    connectionString = db.ConnectionString;
                }
                sb.ConnectionString = connectionString;
            } catch { }

            if (sb.DataSource.IsNullOrEmpty()) sb.DataSource = "(local)";
            if (sb.DataSource == ".") sb.DataSource = "(local)";
            m_cmbDataSource.Text = sb.DataSource;
            m_cmbDatabase.Text = sb.InitialCatalog ?? string.Empty;
            m_cmbUsername.Text = sb.UserID ?? string.Empty;
            m_txtPassword.Text = sb.Password ?? string.Empty;

            if (!m_cmbUsername.Text.IsNullOrEmpty() && !m_txtPassword.Text.IsNullOrEmpty())
            {
                m_radUserPass.Checked = true;
            }
            else m_radIntegratedSecurity.Checked = true;
        }

        private void m_rad_CheckedChanged(object sender, EventArgs e)
        {
            var rad = sender as RadioButton;
            EnableUserPass(rad == m_radUserPass && rad.Checked);

            string s = m_cmbDatabase.Text;
            m_cmbDatabase.DataSource = null;
            m_cmbDatabase.Text = s;
        }
        private void EnableUserPass(bool enabled)
        {
            m_lblUsername.Enabled = enabled;
            m_lblPassword.Enabled = enabled;
            m_txtPassword.Enabled = enabled;
            m_cmbUsername.Enabled = enabled;
            m_chkShowPassword.Enabled = enabled;
        }

        private void m_cmbDataSource_DropDown(object sender, EventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb.DataSource == null)
            {
                object dataSource = null;
                PleaseWait.Show(this, "Getting list of known servers...",
                    delegate(object value) { dataSource = GetServers(); });
                string v = cmb.Text;
                cmb.DataSource = dataSource;
                cmb.SelectedItem = v;
                cmb.Text = v;
            }
        }
        private void m_cmbUsername_DropDown(object sender, EventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb.DataSource == null)
            {
                object dataSource = null;
                string cs = this.SqlConnectionString;
                PleaseWait.Show(this, string.Format("Getting Sql Users for {0}...", m_cmbDataSource.Text),
                    delegate(object value) { dataSource = GetSqlUsers(cs); });
                string v = cmb.Text;
                cmb.DataSource = dataSource;
                cmb.SelectedItem = v;
                cmb.Text = v;
            }
        }
        private void m_cmbDatabase_DropDown(object sender, EventArgs e)
        {
            var cmb = sender as ComboBox;
            if (cmb.DataSource == null)
            {
                object dataSource = null;
                string cs = this.SqlConnectionString;
                PleaseWait.Show(this, string.Format("Getting known databases for {0}...", m_cmbDataSource.Text),
                    delegate(object value) { dataSource = GetDBs(cs); });
                string v = cmb.Text;
                cmb.DataSource = dataSource;
                cmb.SelectedItem = v;
                cmb.Text = v;
            }
        }

        private bool m_cmbDataSource_modified = false;
        private bool m_cmbUsername_modified = false;
        private void m_cmbDataSource_TextUpdate(object sender, EventArgs e)
        {
            m_cmbDataSource_modified = true;
        }
        private void m_cmbUsername_TextUpdate(object sender, EventArgs e)
        {
            m_cmbUsername_modified = true;
        }
        private void m_cmbDataSource_Leave(object sender, EventArgs e)
        {
            if (!m_cmbDataSource_modified) return;
            m_cmbDataSource_modified = false;
            string s;

            s = m_cmbDatabase.Text;
            m_cmbDatabase.DataSource = null;
            m_cmbDatabase.Text = s;

            s = m_cmbUsername.Text;
            m_cmbUsername.DataSource = null;
            m_cmbUsername.Text = s;

        }
        private void m_cmbUsername_Leave(object sender, EventArgs e)
        {
            if (!m_cmbUsername_modified) return;
            m_cmbUsername_modified = false;

            string s = m_cmbDatabase.Text;
            m_cmbDatabase.DataSource = null;
            m_cmbDatabase.Text = s;
        }

        private void m_chkShowPassword_CheckedChanged(object sender, EventArgs e)
        {
            var chk = sender as CheckBox;
            m_txtPassword.UseSystemPasswordChar = !chk.Checked;
        }

        private void m_btnTestConnection_Click(object sender, EventArgs e)
        {
            if (!ValidateConnectionStringFormat()) return;
            if (isOleDb) TestOleDbConnection();
            else TestSqlDbConnection();
        }
        private void TestOleDbConnection()
        {
            try
            {
                using (var conn = new OleDbConnection(this.ConnectionString))
                {
                    using (var cmd = new OleDbCommand("SELECT TOP 1 name FROM sys.server_principals", conn))
                    {
                        conn.Open();
                        string value = cmd.ExecuteScalar() as string;  //should be 'sa'
                        if (!value.IsNullOrEmpty())
                            MessageBoxEx.Show(this, "Connection Successful.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBoxEx.Show(this, "Connection Failed.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, string.Format("Connection Failed.\r\n{0}", ex.Message), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void TestSqlDbConnection()
        {
            try
            {
                using (var conn = new SqlConnection(this.ConnectionString))
                {
                    using (var cmd = new SqlCommand("SELECT TOP 1 name FROM sys.server_principals", conn))
                    {
                        conn.Open();
                        string value = cmd.ExecuteScalar() as string;  //should be 'sa'
                        if (!value.IsNullOrEmpty())
                            MessageBoxEx.Show(this, "Connection Successful.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Information);
                        else
                            MessageBoxEx.Show(this, "Connection Failed.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBoxEx.Show(this, string.Format("Connection Failed.\r\n{0}", ex.Message), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            if (!ValidateConnectionStringFormat()) return;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private bool ValidateConnectionStringFormat()
        {
            if (m_cmbDataSource.Text.IsNullOrEmpty())
            {
                MessageBoxEx.Show(this, "Missing DataSource (e.g. computername).", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (m_cmbDatabase.Text.IsNullOrEmpty())
            {
                MessageBoxEx.Show(this, "Missing Database Name.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            if (m_radUserPass.Checked && (m_cmbUsername.Text.IsNullOrEmpty() || m_txtPassword.Text.IsNullOrEmpty()))
            {
                MessageBoxEx.Show(this, "Using Sql login and username or password is empty.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }

        private string[] GetServers()
        {
            try
            {
                DataTable sources = System.Data.Sql.SqlDataSourceEnumerator.Instance.GetDataSources();
                List<string> servers = new List<string>(sources.Rows.Count);
                DataColumn serverName = sources.Columns["ServerName"];
                DataColumn instanceName = sources.Columns["InstanceName"];

                foreach (DataRow row in sources.Rows)
                {
                    string serv = row[serverName] as String;
                    string inst = row[instanceName] as String;
                    if (serv.IsNullOrEmpty()) continue;
                    if (!inst.IsNullOrEmpty()) serv = string.Format("{0}\\{1}", serv, inst);
                    servers.Add(serv);
                }
                if (servers.Count > 1) servers.Sort(StringComparer.InvariantCultureIgnoreCase);
                if (servers.Contains(Dns.GetHostName())) servers.Insert(0, "(local)");
                return servers.ToArray();
            }
            catch
            {
                return new string[0];
            }
        }

        private List<string> GetDBs(string connectionString)
        {
            var list = new List<string>();
            string[] invalidDBs = { "master", "tempdb", "model", "msdb" };

            var sb = new SqlConnectionStringBuilder(connectionString);
            sb.InitialCatalog = "master";
            connectionString = sb.ConnectionString;

            try
            {
                using (var conn = new SqlConnection(connectionString))
                {
                    using (var cmd = new SqlCommand("SELECT name FROM sys.databases", conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            while (reader.Read())
                            {
                                string value = reader.GetValue(0) as string;
                                if (value.IsNullOrEmpty()) continue;
                                if (invalidDBs.Contains(value, StringComparer.InvariantCultureIgnoreCase)) continue;
                                list.Add(value);
                            }
                        }
                    }
                }
            }
            catch { }
            if (list.Count > 1) list.Sort(StringComparer.InvariantCultureIgnoreCase);
            return list;
        }
        private List<string> GetSqlUsers(string connectionString)
        {
            var list = new List<string>();
            try
            {
                var sb = new SqlConnectionStringBuilder(connectionString);
                sb.InitialCatalog = "master";
                connectionString = sb.ConnectionString;

                using (var conn = new SqlConnection(connectionString))
                {
                    using (var cmd = new SqlCommand("SELECT name FROM sys.server_principals where type='S' and is_disabled=0 and name not like '##%'", conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            while (reader.Read())
                            {
                                string value = reader.GetValue(0) as string;
                                if (value.IsNullOrEmpty()) continue;
                                list.Add(value);
                            }
                        }
                    }
                }
            }
            catch { }
            if (list.Count > 1) list.Sort(StringComparer.InvariantCultureIgnoreCase);
            return list;
        }
        private List<string> GetWindowsUsers(string connectionString)
        {
            var list = new List<string>();
            try
            {
                var sb = new SqlConnectionStringBuilder(connectionString);
                sb.InitialCatalog = "master";
                connectionString = sb.ConnectionString;

                using (var conn = new SqlConnection(connectionString))
                {
                    using (var cmd = new SqlCommand("SELECT name FROM sys.server_principals where type='U' or type='G' and is_disabled=0 and name not like '##%'", conn))
                    {
                        conn.Open();
                        using (var reader = cmd.ExecuteReader(CommandBehavior.SingleResult))
                        {
                            while (reader.Read())
                            {
                                string value = reader.GetValue(0) as string;
                                if (value.IsNullOrEmpty()) continue;
                                list.Add(value);
                            }
                        }
                    }
                }
            }
            catch { }
            if (list.Count > 1) list.Sort(StringComparer.InvariantCultureIgnoreCase);
            return list;
        }
    }
}
