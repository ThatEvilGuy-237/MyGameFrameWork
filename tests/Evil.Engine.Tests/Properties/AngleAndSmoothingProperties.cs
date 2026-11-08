using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties;

internal static class AngleAndSmoothingProperties
{
    [Test]
    public static void WrapLandsInZeroTo360AndKeepsTheDirection()
    {
        Property.ForAll(
            nameof(WrapLandsInZeroTo360AndKeepsTheDirection),
            random => Gen.Chance(random, 0.1) ? 360f * random.Next(-30, 30) : Gen.Number(random, -10000f, 10000f),
            degrees =>
            {
                float wrapped = Angle.Wrap(degrees);
                Check.True(wrapped >= 0f && wrapped < 360f, $"{wrapped} is outside 0..360");
                double turns = ((double)degrees - wrapped) / 360.0;
                Check.True(Math.Abs(turns - Math.Round(turns)) < 1e-4, $"{degrees} and {wrapped} are {turns} turns apart, not whole turns");
            });
    }

    [Test]
    public static void RadiansAndDegreesRoundTrip()
    {
        Property.ForAll(
            nameof(RadiansAndDegreesRoundTrip),
            random => Gen.Number(random, -10000f, 10000f),
            degrees => Check.Near(degrees, Angle.ToDegrees(Angle.ToRadians(degrees)), "round trip", MathF.Abs(degrees) * 1e-6f + 1e-6f));
    }

    [Test]
    public static void TwoHalfStepsOfSmoothingAreOneFullStep()
    {
        Property.ForAll(
            nameof(TwoHalfStepsOfSmoothingAreOneFullStep),
            random => (From: Gen.Number(random, -100f, 100f), Goal: Gen.Number(random, -100f, 100f), Sharpness: Gen.Number(random, 0.1f, 20f), Seconds: Gen.Number(random, 0f, 0.5f)),
            input =>
            {
                float half = Smoothing.Toward(input.From, input.Goal, input.Sharpness, input.Seconds / 2f);
                float twice = Smoothing.Toward(half, input.Goal, input.Sharpness, input.Seconds / 2f);
                float once = Smoothing.Toward(input.From, input.Goal, input.Sharpness, input.Seconds);
                Check.Near(once, twice, "the same at any frame rate", 1e-4f * (1f + MathF.Abs(input.From - input.Goal)));
            });
    }

    [Test]
    public static void SmoothingNeverOvershootsTheGoal()
    {
        Property.ForAll(
            nameof(SmoothingNeverOvershootsTheGoal),
            random => (From: Gen.Number(random, -100f, 100f), Goal: Gen.Number(random, -100f, 100f), Sharpness: Gen.Number(random, 0f, 50f), Seconds: Gen.Number(random, 0f, 2f)),
            input =>
            {
                float next = Smoothing.Toward(input.From, input.Goal, input.Sharpness, input.Seconds);
                float low = MathF.Min(input.From, input.Goal) - 1e-4f;
                float high = MathF.Max(input.From, input.Goal) + 1e-4f;
                Check.True(next >= low && next <= high, $"{next} is not between {input.From} and {input.Goal}");
            });
    }

    [Test]
    public static void SmoothingAVectorSmoothsEachAxis()
    {
        Property.ForAll(
            nameof(SmoothingAVectorSmoothsEachAxis),
            random => (From: Gen.Point3(random, 100f), Goal: Gen.Point3(random, 100f), Sharpness: Gen.Number(random, 0.1f, 20f), Seconds: Gen.Number(random, 0f, 1f)),
            input =>
            {
                Vector3 together = Smoothing.Toward(input.From, input.Goal, input.Sharpness, input.Seconds);
                Vector3 apart = new Vector3(
                    Smoothing.Toward(input.From.X, input.Goal.X, input.Sharpness, input.Seconds),
                    Smoothing.Toward(input.From.Y, input.Goal.Y, input.Sharpness, input.Seconds),
                    Smoothing.Toward(input.From.Z, input.Goal.Z, input.Sharpness, input.Seconds));
                Check.Near(apart, together, "per axis", 1e-4f);
            });
    }
}
