namespace ChuckHill2.LoggerEditor
{
    partial class SwitchGroupsCtrl
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
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            this.m_grid = new System.Windows.Forms.DataGridView();
            this.m_gridcolName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.m_gridcolSourceLevel = new System.Windows.Forms.DataGridViewComboBoxColumn();
            this.m_gridBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.m_lblTrace = new ChuckHill2.Forms.GradientLabel();
            ((System.ComponentModel.ISupportInitialize)(this.m_grid)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_gridBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // m_grid
            // 
            this.m_grid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_grid.AutoGenerateColumns = false;
            this.m_grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.m_grid.BackgroundColor = System.Drawing.Color.AliceBlue;
            this.m_grid.ClipboardCopyMode = System.Windows.Forms.DataGridViewClipboardCopyMode.EnableWithoutHeaderText;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle1.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.m_grid.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.m_grid.ColumnHeadersHeight = 22;
            this.m_grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.m_grid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.m_gridcolName,
            this.m_gridcolSourceLevel});
            this.m_grid.DataSource = this.m_gridBindingSource;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.AliceBlue;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.m_grid.DefaultCellStyle = dataGridViewCellStyle2;
            this.m_grid.EnableHeadersVisualStyles = false;
            this.m_grid.GridColor = System.Drawing.Color.AliceBlue;
            this.m_grid.Location = new System.Drawing.Point(3, 30);
            this.m_grid.Name = "m_grid";
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.LightSteelBlue;
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            dataGridViewCellStyle3.ForeColor = System.Drawing.SystemColors.WindowText;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.Color.SkyBlue;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.m_grid.RowHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.m_grid.RowHeadersVisible = false;
            this.m_grid.RowTemplate.Height = 20;
            this.m_grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.m_grid.Size = new System.Drawing.Size(507, 388);
            this.m_grid.TabIndex = 0;
            this.m_grid.CellValidating += new System.Windows.Forms.DataGridViewCellValidatingEventHandler(this.m_grid_CellValidating);
            this.m_grid.CurrentCellChanged += new System.EventHandler(this.m_grid_CurrentCellChanged);
            this.m_grid.DataError += new System.Windows.Forms.DataGridViewDataErrorEventHandler(this.m_grid_DataError);
            this.m_grid.DefaultValuesNeeded += new System.Windows.Forms.DataGridViewRowEventHandler(this.m_grid_DefaultValuesNeeded);
            this.m_grid.UserAddedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.m_grid_UserAddedRow);
            this.m_grid.UserDeletedRow += new System.Windows.Forms.DataGridViewRowEventHandler(this.m_grid_UserDeletedRow);
            this.m_grid.UserDeletingRow += new System.Windows.Forms.DataGridViewRowCancelEventHandler(this.m_grid_UserDeletingRow);
            // 
            // m_gridcolName
            // 
            this.m_gridcolName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.m_gridcolName.DataPropertyName = "Name";
            this.m_gridcolName.HeaderText = "Name";
            this.m_gridcolName.MinimumWidth = 103;
            this.m_gridcolName.Name = "m_gridcolName";
            this.m_gridcolName.ToolTipText = "Switch group name";
            this.m_gridcolName.Width = 103;
            // 
            // m_gridcolSourceLevel
            // 
            this.m_gridcolSourceLevel.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.None;
            this.m_gridcolSourceLevel.DataPropertyName = "SourceLevelString";
            this.m_gridcolSourceLevel.DisplayStyleForCurrentCellOnly = true;
            this.m_gridcolSourceLevel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.m_gridcolSourceLevel.HeaderText = "Severity Level";
            this.m_gridcolSourceLevel.MinimumWidth = 100;
            this.m_gridcolSourceLevel.Name = "m_gridcolSourceLevel";
            this.m_gridcolSourceLevel.Resizable = System.Windows.Forms.DataGridViewTriState.True;
            this.m_gridcolSourceLevel.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.Automatic;
            this.m_gridcolSourceLevel.ToolTipText = "Select a severity level. Note severity levels are cumulative\\n(e.g. Error=Errors " +
    "only, Warning=Errors+Warnings, etc).\\nUse Right-click to change multiple rows at" +
    " one time.";
            // 
            // m_gridBindingSource
            // 
            this.m_gridBindingSource.AllowNew = true;
            this.m_gridBindingSource.DataMember = "Groups";
            this.m_gridBindingSource.DataSource = typeof(ChuckHill2.LoggerEditor.SwitchGroupsCtrl);
            // 
            // m_lblTrace
            // 
            this.m_lblTrace.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.m_lblTrace.BackgroundGradient = new ChuckHill2.Forms.GradientBrush(null, System.Drawing.SystemColors.ControlDark, System.Drawing.Color.Transparent, ChuckHill2.Forms.GradientStyle.Horizontal, false);
            this.m_lblTrace.Location = new System.Drawing.Point(3, 3);
            this.m_lblTrace.Name = "m_lblTrace";
            this.m_lblTrace.Size = new System.Drawing.Size(507, 23);
            this.m_lblTrace.TabIndex = 1;
            this.m_lblTrace.Text = " Switch Groups";
            this.m_lblTrace.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // SwitchGroupsCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.m_lblTrace);
            this.Controls.Add(this.m_grid);
            this.Name = "SwitchGroupsCtrl";
            this.Size = new System.Drawing.Size(513, 421);
            ((System.ComponentModel.ISupportInitialize)(this.m_grid)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.m_gridBindingSource)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView m_grid;
        private System.Windows.Forms.BindingSource m_gridBindingSource;
        private System.Windows.Forms.DataGridViewTextBoxColumn m_gridcolName;
        private System.Windows.Forms.DataGridViewComboBoxColumn m_gridcolSourceLevel;
        private Forms.GradientLabel m_lblTrace;
    }
}
