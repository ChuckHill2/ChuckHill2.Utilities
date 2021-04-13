//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="AssemblyBuildTimeStampAttribute.cs" company="Chuck Hill">
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

namespace ChuckHill2
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
