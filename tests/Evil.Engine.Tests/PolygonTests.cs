using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests;

internal static class PolygonTests
{
    [Test]
    public static void SquareMakesTwoTrianglesInBothDirections()
    {
        Polygon clockwise = new Polygon(new Vector2(0f, 0f), new Vector2(0f, 10f), new Vector2(10f, 10f), new Vector2(10f, 0f));
        Polygon counterClockwise = new Polygon(new Vector2(0f, 0f), new Vector2(10f, 0f), new Vector2(10f, 10f), new Vector2(0f, 10f));
        foreach (Polygon? square in new[] { clockwise, counterClockwise })
        {
            Check.Equal(6, square.Triangles.Count, "indices");
            Check.Near(100f, TriangleArea(square), "covered area");
        }
    }

    [Test]
    public static void ConcaveLShapeIsCoveredExactly()
    {
        Polygon shape = new Polygon(new Vector2(0f, 0f), new Vector2(2f, 0f), new Vector2(2f, 1f), new Vector2(1f, 1f), new Vector2(1f, 2f), new Vector2(0f, 2f));
        Check.Equal(12, shape.Triangles.Count, "indices");
        Check.Near(3f, shape.Area, "polygon area");
        Check.Near(3f, TriangleArea(shape), "covered area");
        Check.True(!shape.IsConvex, "an L is concave");
    }

    [Test]
    public static void StarIsCoveredExactly()
    {
        Polygon star = Polygon.Star(Vector2.Zero, 60f, 26f, 5);
        Check.Equal(10, star.Count, "points");
        Check.Equal(24, star.Triangles.Count, "indices");
        Check.Near(star.Area, TriangleArea(star), "covered area", 0.05f);
    }

    [Test]
    public static void CollinearPointsAreHandled()
    {
        Polygon shape = new Polygon(new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(2f, 0f), new Vector2(2f, 2f), new Vector2(0f, 2f));
        Check.Near(4f, TriangleArea(shape), "covered area");
    }

    [Test]
    public static void SelfIntersectingInputStillFinishes()
    {
        Polygon cloud = new Polygon(
            new Vector2(100f, 120f), new Vector2(150f, 200f), new Vector2(200f, 250f), new Vector2(50f, 170f), new Vector2(180f, 90f),
            new Vector2(120f, 60f), new Vector2(110f, 180f), new Vector2(160f, 140f), new Vector2(220f, 190f), new Vector2(130f, 210f));
        IReadOnlyList<int> indices = cloud.Triangles;
        Check.Equal(0, indices.Count % 3, "whole triangles");
        Check.True(indices.All(index => index >= 0 && index < cloud.Count), "indices in range");
    }

    [Test]
    public static void TooFewPointsMakeNoTriangles()
    {
        Check.Equal(0, new Polygon(new Vector2(0f, 0f), new Vector2(1f, 1f)).Triangles.Count, "indices");
    }

    [Test]
    public static void ContainsUsesTheRealOutline()
    {
        Polygon star = Polygon.Star(Vector2.Zero, 60f, 26f, 5);
        Check.True(star.Contains(Vector2.Zero), "center");
        Check.True(star.Contains(new Vector2(0f, -55f)), "inside the top point");
        Check.True(!star.Contains(new Vector2(40f, -40f)), "between two points");
    }

    [Test]
    public static void RegularHexagonMeasures()
    {
        Polygon hexagon = Polygon.Regular(new Vector2(50f, 50f), 10f, 6);
        Check.True(hexagon.IsConvex, "convex");
        Check.Near(3f * MathF.Sqrt(3f) / 2f * 100f, hexagon.Area, "area", 0.01f);
        Check.Near(new Vector2(50f, 50f), hexagon.Centroid, "centroid");
        Check.Throws<ArgumentOutOfRangeException>(() => Polygon.Regular(Vector2.Zero, 1f, 2));
    }

    [Test]
    public static void CentroidAndBoundsOfARectangle()
    {
        Polygon rect = Polygon.Rectangle(new RectF(10f, 10f, 20f, 40f));
        Check.Near(new Vector2(20f, 30f), rect.Centroid, "centroid");
        Check.Equal(new RectF(10f, 10f, 20f, 40f), rect.Bounds, "bounds");
        Check.Near(Vector2.Zero, rect.CenteredOnOrigin().Centroid, "centered");
    }

    [Test]
    public static void ShapesFarFromTheOriginKeepTheirArea()
    {
        Polygon near = new Polygon(new Vector2(0f, 0f), new Vector2(1.5f, 0f), new Vector2(1.5f, 0.3f), new Vector2(0.3f, 0.3f), new Vector2(0.3f, 1.5f), new Vector2(0f, 1.5f));
        Polygon far = near.Translate(new Vector2(20000f, 15000f));
        Check.Near(near.Area, far.Area, "area", 0.01f);
        Check.Near(far.Area, TriangleArea(far), "covered area", 0.05f);
        Check.Near(near.Centroid + new Vector2(20000f, 15000f), far.Centroid, "centroid", 0.05f);

        Polygon star = Polygon.Star(new Vector2(8000f, 6000f), 16f, 7f, 5);
        Check.Near(new Vector2(8000f, 6000f), star.Centroid, "small star centroid", 0.05f);
    }

    private static float TriangleArea(Polygon polygon)
    {
        float total = 0f;
        IReadOnlyList<int> indices = polygon.Triangles;
        for (int i = 0; i < indices.Count; i += 3)
        {
            Vector2 a = polygon.Points[indices[i]];
            Vector2 b = polygon.Points[indices[i + 1]];
            Vector2 c = polygon.Points[indices[i + 2]];
            total += MathF.Abs((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) / 2f;
        }

        return total;
    }
}
