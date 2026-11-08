using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed class ContainsAgreesWithTheTriangles : Property<CornersAndSeed>
{
    protected override CornersAndSeed Generate(Random random) =>
        new CornersAndSeed(Gen.StarShape(random), random.Next());

    protected override void Verify(CornersAndSeed sample)
    {
        Triangulation triangles = new Triangulation(sample.Corners);
        Random random = new Random(sample.Seed);
        RectF area = triangles.Shape.Bounds.Inflate(1f);
        for (int i = 0; i < 40; i++)
        {
            Vector2 point = new Vector2(Gen.Number(random, area.Left, area.Right), Gen.Number(random, area.Top, area.Bottom));
            if (triangles.DistanceToEdges(point) >= 1e-2f)
            {
                Check.Equal(triangles.Covers(point), triangles.Shape.Contains(point), $"inside at {point}");
            }
        }
    }
}
