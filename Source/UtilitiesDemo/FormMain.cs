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
        private static Form ThisForm; //for MiniMessageBox Test, below.

        public FormMain()
        {
            InitializeComponent();
            ThisForm = this;
        }

        protected override void OnLoad(EventArgs e)
        {
            SystemMenu.Insert(this, -2); //insert divider 2 items from the bottom
            SystemMenu.Insert(this, -2, "SystemMenu Test...", 999); //Insert menu item 2 items from the bottom (after above divider)
            SystemMenu.SetHandler(this, id =>
            {
                if (id == 999)
                {
                    MessageBoxEx.Show(this,"This is a test of the SystemMenu API.","SystemMenu Test",MessageBoxButtons.OK,MessageBoxIcon.Information);
                    return true; // This id is handled
                }
                return false; // Everything else is not handled by this api.
            });

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
            using(var dlg = new ToolTipManagerTestForm())
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
                if (dlg.ShowDialog(this)==DialogResult.OK)
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
            if (newFolder!=null)
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
            public Form Owner;
            public string Caption;
            public string Message;
            public MessageBoxIcon Icon;
            public MessageBoxButtons Buttons;
            public LayoutTest(bool ism, Form f, string c, string m, MessageBoxIcon i, MessageBoxButtons b) { IsModal = ism; Owner = f;  Caption = c; Message = m; Icon = i; Buttons = b; }
        }
        
        private LayoutTest[] _tests = new[]
        {
            new LayoutTest(true, ThisForm, "This is a caption longer then the message body.","(message)", MessageBoxIcon.Error, MessageBoxButtons.OK),
            new LayoutTest(true, null, "No Owner Test","No owner form has been passed to this messagebox. It will attempt to find the owning form to stay in front of it. If it can't, it will be owned by the desktop.", MessageBoxIcon.Warning, MessageBoxButtons.OKCancel),
            new LayoutTest(true, ThisForm, "This is the caption","This is the message body with a warning status icon.", MessageBoxIcon.Warning, MessageBoxButtons.RetryCancel),
            new LayoutTest(true, ThisForm, "This is the caption","This is the message body with a question status icon.", MessageBoxIcon.Question, MessageBoxButtons.YesNo),
            new LayoutTest(true, ThisForm, "This is the caption","This is the message body with a information status icon.", MessageBoxIcon.Information, MessageBoxButtons.YesNoCancel),
            new LayoutTest(true, ThisForm, "This is the caption","This is the message body with no status icon.", MessageBoxIcon.None, MessageBoxButtons.AbortRetryIgnore),
            new LayoutTest(false, ThisForm, "Modalless Messagebox","The user is not allowed to close this (Waiting for some action to be completed and abort not allowed?). The application must close this messagebox. However for demo purposes, you may close this by clicking the test button again.", MessageBoxIcon.None, (MessageBoxButtons)(-1)),
            new LayoutTest(false, ThisForm, null,null, 0, 0),
            new LayoutTest(true, ThisForm, null,"This is just the message body and no caption.", MessageBoxIcon.Error, MessageBoxButtons.OK),
            new LayoutTest(true, ThisForm, "This is the caption without a message with a status icon.",null, MessageBoxIcon.Error, MessageBoxButtons.OK),
            new LayoutTest(true, ThisForm, "This is the caption without a message and no status icon.",null, MessageBoxIcon.None, MessageBoxButtons.OK),
            new LayoutTest(true, ThisForm, null ,null, MessageBoxIcon.None, MessageBoxButtons.OK),
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
                result = MiniMessageBox.ShowDialog(lt.Owner, lt.Message, lt.Caption, lt.Buttons, lt.Icon);
            }

            m_lblPopupStatus.Text = "MiniMessageBox Result = " + result;
        }
        #endregion

        #endregion //Popup Tab Click Events
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
