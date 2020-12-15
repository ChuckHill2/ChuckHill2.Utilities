using System;
using System.Drawing;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Diagnostics;
using ChuckHill2.Utilities;

namespace ColorEditor
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            //ColorExtensions.DumpColors();
        }

        protected override void OnLoad(EventArgs e)
        {
            m_clbColorListBox.AddColor(Color.FromArgb(178, 0, 255)); //nearest color==Color.DarkViolet
            m_clbColorListBox.AddColor(Color.FromArgb(128, Color.Peru.R, Color.Peru.G, Color.Peru.B));
            m_clbColorListBox.AddColor(Color.CadetBlue); //Not added because it already exists
            m_clbColorListBox.AddColor(Color.Empty); //Not added because it is invalid.
            m_clbColorListBox.Selected = Color.CadetBlue;

            m_ctvColorTreeView.AddColor(Color.FromArgb(57, 198, 149)); //nearest color==Color.MediumSeaGreen
            m_ctvColorTreeView.AddColor(Color.FromArgb(128, Color.MediumSeaGreen.R, Color.MediumSeaGreen.G, Color.MediumSeaGreen.B));
            m_ctvColorTreeView.AddColor(Color.FromArgb(57, 198, 149)); //nearest color==Color.MediumSeaGreen Already added.
            m_ctvColorTreeView.AddColor(Color.FromArgb(218, 165, 32)); //==Color.Goldenrod. Not added. Equivalant to known color
            m_ctvColorTreeView.Selected = Color.FromArgb(128, Color.MediumSeaGreen.R, Color.MediumSeaGreen.G, Color.MediumSeaGreen.B);

            m_cbbColorComboBox.AddColor(Color.FromArgb(218, 255, 127)); //nearest color==Color.YellowGreen
            m_cbbColorComboBox.AddColor(Color.FromArgb(128, 204, 242, 140)); //A=128, nearest color==Color.Khaki
            m_cbbColorComboBox.Selected = Color.MediumSeaGreen;

            //base.OnLoad(e);
        }

        private void m_btnSystemColorDlg_Click(object sender, EventArgs e)
        {
            using (var dlg = new ColorDialog())
            {
                dlg.Color = SystemColors.ActiveCaption;
                DialogResult result = dlg.ShowDialog(this);
                Debug.WriteLine($"SystemColorDlg Result={dlg.Color}");
            }
        }

        private void m_btnSystemCustomColorDialog_Click(object sender, EventArgs e)
        {
            using (var dlg = new CustomColorDialog())
            {
                dlg.Color = SystemColors.ActiveCaption;
                DialogResult result = dlg.ShowDialog(this);
                Debug.WriteLine($"SystemCustomColorDlg Result={dlg.Color}");
            }
        }

        int clb_i = 0;
        private void m_btnColorListBox_Click(object sender, EventArgs e)
        {
            switch (clb_i % 5)
            {
                case 0: m_clbColorListBox.Selected = Color.Red; break;
                case 1: m_clbColorListBox.Selected = Color.Yellow; break;
                case 2: m_clbColorListBox.Selected = SystemColors.ControlText; break;
                case 3: m_clbColorListBox.Selected = Color.FromArgb(128, Color.Peru.R, Color.Peru.G, Color.Peru.B); break;
                case 4: m_clbColorListBox.Selected = Color.FromArgb(1, 2, 3); break; //not in list
                default: throw new Exception("Should not get here!");
            }
            clb_i++;
        }

        int ctv_i = 0;
        private void m_btnColorTreeView_Click(object sender, EventArgs e)
        {
            switch (ctv_i % 5)
            {
                case 0: m_ctvColorTreeView.Selected = Color.Red; break;
                case 1: m_ctvColorTreeView.Selected = Color.Yellow; break;
                case 2: m_ctvColorTreeView.Selected = SystemColors.ControlText; break;
                case 3: m_ctvColorTreeView.Selected = Color.FromArgb(128, Color.MediumSeaGreen.R, Color.MediumSeaGreen.G, Color.MediumSeaGreen.B); break;
                case 4: m_ctvColorTreeView.Selected = Color.FromArgb(1, 2, 3); break; //not in list
                default: throw new Exception("Should not get here!");
            }
            ctv_i++;
        }

        int cbb_i = 0;
        private void m_btnColorComboBox_Click(object sender, EventArgs e)
        {
            switch (cbb_i % 5)
            {
                case 0: m_cbbColorComboBox.Selected = Color.Red; break;
                case 1: m_cbbColorComboBox.Selected = Color.Yellow; break;
                case 2: m_cbbColorComboBox.Selected = SystemColors.ControlText; break;
                case 3: m_cbbColorComboBox.Selected = Color.FromArgb(128, 204, 242, 140); break;
                case 4: m_cbbColorComboBox.Selected = Color.FromArgb(1, 2, 3); break; //not in list
                default: throw new Exception("Should not get here!");
            }
            cbb_i++;
        }

        [Conditional("DEBUG")]
        private void ValidateImageAttributeClass()
        {
            Image a;

            var absolutePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), @"Resources\ImageAttributeTest1.bmp");
            a = new ImageAttribute(absolutePath).Image;
            Debug.Assert(a != null, "ImageAttribute (file): BMP Absolute File Path");

            a = new ImageAttribute(@"Resources\ImageAttributeTest2.tif").Image;
            Debug.Assert(a != null, "ImageAttribute (file): TIF Relative File Path: Copy Local");

            a = new ImageAttribute(@"Resources\ImageAttributeTest3.jpg").Image;
            Debug.Assert(a != null, "ImageAttribute (file): JPG Relative File Path: Copy Local");

            a = new ImageAttribute(@"Resources\ImageAttributeTest4.png").Image;
            Debug.Assert(a != null, "ImageAttribute (file): PNG Relative File Path: Copy Local");

            a = new ImageAttribute(@"Resources\ImageAttributeTest5.ico").Image;
            Debug.Assert(a != null, "ImageAttribute (file): ICO Relative File Path: Copy Local");

            a = new ImageAttribute(typeof(FormMain)).Image;
            Debug.Assert(a != null, "ImageAttribute (manifest): typeof(FormMain) => FormMain.png");

            //Manually adding anything to a Form resource will will be erased by the designer.... However we can access designer-generated resources...
            a = new ImageAttribute(typeof(FormMain), "$this.Icon").Image;
            Debug.Assert(a != null, "ImageAttribute (Form resource): typeof(FormMain) => $this.Icon");

            a = new ImageAttribute(typeof(Panel)).Image;
            Debug.Assert(a != null, "ImageAttribute (manifest): typeof(Panel) => Panel.bmp");

            a = new ImageAttribute(typeof(Panel), "CheckBox").Image;
            Debug.Assert(a != null, "ImageAttribute (manifest): typeof(Panel),CheckBox => CheckBox.bmp");

            a = new ImageAttribute(typeof(Panel), "CheckBox.jpg").Image;
            Debug.Assert(a != null, "ImageAttribute (manifest): typeof(Panel),CheckBox.jpg => CheckBox.bmp");

            a = new ImageAttribute(typeof(Panel), "checkbox").Image;
            Debug.Assert(a == null, "ImageAttribute (manifest): typeof(Panel),checkbox => not case-sensitive");

            a = new ImageAttribute(this.GetType(), "ImageAttributeTest5").Image;
            Debug.Assert(a != null, "ImageAttribute (manifest): typeof(this),ImageAttributeTest5 => ImageAttributeTest5.ico");
            Debug.Assert(a.Width == 32 && a.Height == 32, "ImageAttribute (manifest): typeof(this),ImageAttributeTest5 => ImageAttributeTest5.ico not 32x32");
        }
    }
}
