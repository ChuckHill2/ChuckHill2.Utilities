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
            this.m_btnColorDialog = new System.Windows.Forms.Button();
            this.m_btnSelectTvColor = new System.Windows.Forms.Button();
            this.m_btnSelectLbColor = new System.Windows.Forms.Button();
            this.m_btnColorPickerDlg = new System.Windows.Forms.Button();
            this.colorPickerPanel1 = new ChuckHill2.Utilities.ColorPickerPanel();
            this.SuspendLayout();
            // 
            // m_btnColorDialog
            // 
            this.m_btnColorDialog.Location = new System.Drawing.Point(15, 14);
            this.m_btnColorDialog.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnColorDialog.Name = "m_btnColorDialog";
            this.m_btnColorDialog.Size = new System.Drawing.Size(121, 52);
            this.m_btnColorDialog.TabIndex = 0;
            this.m_btnColorDialog.Text = "Popup ColorDialog";
            this.m_btnColorDialog.UseVisualStyleBackColor = true;
            this.m_btnColorDialog.Click += new System.EventHandler(this.m_btnColorDialog_Click);
            // 
            // m_btnSelectTvColor
            // 
            this.m_btnSelectTvColor.Location = new System.Drawing.Point(15, 73);
            this.m_btnSelectTvColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnSelectTvColor.Name = "m_btnSelectTvColor";
            this.m_btnSelectTvColor.Size = new System.Drawing.Size(121, 25);
            this.m_btnSelectTvColor.TabIndex = 4;
            this.m_btnSelectTvColor.Text = "Select TV Color";
            this.m_btnSelectTvColor.UseVisualStyleBackColor = true;
            this.m_btnSelectTvColor.Click += new System.EventHandler(this.m_btnSelectColor_Click);
            // 
            // m_btnSelectLbColor
            // 
            this.m_btnSelectLbColor.Location = new System.Drawing.Point(15, 105);
            this.m_btnSelectLbColor.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnSelectLbColor.Name = "m_btnSelectLbColor";
            this.m_btnSelectLbColor.Size = new System.Drawing.Size(121, 25);
            this.m_btnSelectLbColor.TabIndex = 5;
            this.m_btnSelectLbColor.Text = "Select LB Color";
            this.m_btnSelectLbColor.UseVisualStyleBackColor = true;
            this.m_btnSelectLbColor.Click += new System.EventHandler(this.m_btnSelectLbColor_Click);
            // 
            // m_btnColorPickerDlg
            // 
            this.m_btnColorPickerDlg.Location = new System.Drawing.Point(16, 137);
            this.m_btnColorPickerDlg.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.m_btnColorPickerDlg.Name = "m_btnColorPickerDlg";
            this.m_btnColorPickerDlg.Size = new System.Drawing.Size(121, 25);
            this.m_btnColorPickerDlg.TabIndex = 6;
            this.m_btnColorPickerDlg.Text = "ColorPicker Dlg";
            this.m_btnColorPickerDlg.UseVisualStyleBackColor = true;
            this.m_btnColorPickerDlg.Click += new System.EventHandler(this.m_btnColorPickerDlg_Click);
            // 
            // colorPickerPanel1
            // 
            this.colorPickerPanel1.BackColor = System.Drawing.Color.Transparent;
            this.colorPickerPanel1.Color = System.Drawing.Color.White;
            this.colorPickerPanel1.Font = new System.Drawing.Font("Tahoma", 8F);
            this.colorPickerPanel1.Location = new System.Drawing.Point(165, 148);
            this.colorPickerPanel1.Margin = new System.Windows.Forms.Padding(0);
            this.colorPickerPanel1.MaximumSize = new System.Drawing.Size(1000, 999);
            this.colorPickerPanel1.MinimumSize = new System.Drawing.Size(376, 182);
            this.colorPickerPanel1.Name = "colorPickerPanel1";
            this.colorPickerPanel1.Size = new System.Drawing.Size(468, 260);
            this.colorPickerPanel1.TabIndex = 7;
            // 
            // FormMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(835, 795);
            this.Controls.Add(this.colorPickerPanel1);
            this.Controls.Add(this.m_btnColorPickerDlg);
            this.Controls.Add(this.m_btnSelectLbColor);
            this.Controls.Add(this.m_btnSelectTvColor);
            this.Controls.Add(this.m_btnColorDialog);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "FormMain";
            this.Text = "ColorEditor Test";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button m_btnColorDialog;
        private System.Windows.Forms.Button m_btnSelectTvColor;
        private System.Windows.Forms.Button m_btnSelectLbColor;
        private System.Windows.Forms.Button m_btnColorPickerDlg;
        private ChuckHill2.Utilities.ColorPickerPanel colorPickerPanel1;
    }
}

