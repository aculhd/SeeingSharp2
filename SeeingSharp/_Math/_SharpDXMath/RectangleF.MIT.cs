﻿using System;
using System.Numerics;
using System.Runtime.InteropServices;

namespace SeeingSharp
{
    /// <summary>
    /// Define a RectangleF. This structure is slightly different from System.Drawing.RectangleF as It is 
    /// internally storing Left,Top,Right,Bottom instead of Left,Top,Width,Height.
    /// Although automatic casting from a to System.Drawing.Rectangle is provided by this class.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public partial struct RectangleF : IEquatable<RectangleF>
    {
        private float _left;
        private float _top;
        private float _right;
        private float _bottom;

        /// <summary>
        /// An empty rectangle
        /// </summary>
        public static readonly RectangleF Empty;

        static RectangleF()
        {
            Empty = new RectangleF();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RectangleF"/> struct.
        /// </summary>
        /// <param name="x">The left.</param>
        /// <param name="y">The top.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public RectangleF(float x, float y, float width, float height)
        {
            _left = x;
            _top = y;
            _right = x + width;
            _bottom = y + height;
        }

        /// <summary>
        /// Checks, if specified point is inside <see cref="SeeingSharp.RectangleF"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="SeeingSharp.RectangleF"/>, otherwise <c>false</c>.</returns>
        public bool Contains(int x, int y)
        {
            if (x >= _left && x <= _right && y >= _top && y <= _bottom)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks, if specified point is inside <see cref="SeeingSharp.RectangleF"/>.
        /// </summary>
        /// <param name="x">X point coordinate.</param>
        /// <param name="y">Y point coordinate.</param>
        /// <returns><c>true</c> if point is inside <see cref="SeeingSharp.RectangleF"/>, otherwise <c>false</c>.</returns>
        public bool Contains(float x, float y)
        {
            if (x >= _left && x <= _right && y >= _top && y <= _bottom)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks, if specified <see cref="System.Numerics.Vector2"/> is inside <see cref="SeeingSharp.RectangleF"/>. 
        /// </summary> 
        /// <param name="vector2D">Coordinate <see cref="System.Numerics.Vector2"/>.</param>
        /// <returns><c>true</c> if <see cref="System.Numerics.Vector2"/> is inside <see cref="SeeingSharp.RectangleF"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Vector2 vector2D)
        {
            if (vector2D.X >= _left && vector2D.X <= _right && vector2D.Y >= _top && vector2D.Y <= _bottom)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Checks, if specified <see cref="SeeingSharp.Point"/> is inside <see cref="SeeingSharp.RectangleF"/>. 
        /// </summary>
        /// <param name="point">Coordinate <see cref="SeeingSharp.Point"/>.</param> 
        /// <returns><c>true</c> if <see cref="SeeingSharp.Point"/> is inside <see cref="SeeingSharp.RectangleF"/>, otherwise <c>false</c>.</returns>
        public bool Contains(Point point)
        {
            if (point.X >= _left && point.X <= _right && point.Y >= _top && point.Y <= _bottom)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets or sets the left.
        /// </summary>
        /// <value>The left.</value>
        public float Left
        {
            get => _left;
            set
            {
                _right = value + this.Width;
                _left = value;
            }
        }

        /// <summary>
        /// Gets or sets the top.
        /// </summary>
        /// <value>The top.</value>
        public float Top
        {
            get => _top;
            set
            {
                _bottom = value + this.Height;
                _top = value;
            }
        }

        /// <summary>
        /// Gets or sets the right.
        /// </summary>
        /// <value>The right.</value>
        public float Right
        {
            get => _right;
            set => _right = value;
        }

        /// <summary>
        /// Gets or sets the bottom.
        /// </summary>
        /// <value>The bottom.</value>
        public float Bottom
        {
            get => _bottom;
            set => _bottom = value;
        }

        /// <summary>
        /// Gets the left position.
        /// </summary>
        /// <value>The left position.</value>
        public float X
        {
            get => _left;
            set
            {
                _right = value + this.Width;
                _left = value;
            }
        }

        /// <summary>
        /// Gets the top position.
        /// </summary>
        /// <value>The top position.</value>
        public float Y
        {
            get => _top;
            set
            {
                _bottom = value + this.Height;
                _top = value;
            }
        }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public float Width
        {
            get => _right - _left;
            set => _right = _left + value;
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public float Height
        {
            get => _bottom - _top;
            set => _bottom = _top + value;
        }

        /// <summary>Gets or sets the upper-left value of the Rectangle.</summary>
        public Vector2 Location
        {
            get => new Vector2(this.X, this.Y);
            set
            {
                this.X = value.X;
                this.Y = value.Y;
            }
        }
        /// <summary>Gets the Point that specifies the center of the rectangle.</summary>
        public Vector2 Center => new Vector2(this.X + this.Width / 2, this.Y + this.Height / 2);

        /// <summary>Gets a value that indicates whether the Rectangle is empty.</summary>
        public bool IsEmpty => this.Width == 0.0f && this.Height == 0.0f && this.X == 0.0f && this.Y == 0.0f;

        /// <summary>Changes the position of the Rectangle.</summary>
        /// <param name="amount">The values to adjust the position of the Rectangle by.</param>
        public void Offset(Point amount)
        {
            this.X += amount.X;
            this.Y += amount.Y;
        }

        /// <summary>Changes the position of the Rectangle.</summary>
        /// <param name="offsetX">Change in the x-position.</param>
        /// <param name="offsetY">Change in the y-position.</param>
        public void Offset(int offsetX, int offsetY)
        {
            this.X += offsetX;
            this.Y += offsetY;
        }

        /// <summary>Pushes the edges of the Rectangle out by the horizontal and vertical values specified.</summary>
        /// <param name="horizontalAmount">Value to push the sides out by.</param>
        /// <param name="verticalAmount">Value to push the top and bottom out by.</param>
        public void Inflate(int horizontalAmount, int verticalAmount)
        {
            this.X -= horizontalAmount;
            this.Y -= verticalAmount;
            this.Width += horizontalAmount * 2;
            this.Height += verticalAmount * 2;
        }

        /// <summary>Determines whether this Rectangle contains a specified Point.</summary>
        /// <param name="value">The Point to evaluate.</param>
        /// <param name="result">[OutAttribute] true if the specified Point is contained within this Rectangle; false otherwise.</param>
        public void Contains(ref Vector2 value, out bool result)
        {
            result = this.X <= value.X && value.X < this.Right && this.Y <= value.Y && value.Y < this.Bottom;
        }

        /// <summary>Determines whether this Rectangle entirely contains a specified Rectangle.</summary>
        /// <param name="value">The Rectangle to evaluate.</param>
        public bool Contains(Rectangle value)
        {
            return this.X <= value.X && value.Right <= this.Right && this.Y <= value.Y && value.Bottom <= this.Bottom;
        }

        /// <summary>Determines whether this Rectangle entirely contains a specified Rectangle.</summary>
        /// <param name="value">The Rectangle to evaluate.</param>
        /// <param name="result">[OutAttribute] On exit, is true if this Rectangle entirely contains the specified Rectangle, or false if not.</param>
        public void Contains(ref RectangleF value, out bool result)
        {
            result = this.X <= value.X && value.Right <= this.Right && this.Y <= value.Y && value.Bottom <= this.Bottom;
        }

        /// <summary>Determines whether a specified Rectangle intersects with this Rectangle.</summary>
        /// <param name="value">The Rectangle to evaluate.</param>
        public bool Intersects(RectangleF value)
        {
            return value.X < this.Right && this.X < value.Right && value.Y < this.Bottom && this.Y < value.Bottom;
        }

        /// <summary>
        /// Determines whether a specified Rectangle intersects with this Rectangle.
        /// </summary>
        /// <param name="value">The Rectangle to evaluate</param>
        /// <param name="result">[OutAttribute] true if the specified Rectangle intersects with this one; false otherwise.</param>
        public void Intersects(ref RectangleF value, out bool result)
        {
            result = value.X < this.Right && this.X < value.Right && value.Y < this.Bottom && this.Y < value.Bottom;
        }

        /// <summary>
        /// Creates a Rectangle defining the area where one rectangle overlaps with another rectangle.
        /// </summary>
        /// <param name="value1">The first Rectangle to compare.</param>
        /// <param name="value2">The second Rectangle to compare.</param>
        /// <returns>Rectangle.</returns>
        public static RectangleF Intersect(RectangleF value1, RectangleF value2)
        {
            var newLeft = value1.X > value2.X ? value1.X : value2.X;
            var newTop = value1.Y > value2.Y ? value1.Y : value2.Y;
            var newRight = value1.Right < value2.Right ? value1.Right : value2.Right;
            var newBottom = value1.Bottom < value2.Bottom ? value1.Bottom : value2.Bottom;
            if (newRight > newLeft && newBottom > newTop)
            {
                return new RectangleF(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            return Empty;
        }

        /// <summary>Creates a Rectangle defining the area where one rectangle overlaps with another rectangle.</summary>
        /// <param name="value1">The first Rectangle to compare.</param>
        /// <param name="value2">The second Rectangle to compare.</param>
        /// <param name="result">[OutAttribute] The area where the two first parameters overlap.</param>
        public static void Intersect(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
        {
            var newLeft = value1.X > value2.X ? value1.X : value2.X;
            var newTop = value1.Y > value2.Y ? value1.Y : value2.Y;
            var newRight = value1.Right < value2.Right ? value1.Right : value2.Right;
            var newBottom = value1.Bottom < value2.Bottom ? value1.Bottom : value2.Bottom;
            if (newRight > newLeft && newBottom > newTop)
            {
                result = new RectangleF(newLeft, newTop, newRight - newLeft, newBottom - newTop);
            }
            else
            {
                result = Empty;
            }
        }

        /// <summary>
        /// Creates a new Rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first Rectangle to contain.</param>
        /// <param name="value2">The second Rectangle to contain.</param>
        /// <returns>Rectangle.</returns>
        public static RectangleF Union(RectangleF value1, RectangleF value2)
        {
            var num6 = value1.X + value1.Width;
            var num5 = value2.X + value2.Width;
            var num4 = value1.Y + value1.Height;
            var num3 = value2.Y + value2.Height;
            var num2 = value1.X < value2.X ? value1.X : value2.X;
            var num = value1.Y < value2.Y ? value1.Y : value2.Y;
            var num8 = num6 > num5 ? num6 : num5;
            var num7 = num4 > num3 ? num4 : num3;
            return new RectangleF(num2, num, num8 - num2, num7 - num);
        }

        /// <summary>
        /// Creates a new Rectangle that exactly contains two other rectangles.
        /// </summary>
        /// <param name="value1">The first Rectangle to contain.</param>
        /// <param name="value2">The second Rectangle to contain.</param>
        /// <param name="result">[OutAttribute] The Rectangle that must be the union of the first two rectangles.</param>
        public static void Union(ref RectangleF value1, ref RectangleF value2, out RectangleF result)
        {
            var num6 = value1.X + value1.Width;
            var num5 = value2.X + value2.Width;
            var num4 = value1.Y + value1.Height;
            var num3 = value2.Y + value2.Height;
            var num2 = value1.X < value2.X ? value1.X : value2.X;
            var num = value1.Y < value2.Y ? value1.Y : value2.Y;
            var num8 = num6 > num5 ? num6 : num5;
            var num7 = num4 > num3 ? num4 : num3;
            result = new RectangleF(num2, num, num8 - num2, num7 - num);
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        /// 	<c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }
            if (obj.GetType() != typeof(RectangleF))
            {
                return false;
            }
            return this.Equals((RectangleF)obj);
        }

        /// <inheritdoc/>
        public bool Equals(RectangleF other)
        {
            return Math.Abs(other.Left - this.Left) < MathUtil.ZeroTolerance &&
                   Math.Abs(other.Right - this.Right) < MathUtil.ZeroTolerance &&
                   Math.Abs(other.Top - this.Top) < MathUtil.ZeroTolerance &&
                   Math.Abs(other.Bottom - this.Bottom) < MathUtil.ZeroTolerance;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var result = _left.GetHashCode();
                result = (result * 397) ^ _top.GetHashCode();
                result = (result * 397) ^ _right.GetHashCode();
                result = (result * 397) ^ _bottom.GetHashCode();
                return result;
            }
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator ==(RectangleF left, RectangleF right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>The result of the operator.</returns>
        public static bool operator !=(RectangleF left, RectangleF right)
        {
            return !(left == right);
        }
    }
}
