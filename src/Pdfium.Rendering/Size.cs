using System;

namespace Pdfium.Rendering;

/// <summary>
/// Struct representing a size
/// </summary>
public readonly struct Size : IEquatable<Size>
{
    /// <summary>
    /// Construct a new <see cref="Size"/>
    /// </summary>
    /// <param name="width">The width the new <see cref="Size"/> gets</param>
    /// <param name="height">The height the new <see cref="Size"/> gets</param>
    public Size(float width, float height)
    {
        Width = width;
        Height = height;
    }

    /// <summary>
    /// The width of this <see cref="Size"/>
    /// </summary>
    public float Width { get; }

    /// <summary>
    /// The height of this <see cref="Size"/>
    /// </summary>
    public float Height { get; }

    /// <inheritdoc />
    public override int GetHashCode()
        => HashCode.Combine(Width, Height);

    /// <inheritdoc />
    public override bool Equals(object? obj)
        => obj is Size size && Equals(size);

    /// <inheritdoc />
    public bool Equals(Size other)
        => other.Width == Width && other.Height == Height;

    /// <summary>
    /// Deconstruct this <see cref="Size"/> into width and height floats
    /// </summary>
    /// <param name="width">The width of this <see cref="Size"/></param>
    /// <param name="height">The height of this <see cref="Size"/></param>
    public void Deconstruct(out float width, out float height)
    {
        width = Width;
        height = Height;
    }

    /// <summary>
    /// Check if <paramref name="left"/> and <paramref name="right"/> are equal
    /// </summary>
    /// <param name="left">The left side of the equality check.</param>
    /// <param name="right">The right side of the equality check.</param>
    /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are equal.</returns>
    public static bool operator ==(Size left, Size right)
        => left.Equals(right);

    /// <summary>
    /// Check if <paramref name="left"/> and <paramref name="right"/> are not equal
    /// </summary>
    /// <param name="left">The left side of the inequality check.</param>
    /// <param name="right">The right side of the inequality check.</param>
    /// <returns>true if <paramref name="left"/> and <paramref name="right"/> are not equal.</returns>
    public static bool operator !=(Size left, Size right)
        => !(left == right);
}
