using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Angles;

internal sealed class RadiansAndDegreesRoundTrip : Property<float>
{
    protected override float Generate(Random random) =>
        Gen.Number(random, -10000f, 10000f);

    protected override void Verify(float sample)
    {
        Check.Near(sample, Angle.ToDegrees(Angle.ToRadians(sample)), "round trip", MathF.Abs(sample) * 1e-6f + 1e-6f);
    }
}
