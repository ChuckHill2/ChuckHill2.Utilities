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
            this.m_radDisabled = new System.Windows.Forms.RadioButton();
            this.m_radEnabled = new System.Windows.Forms.RadioButton();
            this.m_clbColorListBox = new ChuckHill2.Forms.NamedColorListBox();
            this.m_ctvColorTreeView = new ChuckHill2.Forms.NamedColorTreeView();
            this.m_cbbColorComboBox = new ChuckHill2.Forms.NamedColorComboBox();
            this.m_roundedRect = new System.Windows.Forms.Panel();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnSystemColorDialog
            // 
            this.m_btnSystemColorDialog.Location = new System.Drawing.Point(10, 31);
            this.m_btnSystemColorDialog.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.m_btnSystemColorDialog.Name = "m_btnSystemColorDialog";
            this.m_btnSystemColorDialog.Size = new System.Drawing.Size(106, 38);
            this.m_btnSystemColorDialog.TabIndex = 0;
            this.m_btnSystemColorDialog.Text = "System ColorDialog";
            this.m_btnSystemColorDialog.UseVisualStyleBackColor = true;
            this.m_btnSystemColorDialog.Click += new System.EventHandler(this.m_btnSystemColorDlg_Click);
            // 
            // m_btnColorTreeView
            // 
            this.m_btnColorTreeView.Location = new System.Drawing.Point(288, 13);
            this.m_btnColorTreeView.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.m_btnColorTreeView.Name = "m_btnColorTreeView";
            this.m_btnColorTreeView.Size = new System.Drawing.Size(196, 23);
            this.m_btnColorTreeView.TabIndex = 4;
            this.m_btnColorTreeView.Text = "Select Color";
            this.m_btnColorTreeView.UseVisualStyleBackColor = true;
            this.m_btnColorTreeView.Click += new System.EventHandler(this.m_btnColorTreeView_Click);
            // 
            // m_btnColorListBox
            // 
            this.m_btnColorListBox.Location = new System.Drawing.Point(124, 13);
            this.m_btnColorListBox.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.m_btnColorListBox.Name = "m_btnColorListBox";
            this.m_btnColorListBox.Size = new System.Drawing.Size(157, 23);
            this.m_btnColorListBox.TabIndex = 5;
            this.m_btnColorListBox.Text = "Select Color";
            this.m_btnColorListBox.UseVisualStyleBackColor = true;
            this.m_btnColorListBox.Click += new System.EventHandler(this.m_btnColorListBox_Click);
            // 
            // m_btnSystemCustomColorDialog
            // 
            this.m_btnSystemCustomColorDialog.Location = new System.Drawing.Point(10, 76);
            this.m_btnSystemCustomColorDialog.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.m_btnSystemCustomColorDialog.Name = "m_btnSystemCustomColorDialog";
            this.m_btnSystemCustomColorDialog.Size = new System.Drawing.Size(106, 53);
            this.m_btnSystemCustomColorDialog.TabIndex = 6;
            this.m_btnSystemCustomColorDialog.Text = "System CustomColor Dialog";
            this.m_btnSystemCustomColorDialog.UseVisualStyleBackColor = true;
            this.m_btnSystemCustomColorDialog.Click += new System.EventHandler(this.m_btnSystemCustomColorDialog_Click);
            // 
            // m_btnColorComboBox
            // 
            this.m_btnColorComboBox.Location = new System.Drawing.Point(490, 13);
            this.m_btnColorComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.m_btnColorComboBox.Name = "m_btnColorComboBox";
            this.m_btnColorComboBox.Size = new System.Drawing.Size(157, 23);
            this.m_btnColorComboBox.TabIndex = 10;
            this.m_btnColorComboBox.Text = "Select Color";
            this.m_btnColorComboBox.UseVisualStyleBackColor = true;
            this.m_btnColorComboBox.Click += new System.EventHandler(this.m_btnColorComboBox_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.m_radDisabled);
            this.groupBox1.Controls.Add(this.m_radEnabled);
            this.groupBox1.Location = new System.Drawing.Point(10, 136);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(106, 77);
            this.groupBox1.TabIndex = 11;
            this.groupBox1.TabStop = false;
            // 
            // m_radDisabled
            // 
            this.m_radDisabled.AutoSize = true;
            this.m_radDisabled.Location = new System.Drawing.Point(16, 44);
            this.m_radDisabled.Margin = new System.Windows.Forms.Padding(2);
            this.m_radDisabled.Name = "m_radDisabled";
            this.m_radDisabled.Size = new System.Drawing.Size(70, 19);
            this.m_radDisabled.TabIndex = 1;
            this.m_radDisabled.Text = "Disabled";
            this.m_radDisabled.UseVisualStyleBackColor = true;
            // 
            // m_radEnabled
            // 
            this.m_radEnabled.AutoSize = true;
            this.m_radEnabled.Checked = true;
            this.m_radEnabled.Location = new System.Drawing.Point(16, 20);
            this.m_radEnabled.Margin = new System.Windows.Forms.Padding(2);
            this.m_radEnabled.Name = "m_radEnabled";
            this.m_radEnabled.Size = new System.Drawing.Size(67, 19);
            this.m_radEnabled.TabIndex = 0;
            this.m_radEnabled.TabStop = true;
            this.m_radEnabled.Text = "Enabled";
            this.m_radEnabled.UseVisualStyleBackColor = true;
            this.m_radEnabled.CheckedChanged += new System.EventHandler(this.m_radEnabled_CheckedChanged);
            // 
            // m_clbColorListBox
            // 
            this.m_clbColorListBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_clbColorListBox.IntegralHeight = false;
            this.m_clbColorListBox.Location = new System.Drawing.Point(124, 44);
            this.m_clbColorListBox.Margin = new System.Windows.Forms.Padding(2);
            this.m_clbColorListBox.Name = "m_clbColorListBox";
            this.m_clbColorListBox.Size = new System.Drawing.Size(158, 448);
            this.m_clbColorListBox.TabIndex = 7;
            this.m_clbColorListBox.SelectionChanged += new ChuckHill2.Forms.NamedColorEventHandler(this.m_clbColorListBox_SelectionChanged);
            // 
            // m_ctvColorTreeView
            // 
            this.m_ctvColorTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_ctvColorTreeView.HideSelection = false;
            this.m_ctvColorTreeView.Location = new System.Drawing.Point(288, 44);
            this.m_ctvColorTreeView.Margin = new System.Windows.Forms.Padding(2);
            this.m_ctvColorTreeView.Name = "m_ctvColorTreeView";
            this.m_ctvColorTreeView.Size = new System.Drawing.Size(196, 448);
            this.m_ctvColorTreeView.TabIndex = 12;
            // 
            // m_cbbColorComboBox
            // 
            this.m_cbbColorComboBox.Location = new System.Drawing.Point(490, 45);
            this.m_cbbColorComboBox.Margin = new System.Windows.Forms.Padding(2);
            this.m_cbbColorComboBox.Name = "m_cbbColorComboBox";
            this.m_cbbColorComboBox.Size = new System.Drawing.Size(158, 19);
            this.m_cbbColorComboBox.TabIndex = 9;
            this.m_cbbColorComboBox.SelectionChanged += new ChuckHill2.Forms.NamedColorEventHandler(this.m_cbbColorComboBox_SelectionChanged);
            // 
            // m_roundedRect
            // 
            this.m_roundedRect.Location = new System.Drawing.Point(490, 76);
            this.m_roundedRect.Name = "m_roundedRect";
            this.m_roundedRect.Size = new System.Drawing.Size(200, 100);
            this.m_roundedRect.TabIndex = 13;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(874, 504);
            this.Controls.Add(this.m_roundedRect);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.m_btnSystemCustomColorDialog);
            this.Controls.Add(this.m_btnColorListBox);
            this.Controls.Add(this.m_btnColorTreeView);
            this.Controls.Add(this.m_btnSystemColorDialog);
            this.Controls.Add(this.m_btnColorComboBox);
            this.Controls.Add(this.m_clbColorListBox);
            this.Controls.Add(this.m_ctvColorTreeView);
            this.Controls.Add(this.m_cbbColorComboBox);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
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
        private ChuckHill2.Forms.NamedColorListBox m_clbColorListBox;
        //private ChuckHill2.NamedColorTreeView m_ctvColorTreeView;
        private System.Windows.Forms.Button m_btnColorComboBox;
        private ChuckHill2.Forms.NamedColorComboBox m_cbbColorComboBox;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton m_radDisabled;
        private System.Windows.Forms.RadioButton m_radEnabled;
        private ChuckHill2.Forms.NamedColorTreeView m_ctvColorTreeView;
        private System.Windows.Forms.Panel m_roundedRect;
    }
}

