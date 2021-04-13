//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ImageAttribute.cs" company="Chuck Hill">
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
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Resources;

namespace ChuckHill2
{
    /// <summary>
    /// Associate an image with any application element. 
    /// </summary>
    /// <remarks>
    /// <H3>Definitions</H3>
    /// <H4>Assembly Manifest resources</H4>
    /// This is the sole location within the assembly that.NET resources reside.The
    /// object resource is included as named binary object at compile time.The name of
    /// the resource within the manifest is namespace.folderpath.filename.ext where
    /// 'folderpath' is relative to the project folder and '\\' delimiters are converted
    /// to '.'.
    /// 
    /// <H4>Project resources</H4>
    /// Object resources specific to the entire project.These object resources are
    /// stored in a resx(special XML) file and compiled into 'Resources.resources' file
    /// which is in turn embedded into the assembly manifest as
    /// namespace.folderpath.Resources.resources.The names of the objects stored in this
    /// file consist of just the filename(no extension) where illegal C# variable name
    /// characters are converted to '_'.
    /// 
    /// <H4>Form resources</H4>
    /// Object resources specific to the Form Designer.The format is the same as project
    /// resources but the name in the assembly manifest is
    /// namespace.folderpath.classname.resources. Be careful. Manually entered items may
    /// be overwritten by the designer.
    /// 
    /// <H3>ImageAttribute</H3>
    /// Attribute can be applied to any application element. It's purpose is to
    /// associate any object member(class, fields, properties, enums, etc) with an image
    /// at compile time. It's up to the caller to make use of it. The image file formats
    /// supported are: bmp, png, gif, jpg, tif, emf, wmf, ico
    /// 
    /// There are 3 constructors, 2 public properties, and nothing else.
    /// 
    /// **ImageAttribute**(*string filename, object tag = null*)<br />
    /// Retrieve an image from an absolute path or relative path where the path is
    /// relative to where the execuatable resides or the current working directory.
    /// 
    /// **ImageAttribute**(*Type t, object tag = null*)<br />
    /// Retrieve a default image for the type from the defining assembly. Within the
    /// assembly manifest, it will be named namespace.classname.ext where 'ext' can be
    /// any image extension; Or within any of the assembly manifest.resources, that
    /// contains 'classname'.
    /// 
    /// **ImageAttribute**(*Type t, string name, object tag = null*)<br />
    /// Retrieve an image from the defining assembly with a specific case-sensitive
    /// name. Within the assembly manifest, it will be named
    /// namespace.foldername.name.ext where 'namespace' is optional and ignored,
    /// 'foldername' and 'ext' are only needed to narrow the search in case of
    /// duplicates.Only 'name' is required; Or within any of the assembly
    /// manifest.resources, that contains 'name'.
    /// 
    /// Property *Image* **Image** { get; }<br />
    /// The image retrieved or null if not found.The image is not loaded until the
    /// caller actually attempts to retrieve it. Only one instance is created unless it
    /// is disposed and the caller re-retrieves the image.
    /// 
    /// Property *object* **Tag** { get; }<br />
    /// Tag is just an optional unique identifier in case an object member has multiple
    /// ImageAttribute's declared.
    /// </remarks>
    [AttributeUsage(AttributeTargets.All, AllowMultiple = true)]
    public class ImageAttribute : Attribute
    {
        private readonly string _imageFile;
        private readonly Type _type;
        private readonly string _name;
        private readonly object _tag;

        private bool __loaded = false; //don't attempt to load again if we didn't get it the first time.
        private Image __image;

        /// <summary>
        ///  The load-on-demand image associated with this attribute. If disposed by caller and retrieved again, it will be recreated.
        ///  If not found or invalid, it quietly returns null. It is up to the caller to resize as necessary.
        /// </summary>
        public Image Image
        {
            get
            {
                if ((!__loaded && __image == null) || __image.PixelFormat == PixelFormat.Undefined)
                {
                    if (_imageFile != null) __image = GetFileImage(_imageFile);
                    else __image = GetResourceImage(_type, _name);
                    __loaded = true;
                }

                return __image;
            }
        }

