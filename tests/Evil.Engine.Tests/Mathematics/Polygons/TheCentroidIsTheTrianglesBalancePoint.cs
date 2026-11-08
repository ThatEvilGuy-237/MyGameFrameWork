using System.Numerics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed class TheCentroidIsTheTrianglesBalancePoint : Property<Vector2[]>
{
    protected override Vector2[] Generate(Random random) =>
        Gen.StarShape(random);

    protected override void Verify(Vector2[] sample)
    {
        Triangulation triangles = new Triangulation(sample);
        Check.Near(triangles.BalancePoint, triangles.Shape.Centroid, "centroid", 1e-2f);
    }
}
