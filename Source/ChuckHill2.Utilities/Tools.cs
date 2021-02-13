using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using ChuckHill2.Extensions;

namespace ChuckHill2
{
    /// <summary>
    /// A collection of functions that are not extensions.
    /// </summary>
    public static class StringEx
    {
        /// <summary>
        /// Create a formatter func from an interpolated format string where all the variables come from a single class.
        /// Variables are case-insensitive, whitespace trimmed, and handles unknown/missing properties.
        /// Literal "\r", "\n", "\t", and "\\" strings are converted to their character equivalants.
        /// </summary>
        /// <remarks>
        /// Internally, C# compiler converts interpolation string into a string.Format call. We do the same here at runtime.
        /// </remarks>
        /// <typeparam name="T">Type of class containing the interpolated public properties.</typeparam>
        /// <param name="formatString">Interpolated format string</param>
        /// <returns>Lambda expression Func<T,string>()</returns>
        /// <see cref="https://stackoverflow.com/questions/56184406/string-expression-to-c-sharp-function-delegate"/>
        public static Func<T, string> GetFormatter<T>(string formatString)
        {
            var args = new List<string>();
            #region formatString fixup
            //Parse out args and replace with string.Format index.
            int index = 0;
            formatString = InterpolationParser.Replace(formatString, (ev) =>
            {
                var a = ev.Groups[1].Value;
                args.Add(a);
                var v = ev.Value.Replace(a, (index++).ToString());
                return v;
            });

            //Replace escaped newlines
            char prev_ch = '\0';
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < formatString.Length; i++)
            {
                char ch = formatString[i];
                if (ch == 'r' && prev_ch == '\\') { ch = '\r'; prev_ch = ch; sb.Length -= 1; sb.Append(ch); continue; }
                if (ch == 'n' && prev_ch == '\\') { ch = '\n'; prev_ch = ch; sb.Length -= 1; sb.Append(ch); continue; }
                if (ch == 't' && prev_ch == '\\') { ch = '\t'; prev_ch = ch; sb.Length -= 1; sb.Append(ch); continue; }
                if (ch == '\\' && prev_ch == '\\') { prev_ch = '\0'; sb.Length -= 1; sb.Append(ch); continue; }
                prev_ch = ch;
                sb.Append(ch);
            }
            formatString = sb.ToString();

            //Validate parameters - trim whitespace, fix case, and handle invalid property names.
            var properties = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            foreach (var pi in typeof(T).GetProperties()) { properties.Add(pi.Name, pi.Name); }
            for (int i = 0; i < args.Count; i++)
            {
                args[i] = properties.TryGetValue(args[i].Trim(), out string v) ? v : $"({typeof(T).Name}.{args[i].Trim()} missing)";
            }
            #endregion

            var argumentExpression = Expression.Parameter(typeof(T));
            var formatParamsArrayExpr = Expression.NewArrayInit(typeof(object), args.Select(arg =>
                arg.EndsWith(" missing)") ?
                    (Expression)Expression.Constant(arg) :
                    (Expression)Expression.Convert(Expression.Property(argumentExpression, arg), typeof(object))
                ));
            var formatExpr = Expression.Call(
                StringFormatMethod,
                Expression.Constant(formatString, typeof(string)), formatParamsArrayExpr);
            var resultExpr = Expression.Lambda<Func<T, string>>(
                formatExpr,
                argumentExpression); // Expression<Func<Foo, string>> a = (Foo x) => string.Format("{0}-{1}", x.Id, x.Description);

            return resultExpr.Compile(); //(cache) => System.String.Format(formatString, args.ToArray());
        }

        private static readonly Regex InterpolationParser = new Regex(@"\{([^\{\}:]+)[:\}]", RegexOptions.Compiled);
        private static readonly MethodInfo StringFormatMethod = typeof(string).GetMethod("Format", BindingFlags.Static | BindingFlags.Public, null, new[] { typeof(string), typeof(object[]) }, null);

