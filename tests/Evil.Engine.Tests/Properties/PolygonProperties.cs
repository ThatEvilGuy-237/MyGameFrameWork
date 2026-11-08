using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties;

internal static class PolygonProperties
{
    [Test]
    public static void TrianglesCoverTheAreaExactlyOnce()
    {
        Property.ForAll(
            nameof(TrianglesCoverTheAreaExactlyOnce),
            Gen.StarShape,
            corners =>
            {
                Polygon shape = new Polygon(corners);
                IReadOnlyList<int> triangles = shape.Triangles;
                Check.Equal(0, triangles.Count % 3, "whole triangles");
                Check.True(triangles.Count / 3 <= corners.Length - 2, $"{triangles.Count / 3} triangles for {corners.Length} corners");
                float covered = 0f;
                for (int i = 0; i < triangles.Count; i += 3)
                {
                    float area = TriangleArea(corners[triangles[i]], corners[triangles[i + 1]], corners[triangles[i + 2]]);
                    Check.True(area >= -1e-3f, $"triangle {i / 3} is turned over (area {area})");
                    covered += MathF.Abs(area);
                }

                Check.Near(shape.Area, covered, "the triangles cover the polygon once", 1e-3f * shape.Area + 1e-3f);
            });
    }

    [Test]
    public static void ContainsAgreesWithTheTriangles()
    {
        Property.ForAll(
            nameof(ContainsAgreesWithTheTriangles),
            random => (Corners: Gen.StarShape(random), Seed: random.Next()),
            input =>
            {
                Polygon shape = new Polygon(input.Corners);
                Random random = new Random(input.Seed);
                RectF area = shape.Bounds.Inflate(1f);
                for (int i = 0; i < 40; i++)
                {
                    Vector2 point = new Vector2(Gen.Number(random, area.Left, area.Right), Gen.Number(random, area.Top, area.Bottom));
                    if (DistanceToEdges(input.Corners, point) < 1e-2f)
                    {
                        continue;
                    }

                    Check.Equal(InSomeTriangle(input.Corners, shape.Triangles, point), shape.Contains(point), $"inside at {point}");
                }
            });
    }

    [Test]
    public static void TheCentroidIsTheTrianglesBalancePoint()
    {
        Property.ForAll(
            nameof(TheCentroidIsTheTrianglesBalancePoint),
            Gen.StarShape,
            corners =>
            {
                Polygon shape = new Polygon(corners);
                Vector2 weighted = Vector2.Zero;
                float total = 0f;
                for (int i = 0; i < shape.Triangles.Count; i += 3)
                {
                    Vector2 a = corners[shape.Triangles[i]];
                    Vector2 b = corners[shape.Triangles[i + 1]];
                    Vector2 c = corners[shape.Triangles[i + 2]];
                    float area = MathF.Abs(TriangleArea(a, b, c));
                    weighted += (a + b + c) / 3f * area;
                    total += area;
                }

                Check.Near(weighted / total, shape.Centroid, "centroid", 1e-2f);
            });
    }

    [Test]
    public static void TheWindingOnlyFlipsTheSign()
    {
        Property.ForAll(
            nameof(TheWindingOnlyFlipsTheSign),
            Gen.StarShape,
            corners =>
            {
                Vector2[] backwards = [.. corners.Reverse()];
                Polygon shape = new Polygon(corners);
                Polygon flipped = new Polygon(backwards);
                Check.Near(-shape.SignedArea, flipped.SignedArea, "signed area", 1e-3f * shape.Area + 1e-3f);
                Check.Near(shape.Centroid, flipped.Centroid, "centroid", 1e-2f);
            });
    }

