using System.Numerics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed class TrianglesCoverTheAreaExactlyOnce : Property<Vector2[]>
{
    protected override Vector2[] Generate(Random random) =>
        Gen.StarShape(random);

    protected override void Verify(Vector2[] sample)
    {
        Triangulation triangles = new Triangulation(sample);
        Check.Equal(0, triangles.Shape.Triangles.Count % 3, "whole triangles");
        Check.True(triangles.Count <= sample.Length - 2, $"{triangles.Count} triangles for {sample.Length} corners");
        Check.True(triangles.SmallestArea >= -1e-3f, $"a triangle is turned over (area {triangles.SmallestArea})");
        Check.Near(triangles.Shape.Area, triangles.CoveredArea, "the triangles cover the polygon once", 1e-3f * triangles.Shape.Area + 1e-3f);
    }
}
