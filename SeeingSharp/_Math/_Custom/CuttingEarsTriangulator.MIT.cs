﻿/*
    Seeing# and all games/applications distributed together with it. 
    Exception are projects where it is noted otherwhise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the autors homepage, german)
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

using System.Collections.Generic;
using SharpDX;

namespace SeeingSharp
{
    /// <summary>
    /// A cutting ears triangulator for simple polygons with no holes. O(n^2)
    /// This algorithm is based on the implementation of the Helix Toolkit (http://helixtoolkit.codeplex.com/)
    /// </summary>
    /// <remarks>
    /// Regarding the Helix Toolkit:
    /// Based on http://www.flipcode.com/archives/Efficient_Polygon_Triangulation.shtml
    /// References
    /// http://en.wikipedia.org/wiki/Polygon_triangulation
    /// http://computacion.cs.cinvestav.mx/~anzures/geom/triangulation.php
    /// http://www.codeproject.com/KB/recipes/cspolygontriangulation.aspx
    /// </remarks>
    public static class CuttingEarsTriangulator
    {
        /// <summary>
        /// The epsilon.
        /// </summary>
        private const double Epsilon = 1e-10;

        /// <summary>
        /// Triangulate a polygon using the cutting ears algorithm.
        /// </summary>
        /// <remarks>
        /// The algorithm does not support holes.
        /// </remarks>
        /// <param name="contour">
        /// the polygon contour
        /// </param>
        /// <returns>
        /// collection of triangle Vector2s
        /// </returns>
        public static IEnumerable<int> Triangulate(IList<Vector2> contour)
        {
            // allocate and initialize list of indices in polygon
            var result = new List<int>(contour.Count * 3);

            var n = contour.Count;
            if (n < 3)
            {
                return null;
            }

            var V = new int[n];

            // we want a counter-clockwise polygon in V
            if (Area(contour) > 0)
            {
                for (var v = 0; v < n; v++)
                {
                    V[v] = v;
                }
            }
            else
            {
                for (var v = 0; v < n; v++)
                {
                    V[v] = n - 1 - v;
                }
            }

            var nv = n;

            // remove nv-2 Vertices, creating 1 triangle every time
            var count = 2 * nv; // error detection

            for (var v = nv - 1; nv > 2; )
            {
                // if we loop, it is probably a non-simple polygon
                if (0 >= count--)
                {
                    // ERROR - probable bad polygon!
                    //return null;
                    return result;
                }

                // three consecutive vertices in current polygon, <u,v,w>
                var u = v;
                if (nv <= u)
                {
                    u = 0; // previous
                }

                v = u + 1;
                if (nv <= v)
                {
                    v = 0; // new v
                }

                var w = v + 1;
                if (nv <= w)
                {
                    w = 0; // next
                }

                if (Snip(contour, u, v, w, nv, V))
                {
                    int s, t;

                    // true names of the vertices
                    var a = V[u];
                    var b = V[v];
                    var c = V[w];

                    // output Triangle
                    result.Add(a);
                    result.Add(b);
                    result.Add(c);

                    // remove v from remaining polygon
                    for (s = v, t = v + 1; t < nv; s++, t++)
                    {
                        V[s] = V[t];
                    }

                    nv--;

                    // resets error detection counter
                    count = 2 * nv;
                }
            }

            return result;
        }

        // compute area of a contour/polygon
        /// <summary>
        /// Calculates the area.
        /// </summary>
        /// <param name="contour">The contour.</param>
        /// <returns>The area.</returns>
        private static double Area(IList<Vector2> contour)
        {
            var n = contour.Count;
            var A = 0.0;
            for (int p = n - 1, q = 0; q < n; p = q++)
            {
                A += contour[p].X * contour[q].Y - contour[q].X * contour[p].Y;
            }

            return A * 0.5f;
        }

        /// <summary>
        /// Decide if Vector2 (Px,Py) is inside triangle defined by (Ax,Ay) (Bx,By) (Cx,Cy).
        /// </summary>
        /// <param name="Ax">
        /// The ax.
        /// </param>
        /// <param name="Ay">
        /// The ay.
        /// </param>
        /// <param name="Bx">
        /// The bx.
        /// </param>
        /// <param name="By">
        /// The by.
        /// </param>
        /// <param name="Cx">
        /// The cx.
        /// </param>
        /// <param name="Cy">
        /// The cy.
        /// </param>
        /// <param name="Px">
        /// The px.
        /// </param>
        /// <param name="Py">
        /// The py.
        /// </param>
        /// <returns>
        /// The inside triangle.
        /// </returns>
        private static bool InsideTriangle(
            double Ax, double Ay, double Bx, double By, double Cx, double Cy, double Px, double Py)
        {
            var ax = Cx - Bx;
            var ay = Cy - By;
            var bx = Ax - Cx;
            var by = Ay - Cy;
            var cx = Bx - Ax;
            var cy = By - Ay;
            var apx = Px - Ax;
            var apy = Py - Ay;
            var bpx = Px - Bx;
            var bpy = Py - By;
            var cpx = Px - Cx;
            var cpy = Py - Cy;

            var aCROSSbp = ax * bpy - ay * bpx;
            var cCROSSap = cx * apy - cy * apx;
            var bCROSScp = bx * cpy - by * cpx;

            return aCROSSbp >= 0.0f && bCROSScp >= 0.0f && cCROSSap >= 0.0f;
        }

        /// <summary>
        /// The snip.
        /// </summary>
        /// <param name="contour">The contour.</param>
        /// <param name="u">The u.</param>
        /// <param name="v">The v.</param>
        /// <param name="w">The w.</param>
        /// <param name="n">The n.</param>
        /// <param name="V">The v.</param>
        /// <returns>The snip.</returns>
        private static bool Snip(IList<Vector2> contour, int u, int v, int w, int n, int[] V)
        {
            int p;

            double Ax = contour[V[u]].X;
            double Ay = contour[V[u]].Y;

            double Bx = contour[V[v]].X;
            double By = contour[V[v]].Y;

            double Cx = contour[V[w]].X;
            double Cy = contour[V[w]].Y;

            if (Epsilon > (Bx - Ax) * (Cy - Ay) - (By - Ay) * (Cx - Ax))
            {
                return false;
            }

            for (p = 0; p < n; p++)
            {
                if (p == u || p == v || p == w)
                {
                    continue;
                }

                double Px = contour[V[p]].X;
                double Py = contour[V[p]].Y;
                if (InsideTriangle(Ax, Ay, Bx, By, Cx, Cy, Px, Py))
                {
                    return false;
                }
            }

            return true;
        }
    }
}