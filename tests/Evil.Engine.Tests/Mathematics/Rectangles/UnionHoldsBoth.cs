using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rectangles;

internal sealed class UnionHoldsBoth : Property<RectPair>
{
    protected override RectPair Generate(Random random) =>
        new RectPair(Gen.Rect(random, 50f), Gen.Rect(random, 50f));

    protected override void Verify(RectPair sample)
    {
        RectF union = sample.A.Union(sample.B).Inflate(1e-4f);
        Check.True(union.Contains(sample.A) && union.Contains(sample.B), $"{union} misses a part");
        Check.Equal(sample.A.Union(sample.B), sample.B.Union(sample.A), "the order does not matter");
    }
}
