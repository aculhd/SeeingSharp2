/*
    Seeing# and all applications distributed together with it. 
	Exceptions are projects where it is noted otherwise.
    More info at 
     - https://github.com/RolandKoenig/SeeingSharp2 (sourcecode)
     - http://www.rolandk.de (the authors homepage, german)
    Copyright (C) 2019 Roland K�nig (RolandK)
    
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

using System;
using SeeingSharp.Multimedia.Core;
using SharpDX;

namespace SeeingSharp.Multimedia.Objects
{
    public partial class VertexStructureSurface
    {
        // Members for build-time transform
        private Vector2 m_tileSize = Vector2.Zero;

        /// <summary>
        /// Enables texture tile mode.
        /// </summary>
        public void EnableTextureTileMode(Vector2 tileSize)
        {
            m_tileSize = tileSize;
        }

        /// <summary>
        /// Disables texture tile mode.
        /// </summary>
        public void DisableTextureTileMode()
        {
            m_tileSize = Vector2.Zero;
        }

        /// <summary>
        /// Performs a simple picking test against all triangles of this object.
        /// </summary>
        /// <param name="pickingRay">The picking ray.</param>
        /// <param name="distance">Additional picking options.</param>
        /// <param name="pickingOptions">The distance if picking succeeds.</param>
        public bool Intersects(Ray pickingRay, PickingOptions pickingOptions, out float distance)
        {
            distance = float.MaxValue;
            var result = false;

            for (var loop = 0; loop < IndicesInternal.Count; loop += 3)
            {
                var vertex1 = Owner.VerticesInternal[IndicesInternal[loop]].Position;
                var vertex2 = Owner.VerticesInternal[IndicesInternal[loop + 1]].Position;
                var vertex3 = Owner.VerticesInternal[IndicesInternal[loop + 2]].Position;

                var currentDistance = 0f;

                if (pickingRay.Intersects(ref vertex1, ref vertex2, ref vertex3, out currentDistance))
                {
                    result = true;

                    if (currentDistance < distance)
                    {
                        distance = currentDistance;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Builds a column using 24 vertices.
        /// </summary>
        /// <param name="bottomMiddle">The bottom middle point.</param>
        /// <param name="size">Size on the ground.</param>
        /// <param name="height">Total height of the column.</param>
        public BuiltVerticesRange BuildColumn24V(Vector3 bottomMiddle, float size, float height)
        {
            var startVertex = Owner.CountVertices;

            var halfSize = size / 2f;
            BuildCube24V(
                new Vector3(bottomMiddle.X - halfSize, bottomMiddle.Y, bottomMiddle.Z - halfSize),
                new Vector3(size, height, size));

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        public BuiltVerticesRange BuildHorizontalColumnX24V(Vector3 leftMiddle, float size, float width)
        {
            var startVertex = Owner.CountVertices;

            var halfSize = size / 2f;
            BuildCube24V(
                new Vector3(leftMiddle.X, leftMiddle.Y - halfSize, leftMiddle.Z - halfSize),
                new Vector3(width, size, size));

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        public BuiltVerticesRange BuildHorizontalColumnZ24V(Vector3 frontMiddle, float size, float depth)
        {
            var startVertex = Owner.CountVertices;

            var halfSize = size / 2f;
            BuildCube24V(
                new Vector3(frontMiddle.X - halfSize, frontMiddle.Y - halfSize, frontMiddle.Z),
                new Vector3(size, size, depth));

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a x-axis aligned rectangle.
        /// </summary>
        /// <param name="startPoint">Start point (left center).</param>
        /// <param name="destinationPoint">Destination point (right center).</param>
        /// <param name="size">The size of the rectangle.</param>
        public BuiltVerticesRange BuildXAxisAlignedRect(Vector3 startPoint, Vector3 destinationPoint, float size)
        {
            var startVertex = Owner.CountVertices;

            var halfSize = size / 2f;
            BuildCube24V(
                new Vector3(startPoint.X, startPoint.Y - halfSize, startPoint.Z - halfSize),
                new Vector3(destinationPoint.X - startPoint.X, size, size));

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a z-axis aligned rectangle.
        /// </summary>
        /// <param name="startPoint">Start point (left center).</param>
        /// <param name="destinationPoint">Destination point (right center).</param>
        /// <param name="size">The size of the rectangle.</param>
        public BuiltVerticesRange BuildZAxisAlignedRect(Vector3 startPoint, Vector3 destinationPoint, float size)
        {
            var startVertex = Owner.CountVertices;

            var halfSize = size / 2f;
            BuildCube24V(
                new Vector3(startPoint.X - halfSize, startPoint.Y - halfSize, startPoint.Z),
                new Vector3(size, size, destinationPoint.Z - startPoint.Z));

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        public BuiltVerticesRange BuildCircleFullV(Vector3 middle, float radius, float width, float height, int countOfSegments, Color4 Color)
        {
            var startVertex = Owner.CountVertices;
            if (countOfSegments < 3) { throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", "coundOfSegments"); }

            Matrix3x2 rotationMatrix;

            var halfWidth = width / 2f;
            var halfHeight = height / 2f;
            var nearVector = new Vector2(0f, radius - halfWidth);
            var farVector = new Vector2(0f, radius + halfWidth);
            var lastNearVector = nearVector;
            var lastFarVector = farVector;
            Vector2 nextNearVector;
            Vector2 nextFarVector;

            for (var loop = 1; loop <= countOfSegments; loop++)
            {
                var actPercent = loop / (float)countOfSegments;
                var actAngle = EngineMath.RAD_360DEG * actPercent;

                // Calculate next points
                Matrix3x2.Rotation(actAngle, out rotationMatrix);
                Matrix3x2.TransformPoint(ref rotationMatrix, ref nearVector, out nextNearVector);
                Matrix3x2.TransformPoint(ref rotationMatrix, ref farVector, out nextFarVector);

                // Build current segment
                BuildRect4V(
                    middle + new Vector3(lastNearVector.X, halfHeight, lastNearVector.Y),
                    middle + new Vector3(lastFarVector.X, halfHeight, lastFarVector.Y),
                    middle + new Vector3(nextFarVector.X, halfHeight, nextFarVector.Y),
                    middle + new Vector3(nextNearVector.X, halfHeight, nextNearVector.Y),
                    Vector3.Up,
                    Color4Ex.Transparent);
                BuildRect4V(
                    middle + new Vector3(lastFarVector.X, halfHeight, lastFarVector.Y),
                    middle + new Vector3(lastFarVector.X, -halfHeight, lastFarVector.Y),
                    middle + new Vector3(nextFarVector.X, -halfHeight, nextFarVector.Y),
                    middle + new Vector3(nextFarVector.X, halfHeight, nextFarVector.Y),
                    Vector3.Normalize(new Vector3(lastFarVector.X, 0, lastFarVector.Y)),
                    Color4Ex.Transparent);
                BuildRect4V(
                    middle + new Vector3(lastFarVector.X, -halfHeight, lastFarVector.Y),
                    middle + new Vector3(lastNearVector.X, -halfHeight, lastNearVector.Y),
                    middle + new Vector3(nextNearVector.X, -halfHeight, nextNearVector.Y),
                    middle + new Vector3(nextFarVector.X, -halfHeight, nextFarVector.Y),
                    -Vector3.Up,
                    Color4Ex.Transparent);
                BuildRect4V(
                    middle + new Vector3(lastNearVector.X, halfHeight, lastNearVector.Y),
                    middle + new Vector3(nextNearVector.X, halfHeight, nextNearVector.Y),
                    middle + new Vector3(nextNearVector.X, -halfHeight, nextNearVector.Y),
                    middle + new Vector3(lastNearVector.X, -halfHeight, lastNearVector.Y),
                    Vector3.Normalize(new Vector3(lastFarVector.X, 0, lastFarVector.Y)),
                    Color4Ex.Transparent);

                lastNearVector = nextNearVector;
                lastFarVector = nextFarVector;
            }

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a cone into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cone.</param>
        /// <param name="height">The height of the cone.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color for the generated vertices.</param>
        public BuiltVerticesRange BuildConeFullV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            var startVertex = Owner.CountVertices;

            if (countOfSegments < 5)
            {
                throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", "coundOfSegments");
            }

            var diameter = radius * 2f;

            //Get texture offsets
            var texX = 1f;
            var texY = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = diameter / m_tileSize.X;
                texY = diameter / m_tileSize.Y;
            }

            //Specify bottom and top middle coordinates
            var bottomCoordinate = bottomMiddle;
            var topCoordinate = new Vector3(bottomMiddle.X, bottomMiddle.Y + height, bottomMiddle.Z);

            //Create bottom and top vertices
            var bottomVertex = new Vertex(bottomCoordinate, color, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, -1f, 0f));

            //Add bottom and top vertices to the structure
            var bottomVertexIndex = Owner.AddVertex(bottomVertex);

            //Generate all segments
            var fullRadian = EngineMath.RAD_360DEG;
            var countOfSegmentsF = (float)countOfSegments;

            for (var loop = 0; loop < countOfSegments; loop++)
            {
                //Calculate rotation values for each segment border
                var startRadian = fullRadian * (loop / countOfSegmentsF);
                var targetRadian = fullRadian * ((loop + 1) / countOfSegmentsF);
                var normalRadian = startRadian + (targetRadian - startRadian) / 2f;

                //Generate all normals
                var sideNormal = Vector3Ex.NormalFromHVRotation(normalRadian, 0f);
                var sideLeftNormal = Vector3Ex.NormalFromHVRotation(startRadian, 0f);
                var sideRightNormal = Vector3Ex.NormalFromHVRotation(targetRadian, 0f);

                //Calculate border texture coordinates
                var sideLeftTexCoord = new Vector2(0.5f + sideLeftNormal.X * radius, 0.5f + sideLeftNormal.Z * radius);
                var sideRightTexCoord = new Vector2(0.5f + sideRightNormal.X * radius, 0.5f + sideRightNormal.Z * radius);

                //Generate all points
                var sideLeftBottomCoord = bottomCoordinate + sideLeftNormal * radius;
                var sideRighBottomtCoord = bottomCoordinate + sideRightNormal * radius;
                var sideMiddleBottomCoord = bottomCoordinate + sideNormal * radius;

                //Add segment bottom triangle
                var segmentBottomLeft = bottomVertex.Copy(sideLeftBottomCoord);
                var segmentBottomRight = bottomVertex.Copy(sideRighBottomtCoord);
                AddTriangle(
                    bottomVertexIndex,
                    Owner.AddVertex(segmentBottomLeft),
                    Owner.AddVertex(segmentBottomRight));

                //Generate side normal
                var vectorToTop = topCoordinate - sideMiddleBottomCoord;
                var vectorToTopRotation = Vector3Ex.ToHVRotation(vectorToTop);
                vectorToTopRotation.Y = vectorToTopRotation.Y + EngineMath.RAD_90DEG;
                var topSideNormal = Vector3Ex.NormalFromHVRotation(vectorToTopRotation);

                //Add segment top triangle
                var topVertex = new Vertex(topCoordinate, color, new Vector2(texX / 2f, texY / 2f), topSideNormal);
                var segmentTopLeft = topVertex.Copy(sideLeftBottomCoord);
                var segmentTopRight = topVertex.Copy(sideRighBottomtCoord);

                AddTriangle(
                    Owner.AddVertex(topVertex),
                    Owner.AddVertex(segmentTopRight),
                    Owner.AddVertex(segmentTopLeft));
            }

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        public BuiltVerticesRange BuildCylinderFullV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            return BuildCylinderV(bottomMiddle, radius, height, countOfSegments, color, true, true, true);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        public BuiltVerticesRange BuildCylinderTopV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            return BuildCylinderV(bottomMiddle, radius, height, countOfSegments, color, false, false, true);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        public BuiltVerticesRange BuildCylinderSidesV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            return BuildCylinderV(bottomMiddle, radius, height, countOfSegments, color, true, false, false);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        public BuiltVerticesRange BuildCylinderBottomV(Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color)
        {
            return BuildCylinderV(bottomMiddle, radius, height, countOfSegments, color, false, true, false);
        }

        /// <summary>
        /// Builds a cylinder into the structure with correct texture coordinates and normals.
        /// </summary>
        /// <param name="bottomMiddle">Coordinate of bottom middle.</param>
        /// <param name="radius">The radius of the cylinder.</param>
        /// <param name="height">The height of the cylinder.</param>
        /// <param name="countOfSegments">Total count of segments to generate.</param>
        /// <param name="color">The color to be applied on the vertices.</param>
        /// <param name="buildBottom">Build bottom of the cylinder.</param>
        /// <param name="buildSides">Build sides of the cylinder.</param>
        /// <param name="buildTop">Build top side of the cylinder.</param>
        public BuiltVerticesRange BuildCylinderV(
            Vector3 bottomMiddle, float radius, float height, int countOfSegments, Color4 color,
            bool buildSides, bool buildBottom, bool buildTop)
        {
            var startVertex = Owner.CountVertices;

            if (countOfSegments < 5) { throw new ArgumentException("Segment count of " + countOfSegments + " is too small!", "coundOfSegments"); }
            var diameter = radius * 2f;

            //Get texture offsets
            var texX = 1f;
            var texY = 1f;
            var texSegmentY = 1f;
            var texSegmentX = 1f;
            if (m_tileSize != Vector2.Zero)
            {
                texX = diameter / m_tileSize.X;
                texY = diameter / m_tileSize.Y;
                texSegmentY = height / m_tileSize.Y;
                texSegmentX = EngineMath.RAD_180DEG * diameter / m_tileSize.X;
            }

            //Specify bottom and top middle coordinates
            var bottomCoordinate = bottomMiddle;
            var topCoordinate = new Vector3(bottomMiddle.X, bottomMiddle.Y + height, bottomMiddle.Z);

            //Create bottom and top vertices
            var bottomVertex = new Vertex(bottomCoordinate, color, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, -1f, 0f));
            var topVertex = new Vertex(topCoordinate, color, new Vector2(texX / 2f, texY / 2f), new Vector3(0f, 1f, 0f));

            //Add bottom and top vertices to the structure
            var bottomVertexIndex = Owner.AddVertex(bottomVertex);
            var topVertexIndex = Owner.AddVertex(topVertex);

            //Generate all segments
            var fullRadian = EngineMath.RAD_360DEG;
            var countOfSegmentsF = (float)countOfSegments;

            for (var loop = 0; loop < countOfSegments; loop++)
            {
                //Calculate rotation values for each segment border
                var startRadian = fullRadian * (loop / countOfSegmentsF);
                var targetRadian = fullRadian * ((loop + 1) / countOfSegmentsF);
                var normalRadian = startRadian + (targetRadian - startRadian) / 2f;

                //Generate all normals
                var sideNormal = Vector3Ex.NormalFromHVRotation(normalRadian, 0f);
                var sideLeftNormal = Vector3Ex.NormalFromHVRotation(startRadian, 0f);
                var sideRightNormal = Vector3Ex.NormalFromHVRotation(targetRadian, 0f);

                //
                var sideLeftTexCoord = new Vector2(0.5f + sideLeftNormal.X * radius, 0.5f + sideLeftNormal.Z * radius);
                var sideRightTexCoord = new Vector2(0.5f + sideRightNormal.X * radius, 0.5f + sideRightNormal.Z * radius);

                //Generate all points
                var sideLeftBottomCoord = bottomCoordinate + sideLeftNormal * radius;
                var sideRighBottomtCoord = bottomCoordinate + sideRightNormal * radius;
                var sideLeftTopCoord = new Vector3(sideLeftBottomCoord.X, sideLeftBottomCoord.Y + height, sideLeftBottomCoord.Z);
                var sideRightTopCoord = new Vector3(sideRighBottomtCoord.X, sideRighBottomtCoord.Y + height, sideRighBottomtCoord.Z);

                //Add segment bottom triangle
                if (buildBottom)
                {
                    var segmentBottomLeft = bottomVertex.Copy(sideLeftBottomCoord, sideLeftTexCoord);
                    var segmentBottomRight = bottomVertex.Copy(sideRighBottomtCoord, sideRightTexCoord);
                    AddTriangle(
                        bottomVertexIndex,
                        Owner.AddVertex(segmentBottomLeft),
                        Owner.AddVertex(segmentBottomRight));
                }

                //Add segment top triangle
                if (buildTop)
                {
                    var segmentTopLeft = topVertex.Copy(sideLeftTopCoord, sideLeftTexCoord);
                    var segmentTopRight = topVertex.Copy(sideRightTopCoord, sideRightTexCoord);
                    AddTriangle(
                        topVertexIndex,
                        Owner.AddVertex(segmentTopRight),
                        Owner.AddVertex(segmentTopLeft));
                }

                if (buildSides)
                {
                    //Calculate texture coords for side segment
                    var texCoordSegmentStart = new Vector2(texSegmentX * (loop / (float)countOfSegments), 0f);
                    var texCoordSegmentTarget = new Vector2(texSegmentX * ((loop + 1) / (float)countOfSegments), texSegmentY);

                    //Add segment side
                    BuildRect4V(sideLeftBottomCoord, sideRighBottomtCoord, sideRightTopCoord, sideLeftTopCoord, sideNormal, color, texCoordSegmentStart, texCoordSegmentTarget);
                }
            }

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a sphere geometry.
        /// </summary>
        public BuiltVerticesRange BuildShpere(int tDiv, int pDiv, double radius, Color4 color)
        {
            var startVertex = Owner.CountVertices;

            var dt = Math.PI * 2 / tDiv;
            var dp = Math.PI / pDiv;

            for (var pi = 0; pi <= pDiv; pi++)
            {
                var phi = pi * dp;

                for (var ti = 0; ti <= tDiv; ti++)
                {
                    // we want to start the mesh on the x axis
                    var theta = ti * dt;

                    var position = SphereGetPosition(theta, phi, radius);
                    var vertex = new Vertex(
                        position,
                        color,
                        SphereGetTextureCoordinate(theta, phi),
                        Vector3.Normalize(position));
                    Owner.Vertices.Add(vertex);
                }
            }

            for (var pi = 0; pi < pDiv; pi++)
            {
                for (var ti = 0; ti < tDiv; ti++)
                {
                    var x0 = ti;
                    var x1 = ti + 1;
                    var y0 = pi * (tDiv + 1);
                    var y1 = (pi + 1) * (tDiv + 1);

                    Triangles.Add(
                        x0 + y0,
                        x0 + y1,
                        x1 + y0);

                    Triangles.Add(
                        x1 + y0,
                        x0 + y1,
                        x1 + y1);
                }
            }

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a cube into a vertex structure (this cube is built up of just 8 vertices, so not texturing is supported)
        /// </summary>
        /// <param name="start">Start point of the cube (left-lower-front point)</param>
        /// <param name="size">Size of the cube</param>
        public BuiltVerticesRange BuildCube8V(Vector3 start, Vector3 size)
        {
            return BuildCube8V(start, size, Color4.White);
        }

        /// <summary>
        /// Builds a cube into a vertex structure (this cube is built up of just 8 vertices, so no texturing is supported)
        /// </summary>
        /// <param name="start">Start point of the cube (left-lower-front point)</param>
        /// <param name="size">Size of the cube</param>
        /// <param name="color">Color of the cube</param>
        public BuiltVerticesRange BuildCube8V(Vector3 start, Vector3 size, Color4 color)
        {
            var startVertex = Owner.CountVertices;

            var dest = start + size;
            var vertex = new Vertex(start, color, new Vector2());

            var a = Owner.AddVertex(vertex);
            var b = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z)));
            var c = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z)));
            var d = Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z)));
            var e = Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z)));
            var f = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z)));
            var g = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z)));
            var h = Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z)));

            AddTriangle(a, e, f);  //front side
            AddTriangle(f, b, a);
            AddTriangle(b, f, g);  //right side
            AddTriangle(g, c, b);
            AddTriangle(c, g, h);  //back side
            AddTriangle(h, d, c);
            AddTriangle(d, h, e);  //left side
            AddTriangle(e, a, d);
            AddTriangle(e, h, g);  //top side
            AddTriangle(g, f, e);
            AddTriangle(a, b, c);  //botton side
            AddTriangle(c, d, a);

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="start">Start point of the cube</param>
        /// <param name="size">Size of the cube</param>
        public BuiltVerticesRange BuildCube24V(Vector3 start, Vector3 size)
        {
            return BuildCube24V(start, size, Color4.White);
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="start">Start point of the cube</param>
        /// <param name="size">Size of the cube</param>
        /// <param name="color">Color of the cube</param>
        public BuiltVerticesRange BuildCube24V(Vector3 start, Vector3 size, Color4 color)
        {
            var result = new BuiltVerticesRange(Owner);

            result.Merge(BuildCubeSides16V(start, size, color));
            result.Merge(BuildCubeTop4V(start, size, color));
            result.Merge(BuildCubeBottom4V(start, size, color));

            return result;
        }

        /// <summary>
        /// Builds a cube of 4 vertices and a defined hight.
        /// </summary>
        /// <param name="topA"></param>
        /// <param name="topB"></param>
        /// <param name="topC"></param>
        /// <param name="topD"></param>
        /// <param name="heigh"></param>
        /// <param name="color"></param>
        public BuiltVerticesRange BuildCube24V(Vector3 topA, Vector3 topB, Vector3 topC, Vector3 topD, float heigh, Color4 color)
        {
            var result = new BuiltVerticesRange(Owner)
            {
                StartVertex = Owner.CountVertices
            };

            var startTriangleIndex = CountTriangles;

            // Calculate texture coordinates
            var size = new Vector3(
                (topB - topA).Length(),
                Math.Abs(heigh),
                (topC - topB).Length());
            var texX = 1f;
            var texY = 1f;
            var texZ = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = size.X / m_tileSize.X;
                texY = size.Y / m_tileSize.Y;
                texZ = size.Z / m_tileSize.X;
            }

            // Calculate bottom vectors
            var bottomA = new Vector3(topA.X, topA.Y - heigh, topA.Z);
            var bottomB = new Vector3(topB.X, topB.Y - heigh, topB.Z);
            var bottomC = new Vector3(topC.X, topC.Y - heigh, topC.Z);
            var bottomD = new Vector3(topD.X, topD.Y - heigh, topD.Z);

            // Build Top side
            var vertex = new Vertex(topA, color, new Vector2(texX, 0f), new Vector3(0f, 1f, 0f));
            var a = Owner.AddVertex(vertex);
            var b = Owner.AddVertex(vertex.Copy(topB, new Vector2(texX, texY)));
            var c = Owner.AddVertex(vertex.Copy(topC, new Vector2(0f, texY)));
            var d = Owner.AddVertex(vertex.Copy(topD, new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            // Build Bottom side
            vertex = new Vertex(topA, color, new Vector2(0f, 0f), new Vector3(0f, -1f, 0f));
            a = Owner.AddVertex(vertex);
            b = Owner.AddVertex(vertex.Copy(topD, new Vector2(texX, 0f)));
            c = Owner.AddVertex(vertex.Copy(topC, new Vector2(texX, texY)));
            d = Owner.AddVertex(vertex.Copy(topB, new Vector2(0f, texY)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            // Build Front side
            vertex = new Vertex(topA, color, new Vector2(0f, texY), new Vector3(0f, 0f, -1f));
            a = Owner.AddVertex(vertex);
            b = Owner.AddVertex(vertex.Copy(topB, new Vector2(texX, texY)));
            c = Owner.AddVertex(vertex.Copy(bottomB, new Vector2(texX, 0f)));
            d = Owner.AddVertex(vertex.Copy(bottomA, new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            // Build Right side
            a = Owner.AddVertex(vertex.Copy(topB, new Vector3(1f, 0f, 0f), new Vector2(0f, texY)));
            b = Owner.AddVertex(vertex.Copy(topC, new Vector3(1f, 0f, 0f), new Vector2(texZ, texY)));
            c = Owner.AddVertex(vertex.Copy(bottomC, new Vector3(1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = Owner.AddVertex(vertex.Copy(bottomB, new Vector3(1f, 0f, 0f), new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            // Build Back side
            a = Owner.AddVertex(vertex.Copy(topC, new Vector3(0f, 0f, 1f), new Vector2(0f, texY)));
            b = Owner.AddVertex(vertex.Copy(topD, new Vector3(0f, 0f, 1f), new Vector2(texX, texY)));
            c = Owner.AddVertex(vertex.Copy(bottomD, new Vector3(0f, 0f, 1f), new Vector2(texX, 0f)));
            d = Owner.AddVertex(vertex.Copy(bottomC, new Vector3(0f, 0f, 1f), new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            // Build Left side
            a = Owner.AddVertex(vertex.Copy(topD, new Vector3(-1f, 0f, 0f), new Vector2(0f, texY)));
            b = Owner.AddVertex(vertex.Copy(topA, new Vector3(-1f, 0f, 0f), new Vector2(texZ, texY)));
            c = Owner.AddVertex(vertex.Copy(bottomA, new Vector3(-1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = Owner.AddVertex(vertex.Copy(bottomD, new Vector3(-1f, 0f, 0f), new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            // Calculate normals finally
            CalculateNormalsFlat(startTriangleIndex, CountTriangles - startTriangleIndex);

            result.VertexCount = Owner.CountVertices - result.StartVertex;
            return result;
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="box">Box defining bounds of generated cube.</param>
        /// <param name="color">Color of generated vertices.</param>
        public BuiltVerticesRange BuildCube24V(BoundingBox box, Color4 color)
        {
            return BuildCube24V(box.Minimum, box.Size, color);
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="box">Box defining bounds of generated cube.</param>
        public BuiltVerticesRange BuildCube24V(BoundingBox box)
        {
            return BuildCube24V(box, Color4.White);
        }

        /// <summary>
        /// Builds a cube on the given point with the given color.
        /// </summary>
        /// <param name="centerLocation">The location to draw the cube at.</param>
        /// <param name="sideLength">The side length of the cube.</param>
        /// <param name="color">The color to be used.</param>
        public BuiltVerticesRange BuildCube24V(Vector3 centerLocation, float sideLength, Color4 color)
        {
            return BuildCube24V(
                centerLocation - new Vector3(sideLength / 2f, sideLength / 2f, sideLength / 2f),
                new Vector3(sideLength, sideLength, sideLength),
                color);
        }

        /// <summary>
        /// Builds a cube into this VertexStructure (this cube is built up of 24 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="bottomCenter">Bottom center point of the cube.</param>
        /// <param name="width">Width (and depth) of the cube.</param>
        /// <param name="height">Height of the cube.</param>
        /// <param name="color">Color of the cube</param>
        public BuiltVerticesRange BuildCube24V(Vector3 bottomCenter, float width, float height, Color4 color)
        {
            var start = new Vector3(
                bottomCenter.X - width / 2f,
                bottomCenter.Y,
                bottomCenter.Z - width / 2f);
            var size = new Vector3(width, height, width);
            return BuildCube24V(start, size, color);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, TextureCoordinateCalculationAlignment uCoordAlignment, TextureCoordinateCalculationAlignment vCoordAlignment, float coordRepeatUnit)
        {
            var startVertex = Owner.CountVertices;

            //Define texture coordinate calculation functions
            Func<Vector3, float> caluclateU = actPosition =>
            {
                switch (uCoordAlignment)
                {
                    case TextureCoordinateCalculationAlignment.XAxis:
                        return actPosition.X / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.YAxis:
                        return actPosition.Y / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.ZAxis:
                        return actPosition.Z / coordRepeatUnit;
                }
                return 0f;
            };
            Func<Vector3, float> calculateV = actPosition =>
            {
                switch (vCoordAlignment)
                {
                    case TextureCoordinateCalculationAlignment.XAxis:
                        return actPosition.X / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.YAxis:
                        return actPosition.Y / coordRepeatUnit;

                    case TextureCoordinateCalculationAlignment.ZAxis:
                        return actPosition.Z / coordRepeatUnit;
                }
                return 0f;
            };

            var textureCoordinate = new Vector2(caluclateU(pointA), calculateV(pointA));
            var vertex = new Vertex(pointA, Color4.White, textureCoordinate, normal);

            var a = Owner.AddVertex(vertex);
            var b = Owner.AddVertex(vertex.Copy(pointB, new Vector2(caluclateU(pointB), calculateV(pointB))));
            var c = Owner.AddVertex(vertex.Copy(pointC, new Vector2(caluclateU(pointC), calculateV(pointC))));
            var d = Owner.AddVertex(vertex.Copy(pointD, new Vector2(caluclateU(pointD), calculateV(pointD))));

            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            return new BuiltVerticesRange(Owner, startVertex, Owner.CountVertices - startVertex);
        }

        /// <summary>
        /// Builds the top side of a cube into this VertexStructure (Built up of 4 vertices, so texture coordinates and normals are set)
        /// </summary>
        public BuiltVerticesRange BuildCubeTop4V(Vector3 start, Vector3 size, Color4 color)
        {
            var dest = start + size;

            return BuildRect4V(
                new Vector3(start.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, start.Z),
                new Vector3(dest.X, dest.Y, dest.Z),
                new Vector3(start.X, dest.Y, dest.Z),
                new Vector3(0f, 1f, 0f),
                color);
        }

        /// <summary>
        /// Builds the bottom side of a cube into this VertexStructure (Built up of 4 vertices, so texture coordinates and normals are set)
        /// </summary>
        public BuiltVerticesRange BuildCubeBottom4V(Vector3 start, Vector3 size, Color4 color)
        {
            var dest = start + size;

            return BuildRect4V(
                new Vector3(start.X, start.Y, dest.Z),
                new Vector3(dest.X, start.Y, dest.Z),
                new Vector3(dest.X, start.Y, start.Z),
                new Vector3(start.X, start.Y, start.Z),
                new Vector3(0f, -1f, 0f),
                color);
        }

        /// <summary>
        /// Builds cube sides into this VertexStructure (these sides are built up of  16 vertices, so texture coordinates and normals are set)
        /// </summary>
        /// <param name="start">Start poiint of the cube</param>
        /// <param name="size">Size of the cube</param>
        /// <param name="color">Color of the cube</param>
        public BuiltVerticesRange BuildCubeSides16V(Vector3 start, Vector3 size, Color4 color)
        {
            var result = new BuiltVerticesRange(Owner)
            {
                StartVertex = Owner.CountVertices
            };

            var dest = start + size;

            var texX = 1f;
            var texY = 1f;
            var texZ = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = size.X / m_tileSize.X;
                texY = size.Y / m_tileSize.Y;
                texZ = size.Z / m_tileSize.X;
            }

            //Front side
            var vertex = new Vertex(start, color, new Vector2(0f, texY), new Vector3(0f, 0f, -1f));
            var a = Owner.AddVertex(vertex);
            var b = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z), new Vector2(texX, texY)));
            var c = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z), new Vector2(texX, 0f)));
            var d = Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z), new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            //Right side
            a = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, start.Z), new Vector3(1f, 0f, 0f), new Vector2(0f, texY)));
            b = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z), new Vector3(1f, 0f, 0f), new Vector2(texZ, texY)));
            c = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z), new Vector3(1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, start.Z), new Vector3(1f, 0f, 0f), new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            //Back side
            a = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, start.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(0f, texY)));
            b = Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(texX, texY)));
            c = Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(texX, 0f)));
            d = Owner.AddVertex(vertex.Copy(new Vector3(dest.X, dest.Y, dest.Z), new Vector3(0f, 0f, 1f), new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            //Left side
            a = Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, dest.Z), new Vector3(-1f, 0f, 0f), new Vector2(0f, texY)));
            b = Owner.AddVertex(vertex.Copy(new Vector3(start.X, start.Y, start.Z), new Vector3(-1f, 0f, 0f), new Vector2(texZ, texY)));
            c = Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, start.Z), new Vector3(-1f, 0f, 0f), new Vector2(texZ, 0f)));
            d = Owner.AddVertex(vertex.Copy(new Vector3(start.X, dest.Y, dest.Z), new Vector3(-1f, 0f, 0f), new Vector2(0f, 0f)));
            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            result.VertexCount = Owner.CountVertices - result.StartVertex;
            return result;
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD)
        {
            return BuildRect4V(pointA, pointB, pointC, pointD, Color4.White);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Color4 color)
        {
            var texX = 1f;
            var texY = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / m_tileSize.X;
                texY = (pointC - pointB).Length() / m_tileSize.Y;
            }

            var vertex = new Vertex(pointA, color, new Vector2(0f, texY));

            var a = Owner.AddVertex(vertex);
            var b = Owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            var c = Owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));
            var d = Owner.AddVertex(vertex.Copy(pointD, new Vector2(0f, 0f)));

            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            return new BuiltVerticesRange(Owner, Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal)
        {
            return BuildRect4V(pointA, pointB, pointC, pointD, normal, Color4.White);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, Color4 color)
        {
            var texX = 1f;
            var texY = 1f;

            if (m_tileSize != Vector2.Zero)
            {
                texX = (pointB - pointA).Length() / m_tileSize.X;
                texY = (pointC - pointB).Length() / m_tileSize.Y;
            }

            var vertex = new Vertex(pointA, color, new Vector2(0f, texY), normal);

            var a = Owner.AddVertex(vertex);
            var b = Owner.AddVertex(vertex.Copy(pointB, new Vector2(texX, texY)));
            var c = Owner.AddVertex(vertex.Copy(pointC, new Vector2(texX, 0f)));
            var d = Owner.AddVertex(vertex.Copy(pointD, new Vector2(0f, 0f)));

            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            return new BuiltVerticesRange(Owner, Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Build a single rectangle into the vertex structure (Supports texturing and normal vectors)
        /// </summary>
        public BuiltVerticesRange BuildRect4V(Vector3 pointA, Vector3 pointB, Vector3 pointC, Vector3 pointD, Vector3 normal, Color4 color, Vector2 minTexCoord, Vector2 maxTexCoord)
        {
            var vertex = new Vertex(pointA, color, new Vector2(minTexCoord.X, maxTexCoord.Y), normal);

            var a = Owner.AddVertex(vertex);
            var b = Owner.AddVertex(vertex.Copy(pointB, new Vector2(maxTexCoord.X, maxTexCoord.Y)));
            var c = Owner.AddVertex(vertex.Copy(pointC, new Vector2(maxTexCoord.X, minTexCoord.Y)));
            var d = Owner.AddVertex(vertex.Copy(pointD, new Vector2(minTexCoord.X, minTexCoord.Y)));

            AddTriangle(a, c, b);
            AddTriangle(a, d, c);

            return new BuiltVerticesRange(Owner, Owner.CountVertices - 4, 4);
        }

        /// <summary>
        /// Changes the index order of each triangle.
        /// </summary>
        public void ToggleTriangleIndexOrder()
        {
            for (var loop = 2; loop < IndicesInternal.Count; loop += 3)
            {
                var index1 = IndicesInternal[loop - 2];
                var index2 = IndicesInternal[loop - 1];
                var index3 = IndicesInternal[loop];
                IndicesInternal[loop] = index1;
                IndicesInternal[loop - 1] = index2;
                IndicesInternal[loop - 2] = index3;
            }
        }

        /// <summary>
        /// Helper method for spehere creation.
        /// </summary>
        private Vector3 SphereGetPosition(double theta, double phi, double radius)
        {
            var x = radius * Math.Sin(theta) * Math.Sin(phi);
            var y = radius * Math.Cos(phi);
            var z = radius * Math.Cos(theta) * Math.Sin(phi);

            return new Vector3((float)x, (float)y, (float)z);
        }

        /// <summary>
        /// Helper method for spehere creation.
        /// </summary>
        private Vector2 SphereGetTextureCoordinate(double theta, double phi)
        {
            return new Vector2(
                (float)(theta / (2 * Math.PI)),
                (float)(phi / Math.PI));
        }
    }
}