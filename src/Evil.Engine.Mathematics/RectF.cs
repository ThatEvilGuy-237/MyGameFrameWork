using System.Numerics;

namespace Evil.Engine.Mathematics;

public readonly record struct RectF(float X, float Y, float Width, float Height)
{
    public static RectF Empty => default;

    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;

    public Vector2 Position => new(X, Y);
    public Vector2 Size => new(Width, Height);
    public Vector2 Center => new(X + Width / 2f, Y + Height / 2f);

    public Vector2 TopLeft => new(Left, Top);
    public Vector2 TopRight => new(Right, Top);
    public Vector2 BottomLeft => new(Left, Bottom);
    public Vector2 BottomRight => new(Right, Bottom);

    public bool IsEmpty => Width <= 0f || Height <= 0f;

    public static RectF FromCenter(Vector2 center, Vector2 size) =>
        new(center.X - size.X / 2f, center.Y - size.Y / 2f, size.X, size.Y);

    public static RectF FromCorners(Vector2 a, Vector2 b)
    {
        var min = Vector2.Min(a, b);
        var max = Vector2.Max(a, b);
        return new RectF(min.X, min.Y, max.X - min.X, max.Y - min.Y);
    }

    public bool Contains(Vector2 point) =>
        point.X >= Left && point.X < Right && point.Y >= Top && point.Y < Bottom;

    public bool Contains(RectF other) =>
        other.Left >= Left && other.Right <= Right && other.Top >= Top && other.Bottom <= Bottom;

    public bool Intersects(RectF other) =>
        other.Left < Right && other.Right > Left && other.Top < Bottom && other.Bottom > Top;

    public RectF Intersect(RectF other)
    {
        var left = MathF.Max(Left, other.Left);
        var top = MathF.Max(Top, other.Top);
        var right = MathF.Min(Right, other.Right);
        var bottom = MathF.Min(Bottom, other.Bottom);
        return right <= left || bottom <= top ? Empty : new RectF(left, top, right - left, bottom - top);
    }

    public RectF Union(RectF other)
    {
        var left = MathF.Min(Left, other.Left);
        var top = MathF.Min(Top, other.Top);
        var right = MathF.Max(Right, other.Right);
        var bottom = MathF.Max(Bottom, other.Bottom);
        return new RectF(left, top, right - left, bottom - top);
    }

    public RectF Offset(Vector2 by) => new(X + by.X, Y + by.Y, Width, Height);

    public RectF Offset(float x, float y) => new(X + x, Y + y, Width, Height);

    public RectF Inflate(float amount) => new(X - amount, Y - amount, Width + amount * 2f, Height + amount * 2f);

    public RectF Scale(float factor) => new(X * factor, Y * factor, Width * factor, Height * factor);
}
