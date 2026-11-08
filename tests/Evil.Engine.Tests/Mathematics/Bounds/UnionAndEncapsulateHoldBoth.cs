using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Bounds;

internal sealed class UnionAndEncapsulateHoldBoth : Property<BoxPair>
{
    protected override BoxPair Generate(Random random) =>
        new BoxPair(Gen.Box(random, 20f), Gen.Box(random, 20f), Gen.Point3(random, 40f));

    protected override void Verify(BoxPair sample)
    {
        Bounds3 union = sample.A.Union(sample.B);
        Check.True(union.Contains(sample.A.Min) && union.Contains(sample.A.Max) && union.Contains(sample.B.Min) && union.Contains(sample.B.Max), "union holds both");
        Check.True(sample.A.Encapsulate(sample.Point).Contains(sample.Point), "encapsulate holds the point");
        Check.Equal(sample.A.Intersects(sample.B), sample.B.Intersects(sample.A), "intersects either way round");
        bool overlap = Overlap(sample.A.Min.X, sample.A.Max.X, sample.B.Min.X, sample.B.Max.X)
            && Overlap(sample.A.Min.Y, sample.A.Max.Y, sample.B.Min.Y, sample.B.Max.Y)
            && Overlap(sample.A.Min.Z, sample.A.Max.Z, sample.B.Min.Z, sample.B.Max.Z);
        Check.Equal(overlap, sample.A.Intersects(sample.B), "intersects means overlap on every axis");
    }

    private static bool Overlap(float minA, float maxA, float minB, float maxB) => minA <= maxB && minB <= maxA;
}
