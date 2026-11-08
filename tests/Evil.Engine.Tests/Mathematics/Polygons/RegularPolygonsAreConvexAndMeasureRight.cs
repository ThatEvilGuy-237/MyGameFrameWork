using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed class RegularPolygonsAreConvexAndMeasureRight : Property<RegularShape>
{
    protected override RegularShape Generate(Random random) =>
        new RegularShape(Gen.Point2(random, 50f), Gen.Number(random, 0.5f, 20f), random.Next(3, 40), Gen.Number(random, -180f, 180f));

    protected override void Verify(RegularShape sample)
    {
        Polygon shape = Polygon.Regular(sample.Center, sample.Radius, sample.Sides, sample.Turn);
        Check.True(shape.IsConvex, "convex");
        float expected = 0.5f * sample.Sides * sample.Radius * sample.Radius * MathF.Sin(MathF.Tau / sample.Sides);
        Check.Near(expected, shape.Area, "area", 1e-3f * expected);
        Check.Near(sample.Center, shape.Centroid, "centroid", 1e-2f);
        foreach (Vector2 corner in shape.Points)
        {
            Check.Near(sample.Radius, Vector2.Distance(sample.Center, corner), "corner on the circle", 1e-3f * sample.Radius);
        }
    }
}
