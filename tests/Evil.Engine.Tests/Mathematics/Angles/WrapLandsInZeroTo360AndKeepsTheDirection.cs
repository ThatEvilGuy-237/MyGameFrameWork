using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Angles;

internal sealed class WrapLandsInZeroTo360AndKeepsTheDirection : Property<float>
{
    protected override float Generate(Random random) =>
        Gen.Chance(random, 0.1) ? 360f * random.Next(-30, 30) : Gen.Number(random, -10000f, 10000f);

    protected override void Verify(float sample)
    {
        float wrapped = Angle.Wrap(sample);
        Check.True(wrapped >= 0f && wrapped < 360f, $"{wrapped} is outside 0..360");
        double turns = ((double)sample - wrapped) / 360.0;
        Check.True(Math.Abs(turns - Math.Round(turns)) < 1e-4, $"{sample} and {wrapped} are {turns} turns apart, not whole turns");
    }
}
