using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Utilities
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

        private Rectangle ImageBounds; // Create rect of color icon image rectangle
        private Point TextOffset;      // Create offset to the starting position to write the text.

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

            var m_tnCustomColors = new TreeNode("Custom Colors") { Name = "Custom" };
            var m_tnWebColors = new TreeNode("Web Colors", ColorEx.KnownColors.Where(c => c.IsKnownColor && !c.IsSystemColor).Select(c => new TreeNode(c.Name) { Name = c.Name, Tag = c }).ToArray()) { Name="Web" };
            var m_tnSystemColors = new TreeNode("System Colors", ColorEx.KnownColors.Where(c => c.IsSystemColor).Select(c => new TreeNode(c.Name) { Name = c.Name, Tag = c }).ToArray()) { Name = "System" };
            base.Nodes.AddRange(new TreeNode[] { m_tnCustomColors, m_tnWebColors, m_tnSystemColors });
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            ImageBounds = new Rectangle(2, 1, graphicWidth, base.ItemHeight - 1 - 2);
            TextOffset = new Point(2 + graphicWidth + 2, 0);
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (!(e.Node.Tag is Color) || e.Bounds.IsEmpty) //no custom drawing needed
            {
                e.DrawDefault = true;
                return;
            }

            var color = (Color)e.Node.Tag;
            var g = Graphics.FromHwnd(this.Handle);  //g.Graphics clip region is wrong!

            // Draw row background

            var selected = e.Node.IsSelected || (e.State & TreeNodeStates.Selected) == TreeNodeStates.Selected;
            var focused = e.Node.TreeView.Focused;

            var foreColor = selected ? (focused ? SystemColors.HighlightText : base.ForeColor) : base.ForeColor;
            var backColor = selected ? (focused ? SystemColors.Highlight : SystemColors.GradientInactiveCaption) : base.BackColor;
            if (base.HideSelection && selected && !focused) { backColor = base.BackColor; foreColor = base.ForeColor; }

            var bounds = base.FullRowSelect ?
                new Rectangle(0, e.Bounds.Y, base.ClientRectangle.Width, e.Bounds.Height) :
                new Rectangle(e.Bounds.X, e.Bounds.Y, base.ClientRectangle.Width - e.Bounds.X, e.Bounds.Height);

            using (var br = new SolidBrush(backColor)) 
                g.FillRectangle(br, bounds);

            // Draw color icon

            var imageBounds = ImageBounds;
            imageBounds.X += e.Bounds.X;
            imageBounds.Y += e.Bounds.Y;

            var textOffset = TextOffset;
            textOffset.X += e.Bounds.X;
            textOffset.Y += e.Bounds.Y;

            if (color.A < 255) //add  background trasparency  checkerboard
            {
                using (var br = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Gainsboro, Color.White))
                    g.FillRectangle(br, imageBounds);
            }

            using (var solidBrush = new SolidBrush(color))
                g.FillRectangle(solidBrush, imageBounds);

            g.DrawRectangle(SystemPens.WindowText, imageBounds.X, imageBounds.Y, imageBounds.Width - 1, imageBounds.Height - 1);

            //finally draw text.

            TextRenderer.DrawText(g, e.Node.Text, e.Node.NodeFont ?? e.Node.TreeView.Font, textOffset, foreColor, Color.Transparent);

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
    }
}
