namespace Evil.Engine.Mathematics;

public readonly record struct RectI(int X, int Y, int Width, int Height)
{
    public static RectI Empty => default;

    public int Left => X;
    public int Top => Y;
    public int Right => X + Width;
    public int Bottom => Y + Height;

    public bool IsEmpty => Width <= 0 || Height <= 0;

    public bool Contains(int x, int y) => x >= Left && x < Right && y >= Top && y < Bottom;

    public bool Contains(RectI other) =>
        other.Left >= Left && other.Right <= Right && other.Top >= Top && other.Bottom <= Bottom;

    public bool Intersects(RectI other) =>
        other.Left < Right && other.Right > Left && other.Top < Bottom && other.Bottom > Top;

    public RectI Offset(int x, int y) => new RectI(X + x, Y + y, Width, Height);

    public RectF ToRectF() => new RectF(X, Y, Width, Height);

    public static implicit operator RectF(RectI rect) => rect.ToRectF();
}
