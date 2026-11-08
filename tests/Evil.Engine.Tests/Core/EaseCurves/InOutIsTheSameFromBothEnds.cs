using Evil.Engine.Core;

namespace Evil.Engine.Tests.Core.EaseCurves;

internal sealed class InOutIsTheSameFromBothEnds : Property<EaseAt>
{
    protected override EaseAt Generate(Random random) =>
        new EaseAt(Gen.Pick(random, EaseCurveKinds.TheSameFromBothEnds), Gen.Number(random, 0f, 1f));

    protected override void Verify(EaseAt sample)
    {
        float both = Ease.Apply(sample.Easing, sample.Progress) + Ease.Apply(sample.Easing, 1f - sample.Progress);
        Check.Near(1f, both, $"{sample.Easing} at {sample.Progress}", 1e-5f);
    }
}
