using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChuckHill2
{
    /// <summary>
    /// Math Utilities
    /// </summary>
    public static class MathEx
    {
        /// <summary>
        /// Get the minimum value of 2 or more values
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="vals">2 or more values</param>
        /// <returns>Minimum value.</returns>
        public static T Min<T>(params T[] vals)
        {
            T v = vals[0];
            for (int i = 1; i < vals.Length; i++)
            {
                if (Comparer<T>.Default.Compare(vals[i], v) < 0) v = vals[i];
            }

            return v;
        }

        /// <summary>
        /// Get the maximum value of 2 or more values
        /// </summary>
        /// <typeparam name="T">Type of objects to compare</typeparam>
        /// <param name="vals">2 or more values</param>
        /// <returns>Maximum value.</returns>
        public static T Max<T>(params T[] vals)
        {
            T v = vals[0];
            for (int i = 1; i < vals.Length; i++)
            {
                if (Comparer<T>.Default.Compare(vals[i], v) > 0) v = vals[i];
            }

            return v;
        }
    }
}