        /// <summary>
        /// An optional unique identifier in case an object member has multiple ImageAttribute's declared.
        /// </summary>
        public object Tag { get { return _tag; } }

        ~ImageAttribute()
        {
            __image?.Dispose();
            __image = null;
        }

        /// <summary>
        /// Initializes a new ImageAttribute object with an image from a specified file.
        /// If the path is relative, it will assume that it is relative from the running executable.
        /// If not found, then it will assume that it is relative from the current working directory.
        /// </summary>
        /// <param name="imageFile">The name of  the file path that contains the image. </param>
        /// <param name="tag">An optional unique identifier in case an object member has multiple ImageAttribute's declared.</param>
        public ImageAttribute(string imageFile, object tag = null)
        {
            _imageFile = imageFile;
            _tag = tag;
        }

        /// <summary>
        /// Initializes a new ImageAttribute object based on an image that is embedded as a manifest resource in a specified assembly.
        /// </summary>
        /// <param name="t">A <see cref="T:System.Type" /> whose defining assembly is searched for the image resource. 
        ///    The icon/image name must be same name as the object name. The embedded image file may have any image extension.  
        /// </param>
        /// <param name="tag">An optional unique identifier in case an object member has multiple ImageAttribute's declared.</param>
        public ImageAttribute(Type t, object tag = null)
        {
            _type = t;
            _tag = tag;
        }

        /// <summary>
        /// Initializes a new ImageAttribute object based on an image that is embedded as a resource in a specified assembly.
        /// </summary>
        /// <param name="t">A <see cref="T:System.Type" /> whose defining assembly is searched for the image resource. </param>
        /// <param name="name">
        ///    The name of the embedded image resource without any namespace.class prefix.
        ///    The name may contain a valid image or icon extension in order to preferentially search for those resources.
        ///    This resource may also reside in any embedded '.resources' object (Resources.resx) or embedded within the project directly.
        ///    Using an image extension just constrains the search. Otherwise some confusion picking the image may occur
        ///    if the named image/icon resides in multiple embedded '.resources' objects.
        /// </param>
        /// <param name="tag">An optional unique identifier in case an object member has multiple ImageAttribute's declared.</param>
        public ImageAttribute(Type t, string name, object tag = null)
        {
            _type = t;
            _name = name;
            _tag = tag;
        }

        /// <summary>Indicates whether the specified object is an ImageAttribute object and is identical to this ImageAttribute object.</summary>
        /// <param name="value">The <see cref="T:System.Object" /> to test. </param>
        /// <returns>This method returns True if 'value'  is both an ImageAttribute object and are identical.</returns>
        public override bool Equals(object value)
        {
            var bma = value as ImageAttribute;
            if (bma == null) return false;
            return this.Image == bma.Image; 
        }

        /// <summary>Gets a hash code for this ImageAttribute object.</summary>
        /// <returns>The hash code for this ImageAttribute object.</returns>
        public override int GetHashCode() => base.GetHashCode();

        private static readonly string[] ImageExtensions = new[] { ".bmp", ".png", ".gif", ".jpg", ".tif", ".emf", ".wmf", ".ico" }; //All GDI+ decoders in order of frequency of use. (I think!)

