using System;

namespace Pdfium.Rendering
{
    /// <summary>
    /// Struct representing a rectangle
    /// </summary>
    public readonly struct Rectangle : IEquatable<Rectangle>
    {
        /// <summary>
        /// Construct a new <see cref="Rectangle"/>
        /// </summary>
        /// <param name="width">The width the new <see cref="Size"/> gets</param>
        /// <param name="height">The height the new <see cref="Size"/> gets</param>
        public Rectangle(float width, float height)
        {
            Left = 0.0f;
            Bottom = 0.0f;
            Right = width;
            Top = height;
        }

        /// <summary>
        /// Construct a new <see cref="Rectangle"/>
        /// </summary>
        /// <param name="left">The left of the new <see cref="Rectangle"/></param>
        /// <param name="bottom">The bottom of the new <see cref="Rectangle"/></param>
        /// <param name="right">The right of the new <see cref="Rectangle"/></param>
        /// <param name="top">The top of the new <see cref="Rectangle"/></param>
        public Rectangle(float left, float bottom, float right, float top)
        {
            Left = left;
            Bottom = bottom;
            Right = right;
            Top = top;
        }

        /// <summary>
        /// The left (LLX) coordinate of the rectangle.
        /// </summary>
        public float Left { get; }

        /// <summary>
        /// The bottom (LLY) coordinate of the rectangle.
        /// </summary>
        public float Bottom { get; }

        /// <summary>
        /// The right (URX) coordinate of the rectangle.
        /// </summary>
        public float Right { get; }

        /// <summary>
        /// The top (URY) coordinate of the rectangle.
        /// </summary>
        public float Top { get; }

        /// <inheritdoc />
        public override int GetHashCode()
            => HashCode.Combine(Left, Bottom, Right, Top);

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => obj is Rectangle rectangle && Equals(rectangle);

        /// <inheritdoc />
        public bool Equals(Rectangle other)
            => other.Left == Left
            && other.Bottom == Bottom
            && other.Right == Right
            && other.Top == Top;

        /// <summary>
        /// Check if <paramref name="left"/> and <paramref name="right"/> are equal
        /// </summary>
        /// <param name="left">The left side of the equality check.</param>
        /// <param name="right">The right side of the equality check.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are equal.</returns>
        public static bool operator ==(Rectangle left, Rectangle right)
            => left.Equals(right);

        /// <summary>
        /// Check if <paramref name="left"/> and <paramref name="right"/> are not equal
        /// </summary>
        /// <param name="left">The left side of the inequality check.</param>
        /// <param name="right">The right side of the inequality check.</param>
        /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal.</returns>
        public static bool operator !=(Rectangle left, Rectangle right)
            => !( left == right );
    }
}
