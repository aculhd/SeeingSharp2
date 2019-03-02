﻿#region License information
/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland König (RolandK)
    
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Lesser General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Lesser General Public License for more details.

    You should have received a copy of the GNU Lesser General Public License
    along with this program.  If not, see http://www.gnu.org/licenses/.
*/
#endregion

using System.Collections.Generic;
using SharpDX;

#if DESKTOP
using System.Windows.Media.Media3D;
#endif

namespace SeeingSharp
{
    #region using
    #endregion

    public static class MathExtensions
    {
        public static bool IsEmpty(this ref Vector2 vector)
        {
            return vector.Equals(Vector2.Zero);
        }

        /// <summary>
        /// Gets all points contained in given line collection.
        /// </summary>
        /// <param name="lines">A list containing all lines.</param>
        public static IEnumerable<Vector3> GetAllPoints(this IEnumerable<Line> lines)
        {
            var lastVector = Vector3Ex.MinValue;

            foreach (var actLine in lines)
            {
                if (lastVector != actLine.StartPosition)
                {
                    yield return actLine.StartPosition;
                }

                yield return actLine.EndPosition;

                lastVector = actLine.EndPosition;
            }
        }
    }
}