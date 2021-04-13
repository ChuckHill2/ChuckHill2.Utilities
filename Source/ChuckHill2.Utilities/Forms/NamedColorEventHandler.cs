//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="NamedColorEventHandler.cs" company="Chuck Hill">
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

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Specify how the list of colors is ordered for NamedColorComboBox, NamedColorListBox, and NamedColorTreeView controls.
    /// </summary>
    public enum OrderBy
    {
        /// <summary>
        /// Colors are ordered by color and shade.
        /// </summary>
        Color,
        /// <summary>
        /// Colors are ordered by name alphabetically.
        /// </summary>
        Name
    }

    /// <summary>
    /// Represents the method that will handle the SelectedColor event of NamedColorComboBox, NamedColorListBox, and NamedColorTreeView controls.
    /// </summary>
    /// <param name="sender">The source of the event. </param>
    /// <param name="e">An EventArgs object that contains the event data. </param>
    public delegate void NamedColorEventHandler(object sender, NamedColorEventArgs e);

    /// <summary>
    /// Provides data for the SelectedColor event of NamedColorComboBox, NamedColorListBox, and NamedColorTreeView controls.
    /// </summary>
    public class NamedColorEventArgs : EventArgs
    {
        private Color __color;

        /// <summary>
        /// Gets the selected color.
        /// </summary>
        public Color Color { get { return __color; } }

        /// <summary>
        /// Initializes a new instance of the NamedColorEventArgs class with the specified selected color.
        /// </summary>
        /// <param name="color">The selected color</param>
        public NamedColorEventArgs(Color color)
        {
            __color = color;
        }
    }
}
