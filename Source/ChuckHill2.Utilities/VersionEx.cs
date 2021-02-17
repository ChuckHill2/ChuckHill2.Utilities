using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;
using System.Globalization;
using System;
using System.Xml.Serialization;

namespace ChuckHill2
{
    /// <summary>
    /// Represents the version number of an assembly, operating system, or the common language runtime. This class cannot be inherited.
    /// </summary>
    /// <remarks>
    /// class System.Version cannot be serialized because its properties are readonly!
    /// This is serializable and implicitly castable to/from System.Version.
    /// </remarks>
    [Serializable]
    [XmlInclude(typeof(Version))] //necessary when using implicit operators
    public sealed class VersionEx : ICloneable, IComparable, IComparable<VersionEx>, IComparable<Version>, IEquatable<VersionEx>, IEquatable<Version>
    {
        // AssemblyName depends on the order staying the same
        private int _Major;
        private int _Minor;
        private int _Build = -1;
        private int _Revision = -1;

        /// <summary>
        /// Initializes a new instance of the VersionEx class with the specified major, minor, build, and revision numbers.
        /// </summary>
        /// <param name="major">The major version number. </param>
        /// <param name="minor">The minor version number. </param>
        /// <param name="build">The build number. </param>
        /// <param name="revision">The revision number. </param>
        /// <exception cref="System.ArgumentOutOfRangeException"><paramref name="major" />, <paramref name="minor" />, <paramref name="build" />, or <paramref name="revision" /> is less than zero. </exception>
        public VersionEx(int major, int minor, int build = -1, int revision = -1) 
        {
            if (major < 0) throw new ArgumentOutOfRangeException("major");
            if (minor < 0) throw new ArgumentOutOfRangeException("minor");
            if (build < -1) throw new ArgumentOutOfRangeException("build");
            if (revision < -1) throw new ArgumentOutOfRangeException("revision");
            
            _Major = major;
            _Minor = minor;
            _Build = build;
            _Revision = revision;
        }

        /// <summary>
        /// Initializes a new instance of the VersionEx class using the specified string.
        /// </summary>
        /// <param name="version">A string containing the major, minor, build, and revision numbers, where each number is delimited with a period character ('.'). </param>
        /// <exception cref="System.ArgumentException"><paramref name="version" /> has fewer than two components or more than four components. </exception>
        /// <exception cref="System.ArgumentNullException"><paramref name="version" /> is <see langword="null" />. </exception>
        /// <exception cref="System.ArgumentOutOfRangeException">A major, minor, build, or revision component is less than zero. </exception>
        /// <exception cref="System.FormatException">At least one component of <paramref name="version" /> does not parse to an integer. </exception>
        /// <exception cref="System.OverflowException">At least one component of <paramref name="version" /> represents a number greater than <see cref="System.Int32.MaxValue" />.</exception>
        public VersionEx(String version) 
        {
            Version v = Version.Parse(version);
            _Major = v.Major;
            _Minor = v.Minor;
            _Build = v.Build;
            _Revision = v.Revision;
        }

        /// <summary>
        /// Initializes a new instance of the VersionEx class.
        /// </summary>
        public VersionEx() 
        {
            _Major = 0;
            _Minor = 0;
        }

        /// <summary>
        /// Initializes a new instance of the VersionEx class using an instance of the <see cref="System.Version" /> class.
        /// </summary>
        /// <param name="v">An instance of the <see cref="System.Version" /> class.</param>
        public VersionEx(Version v)
        {
            _Major = v.Major;
            _Minor = v.Minor;
            _Build = v.Build;
            _Revision = v.Revision;
        }

        /// <summary>Gets the value of the major component of the version number for this object.</summary>
        [XmlAttribute] public int Major { get { return _Major; } set { _Major = value; } }
        /// <summary>Gets the value of the minor component of the version number for this object.</summary>
        [XmlAttribute] public int Minor { get { return _Minor; } set { _Minor = value; } }
        /// <summary>Gets the value of the build component of the version number for this object.</summary>
        /// <returns>The build number, or -1 if the build number is undefined.</returns>
        [XmlAttribute] public int Build { get { return _Build; } set { _Build = value; } }
        /// <summary>Gets the value of the revision component of the version number for this object.</summary>
        /// <returns>The revision number, or -1 if the revision number is undefined.</returns>
        [XmlAttribute] public int Revision { get { return _Revision; } set { _Revision = value; } }

