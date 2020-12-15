using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using ChuckHill2.Utilities;

namespace UtilitiesDemo
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            ValidateImageAttributeClass();
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            propertyGrid1.SelectedObject = new TestUITypeEditors(this);

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

            base.OnLoad(e);
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

    public class TestUITypeEditors
    {
        private Control Host;
        public TestUITypeEditors(Control host) => Host = host;

        private GradientBrush __backgroundGradient = null;
        [Category("GradientBrush Example"), Description("The gradient brush used to fill the background.")]
        public GradientBrush BackgroundGradient
        {
            get => __backgroundGradient == null ? new GradientBrush(Host) : __backgroundGradient;
            set { __backgroundGradient = value; }
        }
        private bool ShouldSerializeBackgroundGradient() => !BackgroundGradient.Equals(new GradientBrush(Host));
        private void ResetBackgroundGradient() => BackgroundGradient = null;

        #region Test Enums
        // Example cloned from System.Windows.Forms.ArrowDirection and tool tips added
        public enum ArrowDirectionEx
        {
            [Image(typeof(ArrowDirectionEx), "Left", 0)] //<--The icon on the item
            [Image(typeof(ArrowDirectionEx), "Left", 1)] //<--The 2nd icon is ignored.
            [Description("The direction is left.")]      //<--The tooltip
            Left = 0,

            [Image(typeof(ArrowDirectionEx), "Up")] //Icon image is sized to fit a square checkBox/radiobutton height -2 pixels. This depends on the font size.
            [Description("The direction is up.")]
            Up = 1,

            [Image(typeof(ArrowDirectionEx), "Right")]
            [Description("The direction is right.")]
            Right = 16,

            [Image(typeof(ArrowDirectionEx), "Down")]
            [Description("The direction is down.")]
            Down = 17
        }

        // Example cloned from System.Windows.Forms.AnchorStyles and extended
        //This editor is overridden by the one on the property.
        [Editor("System.Windows.Forms.Design.AnchorEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(System.Drawing.Design.UITypeEditor))]
        [Flags]
        public enum AnchorStylesEx
        {
            [Description("Anchor to float within parent.")] None = 0,
            [Description("Anchor to top side of parent.")] Top = 1,
            [Description("Anchor to bottom side of parent.")] Bottom = 2,
            [Description("Anchor to left side of parent.")] Left = 4,
            [Description("Anchor to right side of parent.")] Right = 8,
            [Description("Position fixed to top-left of parent.")] TopLeft = Top | Left,
            [Description("Position fixed to bottom-right of parent.")] BottomRight = Bottom | Right,
            [Description("Resize with parent.")] All = Left | Right | Bottom | Top,
        }
        #endregion

        [Category("Enum Examples"), Description("Show the system default for enums. Simple multiple choice, bitwise flags not supported.")]
        public ArrowDirectionEx DefaultEnum { get; set; }

        [Category("Enum Examples"), Description("Test flag/bitwise enums.")]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public FontStyle FontStyle { get; set; }

        [Category("Enum Examples"), Description("Test flag/bitwise enums with one combo 'all' flag.")]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public SecurityPermissionFlag SecurityPermission { get; set; }

        [Category("Enum Examples"), Description("Test flag/bitwise enums with tooltips and combo flags.")]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public AnchorStylesEx Anchor { get; set; }

        [Category("Enum Examples"), Description("Test non-flag/mutually exclusive enums with tool tips and item icons.")]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public ArrowDirectionEx Direction { get; set; }
    }
}