        private static Image GetResourceImage(Type t, string name=null)
        {
            Image result = null;
            var asm = t.Assembly;

            if (string.IsNullOrEmpty(name)) name = t.Name;
            else
            {
                var asmName = new AssemblyName(asm.FullName).Name;
                if (name.StartsWith(asmName + ".")) name = name.Substring(asmName.Length + 1);
            }

            var hasExtension = ImageExtensions.Contains(Path.GetExtension(name));
            if (hasExtension)
            {
                var resname = asm.GetManifestResourceNames().FirstOrDefault(m => m.EndsWith(name, StringComparison.InvariantCultureIgnoreCase));
                if (resname != null)
                {
                    result = GetManifestImage(asm, resname);
                }
                else
                {
                    result = GetResourceImage(t, Path.GetFileNameWithoutExtension(name)); //try again without extension.
                }
            }
            else //no extension. find name with any image extension
            {
                var resnames = asm.GetManifestResourceNames().Where(m => ImageExtensions.Contains(Path.GetExtension(m)));
                var ext = ImageExtensions.FirstOrDefault(e => resnames.FirstOrDefault(m => m.EndsWith("." + name + e)) != null);
                if (ext != null)
                {
                    name += ext;
                    var resname = resnames.FirstOrDefault(m => m.EndsWith(name, StringComparison.InvariantCultureIgnoreCase));
                    result = GetManifestImage(asm, resname);
                }
            }

            //If not yet found, search all the embedded '.resources'
            if (result == null)
            {
                foreach (var resname in asm.GetManifestResourceNames().Where(m => m.EndsWith(".resources")))
                {
                    var rm = new ResourceManager(resname.Substring(0, resname.Length - 10), asm);
                    var o = rm.GetObject(name);
                    if (o is Image)
                    {
                        result = o as Image;
                        break;
                    }
                    if (o is Icon)
                    {
                        result = ((Icon)o).ToBitmap();
                        break;
                    }
                }
            }

            return result;
        }

        private static Image GetManifestImage(Assembly asm, string resname)
        {
            using (var s = asm.GetManifestResourceStream(resname))
            {
                if (IsIcon(s))
                    try { using (var ico = new Icon(s)) return ico.ToBitmap(); } catch { s.Position = 0; }
                else
                    try { return Image.FromStream(s); } catch { s.Position = 0; }
            }

            return null;
        }
        private static bool IsIcon(Stream s)
        {
            // https://en.wikipedia.org/wiki/ICO_(file_format)
            var i = s.Position;
            s.Position = 0;
            var f0 = ReadShort(s);
            var f1 = ReadShort(s);
            s.Position = i;
            return (f0 == 0 && (f1 == 1 || f1 == 2));
        }
        private static short ReadShort(Stream s)
        {
            var b = new byte[2];
            s.Read(b, 0, 2);
            return BitConverter.ToInt16(b, 0);
        }

        private static Image GetFileImage(string fn)
        {
            string fn2 = fn;

            if (!Path.IsPathRooted(fn))
            {
                fn2 = GetExistingPath(Path.Combine(Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName), fn));
                if (fn2 == null) fn2 = GetExistingPath(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), fn));
                if (fn2 == null) fn2 = GetExistingPath(Path.Combine(Path.GetDirectoryName(Assembly.GetCallingAssembly().Location), fn));
                if (fn2 == null) fn2 = GetExistingPath(Path.Combine(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), fn));  //will be different from GetCurrentProcess() if this is not running in the main AppDomain.
            }

            if (fn2 == null) fn2 = GetExistingPath(Path.GetFullPath(fn));
            if (fn2 == null) return null;

            if (fn2.EndsWith(".ico", StringComparison.OrdinalIgnoreCase))
                try { using (var ico = new Icon(fn2)) return ico.ToBitmap(); } catch {}

            try { return Image.FromFile(fn2); } catch {}

            return null;
        }

        private static string GetExistingPath(string fn)
        {
            //Path.GetExtension will return a false positive if there is no extension AND there is a '.' in the name part.
            var ext = Path.GetExtension(fn);
            if (ext.Length != 4) ext = string.Empty; // This is mitigation, but not a 100% fix. We are only concerned with 3-letter extensions.

            if (fn.Contains("..\\")) fn = Path.GetFullPath(fn);

            if (string.IsNullOrEmpty(ext) || !File.Exists(fn)) //add an extension
            {
                var ext2 = ImageExtensions.FirstOrDefault(e => File.Exists(fn + e));
                fn = ext2 == null ? null : fn += ext2;
            }

            return fn;
        }
    }
}
