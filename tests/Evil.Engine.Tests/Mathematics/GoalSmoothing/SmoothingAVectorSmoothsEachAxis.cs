using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.GoalSmoothing;

internal sealed class SmoothingAVectorSmoothsEachAxis : Property<SmoothingMove>
{
    protected override SmoothingMove Generate(Random random) =>
        new SmoothingMove(Gen.Point3(random, 100f), Gen.Point3(random, 100f), Gen.Number(random, 0.1f, 20f), Gen.Number(random, 0f, 1f));

    protected override void Verify(SmoothingMove sample)
    {
        Vector3 together = Smoothing.Toward(sample.From, sample.Goal, sample.Sharpness, sample.Seconds);
        Vector3 apart = new Vector3(Axis(sample, sample.From.X, sample.Goal.X), Axis(sample, sample.From.Y, sample.Goal.Y), Axis(sample, sample.From.Z, sample.Goal.Z));
        Check.Near(apart, together, "per axis", 1e-4f);
    }

    private static float Axis(SmoothingMove sample, float from, float goal) => Smoothing.Toward(from, goal, sample.Sharpness, sample.Seconds);
}
