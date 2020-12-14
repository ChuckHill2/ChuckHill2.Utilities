using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ChuckHill2.Utilities
{
    ///  @image html EnumUIEditor.png
    /// <summary>
    /// UITypeEditor for setting regular and bitwise enums (aka Flags attribute).
    /// </summary>
    /// <remarks>
    ///  For [Flags] bitwise enum values that evaluate to 0 are special in that when that enum is checked, all others are 
    ///  unchecked. Typically the zero'th enum is considered the uninitialized default. Regular enums only allow one selection.
    /// If any enum values have a [Description("Hello World")] attribute associated with them, then they will show up as tooltips.
    /// If any enum values have a [Image(typeof(Direction),"Left.png"] attribute associated with them, then they will show up as icons.<br />
    /// **Usage:**
    /// <pre>
    ///     [Editor(typeof(EnumUIEditor), typeof(UITypeEditor))]
    ///     public Arrows Direction { get; set; }
    /// </pre>
    /// </remarks>
    public class EnumUIEditor : UITypeEditor
    {
        private EnumPanel dropdownControl = null;

        #region Override Methods
        //! @cond DOXYGENHIDE

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.DropDown;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (context != null && context.Instance != null && provider != null)
            {
                IWindowsFormsEditorService edSvc = (IWindowsFormsEditorService)provider?.GetService(typeof(IWindowsFormsEditorService));
                if (edSvc != null)
                {
                    if (dropdownControl == null) //load on demand and reuse it.
                    {
                        //We want to use the same font as the properties control...
                        // edSvc container has a Font and property we need but it is of type System.Windows.Forms.PropertyGridInternal.PropertyGridView and not accessable without reflection. Arrgh!
                        var fontPI = edSvc.GetType().GetProperty("Font", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                        var font = fontPI == null ? SystemFonts.MessageBoxFont : (Font)fontPI.GetValue(edSvc);

                        dropdownControl = new EnumPanel(font);
                    }

                    dropdownControl.EnumValue = (Enum)Convert.ChangeType(value, context.PropertyDescriptor.PropertyType);
                    edSvc.DropDownControl(dropdownControl);

                    return dropdownControl.EnumValue;
                }
            }
            return null;
        }

        //! @endcond
        #endregion

        private class EnumPanel : Panel
        {
            public Font CheckedFont { get; set; }
            public Font UncheckedFont { get; set; }
            private Type EnumType;
            private bool IsFlagged;
            private ToolTip TooltipHandler = null;
            private Size IdealSize;

            [DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden)]
            public Enum EnumValue
            {
                get => (Enum)Enum.ToObject(EnumType, GetCheckState());
                set
                {
                    int iValue = EnumToInt(value);

                    if (EnumType == value.GetType())
                    {
                        this.Size = IdealSize;  //Caller may change the size of this panel so we set it back.
                        SetItemCheckStates(iValue);
                        return;
                    }

                    EnumType = value.GetType();
                    IsFlagged = EnumType.CustomAttributes.FirstOrDefault(d => d.AttributeType == typeof(FlagsAttribute)) != null;

                    this.Controls.Clear();

                    var enumItems = Enum.GetValues(EnumType).Cast<Enum>().Select(e => new EnumItem(e)).ToArray();
                    var hasIcons = enumItems.Any(e => e.Icon != null);

                    int panelWidth = 0;
                    int rowNum = 0;
                    foreach (var ei in enumItems)
                    {
                        Control item = IsFlagged ? (Control)new CheckBoxEx(ei, UncheckedFont, CheckedFont) : new RadioButtonEx(ei, UncheckedFont, CheckedFont);
                        this.Controls.Add(item);
                        item.Text = ei.Caption;
                        if (ei.Description != null)
                        {
                            if (TooltipHandler == null) TooltipHandler = new ToolTip() { BackColor = Color.AliceBlue };
                            TooltipHandler.SetToolTip(item, ei.Description);
                        }

                        var location = new Point(3, item.Height * rowNum);
                        if (hasIcons)
                        {
                            var iconsize = item.Height - 2;
                            this.Controls.Add(new PictureBoxEx(iconsize, new Point(location.X, location.Y + (item.Height - iconsize) / 2), ei.Icon));
                            location.X += iconsize + 3;
                        }

                        item.Location = location;

                        if (item.Right > panelWidth) panelWidth = item.Right;
                        rowNum++;
                    }

                    SetItemCheckStates(iValue);

                    //Set panel to the perfect size to contain the button controls
                    this.Height = this.Controls[0].Height * (rowNum > 20 ? 20 : rowNum);
                    this.Width = panelWidth + (rowNum > 20 ? System.Windows.Forms.SystemInformation.VerticalScrollBarWidth : 0); //Add extra for scroll bar
                    IdealSize = new Size(this.Width, this.Height);
                }
            }

            public EnumPanel(Font font) : base()
            {
                base.AutoScroll = true;
                base.Font = font;
                //base.BackColor = Color.Pink;  //for position testing

                UncheckedFont = base.Font;
                CheckedFont = new Font(font, FontStyle.Bold);
            }

            private void SetItemCheckStates(int iValue)
            {
                if (IsFlagged && iValue != 0)
                {
                    int selected = 0;
                    foreach (var c in this.Controls.OfType<ICheckItem>().OrderByDescending(m => m.Item.Value))
                    {
                        if (selected != 0 && (selected & c.Item.Value) == c.Item.Value) { c.Checked = false; continue; }
                        if ((c.Item.Value & iValue) == c.Item.Value && (c.Item.Value & ~iValue) == 0)
                        {
                            selected |= c.Item.Value;
                            c.Checked = true;
                        }
                        else
                        {
                            c.Checked = false;
                        }
                    }
                }
                else
                {
                    foreach (ICheckItem c in this.Controls.OfType<ICheckItem>()) c.Checked = (c.Item.Value == iValue);
                }
            }

            private int GetCheckState()
            {
                int result = 0;
                foreach (ICheckItem c in this.Controls.OfType<ICheckItem>())
                {
                    result |= c.Checked ? c.Item.Value : 0;
                }

                return result;
            }

            private static int EnumToInt(Enum v) => (int)Convert.ChangeType(v, typeof(int));

            protected override void Dispose(bool disposing)
            {
                if (CheckedFont != null) { CheckedFont.Dispose(); CheckedFont = null; }
                base.Dispose(disposing);
            }

            /// <summary>
            /// Convert any integer into a formatted binary bit string.
            /// In the debugger watch window enter 'ToBinary((byte)value), ac' to view the dynamically updated results.
            /// </summary>
            /// <param name="value">Integer to convert. Cast down value if you don't want a lot of leading zeros.</param>
            /// <returns>string of binary representation of integer.</returns>
            public static string ToBinary(object value)
            {
                var t = value.GetType();
                var byteCount = System.Runtime.InteropServices.Marshal.SizeOf(value);

                ulong x = Convert.ToUInt64(value);

                ulong bit = 1;
                var sb = new StringBuilder();
                for (int i = 1; i <= byteCount * 8; i++)
                {
                    sb.Insert(0, (x & bit) == 0 ? '0' : '1');
                    bit <<= 1;

                    if (i % 4 == 0) sb.Insert(0, i % 8 == 0 ? ' ' : '\xB7');
                }
                sb.Remove(0, 1);

                return sb.ToString();
            }

            public interface ICheckItem
            {
                bool Checked { get; set; }
                EnumItem Item { get; set; }
            }

            public class CheckBoxEx : CheckBox, ICheckItem
            {
                private readonly Font UncheckedFont;
                private readonly Font CheckedFont;
                public EnumItem Item { get; set; }

                public CheckBoxEx(EnumItem item, Font uncheckedFont, Font checkedFont) : base()
                {
                    this.AutoSize = true;
                    this.Margin = new Padding(0);
                    this.UseVisualStyleBackColor = true;
                    //base.BackColor = Color.DarkKhaki;  //for position testing

                    Item = item;
                    UncheckedFont = uncheckedFont;
                    CheckedFont = checkedFont;
                    base.Font = UncheckedFont;
                }

                protected override void OnCheckedChanged(EventArgs e)
                {
                    base.Font = base.Checked ? CheckedFont : UncheckedFont;
                    if (base.Checked)
                    {
                        if (Item.Value == 0) foreach (ICheckItem c in this.Parent.Controls.OfType<ICheckItem>()) { if (c != this) c.Checked = false; }
                        else foreach (ICheckItem c in this.Parent.Controls.OfType<ICheckItem>()) { if (c.Item.Value == 0) c.Checked = false; }
                    }
                    base.OnCheckedChanged(e);
                }
            }

            public class RadioButtonEx : RadioButton, ICheckItem
            {
                private readonly Font UncheckedFont;
                private readonly Font CheckedFont;
                public EnumItem Item { get; set; }

                public RadioButtonEx(EnumItem item, Font uncheckedFont, Font checkedFont) : base()
                {
                    this.AutoSize = true;
                    this.Margin = new Padding(0);
                    this.UseVisualStyleBackColor = true;
                    //base.BackColor = Color.DarkKhaki;  //for position testing

                    Item = item;
                    UncheckedFont = uncheckedFont;
                    CheckedFont = checkedFont;
                    base.Font = UncheckedFont;
                }

                protected override void OnCheckedChanged(EventArgs e)
                {
                    base.Font = base.Checked ? CheckedFont : UncheckedFont;
                    base.OnCheckedChanged(e);
                }
            }

            public class PictureBoxEx : PictureBox
            {
                public PictureBoxEx(int size, Point location, Image image) : base()
                {
                    if (image != null) base.Image = image;
                    base.BackgroundImageLayout = ImageLayout.Stretch;
                    base.Location = location;
                    base.Margin = new Padding(0);
                    base.Name = "EnumIcon";
                    base.Size = new Size(size, size);
                    base.TabStop = false;
                    //base.BackColor = Color.PaleTurquoise;  //for position testing
                }
            }

            public class EnumItem
            {
                public readonly int Value;
                public readonly string Caption;
                public readonly string Description;

                private readonly ImageAttribute _ia;
                public Image Icon => _ia?.Image;  //ImageAttribute.Image is load-on-demand

                public EnumItem(Enum value)
                {
                    Value = (int)Convert.ChangeType(value, typeof(int));
                    Caption = value.ToString();

                    FieldInfo fi = value.GetType().GetField(value.ToString());
                    DescriptionAttribute[] dattr = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
                    Description = dattr.Length > 0 ? dattr[0].Description : null;

                    _ia = fi.GetCustomAttributes<ImageAttribute>().FirstOrDefault();

                }

                public override string ToString() => $"{Caption} : {Value}";
            }
        }
    }
}
