﻿/* Copyright (C) 2012 Fairmat SRL (info@fairmat.com, http://www.fairmat.com/)
 * Author(s): Stefano Angeleri (stefano.angeleri@fairmat.com)
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using DVPLI;
using DVPLUtils;

namespace PFunction2D
{
    /// <summary>
    /// Implements the evaluation functionalities of PFunction2D.
    /// It provides the capabilities to rappresent a matrix and give to it
    /// cordinates, additionally it allows to interpolate values between the values
    /// actually present in the matrix.
    /// </summary>
    public class CPointFunction2D
    {
        #region Function Parameters

        /// <summary>
        /// The matrix containing the value of the 2d function.
        /// </summary>
        private Matrix values;

        /// <summary>
        /// The x cordinates for the matrix.
        /// </summary>
        private Vector cordinatesX;

        /// <summary>
        /// The y cordinates for the matrix.
        /// </summary>
        private Vector cordinatesY;

        /// <summary>
        /// Defines the interpolation technique to apply, if needed.
        /// </summary>
        private EInterpolationType interpolationType;

        /// <summary>
        /// Defines the extrapolation technique to apply, if needed.
        /// </summary>
        private ExtrapolationType extrapolationType;

        #endregion Function Parameters

        #region Constructors

        /// <summary>
        /// Default constructor, just initializes the data storage.
        /// </summary>
        private CPointFunction2D()
        {
            this.values = new Matrix();
            this.cordinatesX = new Vector();
            this.cordinatesY = new Vector();
            this.interpolationType = EInterpolationType.LINEAR;
            this.extrapolationType = ExtrapolationType.CONSTANT;
        }

        /// <summary>
        /// Constructs a new CPointFunction2D through evaluation of IRightValues
        /// in order to fill the data used to evaluate the function.
        /// </summary>
        /// <param name="cordinatesX">
        /// An array of IRightValue whose result is ordered from lower to greater and will
        /// rappresent the x parameter of the function.
        /// </param>
        /// <param name="cordinatesY">
        /// An array of IRightValue whose result is ordered from lower to greater and will
        /// rappresent the y parameter of the function.
        /// </param>
        /// <param name="values">
        /// A bidimensional array containing the defined data points for
        /// all the cordinates specified by cordinatesX and cordinatesY.
        /// </param>
        /// <param name="interpolationType">
        /// The interpolation to apply when evaluating the function
        /// in case the requested cordinates aren't rappresented, but inside them.
        /// </param>
        /// <param name="extrapolationType">
        /// The extrapolation to apply when evaluating the function,
        /// in case the requested cordinates are outside the rappresented ones.
        /// </param>
        public CPointFunction2D(IRightValue[] cordinatesX,
                                IRightValue[] cordinatesY,
                                IRightValue[,] values,
                                EInterpolationType interpolationType,
                                ExtrapolationType extrapolationType) : this()
        {
            this.interpolationType = interpolationType;
            this.extrapolationType = extrapolationType;
            SetSizes(cordinatesX.Length, cordinatesY.Length);
            for (int i = cordinatesX.Length - 1; i >= 0; i--)
            {
                this[i, -1] = cordinatesX[i].V();
            }

            for (int i = cordinatesY.Length - 1; i >= 0; i--)
            {
                this[-1, i] = cordinatesY[i].V();
            }

            for (int x = 0; x < values.GetLength(0); x++)
            {
                for (int y = 0; y < values.GetLength(1); y++)
                {
                    this[x, y] = values[x, y].V();
                }
            }
        }

        #endregion Constructors

        #region Properties

        /// <summary>
        /// Gets or sets the data inside the data structures
        /// after doing checks for consistency.
        /// </summary>
        /// <remarks>If both parameters are -1 nothing will be done.</remarks>
        /// <param name="x">
        /// The x cordinate to use to get or set the element,
        /// if it's -1 it will work on the y cordinates, else
        /// on the values.
        /// </param>
        /// <param name="y">
        /// The y cordinate to use to get or set the element,
        /// if it's -1 it will work on the x cordinates, else
        /// on the values.
        /// </param>
        /// <returns>The requested value at the position.</returns>
        private double this[int x, int y]
        {
            get
            {
                if (y != -1 && x != -1)
                {
                    return this.values[x, y];
                }
                else if (y != -1)
                {
                    return this.cordinatesY[y];
                }
                else if (x != -1)
                {
                    return this.cordinatesX[x];
                }

                return 0;
            }

            set
            {
                if (y != -1 && x != -1)
                {
                    this.values[x, y] = value;
                }
                else if (y != -1)
                {
                    if ((y > 0 && this.cordinatesY[y - 1] > value) ||
                       (y < this.cordinatesY.Count - 1 && this.cordinatesY[y + 1] < value))
                    {
                        throw new Exception("Function integrity wasn't maintained in the " +
                                            "y cordinates.");
                    }

                    this.cordinatesY[y] = value;
                }
                else if (x != -1)
                {
                    if ((x > 0 && this.cordinatesX[x - 1] > value) ||
                       (x < this.cordinatesX.Count - 1 && this.cordinatesX[x + 1] < value))
                    {
                        throw new Exception("Function integrity wasn't maintained in the " +
                                            "x cordinates.");
                    }

                    this.cordinatesX[x] = value;
                }
            }
        }

        #endregion Properties

        #region Getters and Setters

        /// <summary>
        /// Sets the sizes of the data structures which will be used.
        /// </summary>
        /// <param name="x">The x length.</param>
        /// <param name="y">The y length.</param>
        private void SetSizes(int x, int y)
        {
            this.cordinatesX.Resize(x);
            this.cordinatesY.Resize(y);
            this.values.NewSize(x, y);
        }

        #endregion Getters and Setters

        #region Internal functions

        /// <summary>
        /// Finds the position of a cordinate in a vector,
        /// it's used with cordinatesX and cordinatesY.
        /// </summary>
        /// <param name="posVector">
        /// A reference to the vector to use to find the required item index.
        /// </param>
        /// <param name="value">The value to search for, exact search.</param>
        /// <returns>
        /// The position in the given vector of the given value. The first element has index 0.
        /// -1 is returned in case it wasn't possible to find the value inside the given vector.
        /// </returns>
        private int FindPosition(ref Vector posVector, double value)
        {
            for (int i = 0; i < posVector.Count; i++)
            {
                if (posVector[i] == value)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Finds the nearest index before the given one.
        /// </summary>
        /// <param name="posVector">
        /// A reference to the vector to use to find the required item index.
        /// </param>
        /// <param name="value">
        /// The value to search for the nearest lower element to the given one.
        /// </param>
        /// <returns>
        /// The position in the given vector of the nearest value before the requested one.
        /// The first element has index 0.
        /// </returns>
        private int FindNearestBefore(ref Vector posVector, double value)
        {
            for (int i = 0; i < posVector.Count; i++)
            {
                if (posVector[i] > value)
                    return i - 1;
            }

            return posVector.Count - 1;
        }

        /// <summary>
        /// Calculates the value at the requested cordinates through linear
        /// interpolation.
        /// </summary>
        /// <remarks>
        /// The value requested must not be at the margin of the matrix,
        /// so there must be at least one entry under and on the left of the
        /// requested value.</remarks>
        /// <param name="x">The x cordinate where to calculate the value.</param>
        /// <param name="y">The y cordinate where to calculate the value.</param>
        /// <returns>The calculated value.</returns>
        private double CalculateLinear(double x, double y)
        {
            // The indices we will find here will always before the last, so there is no need to
            // do bound checking for this function.
            int beforeX = FindNearestBefore(ref this.cordinatesX, x);
            int beforeY = FindNearestBefore(ref this.cordinatesY, y);

            // The denominator of the formula which gives the linear interpolation.
            // (x2 - x1) * (y2 - y1)
            double denominator = (this.cordinatesX[beforeX + 1] - this.cordinatesX[beforeX]) *
                             (this.cordinatesY[beforeY + 1] - this.cordinatesY[beforeY]);

            // The 4 factors of the formula providing the linear interpolation.
            double factor1 = (this.values[beforeX, beforeY] / denominator) *
                             ((this.cordinatesX[beforeX + 1] - x) * (this.cordinatesY[beforeY + 1] - y));

            double factor2 = (this.values[beforeX + 1, beforeY] / denominator) *
                             ((x - this.cordinatesX[beforeX]) * (this.cordinatesY[beforeY + 1] - y));

            double factor3 = (this.values[beforeX, beforeY + 1] / denominator) *
                             ((this.cordinatesX[beforeX + 1] - x) * (y - this.cordinatesY[beforeY]));

            double factor4 = (this.values[beforeX + 1, beforeY + 1] / denominator) *
                             ((x - this.cordinatesX[beforeX]) * (y - this.cordinatesY[beforeY]));

            // Finally sum the 4 factors togheter and return the result.
            return factor1 + factor2 + factor3 + factor4;
        }

        /// <summary>
        /// Calculates the spline interpolation from left, which is just
        /// the nearest value available before the requested one (left/up).
        /// </summary>
        /// <param name="x">The x cordinate where to calculate the value.</param>
        /// <param name="y">The y cordinate where to calculate the value.</param>
        /// <returns>The calculated value.</returns>
        private double CalculateSpline(double x, double y)
        {
            return 0;
        }

        /// <summary>
        /// Calculates the constant interpolation from left, which is just
        /// the nearest value available before the requested one (left/up).
        /// </summary>
        /// <param name="x">The x cordinate where to calculate the value.</param>
        /// <param name="y">The y cordinate where to calculate the value.</param>
        /// <returns>The calculated value.</returns>
        private double CalculateConstantBefore(double x, double y)
        {
            int selectedX = FindNearestBefore(ref this.cordinatesX, x);
            int selectedY = FindNearestBefore(ref this.cordinatesY, y);

            return this.values[selectedX, selectedY];
        }

        /// <summary>
        /// Calculates the constant interpolation from right, which is just
        /// the nearest value available before the requested one (right/down).
        /// </summary>
        /// <param name="x">The x cordinate where to calculate the value.</param>
        /// <param name="y">The y cordinate where to calculate the value.</param>
        /// <returns>The calculated value.</returns>
        private double CalculateConstantAfter(double x, double y)
        {
            int selectedX = FindNearestBefore(ref this.cordinatesX, x);
            int selectedY = FindNearestBefore(ref this.cordinatesY, y);

            // In case the nearest before is actually a bounduary cell
            // apply a correction to the index in order to take the last
            // value available at the end of the data.
            if (selectedX == this.cordinatesX.Count - 1)
            {
                selectedX--;
            }
            
            if (selectedY == this.cordinatesY.Count - 1)
            {
                selectedY--;
            }

            return this.values[selectedX + 1, selectedY + 1];
        }

        #endregion Internal functions

        #region Public functions

        /// <summary>
        /// Copies the data inside this <see cref="CPointFunction2D"/>
        /// into another <see cref="CPointFunction2D"/>.
        /// </summary>
        /// <param name="other">
        /// The <see cref="CPointFunction2D"/> where to copy the data to.
        /// </param>
        internal void CopyTo(CPointFunction2D other)
        {
            other.SetSizes(this.cordinatesX.Count, this.cordinatesY.Count);
            this.cordinatesX.CopyTo(other.cordinatesX);
            this.cordinatesY.CopyTo(other.cordinatesY);
            this.values.CopyTo(other.values);
            other.interpolationType = this.interpolationType;
            other.extrapolationType = this.extrapolationType;
        }

        /// <summary>
        /// Evaluates the function at the requested x and y point,
        /// using, if necessary, an interpolation.
        /// </summary>
        /// <param name="x">The x cordinate where to evaluate the function.</param>
        /// <param name="y">The y cordinate where to calculate the value.</param>
        /// <returns>The value of the function at the requested point.</returns>
        internal double Evaluate(double x, double y)
        {
            // First of all check if we have any data
            if (this.cordinatesX.Count == 0)
            {
                return 0;
            }

            // Values outside the range of the x and y aren't allowed
            // in those case the last value in the bounduary direction
            // which was exceeded is returned. (Extrapolation)
            // Note: The bounds are determined by the first and last element
            //       of the vectors as they rappresent ordered indexes.
            if (x < this.cordinatesX[0])
            {
                x = this.cordinatesX[0];
            }
            else if (x > this.cordinatesX[this.cordinatesX.Count - 1])
            {
                x = this.cordinatesX[this.cordinatesX.Count - 1];
            }

            if (y < this.cordinatesY[0])
            {
                y = this.cordinatesY[0];
            }
            else if (y > this.cordinatesY[this.cordinatesY.Count - 1])
            {
                y = this.cordinatesY[this.cordinatesY.Count - 1];
            }

            // Bounds check is now done, so it's possible to fetch the requested value.

            // First search for the exact position, if already known. This will
            // also handle, as a result, the case in which the requested value is at
            // the margin of the matrix, so it will never be possible the code
            // which handles interpolation will have to handle this case.
            int selectedX = FindPosition(ref this.cordinatesX, x);
            int selectedY = FindPosition(ref this.cordinatesY, y);
            if (selectedX != -1 && selectedY != -1)
            {
                // In this case the requested position was actually provided
                // so it's enough to just return it.
                return this.values[selectedX, selectedY];
            }
            else
            {
                // If it wasn't possible to find the requested position
                // try to interpolate it with the requested interpolation method.
                switch (this.interpolationType)
                {
                    // A linear interpolation.
                    case EInterpolationType.LINEAR:
                        return CalculateLinear(x, y);

                    // A spline interpolation.
                    case EInterpolationType.SPLINE:
                        return CalculateSpline(x, y);

                    // A constant interpolation (zero order left)
                    // It interpolates by taking the left up values.
                    case EInterpolationType.ZERO_ORDER_LEFT:
                        return CalculateConstantBefore(x, y);

                    // A constant interpolation (zero order)
                    // It interpolates by taking the right down values.
                    case EInterpolationType.ZERO_ORDER:
                        return CalculateConstantAfter(x, y);

                    // Any interpolation type which in't supported will return zero.
                    default:
                        return 0;
                }
            }
        }

        #endregion Public functions
    }
}
