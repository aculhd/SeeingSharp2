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
using SeeingSharp.Checking;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using SeeingSharp.Util;

namespace SeeingSharp.Multimedia.Objects
{
    /// <summary>
    /// A set of triangles of a Geometry which share the
    /// same material settings.
    /// </summary>
    public partial class GeometrySurface
    {
        private UnsafeList<TriangleCorner> m_corners;

        internal GeometrySurface(Geometry owner, int triangleCapacity)
        {
            this.Owner = owner;
            m_corners = new UnsafeList<TriangleCorner>(triangleCapacity * 3);

            this.Indices = new IndexCollection(m_corners);
            this.Corners = new CornerCollection(m_corners);
        }

        /// <summary>
        /// Clones this object.
        /// </summary>
        public GeometrySurface Clone(
            Geometry newOwner,
            bool copyGeometryData = true, int capacityMultiplier = 1,
            int baseIndex = 0)
        {
            newOwner.EnsureNotNull(nameof(newOwner));

            // Create new Geometry object
            var indexCount = m_corners.Count;
            var result = new GeometrySurface(newOwner, indexCount / 3 * capacityMultiplier);

            // Copy geometry
            if (copyGeometryData)
            {
                for (var loop = 0; loop < indexCount; loop++)
                {
                    ref var cornerToAdd = ref m_corners.BackingArray[loop];
                    result.m_corners.Add(new TriangleCorner(
                        cornerToAdd.Index + baseIndex,
                        ref cornerToAdd));
                }
            }

            return result;
        }

        /// <summary>
        /// Adds all vertices and surfaces of the given geometry to this one.
        /// All surfaces of the given geometry are merged to this single surface.
        /// </summary>
        /// <param name="geometry">The geometry.</param>
        public void AddGeometry(Geometry geometry)
        {
            var baseIndex = this.Owner.VerticesInternal.Count;

            // AddObject all vertices to local geometry
            this.Owner.VerticesInternal.AddRange(geometry.VerticesInternal);

            // AddObject all corners to local surface
            foreach (var actSurface in geometry.Surfaces)
            {
                var corners = actSurface.m_corners;
                var cornerCount = corners.Count;

                for (var loop = 0; loop < cornerCount; loop++)
                {
                    ref var cornerToAdd = ref corners.BackingArray[loop];
                    m_corners.Add(new TriangleCorner(
                        cornerToAdd.Index + baseIndex,
                        ref cornerToAdd));
                }
            }
        }

        /// <summary>
        /// Adds tow triangles for a rectangle.
        /// </summary>
        public void AddTriangleCornersForRect(
            int indexA, int indexB, int indexC, int indexD,
            Color4 color, Vector2 texCoordA, Vector2 texCoordC, Vector3 normal)
        {
            // First triangle
            this.AddTriangleCorner(indexA, color, texCoordA, normal);
            this.AddTriangleCorner(indexC, color, texCoordC, normal);
            this.AddTriangleCorner(indexB, color, new Vector2(texCoordC.X, texCoordA.Y), normal);

            // Second triangle
            this.AddTriangleCorner(indexA, color, texCoordA, normal);
            this.AddTriangleCorner(indexD, color, new Vector2(texCoordA.X, texCoordC.Y), normal);
            this.AddTriangleCorner(indexC, color, texCoordC, normal);
        }

        /// <summary>
        /// Adds a new corner with the given vertex index.
        /// </summary>
        /// <param name="index">The index of the vertex.</param>
        internal void AddTriangleCorner(int index)
        {
            m_corners.Add(new TriangleCorner(index));
        }

        /// <summary>
        /// Adds a new corner with the given vertex index.
        /// </summary>
        /// <param name="index">The index of the vertex.</param>
        /// <param name="textureCoordinate">The texture coordinate on this corner.</param>
        internal void AddTriangleCorner(int index, Vector2 textureCoordinate)
        {
            m_corners.Add(new TriangleCorner(index));

            ref var newCorner = ref m_corners.BackingArray[m_corners.Count -1];
            newCorner.TexCoord1 = textureCoordinate;
        }

        /// <summary>
        /// Adds a new corner with the given vertex index.
        /// </summary>
        /// <param name="index">The index of the vertex.</param>
        /// <param name="textureCoordinate">The texture coordinate on this corner.</param>
        /// <param name="normal">The normal vector on this corner.</param>
        internal void AddTriangleCorner(int index, Vector2 textureCoordinate, Vector3 normal)
        {
            m_corners.Add(new TriangleCorner(index));

            ref var newCorner = ref m_corners.BackingArray[m_corners.Count -1];
            newCorner.TexCoord1 = textureCoordinate;
            newCorner.Normal = normal;
        }

