namespace ChuckHill2.LoggerEditor
{
    partial class HelpPopup
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
            this.m_rtfHelp = new System.Windows.Forms.RichTextBox();
            this.SuspendLayout();
            // 
            // m_rtfHelp
            // 
            this.m_rtfHelp.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_rtfHelp.Location = new System.Drawing.Point(0, 0);
            this.m_rtfHelp.Name = "m_rtfHelp";
            this.m_rtfHelp.ReadOnly = true;
            this.m_rtfHelp.Size = new System.Drawing.Size(496, 460);
            this.m_rtfHelp.TabIndex = 0;
            this.m_rtfHelp.Text = "";
            // 
            // HelpPopup
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 460);
            this.Controls.Add(this.m_rtfHelp);
            this.Icon = global::ChuckHill2.LoggerEditor.Properties.Resources.favicon;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "HelpPopup";
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Logger Editor Help";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox m_rtfHelp;
    }
}
