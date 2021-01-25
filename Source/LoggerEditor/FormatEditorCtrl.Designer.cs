namespace ChuckHill2.LoggerEditor
{
    partial class FormatEditorCtrl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.m_txtFormat = new System.Windows.Forms.TextBox();
            this.m_lvChoices = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.m_txtFormat);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.m_lvChoices);
            this.splitContainer1.Size = new System.Drawing.Size(634, 516);
            this.splitContainer1.SplitterDistance = 211;
            this.splitContainer1.TabIndex = 0;
            // 
            // m_txtFormat
            // 
            this.m_txtFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_txtFormat.Location = new System.Drawing.Point(0, 0);
            this.m_txtFormat.Multiline = true;
            this.m_txtFormat.Name = "m_txtFormat";
            this.m_txtFormat.Size = new System.Drawing.Size(634, 211);
            this.m_txtFormat.TabIndex = 0;
            // 
            // m_lvChoices
            // 
            this.m_lvChoices.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_lvChoices.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.m_lvChoices.HideSelection = false;
            this.m_lvChoices.Location = new System.Drawing.Point(0, 0);
            this.m_lvChoices.MultiSelect = false;
            this.m_lvChoices.Name = "m_lvChoices";
            this.m_lvChoices.ShowItemToolTips = true;
            this.m_lvChoices.Size = new System.Drawing.Size(634, 301);
            this.m_lvChoices.TabIndex = 0;
            this.m_lvChoices.UseCompatibleStateImageBehavior = false;
            this.m_lvChoices.View = System.Windows.Forms.View.Details;
            this.m_lvChoices.DoubleClick += new System.EventHandler(this.m_lvChoices_DoubleClick);
            // 
            // FormatBuilder
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "FormatBuilder";
            this.Size = new System.Drawing.Size(634, 516);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TextBox m_txtFormat;
        private System.Windows.Forms.ListView m_lvChoices;
    }
}