        /// <summary>Returns a new VersionEx object whose value is the same as the current VersionEx object.</summary>
        /// <returns>A new <see cref="System.Object" /> whose values are a copy of the current VersionEx object.</returns>
        public Object Clone()
        {
            VersionEx v = new VersionEx();
            v._Major = _Major;
            v._Minor = _Minor;
            v._Build = _Build;
            v._Revision = _Revision;
            return(v);
        }

        /// <summary>
        /// Compares this object to a specified Version or VersionEx object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">A Version or VersionEx object to compare against or <see langword="null" />.</param>
        /// <returns>
        ///   A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///   *  -1 if this is before the specified object.
        ///   *   0 if the content of the two objects are equal.
        ///   *  +1 if this comes after the specified object or specified object is <see langword="null" /> or not a Version or VersionEx object..
        /// </returns>
        public int CompareTo(Object version)
        {
            if (version is VersionEx)
            {
                VersionEx v = version as VersionEx;
                if (this._Major != v._Major) return this._Major > v._Major ? 1 : -1;
                if (this._Minor != v._Minor) return this._Minor > v._Minor ? 1 : -1;
                if (this._Build != v._Build) return this._Build > v._Build ? 1 : -1;
                if (this._Revision != v._Revision) return this._Revision > v._Revision ? 1 : -1;
                return 0;
            }
            if (version is Version)
            {
                Version v = version as Version;
                if (this._Major != v.Major) return this._Major > v.Major ? 1 : -1;
                if (this._Minor != v.Minor) return this._Minor > v.Minor ? 1 : -1;
                if (this._Build != v.Build) return this._Build > v.Build ? 1 : -1;
                if (this._Revision != v.Revision) return this._Revision > v.Revision ? 1 : -1;
                return 0;
            }
            return 1;
        }

        /// <summary>
        /// Compares this object to a specified VersionEx object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">A Version object to compare against or <see langword="null" />.</param>
        /// <returns>
        ///   A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///   *  -1 if this is before the specified object.
        ///   *   0 if the content of the two objects are equal.
        ///   *  +1 if this comes after the specified object or specified object is <see langword="null" />.
        public int CompareTo(VersionEx v)
        {
            if (this._Major != v._Major) return this._Major > v._Major ? 1 : -1;
            if (this._Minor != v._Minor) return this._Minor > v._Minor ? 1 : -1;
            if (this._Build != v._Build) return this._Build > v._Build ? 1 : -1;
            if (this._Revision != v._Revision) return this._Revision > v._Revision ? 1 : -1;
            return 0;
        }

        /// <summary>
        /// Compares this object to a specified Version object and returns an indication of their relative values.
        /// </summary>
        /// <param name="value">A Version object to compare against or <see langword="null" />.</param>
        /// <returns>
        ///   A signed integer that indicates the relative values of the two objects, as shown in the following table.
        ///   *  -1 if this is before the specified object.
        ///   *   0 if the content of the two objects are equal.
        ///   *  +1 if this comes after the specified object or specified object is <see langword="null" />.
        public int CompareTo(Version v)
        {
            if (this._Major != v.Major) return this._Major > v.Major ? 1 : -1;
            if (this._Minor != v.Minor) return this._Minor > v.Minor ? 1 : -1;
            if (this._Build != v.Build) return this._Build > v.Build ? 1 : -1;
            if (this._Revision != v.Revision) return this._Revision > v.Revision ? 1 : -1;
            return 0;
        }

        /// <summary>
        /// Test if this and the specified object are equal.
        /// </summary>
        /// <param name="obj">An object to compare against or <see langword="null" />. </param>
        /// <returns><see langword="true" /> if the content of both are equal</returns>
        public override bool Equals(Object obj)
        {
            if (obj is VersionEx)
            {
                VersionEx v = obj as VersionEx;
                return ((this._Major == v._Major) &&
                        (this._Minor == v._Minor) &&
                        (this._Build == v._Build) &&
                        (this._Revision == v._Revision));
            }

            if (obj is Version)
            {
                Version v = obj as Version;
                return ((this._Major == v.Major) &&
                        (this._Minor == v.Minor) &&
                        (this._Build == v.Build) &&
                        (this._Revision == v.Revision));
            }

            return false;
        }

        /// <summary>
        /// Test if this and the specified object are equal.
        /// </summary>
        /// <param name="v">An object to compare against or <see langword="null" />. </param>
        /// <returns><see langword="true" /> if the content of both are equal</returns>
        public bool Equals(VersionEx v)
        {
            if (v == (VersionEx)null) return false;
            return ((this._Major == v._Major) &&
                    (this._Minor == v._Minor) &&
                    (this._Build == v._Build) &&
                    (this._Revision == v._Revision));
        }

