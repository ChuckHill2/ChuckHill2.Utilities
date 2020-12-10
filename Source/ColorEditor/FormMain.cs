using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChuckHill2;
using ChuckHill2.Utilities;

namespace ColorEditor
{
    public partial class FormMain : Form
    {
        //NamedColorListBox m_lbColors;
        //NamedColorTreeView m_tvColors;

        public FormMain()
        {
            InitializeComponent();

            //m_lbColors = new NamedColorListBox();
            //m_pnlNamedListBox.Controls.Add(m_lbColors);

            //m_lbColors.AddColor(Color.FromArgb(100, 1, 2, 3));
            //m_lbColors.AddColor(Color.FromArgb(128, Color.Peru.R, Color.Peru.G, Color.Peru.B));
            //m_lbColors.AddColor(Color.CadetBlue);
            //m_lbColors.AddColor(Color.Empty);

            //m_lbColors.RemoveColor(Color.FromArgb(100, 1, 2, 3));
            //m_lbColors.RemoveColor(Color.FromArgb(128, Color.Peru.R, Color.Peru.G, Color.Peru.B));
            //m_lbColors.RemoveColor(Color.CadetBlue);
            //m_lbColors.RemoveColor(Color.Empty);

            //m_tvColors = new NamedColorTreeView();
            //m_pnlNamedTreeView.Controls.Add(m_tvColors);

            //m_tvColors.AddColor(Color.FromArgb(100, 1, 2, 3));
            //m_tvColors.AddColor(Color.FromArgb(128, Color.Peru.R, Color.Peru.G, Color.Peru.B));
            //m_tvColors.AddColor(Color.CadetBlue);
            //m_tvColors.AddColor(Color.Empty);

            //m_tvColors.RemoveColor(Color.FromArgb(1, 2, 3));
            //m_tvColors.RemoveColor(Color.FromArgb(128, Color.Peru.R, Color.Peru.G, Color.Peru.B));
            //m_tvColors.RemoveColor(Color.CadetBlue);
            //m_tvColors.RemoveColor(Color.Empty);

            //ColorExtensions.DumpColors();
        }

        private void m_btnColorDialog_Click(object sender, EventArgs e)
        {
           // var dlg = new ColorDialog();
            var dlg = new CustomColorDialog();
            DialogResult result = dlg.ShowDialog(this);
        }

        //int i = 0;
        private void m_btnSelectColor_Click(object sender, EventArgs e)
        {
            //switch (i % 5)
            //{
            //    case 0: m_tvColors.Selected = Color.Red; break;
            //    case 1: m_tvColors.Selected = Color.Yellow; break;
            //    case 2: m_tvColors.Selected = SystemColors.ControlText; break;
            //    case 3: m_tvColors.Selected = Color.FromArgb(100, 1, 2, 3); break;
            //    case 4: m_tvColors.Selected = Color.FromArgb(1, 2, 3); break;
            //    default: throw new Exception("Should not get here!");
            //}
            //i++;
        }

        //int j = 0;
        private void m_btnSelectLbColor_Click(object sender, EventArgs e)
        {
            //switch (j % 5)
            //{
            //    case 0: m_lbColors.Selected = Color.Red; break;
            //    case 1: m_lbColors.Selected = Color.Yellow; break;
            //    case 2: m_lbColors.Selected = SystemColors.ControlText; break;
            //    case 3: m_lbColors.Selected = Color.FromArgb(100, 1, 2, 3); break;
            //    case 4: m_lbColors.Selected = Color.FromArgb(1, 2, 3); break;
            //    default: throw new Exception("Should not get here!");
            //}
            //j++;
        }

        private void m_btnColorPickerDlg_Click(object sender, EventArgs e)
        {
            //using(var dlg = new ColorPickerDialog())
            //{
            //    dlg.Color = SystemColors.ActiveCaption;
            //    DialogResult result = dlg.ShowDialog(this);
            //    Debug.WriteLine($"ColorPickerDialog Result={dlg.Color}");
            //}
        }
    }
}
