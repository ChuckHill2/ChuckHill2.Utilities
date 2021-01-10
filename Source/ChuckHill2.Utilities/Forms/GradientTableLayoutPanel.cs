using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Handles the layout of its components and arranges them in the format of a table automatically.
    /// Extends System.Windows.Forms.TableLayoutPanel to support a gradient color for the background.
    /// </summary>
    [ToolboxItem(true), ToolboxBitmap(typeof(TableLayoutPanel))]
    public class GradientTableLayoutPanel : TableLayoutPanel, IGradientControl
    {
        private GradientBrush __backgroundGradient = null;
        /// <summary> The gradient brush used to fill the background.</summary>
        [Category("Appearance"), Description("The gradient brush used to fill the background.")]
        public GradientBrush BackgroundGradient
        {
            get => __backgroundGradient == null ? new GradientBrush(this.Parent) : __backgroundGradient;
            set { __backgroundGradient = value; OnBackgroundGradientChanged(EventArgs.Empty); }
        }
        private bool ShouldSerializeBackgroundGradient() => !BackgroundGradient.Equals(new GradientBrush(this.Parent));
        private void ResetBackgroundGradient() => BackgroundGradient = null;

        /// <summary>
        /// Occurs when the value of the BackgroundGradient property changes.
        /// </summary>
        [Category("Property Changed")]
        [Description("Event raised when the value of the BackColor property is changed on Control.")]
        public event EventHandler BackgroundGradientChanged;

        #region Hidden/Unused Properties
        //! @cond DOXYGENHIDE
        /// <summary> This is not used. See the BackgroundGradient property.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override Color BackColor { get => BackgroundGradient.Color1; set => BackgroundGradient.Color1 = value; }

        /// <summary> This is not used. See the BackgroundGradientChanged event.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackColorChanged { add { } remove { } }
        //! @endcond
        #endregion Hidden/Unused Properties

        /// <summary>Raises the BackgroundGradientChanged event.</summary>
        /// <param name="e">An empty EventArgs that contains no event data.</param>
        protected virtual void OnBackgroundGradientChanged(EventArgs e)
        {
             this.Invalidate();
            this.BackgroundGradientChanged?.Invoke(this,EventArgs.Empty);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            GradientControlPaint.PaintBackground(this, e);
            DrawCellBorders(e);
        }

        #region private void DrawCellBorders(PaintEventArgs e);
        //The following was extracted and used verbatim from decompiled code in TableLayoutPanel.OnPaintBackground(PaintEventArgs e)
        private static PropertyInfo piCellBorderWidth = typeof(TableLayoutPanel).GetProperty("CellBorderWidth", BindingFlags.NonPublic | BindingFlags.Instance);
        private void DrawCellBorders(PaintEventArgs e)
        {
            int cellBorderWidth = (int)piCellBorderWidth.GetValue(this);  //int cellBorderWidth = this.CellBorderWidth;
            TableLayout.ContainerInfo containerInfo = TableLayout.GetContainerInfo(this);
            TableLayout.Strip[] columns = containerInfo.Columns;
            TableLayout.Strip[] rows = containerInfo.Rows;
            TableLayoutPanelCellBorderStyle cellBorderStyle = this.CellBorderStyle;
            if (columns == null || rows == null)
                return;
            int length1 = columns.Length;
            int length2 = rows.Length;
            int num1 = 0;
            int num2 = 0;
            Graphics graphics = e.Graphics;
            Rectangle displayRectangle = this.DisplayRectangle;
            Rectangle clipRectangle = e.ClipRectangle;
            bool flag = this.RightToLeft == RightToLeft.Yes;
            int x = !flag ? displayRectangle.X + cellBorderWidth / 2 : displayRectangle.Right - cellBorderWidth / 2;
            for (int column = 0; column < length1; ++column)
            {
                int y = displayRectangle.Y + cellBorderWidth / 2;
                if (flag)
                    x -= columns[column].MinSize;
                for (int row = 0; row < length2; ++row)
                {
                    Rectangle bound = new Rectangle(x, y, columns[column].MinSize, rows[row].MinSize);
                    Rectangle rectangle = new Rectangle(bound.X + (cellBorderWidth + 1) / 2, bound.Y + (cellBorderWidth + 1) / 2, bound.Width - (cellBorderWidth + 1) / 2, bound.Height - (cellBorderWidth + 1) / 2);
                    if (clipRectangle.IntersectsWith(rectangle))
                    {
                        using (TableLayoutCellPaintEventArgs e1 = new TableLayoutCellPaintEventArgs(graphics, clipRectangle, rectangle, column, row))
                            this.OnCellPaint(e1);
                        ControlPaint.PaintTableCellBorder(cellBorderStyle, graphics, bound);
                    }
                    y += rows[row].MinSize;
                    if (column == 0)
                        num2 += rows[row].MinSize;
                }
                if (!flag)
                    x += columns[column].MinSize;
                num1 += columns[column].MinSize;
            }
            if (!this.HScroll && !this.VScroll && cellBorderStyle != TableLayoutPanelCellBorderStyle.None)
            {
                Rectangle bound = new Rectangle(cellBorderWidth / 2 + displayRectangle.X, cellBorderWidth / 2 + displayRectangle.Y, displayRectangle.Width - cellBorderWidth, displayRectangle.Height - cellBorderWidth);
                switch (cellBorderStyle)
                {
                    case TableLayoutPanelCellBorderStyle.Inset:
                        graphics.DrawLine(SystemPens.ControlDark, bound.Right, bound.Y, bound.Right, bound.Bottom);
                        graphics.DrawLine(SystemPens.ControlDark, bound.X, bound.Y + bound.Height - 1, bound.X + bound.Width - 1, bound.Y + bound.Height - 1);
                        break;
                    case TableLayoutPanelCellBorderStyle.Outset:
                        using (Pen pen = new Pen(SystemColors.Window))
                        {
                            graphics.DrawLine(pen, bound.X + bound.Width - 1, bound.Y, bound.X + bound.Width - 1, bound.Y + bound.Height - 1);
                            graphics.DrawLine(pen, bound.X, bound.Y + bound.Height - 1, bound.X + bound.Width - 1, bound.Y + bound.Height - 1);
                            break;
                        }
                    default:
                        ControlPaint.PaintTableCellBorder(cellBorderStyle, graphics, bound);
                        break;
                }
                ControlPaint.PaintTableControlBorder(cellBorderStyle, graphics, displayRectangle);
            }
            else
                ControlPaint.PaintTableControlBorder(cellBorderStyle, graphics, displayRectangle);
        }

        // The following are helper classes exclusively used by DrawCellBorders().
        // It turns out that TableLayoutPanel.OnPaintBackground() uses this non-public
        // class, so we simulate it here via reflection just for the class members needed.

        private static class TableLayout
        {
            private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            private static Type tableLayoutType = Type.GetType("System.Windows.Forms.Layout.TableLayout, " + typeof(TableLayoutPanel).Assembly.FullName, false, false);
            private static MethodInfo miContainerInfo = tableLayoutType.GetMethod("GetContainerInfo", bindingFlags);

            public static ContainerInfo GetContainerInfo(Control container)
            {
                return new ContainerInfo(miContainerInfo.Invoke(null, new object[] { container }));
            }

            public class ContainerInfo
            {
                private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                private static Type t = Type.GetType("System.Windows.Forms.Layout.TableLayout+ContainerInfo, " + typeof(TableLayoutPanel).Assembly.FullName, false, false);
                private static PropertyInfo piColumns = t.GetProperty("Columns", bindingFlags);
                private static PropertyInfo piRows = t.GetProperty("Rows", bindingFlags);
                private object containerInfo;

                public ContainerInfo(object containerInfo)
                {
                    this.containerInfo = containerInfo;
                }

                public Strip[] Columns
                {
                    get
                    {
                        var items = piColumns.GetValue(containerInfo) as Array;
                        var strips = new Strip[items.Length];
                        for(int i=0; i<items.Length; i++)
                        {
                            strips[i] = new Strip(items.GetValue(i));
                        }
                        return strips;
                    }
                }
                public Strip[] Rows
                {
                    get
                    {
                        var items = piRows.GetValue(containerInfo) as Array;
                        var strips = new Strip[items.Length];
                        for (int i = 0; i < items.Length; i++)
                        {
                            strips[i] = new Strip(items.GetValue(i));
                        }
                        return strips;
                    }
                }

            }

            public class Strip
            {
                private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
                private static Type t = Type.GetType("System.Windows.Forms.Layout.TableLayout+Strip, " + typeof(TableLayoutPanel).Assembly.FullName, false, false);
                private static PropertyInfo piMinSize = t.GetProperty("MinSize", bindingFlags);
                private object strip;
                public Strip(object strip) { this.strip = strip; }
                public int MinSize => (int)piMinSize.GetValue(strip);
            }
        }

        private static class ControlPaint
        {
            private const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            private static MethodInfo miPaintTableCellBorder = typeof(System.Windows.Forms.ControlPaint).GetMethod("PaintTableCellBorder", bindingFlags, null, new[] { typeof(TableLayoutPanelCellBorderStyle), typeof(Graphics), typeof(Rectangle) }, null);
            private static MethodInfo miPaintTableControlBorder = typeof(System.Windows.Forms.ControlPaint).GetMethod("PaintTableControlBorder", bindingFlags, null, new[] { typeof(TableLayoutPanelCellBorderStyle), typeof(Graphics), typeof(Rectangle) }, null);

            public static void PaintTableCellBorder(TableLayoutPanelCellBorderStyle cellBorderStyle, Graphics graphics, Rectangle bound)
            {
                miPaintTableCellBorder.Invoke(null, new object[] { cellBorderStyle, graphics, bound });
            }

            public static void PaintTableControlBorder(TableLayoutPanelCellBorderStyle cellBorderStyle, Graphics graphics, Rectangle bound)
            {
                miPaintTableControlBorder.Invoke(null, new object[] { cellBorderStyle, graphics, bound });
            }
        }
        #endregion private void DrawCellBorders(PaintEventArgs e)
    }
}
