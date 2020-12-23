using System;
using System.IO;
using System.Linq;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Useful common extensions.
    /// </summary>
    public static class CommonExtensions
    {
        /// <summary>
        /// Handy way to get an manifest resource stream from an assembly that 'type' resides in.
        /// This will not access Project or Form resources (e.g. *.resources).
        /// </summary>
        /// <param name="t">Type whose assembly contains the manifest resources to search.</param>
        /// <param name="name">The unique trailing part of resource name to search. Generally the filename.ext part.</param>
        /// <returns>Found resource stream or null if not found. It's up to the caller to load it into the appropriate object. Generally Image.FromStream(s)</returns>
        /// <remarks>
        /// See ImageAttribute regarding access of any image resource from anywhere.
        /// </remarks>
        public static Stream GetManifestResourceStream(this Type t, string name) => t.Assembly.GetManifestResourceStream(t.Assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(name, StringComparison.OrdinalIgnoreCase)) ?? "NULL");
    }
}
