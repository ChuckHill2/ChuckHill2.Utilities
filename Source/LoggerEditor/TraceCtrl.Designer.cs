namespace ChuckHill2.LoggerEditor
{
    partial class TraceCtrl
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
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.m_lblListeners = new System.Windows.Forms.Label();
            this.m_clbListeners = new System.Windows.Forms.CheckedListBox();
            this.m_lblIndentSize = new System.Windows.Forms.Label();
            this.m_numIndentSize = new System.Windows.Forms.NumericUpDown();
            this.m_grpAutoFlush = new System.Windows.Forms.GroupBox();
            this.m_radAutoFlushNo = new System.Windows.Forms.RadioButton();
            this.m_radAutoFlushYes = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.m_lblTrace = new ChuckHill2.Forms.GradientLabel();
            ((System.ComponentModel.ISupportInitialize)(this.m_numIndentSize)).BeginInit();
            this.m_grpAutoFlush.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_lblListeners
            // 
            this.m_lblListeners.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_lblListeners.AutoSize = true;
            this.m_lblListeners.Location = new System.Drawing.Point(86, 5);
            this.m_lblListeners.Name = "m_lblListeners";
            this.m_lblListeners.Size = new System.Drawing.Size(49, 13);
            this.m_lblListeners.TabIndex = 11;
            this.m_lblListeners.Text = "Listeners";
            this.toolTip1.SetToolTip(this.m_lblListeners, "Check one or more listeners as the\r\noutput destinations for diagnostic\r\ntrace mes" +
        "sages.");
            // 
            // m_clbListeners
            // 
            this.m_clbListeners.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_clbListeners.CheckOnClick = true;
            this.m_clbListeners.FormattingEnabled = true;
            this.m_clbListeners.IntegralHeight = false;
            this.m_clbListeners.Location = new System.Drawing.Point(88, 21);
            this.m_clbListeners.Name = "m_clbListeners";
            this.m_clbListeners.Size = new System.Drawing.Size(144, 306);
            this.m_clbListeners.TabIndex = 10;
            this.toolTip1.SetToolTip(this.m_clbListeners, "Check one or more listeners as the\r\noutput destinations for diagnostic\r\ntrace mes" +
        "sages.");
            // 
            // m_lblIndentSize
            // 
            this.m_lblIndentSize.AutoSize = true;
            this.m_lblIndentSize.Location = new System.Drawing.Point(7, 91);
            this.m_lblIndentSize.Name = "m_lblIndentSize";
            this.m_lblIndentSize.Size = new System.Drawing.Size(60, 13);
            this.m_lblIndentSize.TabIndex = 8;
            this.m_lblIndentSize.Text = "Indent Size";
            this.toolTip1.SetToolTip(this.m_lblIndentSize, "The number of spaces \r\nin an indent.");
            // 
            // m_numIndentSize
            // 
            this.m_numIndentSize.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_numIndentSize.Location = new System.Drawing.Point(10, 107);
            this.m_numIndentSize.Maximum = new decimal(new int[] {
            16,
            0,
            0,
            0});
            this.m_numIndentSize.Name = "m_numIndentSize";
            this.m_numIndentSize.Size = new System.Drawing.Size(69, 20);
            this.m_numIndentSize.TabIndex = 9;
            this.toolTip1.SetToolTip(this.m_numIndentSize, "The number of spaces \r\nin an indent.");
            this.m_numIndentSize.Value = new decimal(new int[] {
            4,
            0,
            0,
            0});
            // 
            // m_grpAutoFlush
            // 
            this.m_grpAutoFlush.Controls.Add(this.m_radAutoFlushNo);
            this.m_grpAutoFlush.Controls.Add(this.m_radAutoFlushYes);
            this.m_grpAutoFlush.Location = new System.Drawing.Point(10, 14);
            this.m_grpAutoFlush.Name = "m_grpAutoFlush";
            this.m_grpAutoFlush.Size = new System.Drawing.Size(69, 62);
            this.m_grpAutoFlush.TabIndex = 7;
            this.m_grpAutoFlush.TabStop = false;
            this.m_grpAutoFlush.Text = "AutoFlush";
            this.toolTip1.SetToolTip(this.m_grpAutoFlush, "True if Flush() is called on the \r\nListeners after every write.");
            // 
            // m_radAutoFlushNo
            // 
            this.m_radAutoFlushNo.AutoSize = true;
            this.m_radAutoFlushNo.Location = new System.Drawing.Point(8, 39);
            this.m_radAutoFlushNo.Name = "m_radAutoFlushNo";
            this.m_radAutoFlushNo.Size = new System.Drawing.Size(39, 17);
            this.m_radAutoFlushNo.TabIndex = 1;
            this.m_radAutoFlushNo.TabStop = true;
            this.m_radAutoFlushNo.Text = "No";
            this.m_radAutoFlushNo.UseVisualStyleBackColor = true;
            // 
            // m_radAutoFlushYes
            // 
            this.m_radAutoFlushYes.AutoSize = true;
            this.m_radAutoFlushYes.Location = new System.Drawing.Point(8, 17);
            this.m_radAutoFlushYes.Name = "m_radAutoFlushYes";
            this.m_radAutoFlushYes.Size = new System.Drawing.Size(43, 17);
            this.m_radAutoFlushYes.TabIndex = 0;
            this.m_radAutoFlushYes.TabStop = true;
            this.m_radAutoFlushYes.Text = "Yes";
            this.m_radAutoFlushYes.UseVisualStyleBackColor = true;
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.m_lblListeners);
            this.panel1.Controls.Add(this.m_clbListeners);
            this.panel1.Controls.Add(this.m_lblIndentSize);
            this.panel1.Controls.Add(this.m_numIndentSize);
            this.panel1.Controls.Add(this.m_grpAutoFlush);
            this.panel1.Location = new System.Drawing.Point(3, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(237, 332);
            this.panel1.TabIndex = 1;
            // 
            // m_lblTrace
            // 
            this.m_lblTrace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblTrace.BackgroundGradient = new ChuckHill2.Forms.GradientBrush(null, System.Drawing.SystemColors.ControlDark, System.Drawing.Color.Transparent, ChuckHill2.Forms.GradientStyle.Horizontal, false);
            this.m_lblTrace.Location = new System.Drawing.Point(3, 3);
            this.m_lblTrace.Name = "m_lblTrace";
            this.m_lblTrace.Size = new System.Drawing.Size(237, 23);
            this.m_lblTrace.TabIndex = 0;
            this.m_lblTrace.Text = " Trace Properties";
            this.m_lblTrace.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // TraceCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.m_lblTrace);
            this.Name = "TraceCtrl";
            this.Size = new System.Drawing.Size(243, 365);
            ((System.ComponentModel.ISupportInitialize)(this.m_numIndentSize)).EndInit();
            this.m_grpAutoFlush.ResumeLayout(false);
            this.m_grpAutoFlush.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private Forms.GradientLabel m_lblTrace;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label m_lblListeners;
        private System.Windows.Forms.CheckedListBox m_clbListeners;
        private System.Windows.Forms.Label m_lblIndentSize;
        private System.Windows.Forms.NumericUpDown m_numIndentSize;
        private System.Windows.Forms.GroupBox m_grpAutoFlush;
        private System.Windows.Forms.RadioButton m_radAutoFlushNo;
        private System.Windows.Forms.RadioButton m_radAutoFlushYes;
    }
}
