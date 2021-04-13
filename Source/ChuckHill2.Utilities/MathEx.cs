//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="MathEx.cs" company="Chuck Hill">
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

        /// <summary>
        /// Compute Median of an array of values.
        /// </summary>
        /// <param name="vector">Array of values</param>
        /// <returns>Median of vector array</returns>
        public static double Median(double[] vector)
        {
            Array.Sort(vector);
            int len = vector.Length;
            if (len == 0) return 0;
            if (len == 1) return vector[0];
            if (len % 2 == 0)
            {
                return (vector[len / 2 - 1] + vector[len / 2]) / 2;
            }

            return vector[len / 2 + 1];
        }

        /// <summary>
        /// Polynomial Least-squares fit of raw data.
        /// </summary>
        /// <param name="xVector">Array of X data points</param>
        /// <param name="yVector">Array of Y data points</param>
        /// <param name="fitOrder">Number of terms in polynomial</param>
        /// <param name="stdev">Returned standard deviation for each term</param>
        /// <returns>Array of computed terms of polynomial</returns>
        public static double[] PolynomialFit(double[] xVector, double[] yVector, int fitOrder, out double[] stdev)
        {
            if (fitOrder < 0) { stdev = new double[] { 0 }; return new double[] { 0.0 }; }              //straight horizontal line at y==0
            if (fitOrder == 0) { stdev = new double[] { 0 }; return new double[] { Median(yVector) }; } //straight horizontal line at y==median

            double[,] xmatrix = new double[xVector.Length, fitOrder + 1];
            for (int i = 0; i < xVector.Length; i++)
            {
                xmatrix[i, 0] = 1;
                xmatrix[i, 1] = xVector[i];

                for (int j = 2; j <= fitOrder; j++)
                {
                    xmatrix[i, j] = xmatrix[i, 1] * xmatrix[i, j - 1];
                }
            }

            double[] terms = new double[fitOrder + 1];
            stdev = new double[fitOrder + 1];
            FitGLM(xmatrix, yVector, terms, stdev);

            return terms;
        }

        /// <summary>
        /// FIT General Linear Model. 
        /// This procedure assumes the xmatrix and yvector are are entered correctly. 
        /// (i.e. each column in the independant matrix represents an independant 
        /// variable and the dependant vector contains only a single column.  This 
        /// procedure DOES NOT check for matched rows in the dependant and independant
        /// data. NOTE: Column 1 of the xmatrix must equal 1, the constant.
        /// </summary>
        /// <param name="xmatrix"></param>
        /// <param name="yvector"></param>
        /// <param name="coef">Coefficients</param>
        /// <param name="ster">Standard Deviation. May be null if not required.</param>
        public static void FitGLM(double[,] xmatrix, double[] yvector, double[] coef, double[] ster)
        {
            int nobs = xmatrix.GetLength(0);  //row count
            int nind = xmatrix.GetLength(1);  //col count
            double[,] a = new double[nind + 1, nind + 1];
            double[] dmin = new double[nind];

            /* Original data tables (xmatrix, yvector) are now ready   */
            /* nind = number of independent variables (= x columns -1) */
            /* nobs = number of observations per variable (= rows)     */

            for (int j = 0; j < nind; j++)
            {
                for (int l = j; l < nind; l++)
                {
                    double sumxx = 0.0;
                    for (int w = 0; w < nobs; w++) { sumxx += xmatrix[w, j] * xmatrix[w, l]; }
                    a[l, j] = sumxx;
                    a[j, l] = sumxx;      /* temporarily store lower triangular matrix */
                }
                double sumxy = 0.0;
                for (int v = 0; v < nobs; v++) { sumxy += xmatrix[v, j] * yvector[v]; }
                a[nind, j] = sumxy;
                a[j, nind] = sumxy;                            /* also for symmetry */
            }
            double sumyy = 0.0;
            for (int w = 0; w < nobs; w++) { sumyy += yvector[w] * yvector[w]; }
            a[nind, nind] = sumyy;

            /* compute and store DMIN values */

            for (int i = 0; i < nind; i++) { dmin[i] = a[i, i] - (a[0, i] * a[0, i]) / nobs; }

            /* --- begin g2sweep --- */

            int nc = nind + 1;
            for (int k = 0; k < nc - 1; k++)                         /* sweep the kth row of A */
            {                                                          /* step 1 */
                double d = a[k, k];                                 /* check for singularity */
                double cdmin = (dmin[k] <= 0.0 ? 1e-10 : 1e-10 * dmin[k]);
                if (d < cdmin)
                {
                    for (int q = 0; q < nc; q++) { a[q, k] = a[k, q] = 0.0; }
                    continue;                          /* go to the next row to sweep */
                }
                for (int j = 0; j < nc; j++) { a[k, j] /= d; }  /* step 2 : divide row k by d */
                for (int i = 0; i < nc; i++)                    /* step 3 : adjust other rows */
                {
                    if (i == k) continue;                                 /* skip row k */
                    double b = a[i, k];
                    for (int j = 0; j < nc; j++) { a[i, j] -= (b * a[k, j]); }
                    a[i, k] = -b / d;
                }
                a[k, k] = 1 / d;                                            /* step 4 */
            }

            /* load returned arrays */

            for (int i = 0; i < nind; i++) { coef[i] = a[i, nind]; }

            if (ster != null)                                 /* compute standard deviation */
            {
                double esigma, variance;

                esigma = (nobs == nind ? 0.0 : a[nind, nind] / (nobs - nind));
                for (int i = 0; i < nind; i++)
                {
                    variance = a[i, i] * esigma;
                    ster[i] = Math.Sqrt(variance < 0.0 ? 0.0 : variance);
                }

                /* NOTE: "T" values = coef[n] / ster[n]; (where ster[n] > 0) */
            }
        }

        /// <summary>
        /// Yet another QuickSort algorithm. Ported from _Numerical Recipes in C_.
        /// </summary>
        /// <typeparam name="T">Type of object to sort</typeparam>
        /// <param name="a">Array to sort</param>
        /// <param name="i">Start Index</param>
        /// <param name="j">End Index</param>
        /// <param name="c">Comparer</param>
        public static void QuickSort<T>(T[] a, int i, int j, IComparer<T> c)
        {
            if (i < j)
            {
                int q = QSPartition(a, i, j, c);
                QuickSort(a, i, q, c);
                QuickSort(a, q + 1, j, c);
            }
        }
        private static int QSPartition<T>(T[] a, int p, int r, IComparer<T> c)
        {

            T x = a[p];
            int i = p - 1;
            int j = r + 1;
            T tmp;
            while (true)
            {
                do { j--; } while (c.Compare(a[j], x) > 0);
                do { i++; } while (c.Compare(a[i], x) < 0);
                if (i < j)
                {
                    tmp = a[i];
                    a[i] = a[j];
                    a[j] = tmp;
                }
                else return j;
            }
        }
    }
}
