namespace ChuckHill2.Utilities
{
    partial class ConnectionStringDlg
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionStringDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.m_cmbDataSource = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.m_radIntegratedSecurity = new System.Windows.Forms.RadioButton();
            this.m_radUserPass = new System.Windows.Forms.RadioButton();
            this.m_lblUsername = new System.Windows.Forms.Label();
            this.m_lblPassword = new System.Windows.Forms.Label();
            this.m_txtPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.m_cmbDatabase = new System.Windows.Forms.ComboBox();
            this.m_btnTestConnection = new System.Windows.Forms.Button();
            this.m_btnCancel = new System.Windows.Forms.Button();
            this.m_btnOK = new System.Windows.Forms.Button();
            this.m_cmbUsername = new System.Windows.Forms.ComboBox();
            this.m_chkShowPassword = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(255, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Specify the following to connect to SQL Server data:";
            // 
            // m_cmbDataSource
            // 
            this.m_cmbDataSource.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cmbDataSource.FormattingEnabled = true;
            this.m_cmbDataSource.Location = new System.Drawing.Point(24, 46);
            this.m_cmbDataSource.Name = "m_cmbDataSource";
            this.m_cmbDataSource.Size = new System.Drawing.Size(267, 21);
            this.m_cmbDataSource.TabIndex = 1;
            this.m_cmbDataSource.DropDown += new System.EventHandler(this.m_cmbDataSource_DropDown);
            this.m_cmbDataSource.TextUpdate += new System.EventHandler(this.m_cmbDataSource_TextUpdate);
            this.m_cmbDataSource.Leave += new System.EventHandler(this.m_cmbDataSource_Leave);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(161, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "1. Select or enter a s&erver name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 75);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(207, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "2. Enter information to log on to the server:";
            // 
            // m_radIntegratedSecurity
            // 
            this.m_radIntegratedSecurity.AutoSize = true;
            this.m_radIntegratedSecurity.Location = new System.Drawing.Point(27, 92);
            this.m_radIntegratedSecurity.Name = "m_radIntegratedSecurity";
            this.m_radIntegratedSecurity.Size = new System.Drawing.Size(199, 17);
            this.m_radIntegratedSecurity.TabIndex = 5;
            this.m_radIntegratedSecurity.TabStop = true;
            this.m_radIntegratedSecurity.Text = "Use &Windows NT Integrated security";
            this.m_radIntegratedSecurity.UseVisualStyleBackColor = true;
            this.m_radIntegratedSecurity.CheckedChanged += new System.EventHandler(this.m_rad_CheckedChanged);
            // 
            // m_radUserPass
            // 
            this.m_radUserPass.AutoSize = true;
            this.m_radUserPass.Location = new System.Drawing.Point(27, 114);
            this.m_radUserPass.Name = "m_radUserPass";
            this.m_radUserPass.Size = new System.Drawing.Size(216, 17);
            this.m_radUserPass.TabIndex = 6;
            this.m_radUserPass.TabStop = true;
            this.m_radUserPass.Text = "&Use a specific user name and password:";
            this.m_radUserPass.UseVisualStyleBackColor = true;
            this.m_radUserPass.CheckedChanged += new System.EventHandler(this.m_rad_CheckedChanged);
            // 
            // m_lblUsername
            // 
            this.m_lblUsername.Location = new System.Drawing.Point(4, 130);
            this.m_lblUsername.Name = "m_lblUsername";
            this.m_lblUsername.Size = new System.Drawing.Size(100, 23);
            this.m_lblUsername.TabIndex = 7;
            this.m_lblUsername.Text = "User &name:";
            this.m_lblUsername.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // m_lblPassword
            // 
            this.m_lblPassword.Location = new System.Drawing.Point(4, 154);
            this.m_lblPassword.Name = "m_lblPassword";
            this.m_lblPassword.Size = new System.Drawing.Size(100, 23);
            this.m_lblPassword.TabIndex = 9;
            this.m_lblPassword.Text = "&Password:";
            this.m_lblPassword.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // m_txtPassword
            // 
            this.m_txtPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_txtPassword.Location = new System.Drawing.Point(103, 156);
            this.m_txtPassword.Name = "m_txtPassword";
            this.m_txtPassword.Size = new System.Drawing.Size(188, 20);
            this.m_txtPassword.TabIndex = 10;
            this.m_txtPassword.UseSystemPasswordChar = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 198);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(182, 13);
            this.label6.TabIndex = 12;
            this.label6.Text = "3. Select the &database on the server:";
            // 
            // m_cmbDatabase
            // 
            this.m_cmbDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cmbDatabase.FormattingEnabled = true;
            this.m_cmbDatabase.Location = new System.Drawing.Point(27, 214);
            this.m_cmbDatabase.Name = "m_cmbDatabase";
            this.m_cmbDatabase.Size = new System.Drawing.Size(264, 21);
            this.m_cmbDatabase.TabIndex = 13;
            this.m_cmbDatabase.DropDown += new System.EventHandler(this.m_cmbDatabase_DropDown);
            // 
            // m_btnTestConnection
            // 
            this.m_btnTestConnection.Location = new System.Drawing.Point(27, 247);
            this.m_btnTestConnection.Name = "m_btnTestConnection";
            this.m_btnTestConnection.Size = new System.Drawing.Size(104, 23);
            this.m_btnTestConnection.TabIndex = 14;
            this.m_btnTestConnection.Text = "&Test Connection";
            this.m_btnTestConnection.UseVisualStyleBackColor = true;
            this.m_btnTestConnection.Click += new System.EventHandler(this.m_btnTestConnection_Click);
            // 
            // m_btnCancel
            // 
            this.m_btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.m_btnCancel.Location = new System.Drawing.Point(218, 247);
            this.m_btnCancel.Name = "m_btnCancel";
            this.m_btnCancel.Size = new System.Drawing.Size(75, 23);
            this.m_btnCancel.TabIndex = 15;
            this.m_btnCancel.Text = "Cancel";
            this.m_btnCancel.UseVisualStyleBackColor = true;
            this.m_btnCancel.Click += new System.EventHandler(this.m_btnCancel_Click);
            // 
            // m_btnOK
            // 
            this.m_btnOK.Location = new System.Drawing.Point(137, 247);
            this.m_btnOK.Name = "m_btnOK";
            this.m_btnOK.Size = new System.Drawing.Size(75, 23);
            this.m_btnOK.TabIndex = 16;
            this.m_btnOK.Text = "OK";
            this.m_btnOK.UseVisualStyleBackColor = true;
            this.m_btnOK.Click += new System.EventHandler(this.m_btnOK_Click);
            // 
            // m_cmbUsername
            // 
            this.m_cmbUsername.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cmbUsername.FormattingEnabled = true;
            this.m_cmbUsername.Location = new System.Drawing.Point(103, 132);
            this.m_cmbUsername.Name = "m_cmbUsername";
            this.m_cmbUsername.Size = new System.Drawing.Size(188, 21);
            this.m_cmbUsername.TabIndex = 17;
            this.m_cmbUsername.DropDown += new System.EventHandler(this.m_cmbUsername_DropDown);
            this.m_cmbUsername.TextUpdate += new System.EventHandler(this.m_cmbUsername_TextUpdate);
            this.m_cmbUsername.Leave += new System.EventHandler(this.m_cmbUsername_Leave);
            // 
            // m_chkShowPassword
            // 
            this.m_chkShowPassword.AutoSize = true;
            this.m_chkShowPassword.Location = new System.Drawing.Point(103, 178);
            this.m_chkShowPassword.Name = "m_chkShowPassword";
            this.m_chkShowPassword.Size = new System.Drawing.Size(102, 17);
            this.m_chkShowPassword.TabIndex = 18;
            this.m_chkShowPassword.Text = "Show Password";
            this.m_chkShowPassword.UseVisualStyleBackColor = true;
            this.m_chkShowPassword.CheckedChanged += new System.EventHandler(this.m_chkShowPassword_CheckedChanged);
            // 
            // ConnectionStringDlg
            // 
            this.AcceptButton = this.m_btnOK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.m_btnCancel;
            this.ClientSize = new System.Drawing.Size(307, 283);
            this.Controls.Add(this.m_cmbUsername);
            this.Controls.Add(this.m_btnOK);
            this.Controls.Add(this.m_btnCancel);
            this.Controls.Add(this.m_btnTestConnection);
            this.Controls.Add(this.m_cmbDatabase);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.m_txtPassword);
            this.Controls.Add(this.m_lblPassword);
            this.Controls.Add(this.m_radUserPass);
            this.Controls.Add(this.m_radIntegratedSecurity);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.m_cmbDataSource);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.m_lblUsername);
            this.Controls.Add(this.m_chkShowPassword);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(9999, 322);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(323, 322);
            this.Name = "ConnectionStringDlg";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Sql Server Connection String";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox m_cmbDataSource;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton m_radIntegratedSecurity;
        private System.Windows.Forms.RadioButton m_radUserPass;
        private System.Windows.Forms.Label m_lblUsername;
        private System.Windows.Forms.Label m_lblPassword;
        private System.Windows.Forms.TextBox m_txtPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox m_cmbDatabase;
        private System.Windows.Forms.Button m_btnTestConnection;
        private System.Windows.Forms.Button m_btnCancel;
        private System.Windows.Forms.Button m_btnOK;
        private System.Windows.Forms.ComboBox m_cmbUsername;
        private System.Windows.Forms.CheckBox m_chkShowPassword;
    }
}
