//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="New.cs" company="Chuck Hill">
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
using System.Linq.Expressions;
using System.Runtime.Serialization;

namespace ChuckHill2
{
    /// <summary>
    /// Create object with default constructor even if it doesn't have a default parameterless constructor. 
    /// </summary>
    /// <typeparam name="T">Type of default object to create</typeparam>
    /// <remarks>
    /// The performance of (T)FormatterServices.GetUninitializedObject(typeof(T)) is slower than a compiled 
    /// parameterless constructor. At the same time compiled expressions would give you great speed 
    /// improvements though they work only for types with default constructor. The hybrid approach was 
    /// taken. This means the create expression is effectively cached and incurs penalty only the first time 
    /// the type is loaded. Will handle value types too in an efficient manner.<br />
    /// Usage:
    /// @code{.cs}
    /// var myObj = New&lt;System.Globalization.RegionInfo&gt;.Create()
    /// @endcode
    /// Warning: If the type does not have a parameterless constructor, any initalization that occurs within
    /// the constructors will NOT occur here. The entire object is zero'd out. All fields will be either 
    /// null or for value types, the default value (aka: 0, false, etc).
    /// </remarks>
    /// <see cref="http://stackoverflow.com/questions/390578/creating-instance-of-type-without-default-constructor-in-c-sharp-using-reflectio"/>
    public static class New<T>
    {
        public static readonly Func<T> Create = Creator();

        static Func<T> Creator()
        {
            Type t = typeof(T);

            //Special Case: GetUninitializedObject() fails with type 'string' so it is handled here.
            if (t == typeof(string))
                return Expression.Lambda<Func<T>>(Expression.Constant(string.Empty)).Compile();

            //If the type has a parameterless constructor, use it. It's more efficient than GetUninitializedObject()
            if (t.IsValueType || t.GetConstructor(Type.EmptyTypes) != null)
                return Expression.Lambda<Func<T>>(Expression.New(t)).Compile();

            return () => (T)FormatterServices.GetUninitializedObject(t);
        }
    }
}
