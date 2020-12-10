using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Color selector treeview control with three root nodes 'Custom', 'Known', and 'System' colors.
    /// Only custom colors can be added or removed.
    /// </summary>
    public class NamedColorTreeView : TreeView
    {
        private int graphicWidth = 22;  //default pixel values at 96dpi
        private int pixel_2 = 2;
        private int pixel_4 = 4;

        public NamedColorTreeView() : base()
        {
            base.Margin = new Padding(0);
            base.BorderStyle = BorderStyle.None;
            base.Dock = DockStyle.Fill;
            base.Name = "NamedColorTreeView";
            base.DrawMode = TreeViewDrawMode.OwnerDrawText;
            base.ShowLines = false;
            base.FullRowSelect = true;

            var pixelFactor = DpiScalingFactor() / 100.0;
            this.graphicWidth = ConvertToGivenDpiPixel(this.graphicWidth, pixelFactor);
            this.pixel_2 = ConvertToGivenDpiPixel(this.pixel_2, pixelFactor);
            this.pixel_4 = ConvertToGivenDpiPixel(this.pixel_4, pixelFactor);

            var m_tnCustomColors = new TreeNode("Custom Colors");
            var m_tnWebColors = new TreeNode("Web Colors", ColorExtensions.KnownColors.Where(c => c.IsKnownColor && !c.IsSystemColor).Select(c => new TreeNode(c.Name) { Name = c.Name, Tag = c }).ToArray());
            var m_tnSystemColors = new TreeNode("System Colors", ColorExtensions.KnownColors.Where(c => c.IsSystemColor).Select(c => new TreeNode(c.Name) { Name = c.Name, Tag = c }).ToArray());
            base.Nodes.AddRange(new TreeNode[] { m_tnCustomColors, m_tnWebColors, m_tnSystemColors });
        }

        protected override void OnCreateControl()
        {
            base.ItemHeight = base.Font.Height;
            base.OnCreateControl();
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

            var rc = new Rectangle(e.Bounds.X + this.pixel_2, e.Bounds.Y + this.pixel_2, this.graphicWidth, e.Bounds.Height - this.pixel_4 + 1);

            if (color.A < 255) //add  background trasparency  checkerboard
            {
                using (var br = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Gainsboro, Color.Transparent))
                    g.FillRectangle(br, rc);
            }

            using (var solidBrush = new SolidBrush(color))
                g.FillRectangle(solidBrush, rc);

            g.DrawRectangle(SystemPens.WindowText, rc.X, rc.Y, rc.Width - 1, rc.Height - 1);

            var x2 = e.Bounds.X + this.graphicWidth + this.pixel_4;
            var y2 = e.Bounds.Y;

            TextRenderer.DrawText(g, e.Node.Name, base.Font, new Point(x2, y2), base.ForeColor, Color.Transparent);
        }

        /// <summary>
        /// Add custom color to list.
        /// Known colors will not be added as they already exist.
        /// </summary>
        /// <param name="c"></param>
        public void AddColor(Color c)
        {
            if (c.IsEmpty) return;
            if (base.Nodes.Cast<TreeNode>().SelectMany(tn => tn.Nodes.Cast<TreeNode>()).FirstOrDefault(tn => Equals(c, (Color)tn.Tag)) != null) return;

            var name = c.Name;
            if (!c.IsNamedColor)
            {
                var node = base.Nodes[1].Nodes.Cast<TreeNode>().FirstOrDefault(tn => Equals(c, (Color)tn.Tag, true));
                if (node==null) node = base.Nodes[2].Nodes.Cast<TreeNode>().FirstOrDefault(tn => Equals(c, (Color)tn.Tag, true));
                if (node != null) name = ((Color)node.Tag).Name + c.A.ToString();
                else name = c.A < 255 ? $"({c.A},{c.R},{c.G},{c.B})" : $"({c.R},{c.G},{c.B})";
            }
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

        private static int ConvertToGivenDpiPixel(int value, double pixelFactor) => Math.Max(1, (int)(value * pixelFactor + 0.5));
        private new TreeNodeCollection Nodes => throw new InvalidOperationException($"{nameof(Nodes)} property is disabled for {typeof(NamedColorTreeView).Name}. For internal use only.");

        #region public static int DpiScalingFactor()
        [DllImport("gdi32.dll")] private static extern int GetDeviceCaps(IntPtr hdc, int nIndex);
        private enum DeviceCap { VERTRES = 10, DESKTOPVERTRES = 117, LOGPIXELSY = 90 }
        [DllImport("user32.dll")] private static extern IntPtr GetDC(IntPtr hWnd);
        [DllImport("user32.dll")] private static extern bool ReleaseDC(IntPtr hWnd, IntPtr hDC);

        /// <summary>
        /// Get current DPI scaling factor as a percentage
        /// </summary>
        /// <returns>Scaling percentage</returns>
        public static float DpiScalingFactor()
        {
            IntPtr hDC = IntPtr.Zero;
            try
            {
                hDC = GetDC(IntPtr.Zero);
                int logpixelsy = GetDeviceCaps(hDC, (int)DeviceCap.LOGPIXELSY);
                float dpiScalingFactor = logpixelsy / 96f;
                //Smaller - 100% == screenScalingFactor=1.0 dpiScalingFactor=1.0
                //Medium - 125% (default) == screenScalingFactor=1.0 dpiScalingFactor=1.25
                //Larger - 150% == screenScalingFactor=1.0 dpiScalingFactor=1.5
                return dpiScalingFactor * 100f;
            }
            finally
            {
                if (hDC != IntPtr.Zero) ReleaseDC(IntPtr.Zero, hDC);
            }
        }
        #endregion
    }
}
