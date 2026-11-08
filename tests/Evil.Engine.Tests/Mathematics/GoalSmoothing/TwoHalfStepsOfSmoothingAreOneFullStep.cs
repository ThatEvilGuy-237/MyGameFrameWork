using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.GoalSmoothing;

internal sealed class TwoHalfStepsOfSmoothingAreOneFullStep : Property<SmoothingStep>
{
    protected override SmoothingStep Generate(Random random) =>
        new SmoothingStep(Gen.Number(random, -100f, 100f), Gen.Number(random, -100f, 100f), Gen.Number(random, 0.1f, 20f), Gen.Number(random, 0f, 0.5f));

    protected override void Verify(SmoothingStep sample)
    {
        float half = Smoothing.Toward(sample.From, sample.Goal, sample.Sharpness, sample.Seconds / 2f);
        float twice = Smoothing.Toward(half, sample.Goal, sample.Sharpness, sample.Seconds / 2f);
        float once = Smoothing.Toward(sample.From, sample.Goal, sample.Sharpness, sample.Seconds);
        Check.Near(once, twice, "the same at any frame rate", 1e-4f * (1f + MathF.Abs(sample.From - sample.Goal)));
    }
}
