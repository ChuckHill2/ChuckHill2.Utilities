//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="RectangleEx.cs" company="Chuck Hill">
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
using System.Diagnostics.Contracts;
using System.Drawing;
using System.Globalization;
using System.Xml.Serialization;

namespace ChuckHill2
{
    /// <summary>
    /// Stores the location and size of a rectangular region. Based upon 
    /// System.Drawing.Rectangle except unlike struct Rectangle (ByValue), 
    /// class RectangleEx is ByRef. Important when updating values that 
    /// need to be passed elsewhere.<br />
    /// In addition:
    ///   * (X,Y,Width,Height) are XML Serializable as Attributes.
    ///   * (X,Y,Width,Height) setters support event handlers XxxxChanged.
    ///   * Implicitly castable between RectangleRef and:
    ///      + System.Drawing.Rectangle
    ///      + System.Drawing.RectangleF (values rounded).
    /// </summary>
    /// <remarks>
    /// The problem is with properties that return rectangles.
    /// example:
    /// @code{.cs}
    ///     Rectangle MyProperty { get; set; }
    ///     MyProperty.X = 3;
    /// @endcode
    /// The value of MyProperty.X will _never_ be set. MyProperty returns a _copy_ of the 
    /// Rectangle because MyProperty is really a method. Meaning setting MyProperty.X = 3, 
    /// changes the value in the copy and the rectangle is promptly thrown away.<br />
    /// The only thing that will work is:
    /// @code{.cs}
    ///     var rect =  MyProperty;
    ///     rect.X = 3;
    ///     MyProperty = rect;
    /// @endcode
    /// </remarks>
    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    [XmlInclude(typeof(Rectangle))] //necessary when using implicit operators
    [XmlInclude(typeof(RectangleF))] //necessary when using implicit operators
    public class RectangleEx : IEquatable<RectangleEx>, IEquatable<Rectangle>
    {
        public static readonly RectangleEx Empty = new RectangleEx();

        private int x;
        private int y;
        private int width;
        private int height;
        private object userData; //aka Tag -- custom user data

        #region Constructors
        public RectangleEx() { }

        public RectangleEx(int x, int y, int width, int height)
        {
            this.x = x;
            this.y = y;
            this.width = width;
            this.height = height;
        }

        public RectangleEx(Point location, Size size)
        {
            this.x = location.X;
            this.y = location.Y;
            this.width = size.Width;
            this.height = size.Height;
        }

        public RectangleEx(PointF location, SizeF size)
        {
            this.x = (int)Math.Round(location.X);
            this.y = (int)Math.Round(location.Y);
            this.width = (int)Math.Round(size.Width);
            this.height = (int)Math.Round(size.Height);
        }

        public RectangleEx(Rectangle rc)
        {
            this.x = rc.X;
            this.y = rc.Y;
            this.width = rc.Width;
            this.height = rc.Height;
        }

        public RectangleEx(RectangleEx rc)
        {
            this.x = rc.X;
            this.y = rc.Y;
            this.width = rc.Width;
            this.height = rc.Height;
        }

        public RectangleEx(RectangleF rc)
        {
            this.x = (int)Math.Round(rc.X);
            this.y = (int)Math.Round(rc.Y);
            this.width = (int)Math.Round(rc.Width);
            this.height = (int)Math.Round(rc.Height);
        }
        #endregion

        #region public events
        /// <summary>XChanged(RectangleRefM thisObject, int previousValue, int currentValue);</summary>
        public event Action<RectangleEx, int, int> XChanged;

        /// <summary>YChanged(RectangleRefM thisObject, int previousValue, int currentValue);</summary>
        public event Action<RectangleEx, int, int> YChanged;

        /// <summary>WidthChanged(RectangleRefM thisObject, int previousValue, int currentValue);</summary>
        public event Action<RectangleEx, int, int> WidthChanged;

        /// <summary>HeightChanged(RectangleRefM thisObject, int previousValue, int currentValue);</summary>
        public event Action<RectangleEx, int, int> HeightChanged;
        #endregion

        #region public properties
        [Browsable(false)]
        [XmlIgnore]
        public Point Location { get { return new Point(X, Y); } set { X = value.X; Y = value.Y; } }

