namespace ChuckHill2.LoggerEditor
{
    partial class SourcesCtrl
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
            this.m_lvSources = new System.Windows.Forms.ListView();
            this.m_ctxSources = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.m_lblListeners = new System.Windows.Forms.Label();
            this.m_clbListeners = new System.Windows.Forms.CheckedListBox();
            this.m_lblSourcelevel = new System.Windows.Forms.Label();
            this.m_cmbSourceLevel = new System.Windows.Forms.ComboBox();
            this.m_lblName = new System.Windows.Forms.Label();
            this.m_cmbName = new System.Windows.Forms.ComboBox();
            this.m_ToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.m_lblSourceProperties = new ChuckHill2.Forms.GradientLabel();
            this.m_ctxSources.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_lvSources
            // 
            this.m_lvSources.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_lvSources.ContextMenuStrip = this.m_ctxSources;
            this.m_lvSources.HideSelection = false;
            this.m_lvSources.Location = new System.Drawing.Point(3, 30);
            this.m_lvSources.MultiSelect = false;
            this.m_lvSources.Name = "m_lvSources";
            this.m_lvSources.ShowGroups = false;
            this.m_lvSources.Size = new System.Drawing.Size(169, 388);
            this.m_lvSources.TabIndex = 11;
            this.m_ToolTip.SetToolTip(this.m_lvSources, "List of available logging sources the \r\nlogging API may write to. Right-click \r\nc" +
        "ontext menu to add and remove.");
            this.m_lvSources.UseCompatibleStateImageBehavior = false;
            this.m_lvSources.View = System.Windows.Forms.View.List;
            this.m_lvSources.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.m_lvSources_ItemSelectionChanged);
            // 
            // m_ctxSources
            // 
            this.m_ctxSources.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.m_ctxSources.Name = "m_ctxListeners";
            this.m_ctxSources.Size = new System.Drawing.Size(118, 48);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Image = global::ChuckHill2.LoggerEditor.Properties.Resources.plus16;
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.ToolTipText = "Add a new blank \r\nlogging source.";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Image = global::ChuckHill2.LoggerEditor.Properties.Resources.minus16;
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(117, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.ToolTipText = "Remove the current logging source. \r\nYou will be asked to verify.";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // panel1
            // 
            this.panel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.m_lblListeners);
            this.panel1.Controls.Add(this.m_clbListeners);
            this.panel1.Controls.Add(this.m_lblSourcelevel);
            this.panel1.Controls.Add(this.m_cmbSourceLevel);
            this.panel1.Controls.Add(this.m_lblName);
            this.panel1.Controls.Add(this.m_cmbName);
            this.panel1.Location = new System.Drawing.Point(178, 30);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(332, 388);
            this.panel1.TabIndex = 12;
            // 
            // m_lblListeners
            // 
            this.m_lblListeners.AutoSize = true;
            this.m_lblListeners.Location = new System.Drawing.Point(5, 91);
            this.m_lblListeners.Name = "m_lblListeners";
            this.m_lblListeners.Size = new System.Drawing.Size(104, 13);
            this.m_lblListeners.TabIndex = 5;
            this.m_lblListeners.Text = "Associated Listeners";
            this.m_ToolTip.SetToolTip(this.m_lblListeners, "Check which listeners that this \r\nsource will write messages to.");
            // 
            // m_clbListeners
            // 
            this.m_clbListeners.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_clbListeners.CheckOnClick = true;
            this.m_clbListeners.FormattingEnabled = true;
            this.m_clbListeners.IntegralHeight = false;
            this.m_clbListeners.Location = new System.Drawing.Point(5, 107);
            this.m_clbListeners.Name = "m_clbListeners";
            this.m_clbListeners.Size = new System.Drawing.Size(320, 275);
            this.m_clbListeners.TabIndex = 4;
            // 
            // m_lblSourcelevel
            // 
            this.m_lblSourcelevel.AutoSize = true;
            this.m_lblSourcelevel.Location = new System.Drawing.Point(5, 47);
            this.m_lblSourcelevel.Name = "m_lblSourcelevel";
            this.m_lblSourcelevel.Size = new System.Drawing.Size(70, 13);
            this.m_lblSourcelevel.TabIndex = 3;
            this.m_lblSourcelevel.Text = "Source Level";
            this.m_ToolTip.SetToolTip(this.m_lblSourcelevel, "Select a standard source \r\nlevel or switch group.");
            // 
            // m_cmbSourceLevel
            // 
            this.m_cmbSourceLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cmbSourceLevel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.m_cmbSourceLevel.FormattingEnabled = true;
            this.m_cmbSourceLevel.Location = new System.Drawing.Point(5, 63);
            this.m_cmbSourceLevel.Name = "m_cmbSourceLevel";
            this.m_cmbSourceLevel.Size = new System.Drawing.Size(320, 21);
            this.m_cmbSourceLevel.TabIndex = 2;
            // 
            // m_lblName
            // 
            this.m_lblName.AutoSize = true;
            this.m_lblName.Location = new System.Drawing.Point(5, 4);
            this.m_lblName.Name = "m_lblName";
            this.m_lblName.Size = new System.Drawing.Size(72, 13);
            this.m_lblName.TabIndex = 1;
            this.m_lblName.Text = "Source Name";
            this.m_ToolTip.SetToolTip(this.m_lblName, "Enter a unique name or select a \r\nbuilt-in one from the dropdown.");
            // 
            // m_cmbName
            // 
            this.m_cmbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_cmbName.Location = new System.Drawing.Point(5, 20);
            this.m_cmbName.Name = "m_cmbName";
            this.m_cmbName.Size = new System.Drawing.Size(320, 21);
            this.m_cmbName.TabIndex = 0;
            this.m_ToolTip.SetToolTip(this.m_cmbName, "Edit the source name or select a built-in \r\none from the dropdown. It must be uni" +
        "que \r\nand is case-insensitive.");
            // 
            // m_lblSourceProperties
            // 
            this.m_lblSourceProperties.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblSourceProperties.BackgroundGradient = new ChuckHill2.Forms.GradientBrush(null, System.Drawing.SystemColors.ControlDark, System.Drawing.Color.Transparent, ChuckHill2.Forms.GradientStyle.Horizontal, false);
            this.m_lblSourceProperties.Location = new System.Drawing.Point(3, 3);
            this.m_lblSourceProperties.Name = "m_lblSourceProperties";
            this.m_lblSourceProperties.Size = new System.Drawing.Size(507, 23);
            this.m_lblSourceProperties.TabIndex = 13;
            this.m_lblSourceProperties.Text = " Source Properties";
            this.m_lblSourceProperties.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SourcesCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_lblSourceProperties);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.m_lvSources);
            this.Name = "SourcesCtrl";
            this.Size = new System.Drawing.Size(513, 421);
            this.m_ctxSources.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView m_lvSources;
        private System.Windows.Forms.ContextMenuStrip m_ctxSources;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label m_lblListeners;
        private System.Windows.Forms.CheckedListBox m_clbListeners;
        private System.Windows.Forms.Label m_lblSourcelevel;
        private System.Windows.Forms.ComboBox m_cmbSourceLevel;
        private System.Windows.Forms.Label m_lblName;
        private System.Windows.Forms.ComboBox m_cmbName;
        private System.Windows.Forms.ToolTip m_ToolTip;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
        private Forms.GradientLabel m_lblSourceProperties;
    }
}
