using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rectangles;

internal sealed class IntersectIsInsideBothAndAgreesWithIntersects : Property<RectPair>
{
    protected override RectPair Generate(Random random) =>
        new RectPair(Gen.Rect(random, 50f), Gen.Rect(random, 50f));

    protected override void Verify(RectPair sample)
    {
        RectF overlap = sample.A.Intersect(sample.B);
        Check.Equal(!overlap.IsEmpty, sample.A.Intersects(sample.B), "Intersect and Intersects agree");
        Check.Equal(overlap, sample.B.Intersect(sample.A), "the order does not matter");
        if (!overlap.IsEmpty)
        {
            Check.True(sample.A.Inflate(1e-4f).Contains(overlap) && sample.B.Inflate(1e-4f).Contains(overlap), $"{overlap} sticks out");
        }
    }
}