        /// <summary>
        /// Adds a new corner with the given vertex index.
        /// </summary>
        /// <param name="index">The index of the vertex.</param>
        /// <param name="textureCoordinate">The texture coordinate on this corner.</param>
        /// <param name="normal">The normal vector on this corner.</param>
        /// <param name="color">The color on this corner.</param>
        internal void AddTriangleCorner(int index, Color4 color, Vector2 textureCoordinate, Vector3 normal)
        {
            m_corners.Add(new TriangleCorner(index));

            ref var newCorner = ref m_corners.BackingArray[m_corners.Count -1];
            newCorner.Color = color;
            newCorner.TexCoord1 = textureCoordinate;
            newCorner.Normal = normal;
        }

        /// <summary>
        /// Adds a new corner with the given vertex index.
        /// </summary>
        /// <param name="index">The index of the vertex.</param>
        /// <param name="textureCoordinate">The texture coordinate on this corner.</param>
        /// <param name="color">The color on this corner.</param>
        internal void AddTriangleCorner(int index, Color4 color, Vector2 textureCoordinate)
        {
            m_corners.Add(new TriangleCorner(index));

            ref var newCorner = ref m_corners.BackingArray[m_corners.Count -1];
            newCorner.Color = color;
            newCorner.TexCoord1 = textureCoordinate;
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        public TriangleCornerRange AddTriangle(int index1, int index2, int index3)
        {
            var result = new TriangleCornerRange(m_corners, m_corners.Count);

            m_corners.Add(new TriangleCorner(index1));
            m_corners.Add(new TriangleCorner(index2));
            m_corners.Add(new TriangleCorner(index3));

            return result;
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        /// <param name="cornerTemplate">Template for generated TriangleCorners</param>
        public TriangleCornerRange AddTriangle(int index1, int index2, int index3, ref TriangleCornerTemplate cornerTemplate)
        {
            var result = new TriangleCornerRange(m_corners, m_corners.Count);

            m_corners.Add(new TriangleCorner(index1, ref cornerTemplate));
            m_corners.Add(new TriangleCorner(index2, ref cornerTemplate));
            m_corners.Add(new TriangleCorner(index3, ref cornerTemplate));

            return result;
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        /// <param name="cornerTemplate1">Template for generated TriangleCorners</param>
        /// <param name="cornerTemplate2">Template for generated TriangleCorners</param>
        /// <param name="cornerTemplate3">Template for generated TriangleCorners</param>
        public TriangleCornerRange AddTriangle(
            int index1, int index2, int index3, 
            ref TriangleCornerTemplate cornerTemplate1, ref TriangleCornerTemplate cornerTemplate2, ref TriangleCornerTemplate cornerTemplate3)
        {
            var result = new TriangleCornerRange(m_corners, m_corners.Count);

            m_corners.Add(new TriangleCorner(index1, ref cornerTemplate1));
            m_corners.Add(new TriangleCorner(index2, ref cornerTemplate2));
            m_corners.Add(new TriangleCorner(index3, ref cornerTemplate3));

            return result;
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="v1">First vertex</param>
        /// <param name="v2">Second vertex</param>
        /// <param name="v3">Third vertex</param>
        public TriangleCornerRange AddTriangle(Vertex v1, Vertex v2, Vertex v3)
        {
            var result = new TriangleCornerRange(m_corners, m_corners.Count);

            m_corners.Add(new TriangleCorner(this.Owner.AddVertex(v1)));
            m_corners.Add(new TriangleCorner(this.Owner.AddVertex(v2)));
            m_corners.Add(new TriangleCorner(this.Owner.AddVertex(v3)));

            return result;
        }

        /// <summary>
        /// Adds a triangle and calculates normals for it.
        /// </summary>
        /// <param name="index1">Index of the first vertex</param>
        /// <param name="index2">Index of the second vertex</param>
        /// <param name="index3">Index of the third vertex</param>
        public void AddTriangleAndCalculateNormalsFlat(int index1, int index2, int index3)
        {
            this.CalculateNormalsFlat(this.AddTriangle(index1, index2, index3));
        }

        /// <summary>
        /// Adds a triangle
        /// </summary>
        /// <param name="v1">First vertex</param>
        /// <param name="v2">Second vertex</param>
        /// <param name="v3">Third vertex</param>
        public void AddTriangleAndCalculateNormalsFlat(Vertex v1, Vertex v2, Vertex v3)
        {
            var index1 = this.Owner.AddVertex(v1);
            var index2 = this.Owner.AddVertex(v2);
            var index3 = this.Owner.AddVertex(v3);

            this.AddTriangleAndCalculateNormalsFlat(index1, index2, index3);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        public void AddPolygonByCuttingEars(IEnumerable<Vertex> vertices)
        {
            //AddObject vertices first
            var indices = new List<int>();

            foreach (var actVertex in vertices)
            {
                indices.Add(this.Owner.AddVertex(actVertex));
            }

            //Calculate cutting ears
            this.AddPolygonByCuttingEars(indices);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="indices">The indices of the polygon's corners.</param>
        /// <param name="twoSided">The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEars(IEnumerable<int> indices, bool twoSided = false)
        {
            this.AddPolygonByCuttingEarsInternal(new List<int>(indices), twoSided);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="vertices">The vertices to add.</param>
        /// <param name = "twoSided" > The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEarsAndCalculateNormals(IEnumerable<Vertex> vertices, bool twoSided = false)
        {
            //AddObject vertices first
            var indices = new List<int>();

            foreach (var actVertex in vertices)
            {
                indices.Add(this.Owner.AddVertex(actVertex));
            }

            //Calculate cutting ears and normals
            this.AddPolygonByCuttingEarsAndCalculateNormals(indices, twoSided);
        }

        /// <summary>
        /// Adds the given polygon using the cutting ears algorithm for triangulation.
        /// </summary>
        /// <param name="indices">The indices of the polygon's corners.</param>
        /// <param name="twoSided">The indexes for front- and backside?</param>
        public void AddPolygonByCuttingEarsAndCalculateNormals(IEnumerable<int> indices, bool twoSided)
        {
            // AddObject the triangles using cutting ears algorithm
            var addedTriangles = this.AddPolygonByCuttingEarsInternal(new List<int>(indices), twoSided);

            // Calculate all normals
            foreach (var actTriangle in addedTriangles)
            {
                this.CalculateNormalsFlat(actTriangle);
            }
        }

        /// <summary>
        /// Builds a line list containing a line for each face binormal.
        /// </summary>
        public List<Vector3> BuildLineListForFaceBinormals()
        {
            var result = new List<Vector3>();

            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actStartIndex = 0; actStartIndex <= cornerList.Length; actStartIndex += 3)
            {
                ref var corner1 = ref cornerList[actStartIndex];
                ref var corner2 = ref cornerList[actStartIndex + 1];
                ref var corner3 = ref cornerList[actStartIndex + 2];
                ref var vertex1 = ref vertexList[corner1.Index];
                ref var vertex2 = ref vertexList[corner2.Index];
                ref var vertex3 = ref vertexList[corner3.Index];

                // Get average values for current face
                var averageBinormal = Vector3.Normalize(Vector3Ex.Average(corner1.Binormal, corner2.Binormal, corner3.Binormal));
                var averagePosition = Vector3Ex.Average(vertex1.Position, vertex2.Position, vertex3.Position);
                averageBinormal *= 0.2f;

                // Generate a line
                if (averageBinormal.Length() > 0.1f)
                {
                    result.Add(averagePosition);
                    result.Add(averagePosition + averageBinormal);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a line list containing a line for each face normal.
        /// </summary>
        public List<Vector3> BuildLineListForFaceNormals()
        {
            var result = new List<Vector3>();

            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actStartIndex = 0; actStartIndex <= cornerList.Length; actStartIndex += 3)
            {
                ref var corner1 = ref cornerList[actStartIndex];
                ref var corner2 = ref cornerList[actStartIndex + 1];
                ref var corner3 = ref cornerList[actStartIndex + 2];
                ref var vertex1 = ref vertexList[corner1.Index];
                ref var vertex2 = ref vertexList[corner2.Index];
                ref var vertex3 = ref vertexList[corner3.Index];

                // Get average values for current face
                var averageBinormal = Vector3.Normalize(Vector3Ex.Average(corner1.Normal, corner2.Normal, corner3.Normal));
                var averagePosition = Vector3Ex.Average(vertex1.Position, vertex2.Position, vertex3.Position);
                averageBinormal *= 0.2f;

                // Generate a line
                if (averageBinormal.Length() > 0.1f)
                {
                    result.Add(averagePosition);
                    result.Add(averagePosition + averageBinormal);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a line list containing a line for each face tangent.
        /// </summary>
        public List<Vector3> BuildLineListForFaceTangents()
        {
            var result = new List<Vector3>();

            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actStartIndex = 0; actStartIndex <= cornerList.Length; actStartIndex += 3)
            {
                ref var corner1 = ref cornerList[actStartIndex];
                ref var corner2 = ref cornerList[actStartIndex + 1];
                ref var corner3 = ref cornerList[actStartIndex + 2];
                ref var vertex1 = ref vertexList[corner1.Index];
                ref var vertex2 = ref vertexList[corner2.Index];
                ref var vertex3 = ref vertexList[corner3.Index];

                // Get average values for current face
                var averageBinormal = Vector3.Normalize(Vector3Ex.Average(corner1.Tangent, corner2.Tangent, corner3.Tangent));
                var averagePosition = Vector3Ex.Average(vertex1.Position, vertex2.Position, vertex3.Position);
                averageBinormal *= 0.2f;

                // Generate a line
                if (averageBinormal.Length() > 0.1f)
                {
                    result.Add(averagePosition);
                    result.Add(averagePosition + averageBinormal);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a list list containing a list for each vertex binormal.
        /// </summary>
        public List<Vector3> BuildLineListForVertexBinormals()
        {
            var result = new List<Vector3>();

            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actCornerIndex = 0; actCornerIndex <= cornerList.Length; actCornerIndex++)
            {
                ref var corner = ref cornerList[actCornerIndex];
                ref var vertex = ref vertexList[corner.Index];

                if (corner.Binormal.Length() > 0.1f)
                {
                    result.Add(vertex.Position);
                    result.Add(vertex.Position + corner.Binormal * 0.2f);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a list list containing a list for each vertex normal.
        /// </summary>
        public List<Vector3> BuildLineListForVertexNormals()
        {
            var result = new List<Vector3>();

            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actCornerIndex = 0; actCornerIndex <= cornerList.Length; actCornerIndex++)
            {
                ref var corner = ref cornerList[actCornerIndex];
                ref var vertex = ref vertexList[corner.Index];

                if (corner.Normal.Length() > 0.1f)
                {
                    result.Add(vertex.Position);
                    result.Add(vertex.Position + corner.Normal * 0.2f);
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a list list containing a list for each vertex tangent.
        /// </summary>
        public List<Vector3> BuildLineListForVertexTangents()
        {
            var result = new List<Vector3>();

            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actCornerIndex = 0; actCornerIndex <= cornerList.Length; actCornerIndex++)
            {
                ref var corner = ref cornerList[actCornerIndex];
                ref var vertex = ref vertexList[corner.Index];

                if (corner.Tangent.Length() > 0.1f)
                {
                    result.Add(vertex.Position);
                    result.Add(vertex.Position + corner.Tangent * 0.2f);
                }
            }

            return result;
        }

        /// <summary>
        /// Build a line list containing all lines for wireframe display.
        /// </summary>
        public List<Vector3> BuildLineListForWireframeView()
        {
            var result = new List<Vector3>();

            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actStartIndex = 0; actStartIndex <= cornerList.Length; actStartIndex += 3)
            {
                ref var corner1 = ref cornerList[actStartIndex];
                ref var corner2 = ref cornerList[actStartIndex + 1];
                ref var corner3 = ref cornerList[actStartIndex + 2];
                ref var vertex1 = ref vertexList[corner1.Index];
                ref var vertex2 = ref vertexList[corner2.Index];
                ref var vertex3 = ref vertexList[corner3.Index];

                // First line (c)
                result.Add(vertex1.Position);
                result.Add(vertex2.Position);

                // Second line (a)
                result.Add(vertex2.Position);
                result.Add(vertex3.Position);

                // Third line (b)
                result.Add(vertex3.Position);
                result.Add(vertex1.Position);
            }

            return result;
        }

        /// <summary>
        /// Gets an index array
        /// </summary>
        public int[] GetIndexArray()
        {
            var result = new int[m_corners.Count];
            for (var loop = 0; loop < m_corners.Count; loop++)
            {
                result[loop] = m_corners[loop].Index;
            }
            return result;
        }

        /// <summary>
        /// Recalculates all normals
        /// </summary>
        public void CalculateNormalsFlat()
        {
            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actCornerIndex = 0; actCornerIndex < cornerList.Length; actCornerIndex += 3)
            {
                ref var corner1 = ref cornerList[actCornerIndex];
                ref var corner2 = ref cornerList[actCornerIndex + 1];
                ref var corner3 = ref cornerList[actCornerIndex + 2];
                ref var vertex1 = ref vertexList[corner1.Index];
                ref var vertex2 = ref vertexList[corner2.Index];
                ref var vertex3 = ref vertexList[corner3.Index];

                var normal = Vector3Ex.CalculateTriangleNormal(vertex1.Position, vertex2.Position, vertex3.Position);

                corner1.Normal = normal;
                corner2.Normal = normal;
                corner3.Normal = normal;
            }
        }

        /// <summary>
        /// Calculates normals for the given triangle.
        /// </summary>
        /// <param name="actTriangle">The triangle for which to calculate the normal (flat).</param>
        public void CalculateNormalsFlat(TriangleCornerRange actTriangle)
        {
            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;

            ref var corner1 = ref cornerList[actTriangle.IndexCorner1];
            ref var corner2 = ref cornerList[actTriangle.IndexCorner1 + 1];
            ref var corner3 = ref cornerList[actTriangle.IndexCorner1 + 2];
            ref var vertex1 = ref vertexList[corner1.Index];
            ref var vertex2 = ref vertexList[corner2.Index];
            ref var vertex3 = ref vertexList[corner3.Index];

            var normal = Vector3Ex.CalculateTriangleNormal(vertex1.Position, vertex2.Position, vertex3.Position);

            corner1.Normal = normal;
            corner2.Normal = normal;
            corner3.Normal = normal;
        }

        /// <summary>
        /// Calculates normals for the given triangle.
        /// </summary>
        /// <param name="countTriangles">Total count of triangles.</param>
        /// <param name="startTriangleIndex">The triangle on which to start.</param>
        public void CalculateNormalsFlat(int startTriangleIndex, int countTriangles)
        {
            for (var loop = 0; loop < countTriangles; loop++)
            {
                this.CalculateNormalsFlat(new TriangleCornerRange(m_corners, (startTriangleIndex + loop) * 3));
            }
        }

        /// <summary>
        /// Calculates tangents for all vectors.
        /// </summary>
        public void CalculateTangentsAndBinormals()
        {
            var cornerList = m_corners.BackingArray;
            var vertexList = this.Owner.VerticesInternal.BackingArray;
            for (var actStartIndex = 0; actStartIndex <= cornerList.Length; actStartIndex += 3)
            {
                ref var corner1 = ref cornerList[actStartIndex];
                ref var corner2 = ref cornerList[actStartIndex + 1];
                ref var corner3 = ref cornerList[actStartIndex + 2];
                ref var vertex1 = ref vertexList[corner1.Index];
                ref var vertex2 = ref vertexList[corner2.Index];
                ref var vertex3 = ref vertexList[corner3.Index];

                // Perform some precalculations
                var w1 = corner1.TexCoord1;
                var w2 = corner2.TexCoord1;
                var w3 = corner3.TexCoord1;
                var x1 = vertex2.Position.X - vertex1.Position.X;
                var x2 = vertex3.Position.X - vertex1.Position.X;
                var y1 = vertex2.Position.Y - vertex1.Position.Y;
                var y2 = vertex3.Position.Y - vertex1.Position.Y;
                var z1 = vertex2.Position.Z - vertex1.Position.Z;
                var z2 = vertex3.Position.Z - vertex1.Position.Z;
                var s1 = w2.X - w1.X;
                var s2 = w3.X - w1.X;
                var t1 = w2.Y - w1.Y;
                var t2 = w3.Y - w1.Y;
                var r = 1f / (s1 * t2 - s2 * t1);
                var sdir = new Vector3((t2 * x1 - t1 * x2) * r, (t2 * y1 - t1 * y2) * r, (t2 * z1 - t1 * z2) * r);
                var tdir = new Vector3((s1 * x2 - s2 * x1) * r, (s1 * y2 - s2 * y1) * r, (s1 * z2 - s2 * z1) * r);

                // Create the tangent vector (assumes that each vertex normal within the face are equal)
                var tangent = Vector3.Normalize(sdir - corner1.Normal * Vector3.Dot(corner1.Normal, sdir));

                // Create the binormal using the tangent
                var tangentDir = Vector3.Dot(Vector3.Cross(corner1.Normal, sdir), tdir) >= 0.0f ? 1f : -1f;
                var binormal = Vector3.Cross(corner1.Normal, tangent) * tangentDir;

                // Setting binormals and tangents to each vertex of current face
                corner1.Tangent = tangent;
                corner1.Binormal = binormal;
                corner2.Tangent = tangent;
                corner2.Binormal = binormal;
                corner3.Tangent = tangent;
                corner3.Binormal = binormal;
            }
        }

        /// <summary>
        /// Toggles all vertices and indexes from left to right handed or right to left handed system.
        /// </summary>
        internal void ToggleCoordinateSystemInternal()
        {
            for (var loopTriangle = 0; loopTriangle + 3 <= m_corners.Count; loopTriangle += 3)
            {
                var corner1 = m_corners[loopTriangle];
                var corner2 = m_corners[loopTriangle + 1];
                var corner3 = m_corners[loopTriangle + 2];
                m_corners[loopTriangle] = corner3;
                m_corners[loopTriangle + 1] = corner2;
                m_corners[loopTriangle + 2] = corner1;
            }
        }

        private IEnumerable<TriangleCornerRange> AddPolygonByCuttingEarsInternal(IList<int> vertexIndices, bool twoSided)
        {
            //Get all coordinates
            var coordinates = new Vector3[vertexIndices.Count];
            for (var loop = 0; loop < vertexIndices.Count; loop++)
            {
                coordinates[loop] = this.Owner.VerticesInternal[vertexIndices[loop]].Position;
            }

            //Triangulate all data
            var polygon = new Polygon(coordinates);
            var triangleIndices = polygon.TriangulateUsingCuttingEars();
            if (triangleIndices == null)
            {
                throw new SeeingSharpGraphicsException("Unable to triangulate given polygon!");
            }

            //AddObject all triangle data
            var indexEnumerator = triangleIndices.GetEnumerator();
            try
            {
                while (indexEnumerator.MoveNext())
                {
                    var index1 = indexEnumerator.Current;
                    var index2 = 0;
                    var index3 = 0;

                    if (indexEnumerator.MoveNext())
                    {
                        index2 = indexEnumerator.Current;
                    }
                    else
                    {
                        break;
                    }
                    if (indexEnumerator.MoveNext())
                    {
                        index3 = indexEnumerator.Current;
                    }
                    else
                    {
                        break;
                    }

                    yield return this.AddTriangle(vertexIndices[index3], vertexIndices[index2], vertexIndices[index1]);
                    if (twoSided)
                    {
                        yield return this.AddTriangle(vertexIndices[index1], vertexIndices[index2], vertexIndices[index3]);
                    }
                }
            }
            finally
            {
                indexEnumerator.Dispose();
            }
        }

        /// <summary>
        /// Gets a collection containing all indexes.
        /// </summary>
        public IndexCollection Indices { get; }

        /// <summary>
        /// Gets a collection containing all corners.
        /// </summary>
        public CornerCollection Corners { get; }

        /// <summary>
        /// Retrieves total count of all triangles within this geometry
        /// </summary>
        public int CountTriangles => m_corners.Count / 3;

        /// <summary>
        /// Gets the owner of this surface.
        /// </summary>
        public Geometry Owner { get; }

        /// <summary>
        /// Retrieves total count of all indexes within this geometry
        /// </summary>
        internal int CountIndices => m_corners.Count;

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all indexes of a Geometry object.
        /// </summary>
        public class IndexCollection : IEnumerable<int>
        {
            private UnsafeList<TriangleCorner> m_corners;

            internal IndexCollection(UnsafeList<TriangleCorner> corners)
            {
                m_corners = corners;
            }

            public IEnumerator<int> GetEnumerator()
            {
                return m_corners.Select(actCorner => actCorner.Index)
                    .GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_corners.Select(actCorner => actCorner.Index)
                    .GetEnumerator();
            }

            /// <summary>
            /// Returns the index at ghe given index
            /// </summary>
            public int this[int index] => m_corners[index].Index;

            public int Count => m_corners.Count;
        }

        //*********************************************************************
        //*********************************************************************
        //*********************************************************************
        /// <summary>
        /// Contains all corners of a Geometry object.
        /// </summary>
        public class CornerCollection : IEnumerable<TriangleCorner>
        {
            private UnsafeList<TriangleCorner> m_corners;

            internal CornerCollection(UnsafeList<TriangleCorner> corners)
            {
                m_corners = corners;
            }

            public IEnumerator<TriangleCorner> GetEnumerator()
            {
                return m_corners.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return m_corners.GetEnumerator();
            }

            /// <summary>
            /// Returns the index at ghe given index
            /// </summary>
            public TriangleCorner this[int index] => m_corners[index];

            public int Count => m_corners.Count;
        }
    }
}