using Evil.Engine.Core;

namespace Evil.Engine.Tests.Core.EaseCurves;

internal sealed class ProgressOutsideZeroToOneIsClamped : Property<EaseAt>
{
    protected override EaseAt Generate(Random random)
    {
        float outside = Gen.Chance(random, 0.5) ? Gen.Number(random, -5f, 0f) : Gen.Number(random, 1f, 5f);
        return new EaseAt(Gen.Pick(random, EaseCurveKinds.Every), outside);
    }

    protected override void Verify(EaseAt sample)
    {
        float edge = sample.Progress < 0.5f ? 0f : 1f;
        Check.Equal(Ease.Apply(sample.Easing, edge), Ease.Apply(sample.Easing, sample.Progress), "held at the nearest end");
    }
}
