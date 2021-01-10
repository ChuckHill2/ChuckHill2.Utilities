using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    ///  @image html NamedColorTreeView.png
    /// <summary>
    /// Color selector treeview control with three root nodes 'Custom', 'Known', and 'System' colors.
    /// Only custom colors can be added or removed.
    /// </summary>
    [ToolboxBitmap(typeof(TreeView))]
    [DefaultEvent("SelectionChanged")]
    [Description("Select from a hierarchical collection of known colors.")]
    public class NamedColorTreeView : TreeView
    {
        private int graphicWidth = 22;  // width of color icon image at 96dpi. Hight is always height of row -1px on the top and bottom.

        private Brush _transparentIconBrush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Gainsboro, Color.White);
        private Brush _disabledTranslucentBackground = new SolidBrush(Color.FromArgb(32, SystemColors.InactiveCaption));
        private Rectangle ImageBounds; // Create rect of color icon image rectangle
        private Point TextOffset;      // Create offset to the starting position to write the text.

        /// <summary>
        ///  Gets or sets the font of the text displayed by the control.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [Category("Appearance"), Description("The font used to display text in the control.")]
        public override Font Font
        {
            get => base.Font;
            set
            {
                base.Font = value;
                UpdateDrawingBounds();
            }
        }

        /// <summary>
        /// The height, in pixels, of items in the tree view. Must be an even number.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [Category("Behavior"), Description("The height, in pixels, of items in the tree view. Must be an even number.")]
        public int ItemsHeight
        {
            //We have to create our own uniquely named ItemsHeight field because the original [DefaultValueAttribute(16)] ItemHeight
            //takes precedence over ShouldSerializeItemHeight()/ResetItemHeight()
            get
            {
                if (__itemsHeight == -1) return DefaultItemsHeight;
                return __itemsHeight;
            }
            set
            {
                if (value < 2) value = 2;
                if (value >= short.MaxValue-2) value = short.MaxValue - 2;
                if (value == __itemsHeight) return;
                __itemsHeight = value + value % 2;
                base.ItemHeight = __itemsHeight;
                UpdateDrawingBounds();
            }
        }
        private int __itemsHeight = -1;
        private bool ShouldSerializeItemsHeight() => ItemsHeight != DefaultItemsHeight; //In lieu of using [DefaultValue(someConst)]
        private void ResetItemsHeight() => ItemsHeight = DefaultItemsHeight;
        private int DefaultItemsHeight
        {
            get
            {
                var x = new ImageFontMetrics(this.Font).EmHeightPixels + 2;
                return x + x % 2; //TreeView expects itemHeight to be an even number.
            }
        }

        private void UpdateDrawingBounds()
        {
            ImageBounds = new Rectangle(2, 1, graphicWidth, this.ItemsHeight - 2);

            // Need to properly center the *visible* text within the ItemHeight.
            var fi = new ImageFontMetrics(this.Font);
            var yOffset = (this.ItemsHeight - fi.EmHeightPixels - fi.InternalLeadingPixels) / 2.0f;

            TextOffset = new Point(2 + graphicWidth + 2, (int)Math.Floor(yOffset));
        }

        #region Hidden/Disabled Properties
        private const string NOTUSED = "Not used in " + nameof(NamedColorTreeView) + ".";
        //! @cond DOXYGENHIDE
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool CheckBoxes { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeViewDrawMode DrawMode { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool LabelEdit { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeNodeCollection Nodes { get; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ImageKey { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool HotTracking { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string PathSeparator { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Scrollable { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int SelectedImageIndex { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string SelectedImageKey { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeNode SelectedNode { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool ShowNodeToolTips { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageList StateImageList { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get; set; }
        [Obsolete(NOTUSED + "Use property ItemsHeight", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int ItemHeight { get; set; }

        #pragma warning disable CS0067 //The event is never used
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler RightToLeftLayoutChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackgroundImageChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackgroundImageLayoutChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler PaddingChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler TextChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event NodeLabelEditEventHandler BeforeLabelEdit;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event NodeLabelEditEventHandler AfterLabelEdit;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeViewCancelEventHandler BeforeCheck;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeViewEventHandler AfterCheck;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeViewCancelEventHandler BeforeCollapse;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeViewEventHandler AfterCollapse;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeViewCancelEventHandler BeforeExpand;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeViewEventHandler AfterExpand;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event DrawTreeNodeEventHandler DrawNode;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event ItemDragEventHandler ItemDrag;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeNodeMouseHoverEventHandler NodeMouseHover;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeViewCancelEventHandler BeforeSelect;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeViewEventHandler AfterSelect;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event PaintEventHandler Paint;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeNodeMouseClickEventHandler NodeMouseClick;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event TreeNodeMouseClickEventHandler NodeMouseDoubleClick;
        #pragma warning restore CS0067 //The event is never used

        //! @endcond
        #endregion

        /// <summary>
        /// Initializes a new instance of the NamedColorTreeView class.
        /// </summary>
        public NamedColorTreeView() : base()
        {
            base.Name = "NamedColorTreeView";
            base.DrawMode = TreeViewDrawMode.OwnerDrawText;
            //base.ShowLines = true; //must be false if FullRowSelect is true
            base.HideSelection = false;
            this.SetStyle(ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer, true);

            var tnCustomColors = new TreeNode("Custom Colors") { Name = "Custom" };
            var tnWebColors = new TreeNode("Web Colors", ColorEx.KnownColors.Where(c => c.IsKnownColor && !c.IsSystemColor).Select(c => new TreeNode(c.Name) { Name = c.Name, Tag = c }).ToArray()) { Name = "Web" };
            var tnSystemColors = new TreeNode("System Colors", ColorEx.KnownColors.Where(c => c.IsSystemColor).Select(c => new TreeNode(c.Name) { Name = c.Name, Tag = c }).ToArray()) { Name = "System" };
            base.Nodes.AddRange(new TreeNode[] { tnCustomColors, tnWebColors, tnSystemColors });
            base.ItemHeight = this.ItemsHeight;
            UpdateDrawingBounds();
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            //https://docs.microsoft.com/en-us/windows/win32/controls/tvm-setitemheight
            //Event Sequence: Control.HandleCreated. Control.BindingContextChanged. Form.Load. Control.VisibleChanged. Form.Activated. Form.Shown
            base.OnHandleCreated(e);
            base.ItemHeight = this.ItemsHeight;
            UpdateDrawingBounds();
        }

        protected override void Dispose(bool disposing)
        {
            if (_transparentIconBrush != null)
            {
                _transparentIconBrush.Dispose();
                _transparentIconBrush = null;
                _disabledTranslucentBackground.Dispose();
                _disabledTranslucentBackground = null;
            }
            base.Dispose(disposing);
        }

        // In the space between OnBeforeExpand and OnAfterExpand, OnDrawNode is called for all the nodes ON THE SAME Y-OFFSET!
        private bool PauseDrawNode = false;
        protected override void OnBeforeExpand(TreeViewCancelEventArgs e)
        {
            PauseDrawNode = true;
            //Debug.WriteLine("OnBeforeExpand");
            base.OnBeforeExpand(e);
        }
        protected override void OnAfterExpand(TreeViewEventArgs e)
        {
            PauseDrawNode = false;
            base.OnAfterExpand(e);
            //Debug.WriteLine("OnAfterExpand");
        }

        protected override void OnPaintBackground(PaintEventArgs pevent)
        {
            //We paint the entire control. This causes wierd clipping artifacts.
            //base.OnPaintBackground(pevent);
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (PauseDrawNode) return;

            //Debug.WriteLine($"OnDrawNode: {e.Node.Text}");

            var selected = e.Node.IsSelected || (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            var focused = e.Node.TreeView.Focused;

            if (!(e.Node.Tag is Color) || e.Bounds.IsEmpty) //no custom drawing needed
            {
                using (var br = new SolidBrush(base.BackColor)) e.Graphics.FillRectangle(br, e.Bounds);
                TextRenderer.DrawText(e.Graphics,
                    e.Node.Text,
                    e.Node.NodeFont ?? e.Node.TreeView.Font, new Point(e.Bounds.X, TextOffset.Y + e.Bounds.Y),
                    base.Enabled ? base.ForeColor : SystemColors.GrayText,
                    Color.Transparent);
                return;
            }

            var color = (Color)e.Node.Tag;
            var g = Graphics.FromHwnd(this.Handle);  //g.Graphics clip region is wrong!

            #region Draw Background
            var foreColor = base.Enabled ? (focused ? (selected ? SystemColors.HighlightText : base.ForeColor) : base.ForeColor) : SystemColors.GrayText;
            var backColor = selected ? (focused ? SystemColors.Highlight : SystemColors.GradientInactiveCaption) : base.BackColor;
            if (base.HideSelection && selected) { backColor = base.BackColor; foreColor = base.Enabled ? base.ForeColor : SystemColors.GrayText; }

            var bounds = base.FullRowSelect ?
                new Rectangle(0, e.Bounds.Y, base.ClientRectangle.Width, e.Bounds.Height) :
                new Rectangle(e.Bounds.X, e.Bounds.Y, base.ClientRectangle.Width - e.Bounds.X, e.Bounds.Height);

            using (var br = new SolidBrush(backColor))
                g.FillRectangle(br, bounds);
            #endregion

            #region Draw Icon
            var imageBounds = ImageBounds;
            imageBounds.X += e.Bounds.X;
            imageBounds.Y += e.Bounds.Y;

            //add  background trasparency  checkerboard
            if (color.A < 255) g.FillRectangle(_transparentIconBrush, imageBounds);

            using (var solidBrush = new SolidBrush(color))
                g.FillRectangle(solidBrush, imageBounds);

            g.DrawRectangle(SystemPens.WindowText, imageBounds.X, imageBounds.Y, imageBounds.Width - 1, imageBounds.Height - 1);
            #endregion

            #region Draw Text
            var textOffset = TextOffset;
            textOffset.X += e.Bounds.X;
            textOffset.Y += e.Bounds.Y;
            TextRenderer.DrawText(g, e.Node.Text, e.Node.NodeFont ?? e.Node.TreeView.Font, textOffset, foreColor, TextFormatFlags.NoClipping|TextFormatFlags.NoPadding);
            #endregion

            g.Dispose(); //cleanup our graphics object that we created.

            //Debug.WriteLine($"{e.Node.Text}: Fore={foreColor.GetName()}, Back={backColor.GetName()} selected={selected}, focused={focused}, HideSelection={HideSelection}");
        }

        /// <summary>
        /// Add custom color to list.
        /// Known colors will not be added as they already exist.
        /// </summary>
        /// <param name="c"></param>
        public void AddColor(Color c)
        {
            if (c.IsKnownColor || c.IsEmpty) return;
            if (base.Nodes.Cast<TreeNode>().SelectMany(tn => tn.Nodes.Cast<TreeNode>()).FirstOrDefault(tn => Equals(c, (Color)tn.Tag)) != null) return;
            var name = c.GetName();
            base.Nodes[0].Nodes.Insert(0, new TreeNode(name) { Name = name, Tag = c });
        }

        /// <summary>
        /// Remove custom color from list.
        /// Known colors will not be removed.
        /// </summary>
        /// <param name="c"></param>
        public void RemoveColor(Color c)
        {
            if (c.IsEmpty) return;
            var node = base.Nodes[0].Nodes.Cast<TreeNode>().FirstOrDefault(tn => Equals(c, (Color)tn.Tag));
            if (node == null) return;
            base.Nodes[0].Nodes.Remove(node);
        }

        /// <summary>
        /// Get or Set the selected color.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Selected
        {
            get => base.SelectedNode?.Tag is Color ? (Color)base.SelectedNode.Tag : Color.Empty;
            set
            {
                TreeNode node = base.Nodes.Cast<TreeNode>().SelectMany(tn => tn.Nodes.Cast<TreeNode>()).FirstOrDefault(tn => Equals(value, tn.Tag is Color ? (Color)tn.Tag : Color.Empty));
                base.CollapseAll();
                base.SelectedNode = node;
                node?.EnsureVisible();
                if (node!=null) base.Focus();
            }
        }

        /// <summary>
        /// Occurs when a color has been selected.
        /// </summary>
        [Category("Behavior"), Description("Occurs when a color has been selected.")]
        public event NamedColorEventHandler SelectionChanged;

        protected override void OnAfterSelect(TreeViewEventArgs e)
        {
            if (e.Node.Tag is Color && SelectionChanged != null)
            {
                SelectionChanged.Invoke(this, new NamedColorEventArgs((Color)e.Node.Tag));
            }

            base.OnAfterSelect(e);
        }

        private static bool Equals(Color c1, Color c2, bool ignoreAlpha = false)
        {
            if (c1.IsEmpty && !c2.IsEmpty) return false;
            if (!c1.IsEmpty && c2.IsEmpty) return false;
            if (c1.IsEmpty && c2.IsEmpty) return true;
            if (ignoreAlpha) return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
            return c1.A == c2.A && c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
        }

        //protected override void WndProc(ref Message m)
        //{
        //    var msg = (Win32.WM)m.Msg;
        //    if (msg != Win32.WM.WM_MOUSEMOVE
        //        && msg != Win32.WM.WM_TIMER
        //        && msg != Win32.WM.WM_SYSTIMER
        //        && msg != Win32.WM.WM_NCHITTEST
        //        && msg != Win32.WM.WM_SETCURSOR
        //        && msg != Win32.WM.WM_NCMOUSEMOVE
        //        && msg != Win32.WM.WM_MOUSEHOVER
        //        && msg != Win32.WM.TVM_HITTEST
        //        )
        //        Debug.WriteLine($"WndProc: {(m.HWnd == IntPtr.Zero ? "(null)" : Control.FromChildHandle(m.HWnd)?.Name ?? m.HWnd.ToString())} {Win32.TranslateWMMessage(m.HWnd, m.Msg)}");

        //    base.WndProc(ref m);
        //}
    }
}
