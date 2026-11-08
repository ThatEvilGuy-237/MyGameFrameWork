using Evil.Engine.Core;

namespace Evil.Engine.Tests.Core.EaseCurves;

internal sealed class EasingsWithoutOvershootNeverGoBack : Property<EaseSpan>
{
    protected override EaseSpan Generate(Random random) =>
        new EaseSpan(Gen.Pick(random, EaseCurveKinds.NeverOvershoot), Gen.Number(random, 0f, 1f), Gen.Number(random, 0f, 1f));

    protected override void Verify(EaseSpan sample)
    {
        float early = Ease.Apply(sample.Easing, MathF.Min(sample.From, sample.To));
        float late = Ease.Apply(sample.Easing, MathF.Max(sample.From, sample.To));
        Check.True(early <= late + 1e-6f, $"{sample.Easing} went back from {early} to {late}");
        Check.True(early >= -1e-6f && late <= 1f + 1e-6f, $"{sample.Easing} left 0..1");
    }
}
