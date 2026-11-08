using System.Numerics;

namespace Evil.Engine.Mathematics;

public readonly record struct RectF(float X, float Y, float Width, float Height)
{
    public static RectF Empty => default;

    public float Left => X;
    public float Top => Y;
    public float Right => X + Width;
    public float Bottom => Y + Height;

    public Vector2 Position => new Vector2(X, Y);
    public Vector2 Size => new Vector2(Width, Height);
    public Vector2 Center => new Vector2(X + Width / 2f, Y + Height / 2f);

    public Vector2 TopLeft => new Vector2(Left, Top);
    public Vector2 TopRight => new Vector2(Right, Top);
    public Vector2 BottomLeft => new Vector2(Left, Bottom);
    public Vector2 BottomRight => new Vector2(Right, Bottom);

    public bool IsEmpty => Width <= 0f || Height <= 0f;

    public static RectF FromCenter(Vector2 center, Vector2 size) =>
        new RectF(center.X - size.X / 2f, center.Y - size.Y / 2f, size.X, size.Y);

    public static RectF FromCorners(Vector2 a, Vector2 b)
    {
        Vector2 min = Vector2.Min(a, b);
        Vector2 max = Vector2.Max(a, b);
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
        float left = MathF.Max(Left, other.Left);
        float top = MathF.Max(Top, other.Top);
        float right = MathF.Min(Right, other.Right);
        float bottom = MathF.Min(Bottom, other.Bottom);
        return right <= left || bottom <= top ? Empty : new RectF(left, top, right - left, bottom - top);
    }

    public RectF Union(RectF other)
    {
        if (IsEmpty)
        {
            return other;
        }

        if (other.IsEmpty)
        {
            return this;
        }

        float left = MathF.Min(Left, other.Left);
        float top = MathF.Min(Top, other.Top);
        float right = MathF.Max(Right, other.Right);
        float bottom = MathF.Max(Bottom, other.Bottom);
        return new RectF(left, top, right - left, bottom - top);
    }

    public RectF Offset(Vector2 by) => new RectF(X + by.X, Y + by.Y, Width, Height);

    public RectF Offset(float x, float y) => new RectF(X + x, Y + y, Width, Height);

    public RectF Inflate(float amount) => new RectF(X - amount, Y - amount, Width + amount * 2f, Height + amount * 2f);

    public RectF Scale(float factor) => new RectF(X * factor, Y * factor, Width * factor, Height * factor);
}
