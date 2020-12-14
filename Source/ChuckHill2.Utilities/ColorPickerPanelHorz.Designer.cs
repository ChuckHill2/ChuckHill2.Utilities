using System;
using Cyotek.Windows.Forms;

namespace ChuckHill2.Utilities
{
    partial class ColorPickerPanelHorz
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip = new System.Windows.Forms.ToolTip(this.components);
            this.colorEditor = new Cyotek.Windows.Forms.ColorEditor();
            this.previewPanel = new ChuckHill2.Utilities.SplitColorPanel();
            this.screenColorPicker = new Cyotek.Windows.Forms.ScreenColorPicker();
            this.colorWheel = new Cyotek.Windows.Forms.ColorWheel();
            this.colorGrid = new Cyotek.Windows.Forms.ColorGrid();
            this.colorEditorManager = new Cyotek.Windows.Forms.ColorEditorManager();
            this.SuspendLayout();
            // 
            // colorEditor
            // 
            this.colorEditor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.colorEditor.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.colorEditor.Location = new System.Drawing.Point(201, 47);
            this.colorEditor.Margin = new System.Windows.Forms.Padding(0);
            this.colorEditor.Name = "colorEditor";
            this.colorEditor.ShowColorSpaceLabels = false;
            this.colorEditor.Size = new System.Drawing.Size(231, 182);
            this.colorEditor.TabIndex = 0;
            // 
            // previewPanel
            // 
            this.previewPanel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.previewPanel.Color = System.Drawing.Color.Empty;
            this.previewPanel.Location = new System.Drawing.Point(444, 152);
            this.previewPanel.Margin = new System.Windows.Forms.Padding(0);
            this.previewPanel.Name = "previewPanel";
            this.previewPanel.Size = new System.Drawing.Size(75, 75);
            this.previewPanel.TabIndex = 3;
            this.previewPanel.Click += new System.EventHandler(this.previewPanel_Click);
            // 
            // screenColorPicker
            // 
            this.screenColorPicker.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.screenColorPicker.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.screenColorPicker.Location = new System.Drawing.Point(444, 48);
            this.screenColorPicker.Margin = new System.Windows.Forms.Padding(0);
            this.screenColorPicker.Name = "screenColorPicker";
            this.screenColorPicker.Size = new System.Drawing.Size(75, 75);
            this.toolTip.SetToolTip(this.screenColorPicker, "Click and drag to select screen color");
            this.screenColorPicker.Zoom = 6;
            // 
            // colorWheel
            // 
            this.colorWheel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.colorWheel.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.colorWheel.Location = new System.Drawing.Point(0, -2);
            this.colorWheel.Margin = new System.Windows.Forms.Padding(0);
            this.colorWheel.Name = "colorWheel";
            this.colorWheel.Size = new System.Drawing.Size(201, 201);
            this.colorWheel.TabIndex = 4;
            // 
            // colorGrid
            // 
            this.colorGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.colorGrid.AutoAddColors = false;
            this.colorGrid.CellBorderStyle = Cyotek.Windows.Forms.ColorCellBorderStyle.None;
            this.colorGrid.Color = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.colorGrid.EditMode = Cyotek.Windows.Forms.ColorEditingMode.Both;
            this.colorGrid.Location = new System.Drawing.Point(0, 199);
            this.colorGrid.Margin = new System.Windows.Forms.Padding(0);
            this.colorGrid.Name = "colorGrid";
            this.colorGrid.Padding = new System.Windows.Forms.Padding(0);
            this.colorGrid.Palette = Cyotek.Windows.Forms.ColorPalette.Paint;
            this.colorGrid.SelectedCellStyle = Cyotek.Windows.Forms.ColorGridSelectedCellStyle.Standard;
            this.colorGrid.ShowCustomColors = false;
            this.colorGrid.Size = new System.Drawing.Size(192, 72);
            this.colorGrid.Spacing = new System.Drawing.Size(0, 0);
            this.colorGrid.TabIndex = 7;
            this.colorGrid.EditingColor += new System.EventHandler<Cyotek.Windows.Forms.EditColorCancelEventArgs>(this.colorGrid_EditingColor);
            // 
            // colorEditorManager
            // 
            this.colorEditorManager.ColorEditor = this.colorEditor;
            this.colorEditorManager.ColorGrid = this.colorGrid;
            this.colorEditorManager.ColorWheel = this.colorWheel;
            this.colorEditorManager.ScreenColorPicker = this.screenColorPicker;
            this.colorEditorManager.ColorChanged += new System.EventHandler(this.colorEditorManager_ColorChanged);
            // 
            // ColorPickerPanelHorz
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.colorEditor);
            this.Controls.Add(this.previewPanel);
            this.Controls.Add(this.screenColorPicker);
            this.Controls.Add(this.colorWheel);
            this.Controls.Add(this.colorGrid);
            this.Margin = new System.Windows.Forms.Padding(0);
            this.MaximumSize = new System.Drawing.Size(1000, 999);
            this.MinimumSize = new System.Drawing.Size(376, 182);
            this.Name = "ColorPickerPanelHorz";
            this.Size = new System.Drawing.Size(519, 277);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Cyotek.Windows.Forms.ColorGrid colorGrid;
        private Cyotek.Windows.Forms.ColorEditor colorEditor;
        private Cyotek.Windows.Forms.ColorWheel colorWheel;
        private Cyotek.Windows.Forms.ColorEditorManager colorEditorManager;
        private Cyotek.Windows.Forms.ScreenColorPicker screenColorPicker;
        private ChuckHill2.Utilities.SplitColorPanel previewPanel;
        private System.Windows.Forms.ToolTip toolTip;
    }
}
