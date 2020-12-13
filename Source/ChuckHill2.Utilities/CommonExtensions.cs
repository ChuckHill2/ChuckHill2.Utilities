using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuckHill2.Utilities
{
    public static class CommonExtensions
    {
        /// <summary>
        /// Handy way to get a manifest resource stream from an assembly that 'type' resides in.
        /// </summary>
        /// <param name="t">Type whose assembly contains the manifest resources to search.</param>
        /// <param name="name">The unique trailing part of resource name to search. Generally the filename.ext part.</param>
        /// <returns>Found resource stream or null if not found. It's up to the caller to load it into the appropriate object. Generally Image.FromStream(s)</returns>
        public static Stream GetManifestResourceStream(this Type t, string name) => t.Assembly.GetManifestResourceStream(t.Assembly.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(name, StringComparison.OrdinalIgnoreCase)) ?? "NULL");
    }
}
