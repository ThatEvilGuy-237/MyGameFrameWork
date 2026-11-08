using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.GoalSmoothing;

internal sealed class SmoothingNeverOvershootsTheGoal : Property<SmoothingStep>
{
    protected override SmoothingStep Generate(Random random) =>
        new SmoothingStep(Gen.Number(random, -100f, 100f), Gen.Number(random, -100f, 100f), Gen.Number(random, 0f, 50f), Gen.Number(random, 0f, 2f));

    protected override void Verify(SmoothingStep sample)
    {
        float next = Smoothing.Toward(sample.From, sample.Goal, sample.Sharpness, sample.Seconds);
        float low = MathF.Min(sample.From, sample.Goal) - 1e-4f;
        float high = MathF.Max(sample.From, sample.Goal) + 1e-4f;
        Check.True(next >= low && next <= high, $"{next} is not between {sample.From} and {sample.Goal}");
    }
}
