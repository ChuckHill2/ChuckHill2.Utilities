namespace ChuckHill2.LoggerEditor
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMain));
            this.m_btnCommit = new System.Windows.Forms.Button();
            this.m_btnExit = new System.Windows.Forms.Button();
            this.m_btnOpen = new System.Windows.Forms.Button();
            this.m_ttToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.m_btnHelp = new System.Windows.Forms.Button();
            this.m_appConfigFile = new ChuckHill2.LoggerEditor.LabeledTextBox();
            this.m_tcMain = new System.Windows.Forms.TabControl();
            this.m_tabListeners = new System.Windows.Forms.TabPage();
            this.m_ListenersControl = new ChuckHill2.LoggerEditor.ListenersCtrl();
            this.m_tabSwitches = new System.Windows.Forms.TabPage();
            this.m_SwitchesControl = new ChuckHill2.LoggerEditor.SwitchesCtrl();
            this.m_tabSources = new System.Windows.Forms.TabPage();
            this.m_SourcesControl = new ChuckHill2.LoggerEditor.SourcesCtrl();
            this.m_tabTrace = new System.Windows.Forms.TabPage();
            this.m_TraceControl = new ChuckHill2.LoggerEditor.TraceCtrl();
            this.m_tcMain.SuspendLayout();
            this.m_tabListeners.SuspendLayout();
            this.m_tabSwitches.SuspendLayout();
            this.m_tabSources.SuspendLayout();
            this.m_tabTrace.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_btnCommit
            // 
            this.m_btnCommit.AccessibleDescription = "";
            this.m_btnCommit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnCommit.Location = new System.Drawing.Point(324, 425);
            this.m_btnCommit.Name = "m_btnCommit";
            this.m_btnCommit.Size = new System.Drawing.Size(75, 23);
            this.m_btnCommit.TabIndex = 0;
            this.m_btnCommit.Text = "Commit";
            this.m_ttToolTip.SetToolTip(this.m_btnCommit, "Commit changes. Does not exit. If not \r\nmodified, this button does nothing. \r\nUse" +
        " Exit button to exit.");
            this.m_btnCommit.UseVisualStyleBackColor = true;
            this.m_btnCommit.Click += new System.EventHandler(this.m_btnCommit_Click);
            // 
            // m_btnExit
            // 
            this.m_btnExit.AccessibleDescription = "";
            this.m_btnExit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnExit.Location = new System.Drawing.Point(407, 425);
            this.m_btnExit.Name = "m_btnExit";
            this.m_btnExit.Size = new System.Drawing.Size(75, 23);
            this.m_btnExit.TabIndex = 1;
            this.m_btnExit.Text = "Exit";
            this.m_ttToolTip.SetToolTip(this.m_btnExit, "Exit without saving any \r\nchanges and without asking.");
            this.m_btnExit.UseVisualStyleBackColor = true;
            this.m_btnExit.Click += new System.EventHandler(this.m_btnExit_Click);
            // 
            // m_btnOpen
            // 
            this.m_btnOpen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.m_btnOpen.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.m_btnOpen.Image = global::ChuckHill2.LoggerEditor.Properties.Resources.openfile;
            this.m_btnOpen.Location = new System.Drawing.Point(462, 13);
            this.m_btnOpen.Name = "m_btnOpen";
            this.m_btnOpen.Size = new System.Drawing.Size(22, 22);
            this.m_btnOpen.TabIndex = 8;
            this.m_ttToolTip.SetToolTip(this.m_btnOpen, "Open the file dialog to select the \r\nexisting application config file to \r\nedit.");
            this.m_btnOpen.UseVisualStyleBackColor = true;
            this.m_btnOpen.Click += new System.EventHandler(this.m_btnOpen_Click);
            // 
            // m_btnHelp
            // 
            this.m_btnHelp.AccessibleDescription = "";
            this.m_btnHelp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.m_btnHelp.Location = new System.Drawing.Point(11, 425);
            this.m_btnHelp.Name = "m_btnHelp";
            this.m_btnHelp.Size = new System.Drawing.Size(75, 23);
            this.m_btnHelp.TabIndex = 10;
            this.m_btnHelp.Text = "Help";
            this.m_ttToolTip.SetToolTip(this.m_btnHelp, "Commit changes. Does not exit. If not \r\nmodified, this button does nothing. \r\nUse" +
        " Exit button to exit.");
            this.m_btnHelp.UseVisualStyleBackColor = true;
            this.m_btnHelp.Click += new System.EventHandler(this.m_btnHelp_Click);
            // 
            // m_appConfigFile
            // 
            this.m_appConfigFile.AllowDrop = true;
            this.m_appConfigFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_appConfigFile.Location = new System.Drawing.Point(12, 14);
            this.m_appConfigFile.Name = "m_appConfigFile";
            this.m_appConfigFile.Size = new System.Drawing.Size(448, 20);
            this.m_appConfigFile.TabIndex = 3;
            this.m_appConfigFile.TextLabel = "Select Application Config File...";
            this.m_ttToolTip.SetToolTip(this.m_appConfigFile, resources.GetString("m_appConfigFile.ToolTip"));
            this.m_appConfigFile.DragDrop += new System.Windows.Forms.DragEventHandler(this.m_appConfigFile_DragDrop);
            this.m_appConfigFile.DragEnter += new System.Windows.Forms.DragEventHandler(this.m_appConfigFile_DragEnter);
            this.m_appConfigFile.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.m_appConfigFile_KeyPress);
            // 
            // m_tcMain
            // 
            this.m_tcMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_tcMain.Controls.Add(this.m_tabListeners);
            this.m_tcMain.Controls.Add(this.m_tabSwitches);
            this.m_tcMain.Controls.Add(this.m_tabSources);
            this.m_tcMain.Controls.Add(this.m_tabTrace);
            this.m_tcMain.Location = new System.Drawing.Point(12, 41);
            this.m_tcMain.Name = "m_tcMain";
            this.m_tcMain.SelectedIndex = 0;
            this.m_tcMain.Size = new System.Drawing.Size(472, 379);
            this.m_tcMain.TabIndex = 9;
            // 
            // m_tabListeners
            // 
            this.m_tabListeners.BackColor = System.Drawing.Color.AliceBlue;
            this.m_tabListeners.Controls.Add(this.m_ListenersControl);
            this.m_tabListeners.Location = new System.Drawing.Point(4, 22);
            this.m_tabListeners.Name = "m_tabListeners";
            this.m_tabListeners.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabListeners.Size = new System.Drawing.Size(464, 353);
            this.m_tabListeners.TabIndex = 0;
            this.m_tabListeners.Text = "Listeners";
            // 
            // m_ListenersControl
            // 
            this.m_ListenersControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_ListenersControl.Location = new System.Drawing.Point(3, 3);
            this.m_ListenersControl.Name = "m_ListenersControl";
            this.m_ListenersControl.Size = new System.Drawing.Size(458, 347);
            this.m_ListenersControl.TabIndex = 0;
            // 
            // m_tabSwitches
            // 
            this.m_tabSwitches.Controls.Add(this.m_SwitchesControl);
            this.m_tabSwitches.Location = new System.Drawing.Point(4, 22);
            this.m_tabSwitches.Name = "m_tabSwitches";
            this.m_tabSwitches.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabSwitches.Size = new System.Drawing.Size(464, 353);
            this.m_tabSwitches.TabIndex = 2;
            this.m_tabSwitches.Text = "Switches";
            this.m_tabSwitches.UseVisualStyleBackColor = true;
            // 
            // m_SwitchesControl
            // 
            this.m_SwitchesControl.BackColor = System.Drawing.Color.AliceBlue;
            this.m_SwitchesControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SwitchesControl.Location = new System.Drawing.Point(3, 3);
            this.m_SwitchesControl.Name = "m_SwitchesControl";
            this.m_SwitchesControl.Size = new System.Drawing.Size(458, 347);
            this.m_SwitchesControl.TabIndex = 0;
            // 
            // m_tabSources
            // 
            this.m_tabSources.Controls.Add(this.m_SourcesControl);
            this.m_tabSources.Location = new System.Drawing.Point(4, 22);
            this.m_tabSources.Name = "m_tabSources";
            this.m_tabSources.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabSources.Size = new System.Drawing.Size(464, 353);
            this.m_tabSources.TabIndex = 3;
            this.m_tabSources.Text = "Sources";
            this.m_tabSources.UseVisualStyleBackColor = true;
            // 
            // m_SourcesControl
            // 
            this.m_SourcesControl.BackColor = System.Drawing.Color.AliceBlue;
            this.m_SourcesControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_SourcesControl.Location = new System.Drawing.Point(3, 3);
            this.m_SourcesControl.Name = "m_SourcesControl";
            this.m_SourcesControl.Size = new System.Drawing.Size(458, 347);
            this.m_SourcesControl.TabIndex = 0;
            // 
            // m_tabTrace
            // 
            this.m_tabTrace.Controls.Add(this.m_TraceControl);
            this.m_tabTrace.Location = new System.Drawing.Point(4, 22);
            this.m_tabTrace.Name = "m_tabTrace";
            this.m_tabTrace.Padding = new System.Windows.Forms.Padding(3);
            this.m_tabTrace.Size = new System.Drawing.Size(464, 353);
            this.m_tabTrace.TabIndex = 1;
            this.m_tabTrace.Text = "Trace";
            this.m_tabTrace.UseVisualStyleBackColor = true;
            // 
            // m_TraceControl
            // 
            this.m_TraceControl.BackColor = System.Drawing.Color.AliceBlue;
            this.m_TraceControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.m_TraceControl.Location = new System.Drawing.Point(3, 3);
            this.m_TraceControl.Name = "m_TraceControl";
            this.m_TraceControl.Size = new System.Drawing.Size(458, 347);
            this.m_TraceControl.TabIndex = 0;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(496, 460);
            this.Controls.Add(this.m_btnHelp);
            this.Controls.Add(this.m_tcMain);
            this.Controls.Add(this.m_btnOpen);
            this.Controls.Add(this.m_appConfigFile);
            this.Controls.Add(this.m_btnExit);
            this.Controls.Add(this.m_btnCommit);
            this.Icon = global::ChuckHill2.LoggerEditor.Properties.Resources.favicon;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(377, 261);
            this.Name = "FormMain";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Show;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Logger Editor";
            this.m_tcMain.ResumeLayout(false);
            this.m_tabListeners.ResumeLayout(false);
            this.m_tabSwitches.ResumeLayout(false);
            this.m_tabSources.ResumeLayout(false);
            this.m_tabTrace.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button m_btnCommit;
        private System.Windows.Forms.Button m_btnExit;
        private ChuckHill2.LoggerEditor.LabeledTextBox m_appConfigFile;
        private System.Windows.Forms.Button m_btnOpen;
        private System.Windows.Forms.ToolTip m_ttToolTip;
        private System.Windows.Forms.TabControl m_tcMain;
        private System.Windows.Forms.TabPage m_tabListeners;
        private System.Windows.Forms.TabPage m_tabTrace;
        private System.Windows.Forms.TabPage m_tabSwitches;
        private System.Windows.Forms.TabPage m_tabSources;
        private ListenersCtrl m_ListenersControl;
        private SwitchesCtrl m_SwitchesControl;
        private TraceCtrl m_TraceControl;
        private SourcesCtrl m_SourcesControl;
        private System.Windows.Forms.Button m_btnHelp;
    }
}