        //FOR DEBUGGING
        //private static readonly MethodInfo StringFormatMethod = typeof(Tool).GetMethod("MyStringFormatDebug", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(string), typeof(object[]) }, null);
        //private static string MyStringFormatDebug(string format, object[] args)
        //{
        //    string result = string.Format((IFormatProvider)null, format, args);
        //    return result;
        //}
    }

    public static class GDI
    {
        [DllImport("Shell32.dll")] private static extern int SHDefExtractIconW([MarshalAs(UnmanagedType.LPWStr)] string pszIconFile, int iIndex, int uFlags, out IntPtr phiconLarge, /*out*/ IntPtr phiconSmall, int nIconSize);
        /// <summary>Returns an icon of the specified size that is contained in the specified file.</summary>
        /// <param name="filePath">The path to the file that contains the icon.</param>
        /// <param name="size">Size of icon to retrieve.</param>
        /// <returns>The Icon representation of the image that is contained in the specified file. Must be disposed after use.</returns>
        /// <exception cref="System.ArgumentException">The parameter filePath does not indicate a valid file.-or- indicates a Universal Naming Convention (UNC) path.</exception>
        /// <remarks>
        /// Icons files contain multiple sizes and bit-depths of an image ranging from 16x16 to 256x256 in multiples of 8. Example: 16x16, 24x24, 32x32, 48x48, 64x64, 96x96, 128*128, 256*256.
        /// Icon.ExtractAssociatedIcon(filePath), retrieves only the 32x32 icon, period. This method will use the icon image that most closely matches the specified size and then resizes it to fit the specified size.
        /// </remarks>
        /// <see cref="https://docs.microsoft.com/en-us/windows/win32/api/shlobj_core/nf-shlobj_core-shdefextracticonw"/>
        /// <see cref="https://devblogs.microsoft.com/oldnewthing/20140501-00/?p=1103"/>
        public static Icon ExtractAssociatedIcon(string filePath, int size)
        {
            const int SUCCESS = 0;
            IntPtr hIcon;

            if (SHDefExtractIconW(filePath, 0, 0, out hIcon, IntPtr.Zero, size) == SUCCESS)
            {
                return Icon.FromHandle(hIcon);
            }

            return null;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }
        [DllImport("dwmapi.dll")] private static extern int DwmExtendFrameIntoClientArea(IntPtr hWnd, ref MARGINS pMarInset);
        [DllImport("dwmapi.dll")] private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);
        /// <summary>
        /// Enable dropshadow to a borderless form. Unlike forms with borders, forms with FormBorderStyle.None have no dropshadow. 
        /// </summary>
        /// <param name="form">Borderless form to add dropshadow to. Must be called AFTER form handle has been created. see Form.Created or Form.Shown events.</param>
        /// <exception cref="InvalidOperationException">Must be called AFTER the form handle has been created.</exception>
        /// <see cref="https://stackoverflow.com/questions/60913399/border-less-winform-form-shadow/60916421#60916421"/>
        /// <remarks>
        /// This method does nothing if the form does not have FormBorderStyle.None.
        /// </remarks>
        public static void ApplyShadows(Form form)
        {
            if (form.FormBorderStyle != FormBorderStyle.None) return;
            if (Environment.OSVersion.Version.Major < 6) return;
            if (!form.IsHandleCreated) throw new InvalidOperationException("Must be called AFTER the form handle has been created.");

            var v = 2;
            DwmSetWindowAttribute(form.Handle, 2, ref v, 4);

            MARGINS margins = new MARGINS()
            {
                bottomHeight = 1,
                leftWidth = 0,
                rightWidth = 1,
                topHeight = 0
            };

            DwmExtendFrameIntoClientArea(form.Handle, ref margins);
        }
    }

    public static class MathEx
    {
        /// <summary>
        /// Get the minimum value of 2 or more values
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="vals">2 or more values</param>
        /// <returns>Minimum value.</returns>
        public static T Min<T>(params T[] vals)
        {
            T v = vals[0];
            for (int i = 1; i < vals.Length; i++)
            {
                if (Comparer<T>.Default.Compare(vals[i], v) < 0) v = vals[i];
            }

            return v;
        }

        /// <summary>
        /// Get the maximum value of 2 or more values
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="vals">2 or more values</param>
        /// <returns>Maximum value.</returns>
        public static T Max<T>(params T[] vals)
        {
            T v = vals[0];
            for (int i = 1; i < vals.Length; i++)
            {
                if (Comparer<T>.Default.Compare(vals[i], v) > 0) v = vals[i];
            }

            return v;
        }
    }

    public static class Manifest
    {
        /// <summary>
        /// Get manifest resource if it doesn't already exist in cache.
        /// Used in load-on-demand readonly properties;
        /// Example:
        /// @code{.cs}
        /// private Image _myImage = null;
        /// public Image MyImage => Manifest.Resource(_myImage, this.GetType(), "MyImage.png");
        /// @endcode
        /// </summary>
        /// <param name="cache">Cache containing resource</param>
        /// <param name="resourceAssembly">Assembly containing resource.</param>
        /// <param name="resourceName">Relative name of resource in assembly</param>
        /// <returns>Resource object</returns>
        public static Image Resource(ref Image cache, Type resourceAssembly, string resourceName)
        {
            if (cache == null || cache.PixelFormat == PixelFormat.DontCare)
            {
                using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
                    cache = Image.FromStream(stream);
            }

            return cache;
        }

        /// <summary>
        /// Get manifest resource if it doesn't already exist in cache.
        /// Used in load-on-demand readonly properties;
        /// Example:
        /// @code{.cs}
        /// private Image _myImage = null;
        /// public Image MyImage => Manifest.Resource(_myImage, this.GetType(), "MyImage.png");
        /// @endcode
        /// </summary>
        /// <param name="cache">Cache containing resource</param>
        /// <param name="resourceAssembly">Assembly containing resource.</param>
        /// <param name="resourceName">Relative name of resource in assembly</param>
        /// <returns>Resource object</returns>
        public static Cursor Resource(ref Cursor cache, Type resourceAssembly, string resourceName)
        {
            if (cache == null || cache.Handle==IntPtr.Zero)
            {
                using (var stream = resourceAssembly.GetManifestResourceStream(resourceName))
                    cache = new Cursor(stream);
            }

            return cache;
        }

        /// <summary>
        /// Get manifest resource if it doesn't already exist in cache.
        /// Used in load-on-demand readonly properties;
        /// Example:
        /// @code{.cs}
        /// private Image _myImage = null;
        /// public Image MyImage => Manifest.Resource(_myImage, this.GetType(), "MyImage.png");
        /// @endcode
        /// </summary>
        /// <param name="cache">Cache containing resource</param>
        /// <param name="resourceAssembly">Assembly containing resource.</param>
        /// <param name="resourceName">Relative name of resource in assembly</param>
        /// <returns>Resource object</returns>
        public static string Resource(ref string cache, Type resourceAssembly, string resourceName)
        {
            if (cache == null)
            {
                using (var stream = new StreamReader(resourceAssembly.GetManifestResourceStream(resourceName)))
                    cache = stream.ReadToEnd();
            }

            return cache;
        }
    }
}
