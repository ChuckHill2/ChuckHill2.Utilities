//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="NamedColorListBox.cs" company="Chuck Hill">
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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    ///  @image html NamedColorListBox.png
    /// <summary>
    /// Color selector ListBox  control containing 'Custom', 'Known', and 'System' colors.
    /// Each group has a dividing line for distinction between the three color sets.
    /// Only custom colors can be added or removed.
    /// </summary>
    [ToolboxBitmap(typeof(ListBox))]
    [DefaultEvent("SelectionChanged")]
    [Description("Select from a list of known colors.")]
    public class NamedColorListBox : ListBox
    {
        private int graphicWidth = 22;  //default pixel values at 96dpi

        private Brush _transparentIconBrush = new HatchBrush(HatchStyle.LargeCheckerBoard, Color.Gainsboro, Color.White);
        private Brush _disabledTranslucentBackground = new SolidBrush(Color.FromArgb(32, SystemColors.InactiveCaption));
        private Rectangle ImageBounds;
        private Point TextOffset;

        private OrderBy __orderBy = OrderBy.Color;
        /// <summary>
        ///  Specify how the list of colors is ordered.
        /// </summary>
        [Category("Appearance"), Description("Specify how the list of colors is ordered.")]
        [DefaultValue(OrderBy.Color)]
        public OrderBy OrderBy
        {
            get => __orderBy;
            set
            {
                if (__orderBy == value) return;
                __orderBy = value;

                base.BeginUpdate();

                var customItems = base.Items.Cast<ColorItem>().TakeWhile(ci => !ci.Color.IsKnownColor);
                base.Items.Clear();
                if (__orderBy == OrderBy.Color)
                {
                    foreach (var ci in customItems) base.Items.Add(ci); //Custom items are always at the top of the list.
                    foreach (var c in ColorEx.KnownColors) base.Items.Add(new ColorItem(c.Name, c));
                }
                else
                {
                    foreach (var ci in customItems) base.Items.Add(ci);
                    foreach (var c in ColorEx.KnownColors.OrderBy(c => c.Name)) base.Items.Add(new ColorItem(c.Name, c));
                }

                base.EndUpdate();
            }
        }

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
        /// Gets or sets the height, in pixels, of items in the list box.
        /// </summary>
        [RefreshProperties(RefreshProperties.Repaint)]
        [Category("Behavior"), Description("The height, in pixels, of items in the list box.")]
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
                if (value >= short.MaxValue - 2) value = short.MaxValue - 2;
                if (value == __itemsHeight) return;
                __itemsHeight = value;
                base.ItemHeight = __itemsHeight;
                UpdateDrawingBounds();
            }
        }
        private int __itemsHeight = -1;
        private bool ShouldSerializeItemsHeight() => ItemsHeight != DefaultItemsHeight; //In lieu of using [DefaultValue(someConst)]
        private void ResetItemsHeight() => ItemsHeight = DefaultItemsHeight;
        private int DefaultItemsHeight => new ImageFontMetrics(this.Font).EmHeightPixels + 2;

        private void UpdateDrawingBounds()
        {
            ImageBounds = new Rectangle(2, 1, graphicWidth, this.ItemsHeight - 2);

            // Need to properly center the *visible* text within the ItemHeight.
            var fi = new ImageFontMetrics(this.Font);
            var yOffset = (this.ItemsHeight - fi.EmHeightPixels - fi.InternalLeadingPixels) / 2.0f;

            TextOffset = new Point(2 + graphicWidth + 2, (int)Math.Floor(yOffset));
        }

        #region Hidden/Disabled Properties
        private const string NOTUSED = "Not used in " + nameof(NamedColorListBox) + ".";
        //! @cond DOXYGENHIDE
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object DataSource { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string DisplayMember { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DrawMode DrawMode { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string FormatString { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool FormattingEnabled { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ObjectCollection Items { get; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string ValueMember { get; set; }
        [Obsolete(NOTUSED + " See property OrderBy", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Sorted { get; set; }

        [Obsolete(NOTUSED + " See property Selected", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int SelectedIndex { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new SelectedIndexCollection SelectedIndices { get; }
        [Obsolete(NOTUSED + " See property Selected", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object SelectedItem { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new SelectedObjectCollection SelectedItems { get; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Text { get; set; }
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int TopIndex { get; set; }
        [Obsolete(NOTUSED + " See property Selected", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new object SelectedValue { get; set; }
        [Obsolete(NOTUSED + "Use property ItemsHeight", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int ItemHeight { get; set; }
        [Obsolete(NOTUSED + "Use field DefaultItemsHeight", true)]
        public new int DefaultItemHeight;

        #pragma warning disable CS0067 //The event is never used
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackgroundImageChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler TextChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event MouseEventHandler MouseClick;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler PaddingChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler Click;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event PaintEventHandler Paint;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event DrawItemEventHandler DrawItem;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event MeasureItemEventHandler MeasureItem;
        [Obsolete(NOTUSED + " See event SelectionChanged", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler SelectedIndexChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler BackgroundImageLayoutChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]

        public new event EventHandler DataSourceChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler DisplayMemberChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event ListControlConvertEventHandler Format;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler FormatInfoChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler FormatStringChanged;
        [Obsolete(NOTUSED, true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler FormattingEnabledChanged;
        [Obsolete(NOTUSED + " See event SelectionChanged", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler ValueMemberChanged;
        [Obsolete(NOTUSED + " See event SelectionChanged", true), Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler SelectedValueChanged;
        #pragma warning restore CS0067 //The event is never used
        //! @endcond
        #endregion

        /// <summary>
        /// Initializes a new instance of the NamedColorListBox class.
        /// </summary>
        public NamedColorListBox():base()
        {
            base.Name = "NamedColorListBox";
            base.DrawMode = DrawMode.OwnerDrawFixed;
            // http://yacsharpblog.blogspot.com/2008/07/listbox-flicker.html
            base.SetStyle(
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.UserPaint,
                true);
            base.ItemHeight = this.ItemsHeight;
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            //Event Sequence: Control.HandleCreated. Control.BindingContextChanged. Form.Load. Control.VisibleChanged. Form.Activated. Form.Shown
            base.OnHandleCreated(e);
            base.ItemHeight = this.ItemsHeight;
            UpdateDrawingBounds();

            base.BeginUpdate();

            if (this.OrderBy == OrderBy.Color)
                foreach (var c in ColorEx.KnownColors) base.Items.Add(new ColorItem(c.Name, c));
            else
                foreach (var c in ColorEx.KnownColors.OrderBy(c => c.Name)) base.Items.Add(new ColorItem(c.Name, c));

            base.EndUpdate();
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

        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e.Index == -1) return;

            #region Set Selected Row Highlight
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                e = new DrawItemEventArgs(e.Graphics,
                                          e.Font,
                                          e.Bounds,
                                          e.Index,
                                          e.State ^ DrawItemState.Selected,
                                          base.Focused ? SystemColors.HighlightText : SystemColors.ControlText,
                                          base.Focused ? SystemColors.Highlight : SystemColors.GradientInactiveCaption);//Choose the color
            #endregion

            var ci = (ColorItem)base.Items[e.Index];

            Graphics g = e.Graphics;
            e.DrawBackground();

            #region Draw Icon
            var imageBounds = ImageBounds;
            imageBounds.X += e.Bounds.X;
            imageBounds.Y += e.Bounds.Y;

            //add  background trasparency  checkerboard
            if (ci.Color.A < 255) g.FillRectangle(_transparentIconBrush, imageBounds);

            using (var solidBrush = new SolidBrush(ci.Color))
                g.FillRectangle(solidBrush, imageBounds);

            g.DrawRectangle(SystemPens.WindowText, imageBounds.X, imageBounds.Y, imageBounds.Width - 1, imageBounds.Height - 1);
            #endregion

            #region Draw Text
            var textOffset = TextOffset;
            textOffset.X += e.Bounds.X;
            textOffset.Y += e.Bounds.Y;
            TextRenderer.DrawText(g, ci.Name, this.Font, textOffset, base.Enabled ? e.ForeColor : SystemColors.GrayText, Color.Transparent);
            #endregion

            #region Draw Divider
            // Create a divider line between CustomColors, WebColors, and SystemColors or if
            // sorted alphabetically, just between CustomColors and all other known colors.

            if (e.Index >= 0 && e.Index < base.Items.Count - 1)
            {
                var ci2 = (ColorItem)base.Items[e.Index + 1]; //compare current vs next color
                if (OrderBy == OrderBy.Color)
                {
                    if (ci.Color.IsSystemColor != ci2.Color.IsSystemColor ||
                        ci.Color.IsKnownColor != ci2.Color.IsKnownColor)
                    {
                        g.DrawLine(SystemPens.WindowText, imageBounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right - imageBounds.Left, e.Bounds.Bottom - 1);
                    }
                }
                else
                {
                    if (ci.Color.IsKnownColor != ci2.Color.IsKnownColor)
                    {
                        g.DrawLine(SystemPens.WindowText, imageBounds.Left, e.Bounds.Bottom - 1, e.Bounds.Right - imageBounds.Left, e.Bounds.Bottom - 1);
                    }
                }
            }
            #endregion

            if (!base.Enabled) g.FillRectangle(_disabledTranslucentBackground, e.Bounds);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Region iRegion = new Region(e.ClipRectangle);
            using(var br = new SolidBrush(base.BackColor))  e.Graphics.FillRegion(br, iRegion);
            if (!base.Enabled) e.Graphics.FillRectangle(_disabledTranslucentBackground, base.ClientRectangle);

            if (base.Items.Count > 0)
            {
                for (int i = 0; i < base.Items.Count; ++i)
                {
                    System.Drawing.Rectangle irect = base.GetItemRectangle(i);
                    if (e.ClipRectangle.IntersectsWith(irect))
                    {
                        if ((base.SelectionMode == SelectionMode.One && base.SelectedIndex == i)
                        || (base.SelectionMode == SelectionMode.MultiSimple && base.SelectedIndices.Contains(i))
                        || (base.SelectionMode == SelectionMode.MultiExtended && base.SelectedIndices.Contains(i)))
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.Selected, base.Enabled ? base.ForeColor : SystemColors.GrayText,
                                base.BackColor));
                        }
                        else
                        {
                            OnDrawItem(new DrawItemEventArgs(e.Graphics, this.Font,
                                irect, i,
                                DrawItemState.Default, base.Enabled ? base.ForeColor : SystemColors.GrayText,
                                base.BackColor));
                        }
                        iRegion.Complement(irect);
                    }
                }
            }

            base.OnPaint(e);
        }

        /// <summary>
        /// Add custom color to list.
        /// Known colors will not be added as they already exist.
        /// </summary>
        /// <param name="c"></param>
        public void AddColor(Color c)
        {
            if (c.IsKnownColor || c.IsEmpty) return;
            if (base.Items.Cast<ColorItem>().TakeWhile(ci => !ci.Color.IsKnownColor).FirstOrDefault(ci => Equals(c, ci.Color)) != null) return;
            base.Items.Insert(0, new ColorItem(c.GetName(), c));  //Custom named colors go to top of list
        }

        /// <summary>
        /// Remove custom color from list.
        /// Known colors will not be removed.
        /// </summary>
        /// <param name="c"></param>
        public void RemoveColor(Color c)
        {
            if (c.IsKnownColor || c.IsEmpty) return;
            var item = base.Items.Cast<ColorItem>().TakeWhile(ci => !ci.Color.IsKnownColor).FirstOrDefault(ci => Equals(c, ci.Color));
            if (item == null) return;
            base.Items.Remove(item);
        }

        /// <summary>
        /// Get or Set the selected color.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color Selected
        {
            get => base.SelectedItem is ColorItem ? ((ColorItem)base.SelectedItem).Color : Color.Empty;
            set
            {
                ColorItem item = base.Items.Cast<ColorItem>().FirstOrDefault(ci => Equals(value, ci.Color));
                base.SelectedItem = item;
            }
        }

        /// <summary>
        /// Occurs when a color has been selected.
        /// </summary>
        [Category("Behavior"), Description("Occurs when a color has been selected.")]
        public event NamedColorEventHandler SelectionChanged;

        protected override void OnSelectedValueChanged(EventArgs e)
        {
            if (SelectionChanged != null)
            {
                var selected = this.Selected;
                if (!selected.IsEmpty)
                    SelectionChanged.Invoke(this, new NamedColorEventArgs(selected));
            }

            base.OnSelectedValueChanged(e);
        }

        private static bool Equals(Color c1, Color c2, bool ignoreAlpha = false)
        {
            if (c1.IsEmpty && !c2.IsEmpty) return false;
            if (!c1.IsEmpty && c2.IsEmpty) return false;
            if (c1.IsEmpty && c2.IsEmpty) return true;
            if (ignoreAlpha) return c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
            return c1.A == c2.A && c1.R == c2.R && c1.G == c2.G && c1.B == c2.B;
        }

        private class ColorItem
        {
            public readonly Color Color;
            public readonly string Name;
            public ColorItem(string name, Color c) { Name = name; Color = c; }
            public override string ToString() => this.Name;
        }
    }
}
