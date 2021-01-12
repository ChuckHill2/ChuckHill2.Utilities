namespace UtilitiesDemo
{
    partial class ToolTipManagerTestForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
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
            this.m_msMenuStrip = new System.Windows.Forms.MenuStrip();
            this.m_msFile = new System.Windows.Forms.ToolStripMenuItem();
            this.m_msFile_Item = new System.Windows.Forms.ToolStripMenuItem();
            this.m_msFile_Item_SubItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_msEdit = new System.Windows.Forms.ToolStripMenuItem();
            this.m_ssStatusStrip = new System.Windows.Forms.StatusStrip();
            this.m_ssStatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.m_ssProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.m_tsToolStrip = new System.Windows.Forms.ToolStrip();
            this.m_tsButton = new System.Windows.Forms.ToolStripButton();
            this.m_tsSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.m_tsTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.m_tsDropDownButton = new System.Windows.Forms.ToolStripDropDownButton();
            this.m_ttToolTipManager = new ChuckHill2.Forms.ToolTipManager(this.components);
            this.m_grpGroupBox.SuspendLayout();
            this.m_pnlPanel.SuspendLayout();
            this.m_tabControl.SuspendLayout();
            this.m_tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbPictureBox)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_numNumericUpDown)).BeginInit();
            this.m_tabPage2.SuspendLayout();
            this.m_msMenuStrip.SuspendLayout();
            this.m_ssStatusStrip.SuspendLayout();
            this.m_tsToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_lblLabel
            // 
            this.m_lblLabel.AutoSize = true;
            this.m_lblLabel.Location = new System.Drawing.Point(12, 61);
            this.m_lblLabel.Name = "m_lblLabel";
            this.m_lblLabel.Size = new System.Drawing.Size(58, 15);
            this.m_lblLabel.TabIndex = 0;
            this.m_lblLabel.Text = "Label Test";
            // 
            // m_txtTextBox
            // 
            this.m_txtTextBox.Location = new System.Drawing.Point(133, 61);
            this.m_txtTextBox.Name = "m_txtTextBox";
            this.m_txtTextBox.Size = new System.Drawing.Size(138, 23);
            this.m_txtTextBox.TabIndex = 1;
            this.m_txtTextBox.Text = "TextBox Test";
            // 
            // m_btnButton
            // 
            this.m_btnButton.Location = new System.Drawing.Point(13, 87);
            this.m_btnButton.Name = "m_btnButton";
            this.m_btnButton.Size = new System.Drawing.Size(75, 22);
            this.m_btnButton.TabIndex = 2;
            this.m_btnButton.Text = "Button Test";
            this.m_btnButton.UseVisualStyleBackColor = true;
            this.m_btnButton.Click += new System.EventHandler(this.button1_Click);
            // 
            // m_grpGroupBox
            // 
            this.m_grpGroupBox.Controls.Add(this.m_cmbComboBox);
            this.m_grpGroupBox.Controls.Add(this.m_dtDateTimePicker);
            this.m_grpGroupBox.Location = new System.Drawing.Point(133, 87);
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
            this.m_pnlPanel.Location = new System.Drawing.Point(13, 117);
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
            this.m_tabControl.Location = new System.Drawing.Point(16, 223);
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
            this.m_rtfRichTextBox.Location = new System.Drawing.Point(168, 6);
            this.m_rtfRichTextBox.Name = "m_rtfRichTextBox";
            this.m_rtfRichTextBox.Size = new System.Drawing.Size(72, 167);
            this.m_rtfRichTextBox.TabIndex = 5;
            this.m_rtfRichTextBox.Text = "RTF Text";
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
            // m_msMenuStrip
            // 
            this.m_msMenuStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_msFile,
            this.m_msEdit});
            this.m_msMenuStrip.Location = new System.Drawing.Point(0, 0);
            this.m_msMenuStrip.Name = "m_msMenuStrip";
            this.m_msMenuStrip.Size = new System.Drawing.Size(287, 24);
            this.m_msMenuStrip.TabIndex = 6;
            this.m_msMenuStrip.Text = "menuStrip1";
            // 
            // m_msFile
            // 
            this.m_msFile.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_msFile_Item});
            this.m_msFile.Name = "m_msFile";
            this.m_msFile.Size = new System.Drawing.Size(37, 20);
            this.m_msFile.Text = "File";
            // 
            // m_msFile_Item
            // 
            this.m_msFile_Item.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_msFile_Item_SubItem});
            this.m_msFile_Item.Name = "m_msFile_Item";
            this.m_msFile_Item.Size = new System.Drawing.Size(98, 22);
            this.m_msFile_Item.Text = "Item";
            // 
            // m_msFile_Item_SubItem
            // 
            this.m_msFile_Item_SubItem.Name = "m_msFile_Item_SubItem";
            this.m_msFile_Item_SubItem.Size = new System.Drawing.Size(123, 22);
            this.m_msFile_Item_SubItem.Text = "Sub-Item";
            // 
            // m_msEdit
            // 
            this.m_msEdit.Name = "m_msEdit";
            this.m_msEdit.Size = new System.Drawing.Size(39, 20);
            this.m_msEdit.Text = "Edit";
            // 
            // m_ssStatusStrip
            // 
            this.m_ssStatusStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_ssStatusLabel,
            this.m_ssProgressBar});
            this.m_ssStatusStrip.Location = new System.Drawing.Point(0, 451);
            this.m_ssStatusStrip.Name = "m_ssStatusStrip";
            this.m_ssStatusStrip.Size = new System.Drawing.Size(287, 23);
            this.m_ssStatusStrip.TabIndex = 7;
            this.m_ssStatusStrip.Text = "statusStrip1";
            // 
            // m_ssStatusLabel
            // 
            this.m_ssStatusLabel.Name = "m_ssStatusLabel";
            this.m_ssStatusLabel.Size = new System.Drawing.Size(67, 18);
            this.m_ssStatusLabel.Text = "StatusLabel";
            // 
            // m_ssProgressBar
            // 
            this.m_ssProgressBar.Name = "m_ssProgressBar";
            this.m_ssProgressBar.Size = new System.Drawing.Size(100, 17);
            // 
            // m_tsToolStrip
            // 
            this.m_tsToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.m_tsButton,
            this.m_tsSeparator,
            this.m_tsTextBox,
            this.m_tsDropDownButton});
            this.m_tsToolStrip.Location = new System.Drawing.Point(0, 24);
            this.m_tsToolStrip.Name = "m_tsToolStrip";
            this.m_tsToolStrip.Size = new System.Drawing.Size(287, 25);
            this.m_tsToolStrip.TabIndex = 8;
            this.m_tsToolStrip.Text = "toolStrip1";
            // 
            // m_tsButton
            // 
            this.m_tsButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_tsButton.Image = ((System.Drawing.Image)(resources.GetObject("m_tsButton.Image")));
            this.m_tsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_tsButton.Name = "m_tsButton";
            this.m_tsButton.Size = new System.Drawing.Size(23, 22);
            this.m_tsButton.Text = "TsButtonText";
            // 
            // m_tsSeparator
            // 
            this.m_tsSeparator.Name = "m_tsSeparator";
            this.m_tsSeparator.Size = new System.Drawing.Size(6, 25);
            // 
            // m_tsTextBox
            // 
            this.m_tsTextBox.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.m_tsTextBox.Name = "m_tsTextBox";
            this.m_tsTextBox.Size = new System.Drawing.Size(100, 25);
            this.m_tsTextBox.Text = "This is text";
            // 
            // m_tsDropDownButton
            // 
            this.m_tsDropDownButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.m_tsDropDownButton.Image = ((System.Drawing.Image)(resources.GetObject("m_tsDropDownButton.Image")));
            this.m_tsDropDownButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.m_tsDropDownButton.Name = "m_tsDropDownButton";
            this.m_tsDropDownButton.Size = new System.Drawing.Size(29, 22);
            this.m_tsDropDownButton.Text = "TsDropDownButton";
            // 
            // m_ttToolTipManager
            // 
            this.m_ttToolTipManager.AddRuntimeToolTips = true;
            this.m_ttToolTipManager.FillColor = System.Drawing.Color.Azure;
            this.m_ttToolTipManager.Host = this;
            // 
            // ToolTipManagerTestForm
            // 
            this.AccessibleDescription = "";
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.ClientSize = new System.Drawing.Size(287, 474);
            this.Controls.Add(this.m_tsToolStrip);
            this.Controls.Add(this.m_ssStatusStrip);
            this.Controls.Add(this.m_tabControl);
            this.Controls.Add(this.m_pnlPanel);
            this.Controls.Add(this.m_grpGroupBox);
            this.Controls.Add(this.m_btnButton);
            this.Controls.Add(this.m_txtTextBox);
            this.Controls.Add(this.m_lblLabel);
            this.Controls.Add(this.m_msMenuStrip);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.MainMenuStrip = this.m_msMenuStrip;
            this.Name = "ToolTipManagerTestForm";
            this.Text = "ToolTipManager Test";
            this.Icon = global::UtilitiesDemo.Properties.Resources.favicon;
            this.m_grpGroupBox.ResumeLayout(false);
            this.m_pnlPanel.ResumeLayout(false);
            this.m_pnlPanel.PerformLayout();
            this.m_tabControl.ResumeLayout(false);
            this.m_tabPage1.ResumeLayout(false);
            this.m_tabPage1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.m_pbPictureBox)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_numNumericUpDown)).EndInit();
            this.m_tabPage2.ResumeLayout(false);
            this.m_msMenuStrip.ResumeLayout(false);
            this.m_msMenuStrip.PerformLayout();
            this.m_ssStatusStrip.ResumeLayout(false);
            this.m_ssStatusStrip.PerformLayout();
            this.m_tsToolStrip.ResumeLayout(false);
            this.m_tsToolStrip.PerformLayout();
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
        private System.Windows.Forms.MenuStrip m_msMenuStrip;
        private System.Windows.Forms.ToolStripMenuItem m_msFile;
        private System.Windows.Forms.ToolStripMenuItem m_msFile_Item;
        private System.Windows.Forms.ToolStripMenuItem m_msFile_Item_SubItem;
        private System.Windows.Forms.ToolStripMenuItem m_msEdit;
        private System.Windows.Forms.StatusStrip m_ssStatusStrip;
        private System.Windows.Forms.ToolStripStatusLabel m_ssStatusLabel;
        private System.Windows.Forms.ToolStripProgressBar m_ssProgressBar;
        private System.Windows.Forms.ToolStrip m_tsToolStrip;
        private System.Windows.Forms.ToolStripButton m_tsButton;
        private System.Windows.Forms.ToolStripSeparator m_tsSeparator;
        private System.Windows.Forms.ToolStripTextBox m_tsTextBox;
        private System.Windows.Forms.ToolStripDropDownButton m_tsDropDownButton;
        private ChuckHill2.Forms.ToolTipManager m_ttToolTipManager;
    }
}

