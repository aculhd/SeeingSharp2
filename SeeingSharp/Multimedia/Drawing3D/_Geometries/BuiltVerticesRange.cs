﻿/*
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

namespace SeeingSharp.Multimedia.Drawing3D
{
    public struct BuiltVerticesRange
    {
        public Geometry Geometry;
        public int StartVertex;
        public int VertexCount;

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltVerticesRange" /> struct.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public BuiltVerticesRange(Geometry geometry)
        {
            Geometry = geometry;
            StartVertex = 0;
            VertexCount = 0;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BuiltVerticesRange" /> struct.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        /// <param name="startVertex">The start vertex.</param>
        /// <param name="vertexCount">The vertex count.</param>
        public BuiltVerticesRange(Geometry geometry, int startVertex, int vertexCount)
        {
            Geometry = geometry;
            StartVertex = startVertex;
            VertexCount = vertexCount;
        }

        /// <summary>
        /// Merges this geometry with the given one.
        /// This method does only work if the given geometry does begin after the end of the current one.
        /// </summary>
        /// <param name="otherRange">The other range to merge with.</param>
        public void Merge(BuiltVerticesRange otherRange)
        {
            if (VertexCount <= 0)
            {
                StartVertex = otherRange.StartVertex;
                VertexCount = otherRange.VertexCount;
            }
            else
            {
                if (StartVertex + VertexCount != otherRange.StartVertex)
                {
                    throw new SeeingSharpGraphicsException("Unable to merge given vertex ranges!");
                }
                VertexCount = VertexCount + otherRange.VertexCount;
            }
        }

        public BuiltVerticesRange DisableTexture()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                ref var actVertex = ref Geometry.GetVertexBasicRef(loop);
                actVertex.TextureFactor = -100;
            }

            return this;
        }

        public BuiltVerticesRange FlipTextureCoordinatesX()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                ref var actVertex = ref Geometry.GetVertexBasicRef(loop);
                actVertex.TexCoord1 = new Vector2(
                    -actVertex.TexCoord1.X,
                    actVertex.TexCoord1.Y);
            }

            return this;
        }

        public BuiltVerticesRange FlipTextureCoordinatesY()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                ref var actVertex = ref Geometry.GetVertexBasicRef(loop);
                actVertex.TexCoord1 = new Vector2(
                    actVertex.TexCoord1.X,
                    -actVertex.TexCoord1.Y);
            }

            return this;
        }

        public BuiltVerticesRange FlipTextureCoordinatesXY()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                ref var actVertex = ref Geometry.GetVertexBasicRef(loop);
                actVertex.TexCoord1 = new Vector2(
                    -actVertex.TexCoord1.X,
                    -actVertex.TexCoord1.Y);
            }

            return this;
        }

        public BuiltVerticesRange RotateTextureCoordinates()
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                ref var actVertex = ref Geometry.GetVertexBasicRef(loop);
                actVertex.TexCoord1 = new Vector2(
                    actVertex.TexCoord1.Y,
                    actVertex.TexCoord1.X);
            }

            return this;
        }

        public BuiltVerticesRange SetVertexColor(Color4 color)
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                ref var actVertex = ref Geometry.GetVertexBasicRef(loop);
                actVertex.Color = color;
            }

            return this;
        }

        public BuiltVerticesRange SetNormal(Vector3 normal)
        {
            var lastVertex = StartVertex + VertexCount;

            for (var loop = StartVertex; loop < lastVertex; loop++)
            {
                ref var actVertex = ref Geometry.GetVertexBasicRef(loop);
                actVertex.Normal = normal;
            }

            return this;
        }

        /// <summary>
        /// Gets a value indicating whether this instance is empty.
        /// </summary>
        public bool IsEmpty => VertexCount <= 0;
    }
}