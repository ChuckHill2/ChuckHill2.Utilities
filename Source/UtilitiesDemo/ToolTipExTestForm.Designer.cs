namespace UtilitiesDemo
{
    partial class ToolTipExTestForm
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
            this.m_lblLabel = new System.Windows.Forms.Label();
            this.m_txtTextBox = new System.Windows.Forms.TextBox();
            this.m_btnButton = new System.Windows.Forms.Button();
            this.m_grpGroupBox = new System.Windows.Forms.GroupBox();
            this.m_cmbComboBox = new System.Windows.Forms.ComboBox();
            this.m_dtDateTimePicker = new System.Windows.Forms.DateTimePicker();
            this.m_pnlPanel = new System.Windows.Forms.Panel();
            this.m_chkCheckBox = new System.Windows.Forms.CheckBox();
            this.m_lblPanel = new System.Windows.Forms.Label();
            this.m_tabControl = new System.Windows.Forms.TabControl();
            this.m_tabPage1 = new System.Windows.Forms.TabPage();
            this.m_rtfRichTextBox = new System.Windows.Forms.RichTextBox();
            this.m_llLinkLabel = new System.Windows.Forms.LinkLabel();
            this.m_pbPictureBox = new System.Windows.Forms.PictureBox();
            this.m_numNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.m_lbListBox = new System.Windows.Forms.ListBox();
            this.m_tabPage2 = new System.Windows.Forms.TabPage();
            this.m_mcMonthCalendar = new System.Windows.Forms.MonthCalendar();
            this.m_grpGroupBox.SuspendLayout();
            this.m_pnlPanel.SuspendLayout();
            this.m_tabControl.SuspendLayout();
            this.m_tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_numNumericUpDown)).BeginInit();
            this.m_tabPage2.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_lblLabel
            // 
            this.m_lblLabel.AutoSize = true;
            this.m_lblLabel.Location = new System.Drawing.Point(12, 12);
            this.m_lblLabel.Name = "m_lblLabel";
            this.m_lblLabel.Size = new System.Drawing.Size(58, 15);
            this.m_lblLabel.TabIndex = 0;
            this.m_lblLabel.Text = "Label Test";
            // 
            // m_txtTextBox
            // 
            this.m_txtTextBox.Location = new System.Drawing.Point(133, 12);
            this.m_txtTextBox.Name = "m_txtTextBox";
            this.m_txtTextBox.Size = new System.Drawing.Size(138, 23);
            this.m_txtTextBox.TabIndex = 1;
            this.m_txtTextBox.Text = "TextBox Test";
            // 
            // m_btnButton
            // 
            this.m_btnButton.Location = new System.Drawing.Point(13, 38);
            this.m_btnButton.Name = "m_btnButton";
            this.m_btnButton.Size = new System.Drawing.Size(75, 23);
            this.m_btnButton.TabIndex = 2;
            this.m_btnButton.Text = "Button Test";
            this.m_btnButton.UseVisualStyleBackColor = true;
            this.m_btnButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // m_grpGroupBox
            // 
            this.m_grpGroupBox.Controls.Add(this.m_cmbComboBox);
            this.m_grpGroupBox.Controls.Add(this.m_dtDateTimePicker);
            this.m_grpGroupBox.Location = new System.Drawing.Point(133, 38);
            this.m_grpGroupBox.Name = "m_grpGroupBox";
            this.m_grpGroupBox.Size = new System.Drawing.Size(138, 130);
            this.m_grpGroupBox.TabIndex = 3;
            this.m_grpGroupBox.TabStop = false;
            this.m_grpGroupBox.Text = "groupBox1";
            // 
            // m_cmbComboBox
            // 
            this.m_cmbComboBox.FormattingEnabled = true;
            this.m_cmbComboBox.Items.AddRange(new object[] {
            "ComboBox Item 1",
            "ComboBox Item 2",
            "ComboBox Item 3",
            "ComboBox Item 4",
            "ComboBox Item 5",
            "ComboBox Item 6"});
            this.m_cmbComboBox.Location = new System.Drawing.Point(6, 61);
            this.m_cmbComboBox.Name = "m_cmbComboBox";
            this.m_cmbComboBox.Size = new System.Drawing.Size(121, 23);
            this.m_cmbComboBox.TabIndex = 1;
            // 
            // m_dtDateTimePicker
            // 
            this.m_dtDateTimePicker.CustomFormat = "";
            this.m_dtDateTimePicker.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.m_dtDateTimePicker.Location = new System.Drawing.Point(6, 19);
            this.m_dtDateTimePicker.Name = "m_dtDateTimePicker";
            this.m_dtDateTimePicker.ShowCheckBox = true;
            this.m_dtDateTimePicker.Size = new System.Drawing.Size(121, 23);
            this.m_dtDateTimePicker.TabIndex = 0;
            // 
            // m_pnlPanel
            // 
            this.m_pnlPanel.BackColor = System.Drawing.Color.PaleGreen;
            this.m_pnlPanel.Controls.Add(this.m_chkCheckBox);
            this.m_pnlPanel.Controls.Add(this.m_lblPanel);
            this.m_pnlPanel.Location = new System.Drawing.Point(13, 68);
            this.m_pnlPanel.Name = "m_pnlPanel";
            this.m_pnlPanel.Size = new System.Drawing.Size(114, 100);
            this.m_pnlPanel.TabIndex = 4;
            // 
            // m_chkCheckBox
            // 
            this.m_chkCheckBox.AutoSize = true;
            this.m_chkCheckBox.Location = new System.Drawing.Point(18, 35);
            this.m_chkCheckBox.Name = "m_chkCheckBox";
            this.m_chkCheckBox.Size = new System.Drawing.Size(83, 19);
            this.m_chkCheckBox.TabIndex = 1;
            this.m_chkCheckBox.Text = "checkBox1";
            this.m_chkCheckBox.UseVisualStyleBackColor = true;
            // 
            // m_lblPanel
            // 
            this.m_lblPanel.AutoSize = true;
            this.m_lblPanel.Location = new System.Drawing.Point(1, 1);
            this.m_lblPanel.Margin = new System.Windows.Forms.Padding(0);
            this.m_lblPanel.Name = "m_lblPanel";
            this.m_lblPanel.Size = new System.Drawing.Size(36, 15);
            this.m_lblPanel.TabIndex = 0;
            this.m_lblPanel.Text = "Panel";
            // 
            // m_tabControl
            // 
            this.m_tabControl.Controls.Add(this.m_tabPage1);
            this.m_tabControl.Controls.Add(this.m_tabPage2);
            this.m_tabControl.Location = new System.Drawing.Point(16, 174);
            this.m_tabControl.Name = "m_tabControl";
            this.m_tabControl.SelectedIndex = 0;
            this.m_tabControl.Size = new System.Drawing.Size(255, 207);
            this.m_tabControl.TabIndex = 5;
            // 
            // m_tabPage1
            // 
            this.m_tabPage1.Controls.Add(this.m_rtfRichTextBox);
            this.m_tabPage1.Controls.Add(this.m_llLinkLabel);
            this.m_tabPage1.Controls.Add(this.m_pbPictureBox);
            this.m_tabPage1.Controls.Add(this.m_numNumericUpDown);
            this.m_tabPage1.Controls.Add(this.m_lbListBox);
            this.m_tabPage1.Location = new System.Drawing.Point(4, 24);
            this.m_tabPage1.Name = "m_tabPage1";
            this.m_tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPage1.Size = new System.Drawing.Size(247, 179);
            this.m_tabPage1.TabIndex = 0;
            this.m_tabPage1.Text = "tabPage1";
            this.m_tabPage1.UseVisualStyleBackColor = true;
            // 
            // m_rtfRichTextBox
            // 
            this.m_rtfRichTextBox.Location = new System.Drawing.Point(169, 6);
            this.m_rtfRichTextBox.Name = "m_rtfRichTextBox";
            this.m_rtfRichTextBox.Size = new System.Drawing.Size(72, 167);
            this.m_rtfRichTextBox.TabIndex = 4;
            this.m_rtfRichTextBox.Text = "";
            // 
            // m_llLinkLabel
            // 
            this.m_llLinkLabel.AutoSize = true;
            this.m_llLinkLabel.Location = new System.Drawing.Point(103, 41);
            this.m_llLinkLabel.Name = "m_llLinkLabel";
            this.m_llLinkLabel.Size = new System.Drawing.Size(60, 15);
            this.m_llLinkLabel.TabIndex = 3;
            this.m_llLinkLabel.TabStop = true;
            this.m_llLinkLabel.Text = "linkLabel1";
            // 
            // m_pbPictureBox
            // 
            this.m_pbPictureBox.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Zoom;
            this.m_pbPictureBox.Image = global::UtilitiesDemo.Properties.Resources.CartoonPeople;
            this.m_pbPictureBox.Location = new System.Drawing.Point(7, 76);
            this.m_pbPictureBox.Name = "m_pbPictureBox";
            this.m_pbPictureBox.Size = new System.Drawing.Size(148, 97);
            this.m_pbPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.m_pbPictureBox.TabIndex = 2;
            this.m_pbPictureBox.TabStop = false;
            // 
            // m_numNumericUpDown
            // 
            this.m_numNumericUpDown.Location = new System.Drawing.Point(103, 6);
            this.m_numNumericUpDown.Name = "m_numNumericUpDown";
            this.m_numNumericUpDown.Size = new System.Drawing.Size(60, 23);
            this.m_numNumericUpDown.TabIndex = 1;
            // 
            // m_lbListBox
            // 
            this.m_lbListBox.FormattingEnabled = true;
            this.m_lbListBox.ItemHeight = 15;
            this.m_lbListBox.Items.AddRange(new object[] {
            "Listbox Item 1",
            "Listbox Item 2",
            "Listbox Item 3",
            "Listbox Item 4"});
            this.m_lbListBox.Location = new System.Drawing.Point(6, 6);
            this.m_lbListBox.Name = "m_lbListBox";
            this.m_lbListBox.Size = new System.Drawing.Size(91, 64);
            this.m_lbListBox.TabIndex = 0;
            // 
            // m_tabPage2
            // 
            this.m_tabPage2.Controls.Add(this.m_mcMonthCalendar);
            this.m_tabPage2.Location = new System.Drawing.Point(4, 24);
            this.m_tabPage2.Name = "m_tabPage2";
            this.m_tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabPage2.Size = new System.Drawing.Size(247, 179);
            this.m_tabPage2.TabIndex = 1;
            this.m_tabPage2.Text = "tabPage2";
            this.m_tabPage2.UseVisualStyleBackColor = true;
            // 
            // m_mcMonthCalendar
            // 
            this.m_mcMonthCalendar.Location = new System.Drawing.Point(9, 9);
            this.m_mcMonthCalendar.Name = "m_mcMonthCalendar";
            this.m_mcMonthCalendar.TabIndex = 0;
            // 
            // ToolTipExTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(278, 384);
            this.Controls.Add(this.m_tabControl);
            this.Controls.Add(this.m_pnlPanel);
            this.Controls.Add(this.m_grpGroupBox);
            this.Controls.Add(this.m_btnButton);
            this.Controls.Add(this.m_txtTextBox);
            this.Controls.Add(this.m_lblLabel);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Icon = global::UtilitiesDemo.Properties.Resources.Tools;
            this.Name = "ToolTipExTestForm";
            this.Text = "ToolTipEx Test";
            this.m_grpGroupBox.ResumeLayout(false);
            this.m_pnlPanel.ResumeLayout(false);
            this.m_pnlPanel.PerformLayout();
            this.m_tabControl.ResumeLayout(false);
            this.m_tabPage1.ResumeLayout(false);
            this.m_tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_numNumericUpDown)).EndInit();
            this.m_tabPage2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label m_lblLabel;
        private System.Windows.Forms.TextBox m_txtTextBox;
        private System.Windows.Forms.Button m_btnButton;
        private System.Windows.Forms.GroupBox m_grpGroupBox;
        private System.Windows.Forms.ComboBox m_cmbComboBox;
        private System.Windows.Forms.DateTimePicker m_dtDateTimePicker;
        private System.Windows.Forms.Panel m_pnlPanel;
        private System.Windows.Forms.CheckBox m_chkCheckBox;
        private System.Windows.Forms.Label m_lblPanel;
        private System.Windows.Forms.TabControl m_tabControl;
        private System.Windows.Forms.TabPage m_tabPage1;
        private System.Windows.Forms.PictureBox m_pbPictureBox;
        private System.Windows.Forms.NumericUpDown m_numNumericUpDown;
        private System.Windows.Forms.ListBox m_lbListBox;
        private System.Windows.Forms.TabPage m_tabPage2;
        private System.Windows.Forms.MonthCalendar m_mcMonthCalendar;
        private System.Windows.Forms.LinkLabel m_llLinkLabel;
        private System.Windows.Forms.RichTextBox m_rtfRichTextBox;
    }
}

