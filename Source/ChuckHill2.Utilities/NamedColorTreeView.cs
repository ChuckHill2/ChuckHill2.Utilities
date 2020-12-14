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
    public class NamedColorTreeView : TreeView
    {
        private int graphicWidth = 22;  //default pixel values at 96dpi

        private Rectangle ImageBounds;
        private Point TextOffset;

        #region Hidden/Disabled Properties
        //! @cond DOXYGENHIDE 
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool CheckBoxes { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeViewDrawMode DrawMode { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool ShowLines { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool FullRowSelect { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool LabelEdit { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new TreeNodeCollection Nodes { get; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ImageKey { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool HotTracking { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool HideSelection { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string PathSeparator { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Scrollable { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int SelectedImageIndex { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string SelectedImageKey { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool ShowNodeToolTips { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ImageList StateImageList { get; set; }
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get; set; }
        //! @endcond  
        #endregion

        /// <summary>
        /// Initializes a new instance of the NamedColorTreeView class.
        /// </summary>
        public NamedColorTreeView() : base()
        {
            base.Name = "NamedColorTreeView";
            base.DrawMode = TreeViewDrawMode.OwnerDrawText;
            base.ShowLines = false; //must be false if FullRowSelect is true
            base.FullRowSelect = true;

            var m_tnCustomColors = new TreeNode("Custom Colors") { Name = "Custom" };
            var m_tnWebColors = new TreeNode("Web Colors", ColorEx.KnownColors.Where(c => c.IsKnownColor && !c.IsSystemColor).Select(c => new TreeNode(c.Name) { Name = c.Name, Tag = c }).ToArray()) { Name="Web" };
            var m_tnSystemColors = new TreeNode("System Colors", ColorEx.KnownColors.Where(c => c.IsSystemColor).Select(c => new TreeNode(c.Name) { Name = c.Name, Tag = c }).ToArray()) { Name = "System" };
            base.Nodes.AddRange(new TreeNode[] { m_tnCustomColors, m_tnWebColors, m_tnSystemColors });
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            ImageBounds = new Rectangle(2, 1, graphicWidth, base.ItemHeight - 1 - 2);
            TextOffset = new Point(2 + graphicWidth + 2, -1); //-1 because we want to be vertically centered in in the blue selected rectangle
        }

        protected override void OnDrawNode(DrawTreeNodeEventArgs e)
        {
            if (!(e.Node.Tag is Color) || e.Bounds.IsEmpty)
            {
                e.DrawDefault = true;
                return;
            }

            var color = (Color)e.Node.Tag;
            Graphics g = e.Graphics;

            var imageBounds = ImageBounds;
            imageBounds.X += e.Bounds.X;
            imageBounds.Y += e.Bounds.Y;

            var textOffset = TextOffset;
            textOffset.X += e.Bounds.X;
            textOffset.Y += e.Bounds.Y;

            if (color.A < 255) //add  background trasparency  checkerboard
            {
                using (var br = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Gainsboro, Color.Transparent))
                    g.FillRectangle(br, imageBounds);
            }

            using (var solidBrush = new SolidBrush(color))
                g.FillRectangle(solidBrush, imageBounds);

            g.DrawRectangle(SystemPens.WindowText, imageBounds.X, imageBounds.Y, imageBounds.Width - 1, imageBounds.Height - 1);

            TextRenderer.DrawText(g, e.Node.Name, base.Font, textOffset, base.ForeColor, Color.Transparent);
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
                TreeNode node;
                if (value.IsKnownColor)
                    node = base.Nodes.Cast<TreeNode>().SelectMany(tn => tn.Nodes.Cast<TreeNode>()).FirstOrDefault(tn => value.Name.Equals(((Color)tn.Tag).Name));
                else node = base.Nodes.Cast<TreeNode>().SelectMany(tn => tn.Nodes.Cast<TreeNode>()).FirstOrDefault(tn => Equals(value, (Color)tn.Tag));
                base.CollapseAll();
                base.SelectedNode = node;
                node?.EnsureVisible();
                if (node!=null) base.Focus();
            }
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