        [Browsable(false)]
        [XmlIgnore]
        public Size Size { get { return new Size(Width, Height); } set { this.Width = value.Width; this.Height = value.Height; } }

        [Browsable(false)]
        [XmlIgnore]
        public SizeF SizeF { get { return new SizeF(Width, Height); } set { this.Width = (int)Math.Round(value.Width); this.Height = (int)Math.Round(value.Height); } }

        [XmlAttribute]
        public int X { get { return x; } set { var v = x; x = value; if (XChanged != null) XChanged(this, v, x); } }

        [XmlAttribute]
        public int Y { get { return y; } set { var v = y; y = value; if (YChanged != null) YChanged(this, v, y); } }

        [XmlAttribute]
        public int Width { get { return width; } set { var v = width; width = value; if (WidthChanged != null) WidthChanged(this, v, width); } }

        [XmlAttribute]
        public int Height { get { return height; } set { var v = height; height = value; if (HeightChanged != null) HeightChanged(this, v, height); } }

        [Browsable(false)]
        public int Left { get { return X; } }

        [Browsable(false)]
        public int Top { get { return Y; } }

        [Browsable(false)]
        public int Right { get { return X + Width; } }

        [Browsable(false)]
        public int Bottom { get { return Y + Height; } }

        [Browsable(false)]
        public bool IsEmpty { get { return height == 0 && width == 0 && x == 0 && y == 0; } }

        [Browsable(false)]
        [XmlIgnore]
        public object Tag { get { return userData; } set { userData = value; } }
        #endregion

        #region public bool Equals(...)
        public override bool Equals(object obj)
        {
            if (!(obj is RectangleEx)) return false;
            RectangleEx other = (RectangleEx)obj;
            return (other.x == this.x) &&
                   (other.y == this.y) &&
                   (other.width == this.width) &&
                   (other.height == this.height);
        }

        public bool Equals(RectangleEx other)
        {
            if (RectangleEx.ReferenceEquals(other, null)) return false; //can't use '==' because it will be recursive!
            return (other.x == this.x) &&
                   (other.y == this.y) &&
                   (other.width == this.width) &&
                   (other.height == this.height);
        }

        public bool Equals(Rectangle other)
        {
            return (other.X == this.x) &&
                   (other.Y == this.y) &&
                   (other.Width == this.width) &&
                   (other.Height == this.height);
        }
        #endregion

        #region Operator Overloads
        public static bool operator ==(RectangleEx left, RectangleEx right)
        {
            if (RectangleEx.ReferenceEquals(left, null) && 
                RectangleEx.ReferenceEquals(right, null)) return true; //can't use '==' because it will be recursive!
            if (RectangleEx.ReferenceEquals(left, null) ||
                RectangleEx.ReferenceEquals(right, null)) return false;

            return (left.x == right.x) &&
                   (left.y == right.y) &&
                   (left.width == right.width) &&
                   (left.height == right.height);
        }

        public static bool operator !=(RectangleEx left, RectangleEx right)
        {
            return !(left == right);
        }

        public static implicit operator RectangleEx(Rectangle r) { return new RectangleEx(r.X, r.Y, r.Width, r.Height); }

        public static implicit operator Rectangle(RectangleEx r) { return new Rectangle(r.X, r.Y, r.Width, r.Height); }

        public static implicit operator RectangleEx(RectangleF r) { return RectangleEx.Round(r); }

        public static implicit operator RectangleF(RectangleEx r) { return new RectangleF(r.X, r.Y, r.Width, r.Height); }
        #endregion

        #region Public Methods
        public static RectangleEx FromLTRB(int left, int top, int right, int bottom)
        {
            return new RectangleEx(left, top, right - left, bottom - top);
        }

