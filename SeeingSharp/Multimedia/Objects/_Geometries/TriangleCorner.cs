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

namespace SeeingSharp.Multimedia.Objects
{
    public struct TriangleCorner
    {
        internal TriangleCorner(int index)
            : this()
        {
            Index = index;

            Normal = Vector3.Zero;
            Tangent = Vector3.Zero;
            Color = Color4.White;
            Binormal = Vector3.Zero;
            TextureFactor = 0f;
            TexCoord1 = Vector2.Zero;
        }

        internal TriangleCorner(int index, Color4 color, Vector2 textureCoordinate, Vector3 normal)
            : this()
        {
            Index = index;

            Normal = normal;
            Tangent = Vector3.Zero;
            Color = color;
            Binormal = Vector3.Zero;
            TextureFactor = 0f;
            TexCoord1 = textureCoordinate;
        }

        internal TriangleCorner(int index, ref TriangleCornerTemplate template)
        {
            this.Index = index;

            Normal = template.Normal;
            Tangent = template.Tangent;
            Color = template.Color;
            Binormal = template.Binormal;
            TextureFactor = template.TextureFactor;
            TexCoord1 = template.TexCoord1;
        }

        internal TriangleCorner(int index, ref TriangleCorner template)
        {
            this.Index = index;

            Normal = template.Normal;
            Tangent = template.Tangent;
            Color = template.Color;
            Binormal = template.Binormal;
            TextureFactor = template.TextureFactor;
            TexCoord1 = template.TexCoord1;
        }

        /// <summary>
        /// The index of this <see cref="TriangleCorner"/>
        /// </summary>
        public readonly int Index;

        /// <summary>
        /// Retrieves or sets the normal of the vertex
        /// </summary>
        public Vector3 Normal;

        /// <summary>
        /// Retrieves or sets the color of the vertex
        /// </summary>
        public Color4 Color;

        /// <summary>
        /// Gets or sets the tangent vector.
        /// </summary>
        public Vector3 Tangent;

        /// <summary>
        /// Gets or sets the binormal vector.
        /// </summary>
        public Vector3 Binormal;

        /// <summary>
        /// Gets or sets the texture factor.
        /// This value decides whether a texture is displayed on this vertex or not.
        /// A value greater or equal 0 will show the texture, all negatives will hide it.
        /// </summary>
        public float TextureFactor;

        /// <summary>
        /// Retrieves or sets first texture coordinate
        /// </summary>
        public Vector2 TexCoord1;
    }
}
