namespace ChuckHill2.LoggerEditor
{
    partial class ListenersCtrl
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
            this.m_pgListenerProps = new System.Windows.Forms.PropertyGrid();
            this.m_lblListenerProps = new System.Windows.Forms.Label();
            this.m_btnAddListener = new System.Windows.Forms.Button();
            this.m_lvListeners = new System.Windows.Forms.ListView();
            this.m_ctxListeners = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.addToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.removeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.m_ctxListeners.SuspendLayout();
            this.SuspendLayout();
            // 
            // m_pgListenerProps
            // 
            this.m_pgListenerProps.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_pgListenerProps.HelpBackColor = System.Drawing.Color.Azure;
            this.m_pgListenerProps.Location = new System.Drawing.Point(178, 26);
            this.m_pgListenerProps.Name = "m_pgListenerProps";
            this.m_pgListenerProps.Size = new System.Drawing.Size(332, 392);
            this.m_pgListenerProps.TabIndex = 0;
            // 
            // m_lblListenerProps
            // 
            this.m_lblListenerProps.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblListenerProps.AutoEllipsis = true;
            this.m_lblListenerProps.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.m_lblListenerProps.Location = new System.Drawing.Point(178, 3);
            this.m_lblListenerProps.Name = "m_lblListenerProps";
            this.m_lblListenerProps.Size = new System.Drawing.Size(332, 23);
            this.m_lblListenerProps.TabIndex = 5;
            this.m_lblListenerProps.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // m_btnAddListener
            // 
            this.m_btnAddListener.Location = new System.Drawing.Point(3, 2);
            this.m_btnAddListener.Name = "m_btnAddListener";
            this.m_btnAddListener.Size = new System.Drawing.Size(169, 25);
            this.m_btnAddListener.TabIndex = 6;
            this.m_btnAddListener.Text = "Add Listener";
            this.toolTip1.SetToolTip(this.m_btnAddListener, "Add a new output \r\ndestination.");
            this.m_btnAddListener.UseVisualStyleBackColor = true;
            this.m_btnAddListener.Click += new System.EventHandler(this.m_btnAddListener_Click);
            // 
            // m_lvListeners
            // 
            this.m_lvListeners.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.m_lvListeners.ContextMenuStrip = this.m_ctxListeners;
            this.m_lvListeners.HideSelection = false;
            this.m_lvListeners.Location = new System.Drawing.Point(3, 33);
            this.m_lvListeners.MultiSelect = false;
            this.m_lvListeners.Name = "m_lvListeners";
            this.m_lvListeners.ShowGroups = false;
            this.m_lvListeners.Size = new System.Drawing.Size(169, 385);
            this.m_lvListeners.TabIndex = 7;
            this.toolTip1.SetToolTip(this.m_lvListeners, "List of available output destinations\r\nthat a source may write to. Right-\r\nclick " +
        "context menu to add and remove.");
            this.m_lvListeners.UseCompatibleStateImageBehavior = false;
            this.m_lvListeners.View = System.Windows.Forms.View.SmallIcon;
            this.m_lvListeners.SelectedIndexChanged += new System.EventHandler(this.m_lvListeners_SelectedIndexChanged);
            // 
            // m_ctxListeners
            // 
            this.m_ctxListeners.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.addToolStripMenuItem,
            this.removeToolStripMenuItem});
            this.m_ctxListeners.Name = "m_ctxListeners";
            this.m_ctxListeners.Size = new System.Drawing.Size(181, 70);
            // 
            // addToolStripMenuItem
            // 
            this.addToolStripMenuItem.Image = global::ChuckHill2.LoggerEditor.Properties.Resources.plus16;
            this.addToolStripMenuItem.Name = "addToolStripMenuItem";
            this.addToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.addToolStripMenuItem.Text = "Add";
            this.addToolStripMenuItem.ToolTipText = "Add a new blank \r\noutput destination.\r\n";
            this.addToolStripMenuItem.Click += new System.EventHandler(this.addToolStripMenuItem_Click);
            // 
            // removeToolStripMenuItem
            // 
            this.removeToolStripMenuItem.Image = global::ChuckHill2.LoggerEditor.Properties.Resources.minus16;
            this.removeToolStripMenuItem.Name = "removeToolStripMenuItem";
            this.removeToolStripMenuItem.Size = new System.Drawing.Size(180, 22);
            this.removeToolStripMenuItem.Text = "Remove";
            this.removeToolStripMenuItem.ToolTipText = "Remove the current output destination. \r\nYou will be asked to verify.";
            this.removeToolStripMenuItem.Click += new System.EventHandler(this.removeToolStripMenuItem_Click);
            // 
            // ListenersCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_lvListeners);
            this.Controls.Add(this.m_btnAddListener);
            this.Controls.Add(this.m_lblListenerProps);
            this.Controls.Add(this.m_pgListenerProps);
            this.Name = "ListenersCtrl";
            this.Size = new System.Drawing.Size(513, 421);
            this.m_ctxListeners.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid m_pgListenerProps;
        private System.Windows.Forms.Label m_lblListenerProps;
        private System.Windows.Forms.Button m_btnAddListener;
        private System.Windows.Forms.ListView m_lvListeners;
        private System.Windows.Forms.ContextMenuStrip m_ctxListeners;
        private System.Windows.Forms.ToolStripMenuItem removeToolStripMenuItem;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem addToolStripMenuItem;
    }
}
