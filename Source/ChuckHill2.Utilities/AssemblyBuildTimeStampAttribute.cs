using System;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Defines a compile time-stamp custom attribute for an assembly manifest in the release configuration.
    /// In the debug cofiguration, this just returns the current datetime. This way we get an accurate build date for release but eliminating constant rebuilds during debugging.
    /// </summary>
    /// <remarks>
    /// This messes up the new deterministic nature of assemblies, but if one uses this, then knowing when this assembly was built is more important.
    /// An alternatitive is to modify the project file and add  "<Deterministic>true</Deterministic>" to the debug configuration
    /// and "<Deterministic>false</Deterministic>" to the release configuration. Then use extension Assembly.PeTimeStamp() to retrieve the build date.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Assembly, Inherited = false)]
    public sealed class AssemblyBuildTimeStampAttribute : Attribute
    {
        #pragma warning disable CS0649 //Field is never assigned to, and will always have its default value 0
        private readonly long utcNowTicks;

        /// <summary>
        /// Constructs an instance of this attribute class using the current  code compile-time time-stamp value.
        /// </summary>
        public AssemblyBuildTimeStampAttribute()
        {
        #if !DEBUG
            utcNowTicks = DateTime.UtcNow.Ticks;
        #endif
        }

        /// <summary>
        /// Gets the source code compile-time time-stamp value.
        /// </summary>
        public string TimeStamp
        {
            get
            {
                if (utcNowTicks==0) return DateTime.Now.ToString("g");
                return new DateTime(utcNowTicks, DateTimeKind.Utc).ToLocalTime().ToString("g");
            }
        }
    }
}
