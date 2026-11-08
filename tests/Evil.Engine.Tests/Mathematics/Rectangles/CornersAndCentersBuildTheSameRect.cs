using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rectangles;

internal sealed class CornersAndCentersBuildTheSameRect : Property<CornerPair>
{
    protected override CornerPair Generate(Random random) =>
        new CornerPair(Gen.Point2(random, 50f), Gen.Point2(random, 50f), Gen.Number(random, -2f, 2f));

    protected override void Verify(CornerPair sample)
    {
        RectF rect = RectF.FromCorners(sample.A, sample.B);
        Check.Equal(rect, RectF.FromCorners(sample.B, sample.A), "either corner first");
        Check.Near((sample.A + sample.B) / 2f, rect.Center, "center", 1e-3f);
        Check.Near(rect.Center, RectF.FromCenter(rect.Center, rect.Size).Center, "from the center", 1e-3f);
        RectF back = rect.Inflate(sample.Grow).Inflate(-sample.Grow);
        Check.Near(rect.Position, back.Position, "inflate and back", 1e-3f);
        Check.Near(rect.Size, back.Size, "inflate and back", 1e-3f);
    }
}
