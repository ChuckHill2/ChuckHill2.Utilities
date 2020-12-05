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
using Cyotek.Windows.Forms;

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
        }

        private void m_btnColorDialog_Click(object sender, EventArgs e)
        {
           // var dlg = new ColorDialog();
            var dlg = new CustomColorDialog();
            DialogResult result = dlg.ShowDialog(this);
        }

        private static readonly ConstructorInfo ciColor = typeof(Color).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(long), typeof(short), typeof(string), typeof(KnownColor) }, null);
        private static long MakeArgb(byte alpha, byte red, byte green, byte blue) => (long)(uint)((int)red << 16 | (int)green << 8 | (int)blue | (int)alpha << 24) & (long)uint.MaxValue;
        /// <summary>
        /// Created a 'Named' color with an alpha value something other than 255 appended to the name. (e.g. 'Red64').
        /// Where:
        ///    color.IsKnownColor == false
        ///    color.IsSystemColor == false
        ///    color.IsNamedColor == true
        /// If the provided color is not a named color, the nearest named color is returned including the optional alpha transparency.
        /// If alpha==255 (the default), a new named color is not created and is returned as-is.
        /// At a minimum, it is a good way to convert a random RGB color to it's nearest named equivalant.
        /// </summary>
        /// <param name="c">Color to convert</param>
        /// <param name="alpha">The transparency value to give the new color.</param>
        /// <returns></returns>
        public static Color MakeNamedColor(Color c, byte alpha = 255)
        {
            //const short StateKnownColorValid = 1;
            const short StateARGBValueValid = 2;
            const short StateNameValid = 8;

            if (!c.IsNamedColor)
            {
                c = ((HSLColor)c).NearestKnownColor();
            }

            if (alpha == 255) return c;

            return (Color)ciColor.Invoke(new object[] { MakeArgb(alpha, c.R, c.G, c.B), (short)(StateARGBValueValid | StateNameValid), c.Name + alpha.ToString(), 0 });
        }
        public static Color MakeUnnamedColor(Color c) => Color.FromArgb(c.A, c.R, c.G, c.B);

        int i = 0;
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

        int j = 0;
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
            using(var dlg = new ColorPickerDialog())
            {
                dlg.Color = SystemColors.ActiveCaption;
                DialogResult result = dlg.ShowDialog(this);
                Debug.WriteLine($"ColorPickerDialog Result={dlg.Color}");
            }
        }
    }
}
