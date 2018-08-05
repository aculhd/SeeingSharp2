﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Text;

namespace SeeingSharp
{
    public static class BoundingBoxEx
    {
        public static BoundingBox Create(IEnumerable<Vector3> containedLocations)
        {
            Vector3 minimum = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
            Vector3 maximum = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            bool anyInteration = false;
            foreach (Vector3 actContainedLocation in containedLocations)
            {
                anyInteration = true;

                if (minimum.X > actContainedLocation.X) { minimum.X = actContainedLocation.X; }
                if (minimum.Y > actContainedLocation.Y) { minimum.Y = actContainedLocation.Y; }
                if (minimum.Z > actContainedLocation.Z) { minimum.Z = actContainedLocation.Z; }

                if (maximum.X < actContainedLocation.X) { maximum.X = actContainedLocation.X; }
                if (maximum.Y < actContainedLocation.Y) { maximum.Y = actContainedLocation.Y; }
                if (maximum.Z < actContainedLocation.Z) { maximum.Z = actContainedLocation.Z; }
            }

            if (!anyInteration) { throw new SeeingSharpException("No vectors given!"); }

            return new BoundingBox(minimum, maximum);
        }

        public static void Transform(this ref BoundingBox bBox, Matrix matrix)
        {
            Vector3[] corners = bBox.GetCorners();
            for (int loop = 0; loop < corners.Length; loop++)
            {
                corners[loop] = Vector3.Transform(corners[loop], matrix).ToXYZ();
            }

            bBox.Redefine(corners);
        }

        /// <summary>
        /// Redefines this bounding box based on given points.
        /// </summary>
        /// <param name="points">All points to apply.</param>
        public static void Redefine(this ref BoundingBox boundingBox, Vector3[] points)
        {
            Vector3 min = new Vector3(float.MaxValue);
            Vector3 max = new Vector3(float.MinValue);
            for (int i = 0; i < points.Length; ++i)
            {
                min = Vector3.Min(min, points[i]);
                max = Vector3.Max(max, points[i]);
            }

            boundingBox.Minimum = min;
            boundingBox.Maximum = max;
        }

        /// <summary>
        /// Expands this AxisAlignedBox so that it contains the given location.
        /// </summary>
        /// <param name="newLocation">New location to be merged to this AxisAlignedBox.</param>
        public static void MergeWith(this ref BoundingBox boundingBox, Vector3 newLocation)
        {
            //Handle x axis
            if (newLocation.X < boundingBox.Minimum.X) { boundingBox.Minimum.X = newLocation.X; }
            else if (newLocation.X > boundingBox.Maximum.X) { boundingBox.Maximum.X = newLocation.X; }

            //Handle y axis
            if (newLocation.Y < boundingBox.Minimum.Y) { boundingBox.Minimum.Y = newLocation.Y; }
            else if (newLocation.Y > boundingBox.Maximum.Y) { boundingBox.Maximum.Y = newLocation.Y; }

            //Handle z axis
            if (newLocation.Z < boundingBox.Minimum.Z) { boundingBox.Minimum.Z = newLocation.Z; }
            else if (newLocation.Z > boundingBox.Maximum.Z) { boundingBox.Maximum.Z = newLocation.Z; }
        }

        /// <summary>
        /// Merges this box with the given one
        /// </summary>
        public static void MergeWith(this ref BoundingBox boundingBox, BoundingBox other)
        {
            Vector3 minimum1 = boundingBox.Minimum;
            Vector3 minimum2 = other.Minimum;
            Vector3 maximum1 = boundingBox.Maximum;
            Vector3 maximum2 = other.Maximum;

            Vector3 newMinimum = Vector3.Min(minimum1, minimum2);
            Vector3 newMaximum = Vector3.Max(maximum1, maximum2);

            boundingBox.Minimum = newMinimum;
            boundingBox.Maximum = newMaximum;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-left border.
        /// </summary>
        public static Vector3 GetBottomLeftMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Minimum.Y;
            result.X = boundingBox.Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-right border.
        /// </summary>
        public static Vector3 GetBottomRightMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Minimum.Y;
            result.X = boundingBox.Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-front border.
        /// </summary>
        public static Vector3 GetBottomFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Minimum.Y;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of bottom-back border.
        /// </summary>
        public static Vector3 GetBottomBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Minimum.Y;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of the middle of the box.
        /// </summary>
        public static Vector3 GetMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            return boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
        }

