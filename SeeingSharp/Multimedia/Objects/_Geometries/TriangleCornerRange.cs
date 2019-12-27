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

using System.Numerics;
using SeeingSharp.Util;
using SeeingSharp.Checking;

namespace SeeingSharp.Multimedia.Objects
{
    /// <summary>
    /// A Triangle inside a Geometry object
    /// </summary>
    public struct TriangleCornerRange
    {
        internal TriangleCornerRange(UnsafeList<TriangleCorner> cornerListInternal, int startCornerIndex)
        {
            this.IndexCorner1 = startCornerIndex;
            this.CornerListInternal = cornerListInternal;
        }

        public ref TriangleCorner GetCornerByIndex(int triangleLocalIndex)
        {
            triangleLocalIndex.EnsureInRange(0, 3, nameof(triangleLocalIndex));

            return ref this.CornerListInternal.BackingArray[this.IndexCorner1 + triangleLocalIndex];
        }

        public TriangleCornerRange SetCornerProperties(int triangleLocalIndex, Vector2 texCoord1)
        {
            triangleLocalIndex.EnsureInRange(0, 3, nameof(triangleLocalIndex));

            ref var corner = ref this.CornerListInternal.BackingArray[this.IndexCorner1 + triangleLocalIndex];
            corner.TexCoord1 = texCoord1;

            return this;
        }

        public readonly int IndexCorner1;

        internal readonly UnsafeList<TriangleCorner> CornerListInternal;
    }
}