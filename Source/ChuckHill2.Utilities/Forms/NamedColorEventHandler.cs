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