        /// <summary>
        /// Gets the coordinate of middle of top rectangle.
        /// </summary>
        public static Vector3 GetTopMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-left border.
        /// </summary>
        public static Vector3 GetTopLeftMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            result.X = boundingBox.Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-right border.
        /// </summary>
        public static Vector3 GetTopRightMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            result.X = boundingBox.Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-front border.
        /// </summary>
        public static Vector3 GetTopFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of top-back border.
        /// </summary>
        public static Vector3 GetTopBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Y = boundingBox.Maximum.Y;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of front rectangle.
        /// </summary>
        public static Vector3 GetFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of back rectangle.
        /// </summary>
        public static Vector3 GetBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left rectangle.
        /// </summary>
        public static Vector3 GetLeftMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Minimum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left-front border.
        /// </summary>
        public static Vector3 GetLeftFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Minimum.X;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of left-back border.
        /// </summary>
        public static Vector3 GetLeftBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Minimum.X;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right rectangle.
        /// </summary>
        public static Vector3 GetRightMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Maximum.X;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right-front border.
        /// </summary>
        public static Vector3 GetRightFrontMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Maximum.X;
            result.Z = boundingBox.Minimum.Z;
            return result;
        }

        /// <summary>
        /// Gets the coordinate of middle of right-back border.
        /// </summary>
        public static Vector3 GetRightBackMiddleCoordinate(this ref BoundingBox boundingBox)
        {
            Vector3 result = boundingBox.Minimum + (boundingBox.Maximum - boundingBox.Minimum) / 2f;
            result.X = boundingBox.Maximum.X;
            result.Z = boundingBox.Maximum.Z;
            return result;
        }

        /// <summary>
        /// Gets the middle center of the box.
        /// </summary>
        public static Vector3 GetMiddleCenter(this ref BoundingBox boundingBox)
        {
            Vector3 size = boundingBox.Size;
            return new Vector3(
                boundingBox.Minimum.X + size.X / 2f,
                boundingBox.Minimum.Y + size.Y / 2f,
                boundingBox.Minimum.Z + size.Z / 2f);
        }

        /// <summary>
        /// Gets the bottom center of the box.
        /// </summary>
        public static Vector3 GetBottomCenter(this ref BoundingBox boundingBox)
        {
            Vector3 size = boundingBox.Size;
            return new Vector3(
                boundingBox.Minimum.X + size.X / 2f,
                boundingBox.Minimum.Y,
                boundingBox.Minimum.Z + size.Z / 2f);
        }

        /// <summary>
        /// Builds a line list containing lines for all borders of this box.
        /// </summary>
        public static List<Vector3> BuildLineListForBorders(this ref BoundingBox boundingBox)
        {
            List<Vector3> result = new List<Vector3>();
            Vector3 size = boundingBox.Size;

            //Add front face
            result.Add(boundingBox.Minimum);
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(boundingBox.Minimum);

            //Add back face
            result.Add(boundingBox.Minimum + new Vector3(0f, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(0f, 0f, size.Z));

            //Add connections
            result.Add(boundingBox.Minimum);
            result.Add(boundingBox.Minimum + new Vector3(0f, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, 0f, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(size.X, size.Y, size.Z));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, 0f));
            result.Add(boundingBox.Minimum + new Vector3(0f, size.Y, size.Z));

            return result;
        }

        /// <summary>
        /// Is this box empty?
        /// </summary>
        public static bool IsEmpty(this ref BoundingBox boundingBox)
        {
            return boundingBox.Minimum.IsEmpty() && boundingBox.Maximum.IsEmpty();
        }
    }
}