//https://stackoverflow.com/questions/37931785/winforms-designer-properties-of-different-derived-types
//https://blackwells.co.uk/extracts/ch07.pdf

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows.Forms;

// NOTE: For Winforms Designer serialization to work consistantly, this 
// MUST be in a separate assembly from the Form that it is being used in. 
// This is probably due to Visual Studio loading the assembly in order to 
// use this and unable to reload when the project is rebuilt.

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Create a simple 2-color gradient brush that is editable in the Winforms Designer.
    /// </summary>
    [Editor(typeof(GradientBrushEditor), typeof(UITypeEditor))]
    [TypeConverter(typeof(GradientBrushConverter))]
    [Category("Appearance"), Description("The gradient brush used to fill the background.")]
    public class GradientBrush
    {
        public enum BrushStyle
        {
            Solid = 0,   //uninitialized/default value.
            Horizontal,
            Vertical,
            ForwardDiagonal,
            BackwardDiagonal,
            Center,
            CenterHorizontal,
            CenterVertical,
            CenterForwardDiagonal,
            CenterBackwardDiagonal
        }

        // This is all for attempting to gather default properties based upon parent current properties, which may change and these defaults along with the parent.
        private Control Host = null; //this is only required at design time
        private Color DefaultColor1 => Host == null ? SystemColors.Control : Host is IGradientControl ? ((IGradientControl)Host).BackgroundGradient.Color1 == SystemColors.Control ? Host.BackColor : ((IGradientControl)Host).BackgroundGradient.Color1 : Host.BackColor;
        private Color DefaultColor2 => Host == null ? SystemColors.Control : Host is IGradientControl ? ((IGradientControl)Host).BackgroundGradient.Color2 == SystemColors.Control ? Host.BackColor : ((IGradientControl)Host).BackgroundGradient.Color2 : Host.BackColor;
        private GradientBrush.BrushStyle DefaultStyle => Host == null ? GradientBrush.BrushStyle.Solid : Host is IGradientControl ? ((IGradientControl)Host).BackgroundGradient.Style : GradientBrush.BrushStyle.Solid;
        private bool DefaultGammaCorrection => Host == null ? false : Host is IGradientControl ? ((IGradientControl)Host).BackgroundGradient.GammaCorrection : false;

        #region Properties
        /// <summary>
        /// The gradient style for the brush.
        /// </summary>
        [Editor(typeof(EnumUIEditor), typeof(UITypeEditor))]
        [Category("Appearance"), Description("The style of the gradient.")]
        public BrushStyle Style { get; set; }
        //private bool ShouldSerializeStyle() => Style != DefaultStyle;  //In lieu of using [DefaultValue(someConst)]
        //private void ResetStyle() => Style = DefaultStyle;

        /// <summary>
        /// The first color of the gradient or this is the solid brush color.
        /// </summary>
        [Category("Appearance"), Description("The first color of the gradient or the solid brush color.")]
        [Editor(typeof(ColorUIEditor), typeof(UITypeEditor))]
        public Color Color1 { get; set; }
        //private bool ShouldSerializeColor1() => Color1 != DefaultColor1;  //In lieu of using [DefaultValue(someConst)]
        //private void ResetColor1() => Color1 = DefaultColor1;

        /// <summary>
        /// The second color of the gradient.
        /// </summary>
        [Category("Appearance"), Description("The second color of the gradient. This is ignored if the style is Solid.")]
        [Editor(typeof(ColorUIEditor), typeof(UITypeEditor))]
        public Color Color2 { get; set; }
        //private bool ShouldSerializeColor2() => Style!=BrushStyle.Solid && Color2 != DefaultColor2;
        //private void ResetColor2() => Color2 = DefaultColor2;

        /// <summary>
        /// Controls the overall brightness and ratio of red to green to blue hues. Enables a more uniform intensity across the gradient. This is ignored if the style is Solid.
        /// </summary>
        [Category("Appearance"), Description("Controls the overall brightness and ratio of red to green to blue hues. Enables a more uniform intensity across the gradient. This is ignored if the style is Solid.")]
        public bool GammaCorrection { get; set; }
        //private bool ShouldSerializeGammaCorrection() => GammaCorrection != DefaultGammaCorrection;
        //private void ResetGammaCorrection() => GammaCorrection = DefaultGammaCorrection;
        #endregion //Properties

        #region Constructors
        //Both of these constructors are required for GradientBrushConverter

        /// <summary>
        /// Create an object with hardcoded default values. This is not used...except maybe by reflection such as Activator.CreateInstance<GradientBrush>()
        /// </summary>
        private GradientBrush() 
        { 
            Color1 = DefaultColor1; 
            Color2 = DefaultColor2;
            Style = DefaultStyle;
            GammaCorrection = DefaultGammaCorrection;
        }

        /// <summary>
        ///  Create an object using parent properties as defaults.
        /// </summary>
        /// <param name="parent">Parent control used to determine what defaults to use. Null is ok.</param>
        public GradientBrush(Control parent) 
        { 
            Host = parent; 
            Color1 = DefaultColor1; 
            Color2 = DefaultColor2;
            Style = DefaultStyle;
            GammaCorrection = DefaultGammaCorrection;
        }

        /// <summary>
        /// Create a new object with the specified properties.
        /// </summary>
        /// <param name="parent">Parent control used to determine what defaults to use. Null is ok.</param>
        /// <param name="color1">The first color of the gradient or this is the solid brush color.</param>
        /// <param name="color2">The second color of the gradient.</param>
        /// <param name="style">The gradient style for the brush.</param>
        /// <param name="gammaCorrection">Controls the overall brightness and ratio of red to green to blue hues. Enables a more uniform intensity across the gradient. This is ignored if the style is Solid.</param>
        public GradientBrush(Control parent, Color color1, Color color2, BrushStyle style, bool gammaCorrection)
        {
            Host = parent;
            Color1 = color1;
            Color2 = color2;
            Style = style;
            GammaCorrection = gammaCorrection;
        }
        #endregion Constructors

        #region private static void SetGammaCorrection(PathGradientBrush pbr, bool enable)
        //Hack: Unlike LinearGradientBrush GammaCorrection property is not exposed for PathGradientBrush!
        //http://www.jose.it-berater.org/gdiplus/reference/flatapi/pathgradientbrush/gdipsetpathgradientgammacorrection.htm
        [DllImport("gdiplus.dll")] private static extern int GdipSetPathGradientGammaCorrection(IntPtr hBrush, bool useGammaCorrection);
        private static readonly FieldInfo _fiNativeBrush = typeof(Brush).GetField("nativeBrush", BindingFlags.NonPublic | BindingFlags.Instance);
        private static void SetGammaCorrection(PathGradientBrush pbr, bool enable) => GdipSetPathGradientGammaCorrection((IntPtr)_fiNativeBrush.GetValue(pbr), enable);
        #endregion

        /// <summary>
        /// Retrieve the gradient brush for the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle that the gradient is going to fill. If null, defaults to a solid brush using the first color.</param>
        /// <returns>Created gradient brush. It is up to the caller to dispose of the brush.</returns>
        public Brush GetBrush(Rectangle? rc = null) => GetBrush(!rc.HasValue ? (RectangleF?)null : new RectangleF(rc.Value.X, rc.Value.Y, rc.Value.Width, rc.Value.Height));

        /// <summary>
        /// Retrieve the gradient brush for the specified rectangle.
        /// </summary>
        /// <param name="rect">The rectangle that the gradient is going to fill. If null, defaults to a solid brush using the first color.</param>
        /// <returns>Created gradient brush. It is up to the caller to dispose of the brush.</returns>
        public Brush GetBrush(RectangleF? rect)
        {
            //https://docs.microsoft.com/en-us/dotnet/desktop/winforms/advanced/how-to-create-a-path-gradient?view=netframeworkdesktop-4.8
            //https://www.codeproject.com/Articles/20018/Gradients-made-easy
            if (!rect.HasValue || Style == BrushStyle.Solid) return new SolidBrush(Color1);
            var rc = rect.Value;
            var center = new PointF(rc.Width / 2f, rc.Height / 2f);

            LinearGradientBrush lbr = null;
            switch (Style)
            {
                case BrushStyle.Horizontal:
                    lbr = new LinearGradientBrush(rc, Color1, Color2, LinearGradientMode.Horizontal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipX };
                    break;

                case BrushStyle.Vertical:
                    lbr = new LinearGradientBrush(rc, Color1, Color2, LinearGradientMode.Vertical) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipY };
                    break;

                case BrushStyle.ForwardDiagonal:
                    lbr = new LinearGradientBrush(rc, Color1, Color2, LinearGradientMode.ForwardDiagonal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipXY };
                    break;

                case BrushStyle.BackwardDiagonal:
                    lbr = new LinearGradientBrush(rc, Color1, Color2, LinearGradientMode.BackwardDiagonal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipXY };
                    break;

                case BrushStyle.Center:
                    GraphicsPath path = new GraphicsPath();
                    rc.Inflate(rc.Width / 4.8f, rc.Height / 4.8f);
                    path.AddEllipse(rc);
                    PathGradientBrush pbr = new PathGradientBrush(path);
                    path.Dispose();
                    SetGammaCorrection(pbr, GammaCorrection);
                    pbr.CenterPoint = center;
                    pbr.CenterColor = Color1;
                    pbr.SurroundColors = new[] { Color2 };
                    return pbr;

                case BrushStyle.CenterHorizontal:
                    lbr = new LinearGradientBrush(rc, Color1, Color2, LinearGradientMode.Horizontal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipX };
                    lbr.SetSigmaBellShape(0.5f, 1.0f);
                    break;

                case BrushStyle.CenterVertical:
                    lbr = new LinearGradientBrush(rc, Color1, Color2, LinearGradientMode.Vertical) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipY };
                    lbr.SetSigmaBellShape(0.5f, 1.0f);
                    break;

                case BrushStyle.CenterForwardDiagonal:
                    lbr = new LinearGradientBrush(rc, Color1, Color2, LinearGradientMode.ForwardDiagonal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipXY };
                    lbr.SetSigmaBellShape(0.5f, 1.0f);
                    break;

                case BrushStyle.CenterBackwardDiagonal:
                    lbr = new LinearGradientBrush(rc, Color1, Color2, LinearGradientMode.BackwardDiagonal) { GammaCorrection = GammaCorrection, WrapMode = WrapMode.TileFlipXY };
                    lbr.SetSigmaBellShape(0.5f, 1.0f);
                    break;
            }

            return lbr;
        }

        #region Override Methods
        //! @cond DOXYGENHIDE

        public override bool Equals(object obj)
        {
            //This override is required for GradientBrushConverter to detect when this instance has been modified.
            var other = obj as GradientBrush;
            if (other == null) return false;

            var c1 = this.Color1 == other.Color1;
            //var c2 = this.Style == BrushStyle.Solid || this.Color2 == other.Color2; //only relevant for gradients
            var c2 = this.Color2 == other.Color2; //only relevant for gradients
            var o  = this.Style == other.Style;
            //var g = this.Style == BrushStyle.Solid || this.GammaCorrection == other.GammaCorrection; //only relevant for gradients
            var g  = this.GammaCorrection == other.GammaCorrection; //only relevant for gradients

            return c1 && c2 && o && g;
        }

        public override int GetHashCode()
        {
            //Optional override. Only here for completeness with required Equals() override.
            //Really only required for hash tables and dictionaries.
            unchecked
            {
                int hash = 13;
                hash = (hash * 7) + Color1.GetHashCode();
                hash = (hash * 7) + this.Style == BrushStyle.Solid ? 0 : Color2.GetHashCode();
                hash = (hash * 7) + Style.GetHashCode();
                hash = (hash * 7) + this.Style == BrushStyle.Solid ? 0 : GammaCorrection.GetHashCode();
                return hash;
            }
        }

        public override string ToString() => $"{ColorToString(Color1)}{(Style == BrushStyle.Solid ? "" : ", " + ColorToString(Color2))}, {Style}";

        //! @endcond
        #endregion

        private static string ColorToString(Color c) => c.IsKnownColor ? c.Name : c.IsNamedColor ? $"'{c.Name}'" : c.A < byte.MaxValue ? $"({c.A},{c.R},{c.G},{c.B})" : $"({c.R},{c.G},{c.B})";
    }

    /// <summary>
    /// WinForms Designer type converter for GradientBrush.
    /// </summary>
    internal class GradientBrushConverter : TypeConverter
    {
        //PropertyHeader string parser. Handles color grouping characters:  none,[],(),{},<>
        private static readonly string pattern = $@"
 ^(?:[\[\(\{{\<']?
     (?<COLOR1>[0-9]{{1,3}},\s?[0-9]{{1,3}},\s?[0-9]{{1,3}}(?:,\s?[0-9]{{1,3}})?|[a-z]+)
     [\]\)\}}\>']?,\s*)
  (?:[\[\(\{{\<']?
     (?<COLOR2>[0-9]{{1,3}},\s?[0-9]{{1,3}},\s?[0-9]{{1,3}}(?:,\s?[0-9]{{1,3}})?|[a-z]+)
     [\]\)\}}\>']?,\s*)?
  (?<STYLE>{ string.Join("|", Enum.GetNames(typeof(GradientBrush.BrushStyle))) })$";
        private static readonly Regex reSplitter = new Regex(pattern, RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        private PropertyDescriptorCollection GradientProps = null;
        private PropertyDescriptorCollection SolidProps = null;

        #region Override Methods
        //! @cond DOXYGENHIDE

        public override bool GetPropertiesSupported(ITypeDescriptorContext context) => true;

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            //The visible properties the user is allowed to modify is different for Solid vs all the other styles.
            if (GradientProps == null)
            {
                var props = TypeDescriptor.GetProperties(typeof(GradientBrush)).Sort(new[] { "Style", "Color1", "Color2", "GammaCorrection" }).Cast<PropertyDescriptor>().Where(p => p.IsBrowsable).ToArray();
                GradientProps = new PropertyDescriptorCollection(props).Sort(new[] { "Style", "Color1", "Color2", "GammaCorrection" });
                SolidProps = new PropertyDescriptorCollection(new[] { props[0], props[1] }).Sort(new[] { "Style", "Color1" });
            }

            return ((GradientBrush)value).Style == GradientBrush.BrushStyle.Solid ? SolidProps : GradientProps;
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) => sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            if (!(value is string str)) return base.ConvertFrom(context, culture, value);
            str = str.Trim();
            if (str.Length == 0) return (object)null;

            Match m = reSplitter.Match(str);
            if (!m.Success) return (object)null;

            var color2 = ColorFromString(m.Groups["COLOR2"].Value);
            if (color2 == Color.Empty) color2 = new GradientBrush(GetHost(context)).Color2;

            return new GradientBrush(GetHost(context))
            {
                Style = (GradientBrush.BrushStyle)Enum.Parse(typeof(GradientBrush.BrushStyle), m.Groups["STYLE"].Value, true),
                Color1 = ColorFromString(m.Groups["COLOR1"].Value),
                Color2 = color2,
                GammaCorrection = false
            };
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => destinationType == typeof(InstanceDescriptor) || base.CanConvertTo(context, destinationType);
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == (Type)null) throw new ArgumentNullException(nameof(destinationType));
            if (destinationType == typeof(string))
            {
                if (!(value is GradientBrush props)) return (object)"(none)";
                return ColorToString(props.Color1) + (props.Style == 0 ? "" : ", " + ColorToString(props.Color2)) + ", " + props.Style.ToString();
            }

            if (destinationType == typeof(InstanceDescriptor) && value is GradientBrush)
            {
                GradientBrush props = (GradientBrush)value;

                MemberInfo constructor = (MemberInfo)typeof(GradientBrush).GetConstructor(new[] { typeof(Control), typeof(Color), typeof(Color), typeof(GradientBrush.BrushStyle), typeof(bool) });
                if (constructor != (MemberInfo)null) return (object)new InstanceDescriptor(constructor, new object[] { GetHost(context), props.Color1, props.Color2, props.Style, props.GammaCorrection });
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }

        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context) => true;

        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            if (propertyValues == null) throw new ArgumentNullException(nameof(propertyValues));

            object obj1 = propertyValues[(object)"Color1"];
            object obj2 = propertyValues[(object)"Color2"];
            object obj3 = propertyValues[(object)"Style"];
            object obj4 = propertyValues[(object)"GammaCorrection"];

            var defalt = new GradientBrush(GetHost(context));

            if (obj1 == null) obj1 = defalt.Color1;
            if (obj2 == null) obj2 = defalt.Color2;
            if (obj3 == null) obj3 = defalt.Style;
            if (obj4 == null) obj4 = defalt.GammaCorrection;

            if (!(obj1 is Color) || !(obj2 is Color) || !(obj3 is GradientBrush.BrushStyle) || !(obj4 is bool))
                throw new ArgumentException("One or more entries are not valid in the IDictionary parameter. Verify that all values match up to the object's properties.");

            return (object)new GradientBrush(GetHost(context), (Color)obj1, (Color)obj2, (GradientBrush.BrushStyle)obj3, (bool)obj4);
        }

        //! @endcond
        #endregion

        private static Control GetHost(ITypeDescriptorContext context)
        {
            //Control host = ((System.Windows.Forms.PropertyGridInternal.GridEntry)context).DesignerHost.RootComponent as Control;
            var pi = context?.GetType().GetProperty("DesignerHost", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            var dh = pi?.GetValue(context) as IDesignerHost;
            return dh?.RootComponent as Control;
        }
        private static string ColorToString(Color c) => c.IsKnownColor ? c.Name : c.IsNamedColor ? $"'{c.Name}'" : c.A < byte.MaxValue ? $"({c.A},{c.R},{c.G},{c.B})" : $"({c.R},{c.G},{c.B})";
        private static Color ColorFromString(string s)
        {
            //splitting on so many grouping chars is not really necessary since the Regex pattern removes all of them anyway... Safety feature?
            //var items = s.Split(new[] { '(', ')', '[', ']', '{', '}', '<', '>', '\'', ',', ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            var items = s.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (items.Length == 0) return Color.Empty;
            if (items.Length == 1) return Color.FromName(items[0]);
            if (items.Length == 3)
            {
                if (!int.TryParse(items[0], out int r)) return Color.Empty;
                if (!int.TryParse(items[1], out int g)) return Color.Empty;
                if (!int.TryParse(items[2], out int b)) return Color.Empty;
                return Color.FromArgb(r, g, b);
            }
            if (items.Length == 4)
            {
                if (!int.TryParse(items[0], out int a)) return Color.Empty;
                if (!int.TryParse(items[1], out int r)) return Color.Empty;
                if (!int.TryParse(items[2], out int g)) return Color.Empty;
                if (!int.TryParse(items[3], out int b)) return Color.Empty;
                return Color.FromArgb(a, r, g, b);
            }
            return Color.Empty;
        }
    }

    //Create nice icon on property header
    internal class GradientBrushEditor : UITypeEditor
    {
        #region Override Methods
        //! @cond DOXYGENHIDE

        public override bool GetPaintValueSupported(ITypeDescriptorContext context) => true;
        public override void PaintValue(PaintValueEventArgs e)
        {
            var p = e.Value as GradientBrush;
            if (p == null) return;

            using (var br = p.GetBrush(e.Bounds))
                e.Graphics.FillRectangle(br, e.Bounds);
        }

        //! @endcond
        #endregion
    }
}
