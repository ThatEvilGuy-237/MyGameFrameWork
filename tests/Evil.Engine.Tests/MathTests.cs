using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests;

internal static class MathTests
{
    [Test]
    public static void AngleConvertsAndWraps()
    {
        Check.Near(MathF.PI, Angle.ToRadians(180f), "180 degrees");
        Check.Near(90f, Angle.ToDegrees(MathF.PI / 2f), "half pi");
        Check.Near(270f, Angle.Wrap(-90f), "wrap -90");
        Check.Near(0f, Angle.Wrap(720f), "wrap 720");
        Check.Near(10f, Angle.Wrap(370f), "wrap 370");
    }

    [Test]
    public static void RectFEdgesAndContains()
    {
        var rect = new RectF(10f, 20f, 30f, 40f);
        Check.Equal(40f, rect.Right, "right");
        Check.Equal(60f, rect.Bottom, "bottom");
        Check.Near(new Vector2(25f, 40f), rect.Center, "center");
        Check.True(rect.Contains(new Vector2(10f, 20f)), "top-left corner is inside");
        Check.True(!rect.Contains(new Vector2(40f, 60f)), "bottom-right corner is outside");
        Check.True(new RectF(0f, 0f, 0f, 5f).IsEmpty, "zero width is empty");
    }

    [Test]
    public static void RectFIntersectAndUnion()
    {
        var a = new RectF(0f, 0f, 10f, 10f);
        var b = new RectF(5f, 5f, 10f, 10f);
        Check.True(a.Intersects(b), "overlapping rects intersect");
        Check.Equal(new RectF(5f, 5f, 5f, 5f), a.Intersect(b), "intersection");
        Check.Equal(new RectF(0f, 0f, 15f, 15f), a.Union(b), "union");
        Check.True(!a.Intersects(new RectF(10f, 0f, 5f, 5f)), "touching edges do not intersect");
        Check.Equal(RectF.Empty, a.Intersect(new RectF(20f, 20f, 1f, 1f)), "no overlap");
    }

    [Test]
    public static void RectFBuilders()
    {
        Check.Equal(new RectF(-5f, -10f, 10f, 20f), RectF.FromCenter(Vector2.Zero, new Vector2(10f, 20f)), "from center");
        Check.Equal(new RectF(1f, 2f, 3f, 4f), RectF.FromCorners(new Vector2(4f, 6f), new Vector2(1f, 2f)), "from corners");
        Check.Equal(new RectF(-1f, -1f, 12f, 12f), new RectF(0f, 0f, 10f, 10f).Inflate(1f), "inflate");
        Check.Equal(new RectF(3f, 4f, 10f, 10f), new RectF(0f, 0f, 10f, 10f).Offset(3f, 4f), "offset");
    }

    [Test]
    public static void RectIConvertsToRectF()
    {
        RectF converted = new RectI(1, 2, 3, 4);
        Check.Equal(new RectF(1f, 2f, 3f, 4f), converted, "implicit conversion");
        Check.True(new RectI(0, 0, 32, 16).Contains(new RectI(16, 0, 16, 16)), "cell inside sheet");
        Check.True(!new RectI(0, 0, 32, 16).Contains(new RectI(20, 0, 16, 16)), "cell past the edge");
    }

    [Test]
    public static void Transform2DRotatesAroundOrigin()
    {
        var transform = Transform2D.At(10f, 20f).Rotated(90f).Around(new Vector2(5f, 0f));
        var matrix = transform.ToMatrix();
        Check.Near(new Vector2(10f, 20f), Vector2.Transform(new Vector2(5f, 0f), matrix), "origin lands on position");
        Check.Near(new Vector2(10f, 21f), Vector2.Transform(new Vector2(6f, 0f), matrix), "one step right turns down");
    }

    [Test]
    public static void Transform2DScales()
    {
        var matrix = Transform2D.At(100f, 0f).Scaled(new Vector2(2f, 3f)).ToMatrix();
        Check.Near(new Vector2(102f, 3f), Vector2.Transform(new Vector2(1f, 1f), matrix), "scaled then moved");
    }
}
