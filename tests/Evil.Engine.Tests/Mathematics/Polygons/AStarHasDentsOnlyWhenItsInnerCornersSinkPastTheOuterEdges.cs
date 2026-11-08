using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Polygons;

internal sealed class AStarHasDentsOnlyWhenItsInnerCornersSinkPastTheOuterEdges : Property<StarShape>
{
    protected override StarShape Generate(Random random) =>
        new StarShape(Gen.Number(random, 2f, 20f), Gen.Number(random, 0.05f, 0.95f), random.Next(3, 12), Gen.Chance(random, 0.5));

    protected override void Verify(StarShape sample)
    {
        float edge = MathF.Cos(MathF.PI / sample.Points);
        float ratio = sample.Dented ? edge * sample.Share : edge + (1f - edge) * sample.Share;
        Polygon star = Polygon.Star(Vector2.Zero, sample.Outer, sample.Outer * ratio, sample.Points);
        Check.Equal(!sample.Dented, star.IsConvex, $"inner at {ratio:0.000} of outer, the outer edges pass at {edge:0.000}");
    }
}