    [Test]
    public static void MovingTurningAndScalingChangeTheAreaAsExpected()
    {
        Property.ForAll(
            nameof(MovingTurningAndScalingChangeTheAreaAsExpected),
            random => (Corners: Gen.StarShape(random), Move: Gen.Point2(random, 30f), Degrees: Gen.Number(random, -180f, 180f), Scale: Gen.Number(random, 0.2f, 4f)),
            input =>
            {
                Polygon shape = new Polygon(input.Corners);
                Polygon moved = shape.Translate(input.Move);
                Check.Near(shape.Centroid + input.Move, moved.Centroid, "the centroid moves along", 1e-2f);
                Check.Near(shape.Area, moved.Area, "moving keeps the area", 1e-3f * shape.Area + 1e-3f);

                Polygon turned = shape.Transform(Matrix3x2.CreateRotation(Angle.ToRadians(input.Degrees)));
                Check.Near(shape.Area, turned.Area, "turning keeps the area", 1e-3f * shape.Area + 1e-3f);

                Polygon scaled = shape.Transform(Matrix3x2.CreateScale(input.Scale));
                Check.Near(shape.Area * input.Scale * input.Scale, scaled.Area, "scaling multiplies the area", 1e-3f * scaled.Area + 1e-3f);
                Check.True(shape.CenteredOnOrigin().Centroid.Length() < 1e-2f, "centered on the origin");
            });
    }

    [Test]
    public static void RegularPolygonsAreConvexAndMeasureRight()
    {
        Property.ForAll(
            nameof(RegularPolygonsAreConvexAndMeasureRight),
            random => (Center: Gen.Point2(random, 50f), Radius: Gen.Number(random, 0.5f, 20f), Sides: random.Next(3, 40), Turn: Gen.Number(random, -180f, 180f)),
            input =>
            {
                Polygon shape = Polygon.Regular(input.Center, input.Radius, input.Sides, input.Turn);
                Check.True(shape.IsConvex, "convex");
                float expected = 0.5f * input.Sides * input.Radius * input.Radius * MathF.Sin(MathF.Tau / input.Sides);
                Check.Near(expected, shape.Area, "area", 1e-3f * expected);
                Check.Near(input.Center, shape.Centroid, "centroid", 1e-2f);
                foreach (Vector2 corner in shape.Points)
                {
                    Check.Near(input.Radius, Vector2.Distance(input.Center, corner), "corner on the circle", 1e-3f * input.Radius);
                }
            });
    }

    [Test]
    public static void AStarHasDentsOnlyWhenItsInnerCornersSinkPastTheOuterEdges()
    {
        Property.ForAll(
            nameof(AStarHasDentsOnlyWhenItsInnerCornersSinkPastTheOuterEdges),
            random => (Outer: Gen.Number(random, 2f, 20f), Share: Gen.Number(random, 0.05f, 0.95f), Points: random.Next(3, 12), Dented: Gen.Chance(random, 0.5)),
            input =>
            {
                float edge = MathF.Cos(MathF.PI / input.Points);
                float ratio = input.Dented ? edge * input.Share : edge + (1f - edge) * input.Share;
                Polygon star = Polygon.Star(Vector2.Zero, input.Outer, input.Outer * ratio, input.Points);
                Check.Equal(!input.Dented, star.IsConvex, $"inner at {ratio:0.000} of outer, the outer edges pass at {edge:0.000}");
            });
    }

    private static float TriangleArea(Vector2 a, Vector2 b, Vector2 c) =>
        ((b.X - a.X) * (c.Y - a.Y) - (c.X - a.X) * (b.Y - a.Y)) / 2f;

    private static bool InSomeTriangle(Vector2[] corners, IReadOnlyList<int> triangles, Vector2 point)
    {
        for (int i = 0; i < triangles.Count; i += 3)
        {
            Vector2 a = corners[triangles[i]];
            Vector2 b = corners[triangles[i + 1]];
            Vector2 c = corners[triangles[i + 2]];
            float abp = TriangleArea(a, b, point);
            float bcp = TriangleArea(b, c, point);
            float cap = TriangleArea(c, a, point);
            bool allLeft = abp >= 0f && bcp >= 0f && cap >= 0f;
            bool allRight = abp <= 0f && bcp <= 0f && cap <= 0f;
            if (allLeft || allRight)
            {
                return true;
            }
        }

        return false;
    }

    private static float DistanceToEdges(Vector2[] corners, Vector2 point)
    {
        float nearest = float.MaxValue;
        for (int i = 0; i < corners.Length; i++)
        {
            Vector2 a = corners[i];
            Vector2 b = corners[(i + 1) % corners.Length];
            Vector2 edge = b - a;
            float along = Math.Clamp(Vector2.Dot(point - a, edge) / MathF.Max(edge.LengthSquared(), 1e-12f), 0f, 1f);
            nearest = MathF.Min(nearest, Vector2.Distance(point, a + edge * along));
        }

        return nearest;
    }
}
