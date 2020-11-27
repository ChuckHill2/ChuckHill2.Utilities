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

namespace GradientTest
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
            propertyGrid1.SelectedObject = new TestGradientControlProperty(this);
            propertyGrid2.SelectedObject = new EnumTestObject();
            base.OnLoad(e);
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
            Debug.Assert(a.Width==32 && a.Height==32, "ImageAttribute (manifest): typeof(this),ImageAttributeTest5 => ImageAttributeTest5.ico not 32x32");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var dlg = new CustomColorDialog();
            //dlg.AllowFullOpen = true;
            dlg.ShowDialog(this);
            dlg.Dispose();
        }
    }

    public class CustomColorDialog : ColorDialog
    {
        private const int COLOR_HUE = 703;
        private const int COLOR_SAT = 704;
        private const int COLOR_LUM = 705;
        private const int COLOR_RED = 706;
        private const int COLOR_GREEN = 707;
        private const int COLOR_BLUE = 708;
        private const int COLOR_ADD = 712;
        private const int COLOR_MIX = 719;
        private IntPtr hInstance;

        public CustomColorDialog()
        {
            Stream manifestResourceStream = typeof(ColorEditor).Module.Assembly.GetManifestResourceStream(typeof(ColorEditor), "colordlg.data");
            int length = (int)(manifestResourceStream.Length - manifestResourceStream.Position);
            byte[] numArray = new byte[length];
            manifestResourceStream.Read(numArray, 0, length);
            this.hInstance = Marshal.AllocHGlobal(length);
            Marshal.Copy(numArray, 0, this.hInstance, length);
        }

        protected override IntPtr Instance => this.hInstance;

        protected override int Options => 66;

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (!(this.hInstance != IntPtr.Zero))
                    return;
                Marshal.FreeHGlobal(this.hInstance);
                this.hInstance = IntPtr.Zero;
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        //protected override IntPtr HookProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        //{
        //    switch (msg)
        //    {
        //        case 272:
        //            NativeMethods.SendDlgItemMessage(hwnd, 703, 211, (IntPtr)3, IntPtr.Zero);
        //            NativeMethods.SendDlgItemMessage(hwnd, 704, 211, (IntPtr)3, IntPtr.Zero);
        //            NativeMethods.SendDlgItemMessage(hwnd, 705, 211, (IntPtr)3, IntPtr.Zero);
        //            NativeMethods.SendDlgItemMessage(hwnd, 706, 211, (IntPtr)3, IntPtr.Zero);
        //            NativeMethods.SendDlgItemMessage(hwnd, 707, 211, (IntPtr)3, IntPtr.Zero);
        //            NativeMethods.SendDlgItemMessage(hwnd, 708, 211, (IntPtr)3, IntPtr.Zero);
        //            IntPtr dlgItem1 = NativeMethods.GetDlgItem(hwnd, 719);
        //            NativeMethods.EnableWindow(dlgItem1, false);
        //            NativeMethods.SetWindowPos(dlgItem1, IntPtr.Zero, 0, 0, 0, 0, 128);
        //            IntPtr dlgItem2 = NativeMethods.GetDlgItem(hwnd, 1);
        //            NativeMethods.EnableWindow(dlgItem2, false);
        //            NativeMethods.SetWindowPos(dlgItem2, IntPtr.Zero, 0, 0, 0, 0, 128);
        //            this.Color = Color.Empty;
        //            break;
        //        case 273:
        //            if (NativeMethods.Util.LOWORD((int)(long)wParam) == 712)
        //            {
        //                bool[] err = new bool[1];
        //                this.Color = Color.FromArgb((int)(byte)NativeMethods.GetDlgItemInt(hwnd, 706, err, false), (int)(byte)NativeMethods.GetDlgItemInt(hwnd, 707, err, false), (int)(byte)NativeMethods.GetDlgItemInt(hwnd, 708, err, false));
        //                NativeMethods.PostMessage(hwnd, 273, (IntPtr)NativeMethods.Util.MAKELONG(1, 0), NativeMethods.GetDlgItem(hwnd, 1));
        //                break;
        //            }
        //            break;
        //    }
        //    return base.HookProc(hwnd, msg, wParam, lParam);
        //}
    }


    public class TestGradientControlProperty
    {
        private Control Host;
        public TestGradientControlProperty(Control host) => Host = host;

        private GradientBrush __backgroundGradient = null;
        [Category("Appearance"), Description("The gradient brush used to fill the background.")]
        public GradientBrush BackgroundGradient
        {
            get => __backgroundGradient == null ? new GradientBrush(Host) : __backgroundGradient;
            set { __backgroundGradient = value; }
        }
        private bool ShouldSerializeBackgroundGradient() => !BackgroundGradient.Equals(new GradientBrush(Host));
        private void ResetBackgroundGradient() => BackgroundGradient = null;
    }

    #region Enum UITypeEditor Test
    class EnumTestObject
    {
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

        [Description("Show the system default for enums. Simple multiple choice, bitwise flags not supported.")]
        public ArrowDirectionEx DefaultEnum { get; set; }

        [Description("Test flag/bitwise enums.")]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public FontStyle FontStyle { get; set; }

        [Description("Test flag/bitwise enums with one combo 'all' flag.")]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public SecurityPermissionFlag SecurityPermission { get; set; }

        [Description("Test flag/bitwise enums with tooltips and combo flags.")]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public AnchorStylesEx Anchor { get; set; }

        [Description("Test non-flag/mutually exclusive enums with tool tips and item icons.")]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public ArrowDirectionEx Direction { get; set; }
    }
    #endregion  UITypeEditor Test
}