        /// <summary>
        /// Test if this and the specified object are equal.
        /// </summary>
        /// <param name="v">An object to compare against or <see langword="null" />. </param>
        /// <returns><see langword="true" /> if the content of both are equal</returns>
        public bool Equals(Version v)
        {
            if (v == (Version)null) return false;
            return ((this._Major == v.Major) &&
                    (this._Minor == v.Minor) &&
                    (this._Build == v.Build) &&
                    (this._Revision == v.Revision));
        }

        /// <summary>Returns a hash code for this object.</summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => 0 | (this._Major & 15) << 28 | (this._Minor & (int)byte.MaxValue) << 20 | (this._Build & (int)byte.MaxValue) << 12 | this._Revision & 4095;

        /// <summary>Converts the value of this object to its equivalent string representation. ex. 1.0.2.34</summary>
        public override String ToString() 
        {
            var sb = new StringBuilder(23);
            sb.Append(_Major); sb.Append('.');
            sb.Append(_Minor); sb.Append('.');
            if (_Build == -1) { sb.Length -= 1; return sb.ToString(); }
            sb.Append(_Build); sb.Append('.');
            if (_Revision == -1) { sb.Length -= 1; return sb.ToString(); }
            sb.Append(_Revision);
            return sb.ToString();
        }

        /// <summary>Converts the string representation of a version number to an equivalent VersionEx object.</summary>
        /// <param name="input">A string that contains a version number to convert.</param>
        /// <returns>An object that is equivalent to the version number specified in the <paramref name="input" /> parameter.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="input" /> is <see langword="null" />.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="input" /> has fewer than two or more than four version components.</exception>
        /// <exception cref="System.ArgumentOutOfRangeException">At least one component in <paramref name="input" /> is less than zero.</exception>
        /// <exception cref="System.FormatException">At least one component in <paramref name="input" /> is not an integer.</exception>
        /// <exception cref="System.OverflowException">At least one component in <paramref name="input" /> represents a number that is greater than <see cref="System.Int32.MaxValue" />.</exception>
        public static VersionEx Parse(string input) 
        {
            return new VersionEx(Version.Parse(input));
        }

        /// <summary>
        /// Tries to convert the string representation of a version number to an equivalent <see cref="System.Version" /> object, and returns a value that indicates whether the conversion succeeded.
        /// </summary>
        /// <param name="input">A string that contains a version number to convert or <see langword="null" /> or ""</param>
        /// <param name="result">
        /// When this method returns, contains the VersionEx object equivalent of the number that is contained in <paramref name="input" />, if the conversion succeeded,
        /// or a VersionEx object whose major and minor version numbers are 0 if the conversion failed.
        /// <returns><see langword="true" /> if the <paramref name="input" /> parameter was converted successfully; otherwise, <see langword="false" />.</returns>
        public static bool TryParse(string input, out VersionEx result) 
        {
            Version v;
            if (Version.TryParse(input, out v)) { result = new VersionEx(v); return true; }
            result = new VersionEx();
            return false;
        }

        public static bool operator ==(VersionEx v1, VersionEx v2) { return v1.Equals(v2); }
        public static bool operator ==(VersionEx v1, Version v2) { return v1.Equals(v2); }
        public static bool operator ==(Version v1, VersionEx v2) { return v2.Equals(v1); }

        public static bool operator !=(VersionEx v1, VersionEx v2) { return !v1.Equals(v2); }
        public static bool operator !=(VersionEx v1, Version v2) { return !v1.Equals(v2); }
        public static bool operator !=(Version v1, VersionEx v2) { return !v2.Equals(v1); }

        public static bool operator <(VersionEx v1, VersionEx v2) { return (v1.CompareTo(v2) < 0); }
        public static bool operator <=(VersionEx v1, VersionEx v2) { return (v1.CompareTo(v2) <= 0); }
        public static bool operator >(VersionEx v1, VersionEx v2) { return (v2 < v1); }
        public static bool operator >=(VersionEx v1, VersionEx v2) { return (v2 <= v1); }

        public static implicit operator Version(VersionEx v)
        {
            if (v.Build == -1) return new Version(v.Major, v.Minor);
            if (v.Revision == -1) return new Version(v.Major, v.Minor, v.Build);
            return new Version(v.Major, v.Minor, v.Build, v.Revision);
        }
        public static implicit operator VersionEx(Version v) { return new VersionEx(v); }
    }
}