        public void SetValues(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public static RectangleEx Ceiling(RectangleF value)
        {
            return new RectangleEx((int)Math.Ceiling(value.X),
                                 (int)Math.Ceiling(value.Y),
                                 (int)Math.Ceiling(value.Width),
                                 (int)Math.Ceiling(value.Height));
        }

        public static RectangleEx Truncate(RectangleF value)
        {
            return new RectangleEx((int)value.X,
                                 (int)value.Y,
                                 (int)value.Width,
                                 (int)value.Height);
        }

        public static RectangleEx Round(RectangleF value)
        {
            return new RectangleEx((int)Math.Round(value.X),
                                 (int)Math.Round(value.Y),
                                 (int)Math.Round(value.Width),
                                 (int)Math.Round(value.Height));
        }

        public static RectangleEx Round(double x, double y, double width, double height)
        {
            return new RectangleEx((int)Math.Round(x),
                                 (int)Math.Round(y),
                                 (int)Math.Round(width),
                                 (int)Math.Round(height));
        }

        [Pure]
        public bool Contains(int x, int y)
        {
            return this.X <= x &&
                x < this.X + this.Width &&
                this.Y <= y &&
                y < this.Y + this.Height;
        }

        [Pure]
        public bool Contains(Point pt)
        {
            return Contains(pt.X, pt.Y);
        }

        [Pure]
        public bool Contains(RectangleEx rect)
        {
            return (this.X <= rect.X) &&
                ((rect.X + rect.Width) <= (this.X + this.Width)) &&
                (this.Y <= rect.Y) &&
                ((rect.Y + rect.Height) <= (this.Y + this.Height));
        }

        public override int GetHashCode()
        {
            return unchecked((int)((UInt32)X ^
                        (((UInt32)Y << 13) | ((UInt32)Y >> 19)) ^
                        (((UInt32)Width << 26) | ((UInt32)Width >> 6)) ^
                        (((UInt32)Height << 7) | ((UInt32)Height >> 25))));
        }

        public void Inflate(int width, int height)
        {
            this.X -= width;
            this.Y -= height;
            this.Width += 2 * width;
            this.Height += 2 * height;
        }

        public void Inflate(Size size)
        {
            Inflate(size.Width, size.Height);
        }

        public static RectangleEx Inflate(RectangleEx rect, int x, int y)
        {
            RectangleEx r = rect;
            r.Inflate(x, y);
            return r;
        }

        public void Intersect(RectangleEx rect)
        {
            RectangleEx result = RectangleEx.Intersect(rect, this);

            this.X = result.X;
            this.Y = result.Y;
            this.Width = result.Width;
            this.Height = result.Height;
        }

        public static RectangleEx Intersect(RectangleEx a, RectangleEx b)
        {
            int x1 = Math.Max(a.X, b.X);
            int x2 = Math.Min(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Max(a.Y, b.Y);
            int y2 = Math.Min(a.Y + a.Height, b.Y + b.Height);

            if (x2 >= x1 && y2 >= y1)
            {
                return new RectangleEx(x1, y1, x2 - x1, y2 - y1);
            }

            return RectangleEx.Empty;
        }

        [Pure]
        public bool IntersectsWith(RectangleEx rect)
        {
            return (rect.X < this.X + this.Width) &&
            (this.X < (rect.X + rect.Width)) &&
            (rect.Y < this.Y + this.Height) &&
            (this.Y < rect.Y + rect.Height);
        }

        [Pure]
        public static RectangleEx Union(RectangleEx a, RectangleEx b)
        {
            int x1 = Math.Min(a.X, b.X);
            int x2 = Math.Max(a.X + a.Width, b.X + b.Width);
            int y1 = Math.Min(a.Y, b.Y);
            int y2 = Math.Max(a.Y + a.Height, b.Y + b.Height);

            return new RectangleEx(x1, y1, x2 - x1, y2 - y1);
        }

        public void Offset(Point pos)
        {
            Offset(pos.X, pos.Y);
        }

        public void Offset(int x, int y)
        {
            this.X += x;
            this.Y += y;
        }
        #endregion

        public override string ToString()
        {
            return string.Concat(
                "{X=", X.ToString(CultureInfo.CurrentCulture), 
                ",Y=", Y.ToString(CultureInfo.CurrentCulture),
                ",Width=",  Width.ToString(CultureInfo.CurrentCulture),
                ",Height=", Height.ToString(CultureInfo.CurrentCulture), "}");
        }
    }
}
