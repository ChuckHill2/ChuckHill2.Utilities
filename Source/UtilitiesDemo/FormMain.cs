//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="FormMain.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/ChuckHill2.Utilities</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;
using ChuckHill2;
using ChuckHill2.Forms;
using ChuckHill2.Win32;

namespace UtilitiesDemo
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            InitMMTestList();
        }

        protected override void OnLoad(EventArgs e)
        {
            SystemMenu.Insert(this, -2); //insert divider 2 items from the bottom
            SystemMenu.Insert(this, -2, "SystemMenu Test...", 999); //Insert menu item 2 items from the bottom (after above divider)
            SystemMenu.SetHandler(this, id =>
            {
                if (id == 999)
                {
                    MessageBoxEx.Show(this, "This is a test of the SystemMenu API.", "SystemMenu Test", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return true; // This id is handled
                }
                return false; // Everything else is not handled by this api.
            });

            CustomStorageTests(); //Test control custom storage API.

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

        #region NamedColor Controls Tab

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

        private void m_clbColorListBox_SelectionChanged(object sender, NamedColorEventArgs e)
        {
            m_lblColorSelectStatus.Text = $"NamedColorListBox Selected Color is {e.Color.GetName()}.";
        }

        private void m_ctvColorTreeView_SelectionChanged(object sender, NamedColorEventArgs e)
        {
            m_lblColorSelectStatus.Text = $"NamedColorTreeView Selected Color is {e.Color.GetName()}.";
        }

        private void m_cbbColorComboBox_SelectionChanged(object sender, NamedColorEventArgs e)
        {
            m_lblColorSelectStatus.Text = $"NamedColorComboBox Selected Color is {e.Color.GetName()}.";
        }

        #endregion //NamedColor Controls Tab Click Events

        #region Popup Tab Click Events

        private void m_btnToolTipManager_Click(object sender, EventArgs e)
        {
            using (var dlg = new ToolTipManagerTestForm())
            {
                dlg.ShowDialog(this);
            }
        }

        private void m_btnToolTipEx_Click(object sender, EventArgs e)
        {
            using (var dlg = new ToolTipExTestForm())
            {
                dlg.ShowDialog(this);
            }
        }

        private Color LastColor = Color.Transparent;
        private void m_btnSysColorDialog_Click(object sender, EventArgs e)
        {
            using (var dlg = new ColorDialog())
            {
                dlg.Color = LastColor;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LastColor = dlg.Color;
                    m_lblPopupStatus.Text = "Color = " + LastColor.GetName();
                }
            }
        }

        private void m_btnSysColorPickerDialog_Click(object sender, EventArgs e)
        {
            using (var dlg = new SysColorPickerDialog())
            {
                dlg.Color = LastColor;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LastColor = dlg.Color;
                    m_lblPopupStatus.Text = "Color = " + LastColor.GetName();
                }
            }
        }

        private void m_btnColorDialogAdv_Click(object sender, EventArgs e)
        {
            using (var dlg = new Cyotek.Windows.Forms.ColorPickerDialog())
            {
                dlg.Color = LastColor;
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    LastColor = dlg.Color;
                    m_lblPopupStatus.Text = "Color = " + LastColor.GetName();
                }
            }
        }

        private string LastFolder = null;
        private void m_btnFolderSelecterEx_Click(object sender, EventArgs e)
        {
            var newFolder = FolderSelectDialog.Show(this, null, LastFolder);
            if (newFolder != null)
            {
                LastFolder = newFolder;
                m_lblPopupStatus.Text = "Folder = " + LastFolder;
            }
        }

        private void m_btnMessageBoxEx_Click(object sender, EventArgs e)
        {
            var result = MessageBoxEx.Show(this, "This a test message.", "Test Title", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
            m_lblPopupStatus.Text = "MessageBoxEx Result = " + result;
        }

        #region MiniMessageBox Test all permutations (also see Program.Main())
        private class LayoutTest
        {
            public bool IsModal;
            public IWin32Window Owner;
            public string Caption;
            public string Message;
            public MiniMessageBox.Symbol Icon;
            public MiniMessageBox.Buttons Buttons;
            public MiniMessageBox.MsgBoxColors MmColors;
            public LayoutTest(bool ism, IWin32Window f, string c, string m,
                MiniMessageBox.Symbol i, MiniMessageBox.Buttons b, MiniMessageBox.MsgBoxColors mmclrs = null)
            { IsModal = ism; Owner = f; Caption = c; Message = m; Icon = i; Buttons = b; MmColors = mmclrs; }
        }

        private LayoutTest[] _tests;
        private void InitMMTestList()
        {
            _tests = new[]
            {
                new LayoutTest(true, this, "This is a caption longer then the message body.","(message)", MiniMessageBox.Symbol.Error, MiniMessageBox.Buttons.OK),
                new LayoutTest(true, null, "No Owner Test","No owner form has been passed to this messagebox. It will attempt to find the owning form to stay in front of it. If it can't, it will be owned by the desktop.", MiniMessageBox.Symbol.Warning, MiniMessageBox.Buttons.OKCancel),
                new LayoutTest(true, m_btnSysColorPickerDialog, "This is the caption","This is the message body with a warning status icon and positioned over SysColorPicker button.", MiniMessageBox.Symbol.Warning, MiniMessageBox.Buttons.RetryCancel),
                new LayoutTest(true, this, "This is the caption","This is the message body with a question status icon.", MiniMessageBox.Symbol.Question, MiniMessageBox.Buttons.YesNo),
                new LayoutTest(true, this, "This is the caption","This is the message body with a information status icon.", MiniMessageBox.Symbol.Information, MiniMessageBox.Buttons.YesNoCancel),
                new LayoutTest(true, this, "This is the caption","This is the message body with a wait status icon.", MiniMessageBox.Symbol.Wait, MiniMessageBox.Buttons.Abort),
                new LayoutTest(true, this, "This is the caption","This is the message body with no status icon.", MiniMessageBox.Symbol.None, MiniMessageBox.Buttons.AbortRetryIgnore),
                new LayoutTest(false, this, "Modalless Messagebox","The user is not allowed to close this (Waiting for some action to be completed and abort not allowed?). The application must close this messagebox. However for demo purposes, you may close this by clicking the test button again.", MiniMessageBox.Symbol.None, MiniMessageBox.Buttons.None),
                new LayoutTest(false, this, null,null, 0, 0),
                new LayoutTest(true, this, null,"This is just the message body and no caption.", MiniMessageBox.Symbol.Error, MiniMessageBox.Buttons.OK),
                new LayoutTest(true, this, "This is the caption without a message with a status icon.",null, MiniMessageBox.Symbol.Error, MiniMessageBox.Buttons.OK),
                new LayoutTest(true, this, "This is the caption without a message and no status icon.",null, MiniMessageBox.Symbol.None, MiniMessageBox.Buttons.OK),
                new LayoutTest(true, this, null ,null, MiniMessageBox.Symbol.None, MiniMessageBox.Buttons.OK),
                new LayoutTest(true, this, "This is the caption","This shows the properties that may be changed!",MiniMessageBox.Symbol.Information, MiniMessageBox.Buttons.YesNoCancel, mmColors),
            };
        }

        private static MiniMessageBox.MsgBoxColors mmColors = new MiniMessageBox.MsgBoxColors()
        {
            CaptionGradientLeft = Color.FromArgb(0, 183, 195),
            CaptionGradientRight = Color.FromArgb(0, 194, 204),
            CaptionText = Color.Black,
            InactiveCaptionGradientLeft = Color.Silver,
            InactiveCaptionGradientRight = Color.Gainsboro,
            InactiveCaptionText = Color.Gray,
            MessageText = Color.Black,
            Background = Color.Cornsilk,
            CaptionFont = new Font("Lucida Calligraphy", 11),
            MessageFont = new Font("Ink Free", 11, FontStyle.Italic)
        };

        private int _index = 0;
        private void m_btnMiniMessageBox_Click(object sender, EventArgs e)
        {
            DialogResult result = DialogResult.None;
            var lt = _tests[(_index++) % _tests.Length];

            if (lt.IsModal == false && lt.Caption == null && lt.Message == null && lt.Icon == 0 && lt.Buttons == 0)
            {
                result = MiniMessageBox.Hide();
            }
            else if (lt.IsModal==false)
            {
                MiniMessageBox.Show(lt.Owner, lt.Message, lt.Caption, lt.Buttons, lt.Icon);
            }
            else
            {
                MiniMessageBox.MsgBoxColors myColors = null;
                if (lt.MmColors != null)
                {
                    myColors = MiniMessageBox.Colors.Backup();
                    MiniMessageBox.Colors.Restore(lt.MmColors);
                }
                result = MiniMessageBox.ShowDialog(lt.Owner, lt.Message, lt.Caption, lt.Buttons, lt.Icon);
                if (myColors != null)
                {
                    MiniMessageBox.Colors.Restore(myColors);
                }
            }

            m_lblPopupStatus.Text = "MiniMessageBox Result = " + result;
        }
        #endregion

        #endregion //Popup Tab Click Events

        [Conditional("DEBUG")]
        private void CustomStorageTests()
        {
            NativeMethods.SetWindowProperty(this, "TestInt", 42);
            NativeMethods.SetWindowProperty(this, "TestLong", 123456789L);
            NativeMethods.SetWindowProperty(this, "TestString", "Hello World");
            NativeMethods.SetWindowProperty(this, "TestSize", new Size(1, 2));
            NativeMethods.SetWindowProperty(this, "TestPoint", new Point(12, 34));
            NativeMethods.SetWindowProperty(this, "TestRect", new Rectangle(11, 22, 33, 44));
            NativeMethods.SetWindowProperty(this, "TestColor", Color.Cyan);

            Debug.Assert(NativeMethods.GetWindowProperty(this, "TestInt", out int r1) && r1 == 42, "NativeMethods.GetWindowProperty[int]");
            Debug.Assert(NativeMethods.GetWindowProperty(this, "TestLong", out long r2) && r2 == 123456789, "NativeMethods.GetWindowProperty[long]");
            Debug.Assert(NativeMethods.GetWindowProperty(this, "TestString", out string r3) && r3 == "Hello World", "NativeMethods.GetWindowProperty[string]");
            Debug.Assert(NativeMethods.GetWindowProperty(this, "TestSize", out Size r4) && r4 == new Size(1, 2), "NativeMethods.GetWindowProperty[Size]");
            Debug.Assert(NativeMethods.GetWindowProperty(this, "TestPoint", out Point r5) && r5 == new Point(12, 34), "NativeMethods.GetWindowProperty[Point]");
            Debug.Assert(NativeMethods.GetWindowProperty(this, "TestRect", out Rectangle r6) && r6 == new Rectangle(11, 22, 33, 44), "NativeMethods.GetWindowProperty[Rectangle]");
            Debug.Assert(NativeMethods.GetWindowProperty(this, "TestColor", out Color r7) && r7 == Color.Cyan, "NativeMethods.GetWindowProperty[struct]");

            NativeMethods.DisposeAllProperties(this);
            Debug.Assert(!NativeMethods.GetWindowProperty(this, "TestRect", out Rectangle r8));

            int key1 = PropertyStore.CreateKey();
            int key2 = PropertyStore.CreateKey();
            int key3 = PropertyStore.CreateKey();

            PropertyStore.SetInteger(this, key1, 42);
            PropertyStore.SetObject(this, key2, _tests[0]);
            PropertyStore.SetObject(this, key3, new KeyValuePair<string, int>("Hello", 42));

            Debug.Assert(PropertyStore.GetInteger(this, key1) == 42, "PropertyStore.GetInteger[int]");
            Debug.Assert(PropertyStore.GetObject(this, key2).Equals(_tests[0]), "PropertyStore.GetInteger[class]");
            Debug.Assert(PropertyStore.GetObject(this, key3).Equals(new KeyValuePair<string, int>("Hello", 42)), "PropertyStore.GetInteger[struct]");
        }
    }

    /// <summary>
    /// Example of all the Designer Type Editors.
    /// </summary>
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
