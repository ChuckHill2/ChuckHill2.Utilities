namespace ColorEditor
{
    partial class FormMain
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
            this.m_btnSystemColorDialog = new System.Windows.Forms.Button();
            this.m_btnColorTreeView = new System.Windows.Forms.Button();
            this.m_btnColorListBox = new System.Windows.Forms.Button();
            this.m_btnSystemCustomColorDialog = new System.Windows.Forms.Button();
            this.m_btnColorComboBox = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.m_radEnabled = new System.Windows.Forms.RadioButton();
            this.m_radDisabled = new System.Windows.Forms.RadioButton();
            this.m_cbbColorComboBox = new ChuckHill2.Utilities.NamedColorComboBox();
            this.m_ctvColorTreeView = new ChuckHill2.Utilities.NamedColorTreeView();
            this.m_clbColorListBox = new ChuckHill2.Utilities.NamedColorListBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnSystemColorDialog
            // 
            this.m_btnSystemColorDialog.Location = new System.Drawing.Point(12, 13);
            this.m_btnSystemColorDialog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnSystemColorDialog.Name = "m_btnSystemColorDialog";
            this.m_btnSystemColorDialog.Size = new System.Drawing.Size(121, 41);
            this.m_btnSystemColorDialog.TabIndex = 0;
            this.m_btnSystemColorDialog.Text = "System ColorDialog";
            this.m_btnSystemColorDialog.UseVisualStyleBackColor = true;
            this.m_btnSystemColorDialog.Click += new System.EventHandler(this.m_btnSystemColorDlg_Click);
            // 
            // m_btnColorTreeView
            // 
            this.m_btnColorTreeView.Location = new System.Drawing.Point(329, 13);
            this.m_btnColorTreeView.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnColorTreeView.Name = "m_btnColorTreeView";
            this.m_btnColorTreeView.Size = new System.Drawing.Size(224, 25);
            this.m_btnColorTreeView.TabIndex = 4;
            this.m_btnColorTreeView.Text = "Select Color";
            this.m_btnColorTreeView.UseVisualStyleBackColor = true;
            this.m_btnColorTreeView.Click += new System.EventHandler(this.m_btnColorTreeView_Click);
            // 
            // m_btnColorListBox
            // 
            this.m_btnColorListBox.Location = new System.Drawing.Point(141, 13);
            this.m_btnColorListBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnColorListBox.Name = "m_btnColorListBox";
            this.m_btnColorListBox.Size = new System.Drawing.Size(180, 25);
            this.m_btnColorListBox.TabIndex = 5;
            this.m_btnColorListBox.Text = "Select Color";
            this.m_btnColorListBox.UseVisualStyleBackColor = true;
            this.m_btnColorListBox.Click += new System.EventHandler(this.m_btnColorListBox_Click);
            // 
            // m_btnSystemCustomColorDialog
            // 
            this.m_btnSystemCustomColorDialog.Location = new System.Drawing.Point(12, 62);
            this.m_btnSystemCustomColorDialog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnSystemCustomColorDialog.Name = "m_btnSystemCustomColorDialog";
            this.m_btnSystemCustomColorDialog.Size = new System.Drawing.Size(121, 56);
            this.m_btnSystemCustomColorDialog.TabIndex = 6;
            this.m_btnSystemCustomColorDialog.Text = "System CustomColor Dialog";
            this.m_btnSystemCustomColorDialog.UseVisualStyleBackColor = true;
            this.m_btnSystemCustomColorDialog.Click += new System.EventHandler(this.m_btnSystemCustomColorDialog_Click);
            // 
            // m_btnColorComboBox
            // 
            this.m_btnColorComboBox.Location = new System.Drawing.Point(560, 13);
            this.m_btnColorComboBox.Name = "m_btnColorComboBox";
            this.m_btnColorComboBox.Size = new System.Drawing.Size(180, 25);
            this.m_btnColorComboBox.TabIndex = 10;
            this.m_btnColorComboBox.Text = "Select Color";
            this.m_btnColorComboBox.UseVisualStyleBackColor = true;
            this.m_btnColorComboBox.Click += new System.EventHandler(this.m_btnColorComboBox_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.m_radDisabled);
            this.groupBox1.Controls.Add(this.m_radEnabled);
            this.groupBox1.Location = new System.Drawing.Point(12, 125);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(121, 82);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            // 
            // m_radEnabled
            // 
            this.m_radEnabled.AutoSize = true;
            this.m_radEnabled.Checked = true;
            this.m_radEnabled.Location = new System.Drawing.Point(19, 21);
            this.m_radEnabled.Name = "m_radEnabled";
            this.m_radEnabled.Size = new System.Drawing.Size(77, 20);
            this.m_radEnabled.TabIndex = 0;
            this.m_radEnabled.TabStop = true;
            this.m_radEnabled.Text = "Enabled";
            this.m_radEnabled.UseVisualStyleBackColor = true;
            this.m_radEnabled.CheckedChanged += new System.EventHandler(this.m_radEnabled_CheckedChanged);
            // 
            // m_radDisabled
            // 
            this.m_radDisabled.AutoSize = true;
            this.m_radDisabled.Location = new System.Drawing.Point(19, 47);
            this.m_radDisabled.Name = "m_radDisabled";
            this.m_radDisabled.Size = new System.Drawing.Size(81, 20);
            this.m_radDisabled.TabIndex = 1;
            this.m_radDisabled.Text = "Disabled";
            this.m_radDisabled.UseVisualStyleBackColor = true;
            // 
            // m_cbbColorComboBox
            // 
            this.m_cbbColorComboBox.Location = new System.Drawing.Point(560, 48);
            this.m_cbbColorComboBox.Name = "m_cbbColorComboBox";
            this.m_cbbColorComboBox.Size = new System.Drawing.Size(180, 23);
            this.m_cbbColorComboBox.TabIndex = 9;
            this.m_cbbColorComboBox.SelectionChanged += new ChuckHill2.Utilities.NamedColorEventHandler(this.m_cbbColorComboBox_SelectionChanged);
            // 
            // m_ctvColorTreeView
            // 
            this.m_ctvColorTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_ctvColorTreeView.HideSelection = false;
            this.m_ctvColorTreeView.Location = new System.Drawing.Point(329, 48);
            this.m_ctvColorTreeView.Name = "m_ctvColorTreeView";
            this.m_ctvColorTreeView.Size = new System.Drawing.Size(224, 477);
            this.m_ctvColorTreeView.TabIndex = 8;
            this.m_ctvColorTreeView.SelectionChanged += new ChuckHill2.Utilities.NamedColorEventHandler(this.m_ctvColorTreeView_SelectionChanged);
            // 
            // m_clbColorListBox
            // 
            this.m_clbColorListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_clbColorListBox.IntegralHeight = false;
            this.m_clbColorListBox.ItemHeight = 17;
            this.m_clbColorListBox.Location = new System.Drawing.Point(141, 47);
            this.m_clbColorListBox.Name = "m_clbColorListBox";
            this.m_clbColorListBox.Size = new System.Drawing.Size(180, 478);
            this.m_clbColorListBox.TabIndex = 7;
            this.m_clbColorListBox.SelectionChanged += new ChuckHill2.Utilities.NamedColorEventHandler(this.m_clbColorListBox_SelectionChanged);
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(752, 538);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.m_btnColorComboBox);
            this.Controls.Add(this.m_cbbColorComboBox);
            this.Controls.Add(this.m_ctvColorTreeView);
            this.Controls.Add(this.m_clbColorListBox);
            this.Controls.Add(this.m_btnSystemCustomColorDialog);
            this.Controls.Add(this.m_btnColorListBox);
            this.Controls.Add(this.m_btnColorTreeView);
            this.Controls.Add(this.m_btnSystemColorDialog);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FormMain";
            this.Text = "ColorEditor Test";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnSystemColorDialog;
        private System.Windows.Forms.Button m_btnColorTreeView;
        private System.Windows.Forms.Button m_btnColorListBox;
        private System.Windows.Forms.Button m_btnSystemCustomColorDialog;
        private ChuckHill2.Utilities.NamedColorListBox m_clbColorListBox;
        private ChuckHill2.Utilities.NamedColorTreeView m_ctvColorTreeView;
        private System.Windows.Forms.Button m_btnColorComboBox;
        private ChuckHill2.Utilities.NamedColorComboBox m_cbbColorComboBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton m_radDisabled;
        private System.Windows.Forms.RadioButton m_radEnabled;
    }
}

