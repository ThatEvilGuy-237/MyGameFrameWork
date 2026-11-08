using System.Numerics;
using Evil.Engine.Core;

namespace Evil.Engine.Tests.Properties;

internal static class EaseAndColorProperties
{
    private static readonly Easing[] Every = Enum.GetValues<Easing>();

    private static readonly Easing[] NeverOvershoot =
        [Easing.Linear, Easing.InQuad, Easing.OutQuad, Easing.InOutQuad, Easing.InCubic, Easing.OutCubic, Easing.InOutCubic, Easing.InOutSine];

    [Test]
    public static void EveryEasingStartsAtZeroAndEndsAtOne()
    {
        foreach (Easing easing in Every)
        {
            Check.Near(0f, Ease.Apply(easing, 0f), $"{easing} at 0", 1e-5f);
            Check.Near(1f, Ease.Apply(easing, 1f), $"{easing} at 1", 1e-5f);
        }
    }

    [Test]
    public static void ProgressOutsideZeroToOneIsClamped()
    {
        Property.ForAll(
            nameof(ProgressOutsideZeroToOneIsClamped),
            random => (Easing: Gen.Pick(random, Every), Before: Gen.Number(random, -5f, 0f), After: Gen.Number(random, 1f, 5f)),
            input =>
            {
                Check.Equal(Ease.Apply(input.Easing, 0f), Ease.Apply(input.Easing, input.Before), "before the start");
                Check.Equal(Ease.Apply(input.Easing, 1f), Ease.Apply(input.Easing, input.After), "after the end");
            });
    }

    [Test]
    public static void EasingsWithoutOvershootNeverGoBack()
    {
        Property.ForAll(
            nameof(EasingsWithoutOvershootNeverGoBack),
            random => (Easing: Gen.Pick(random, NeverOvershoot), A: Gen.Number(random, 0f, 1f), B: Gen.Number(random, 0f, 1f)),
            input =>
            {
                float early = Ease.Apply(input.Easing, MathF.Min(input.A, input.B));
                float late = Ease.Apply(input.Easing, MathF.Max(input.A, input.B));
                Check.True(early <= late + 1e-6f, $"{input.Easing} went back from {early} to {late}");
                Check.True(early >= -1e-6f && late <= 1f + 1e-6f, $"{input.Easing} left 0..1");
            });
    }

    [Test]
    public static void OutIsInPlayedBackwards()
    {
        (Easing In, Easing Out)[] pairs = [(Easing.InQuad, Easing.OutQuad), (Easing.InCubic, Easing.OutCubic)];
        Property.ForAll(
            nameof(OutIsInPlayedBackwards),
            random => (Pair: Gen.Pick(random, pairs), T: Gen.Number(random, 0f, 1f)),
            input => Check.Near(1f - Ease.Apply(input.Pair.In, 1f - input.T), Ease.Apply(input.Pair.Out, input.T), $"{input.Pair.Out} at {input.T}", 1e-5f));
    }

    [Test]
    public static void InOutIsTheSameFromBothEnds()
    {
        Easing[] symmetric = [Easing.InOutQuad, Easing.InOutCubic, Easing.InOutSine];
        Property.ForAll(
            nameof(InOutIsTheSameFromBothEnds),
            random => (Easing: Gen.Pick(random, symmetric), T: Gen.Number(random, 0f, 1f)),
            input => Check.Near(1f, Ease.Apply(input.Easing, input.T) + Ease.Apply(input.Easing, 1f - input.T), $"{input.Easing} at {input.T}", 1e-5f));
    }

    [Test]
    public static void HexRoundTripsEveryByteColour()
    {
        Property.ForAll(
            nameof(HexRoundTripsEveryByteColour),
            random => Color.FromBytes((byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256), (byte)random.Next(256)),
            color =>
            {
                Check.Equal(color, Color.FromHex(color.ToHex()), "through hex");
                Check.Equal(color, Color.FromHex(color.ToHex().ToLowerInvariant()), "through lowercase hex");
            });
    }

    [Test]
    public static void LerpHitsBothEndsAndTheMiddle()
    {
        Property.ForAll(
            nameof(LerpHitsBothEndsAndTheMiddle),
            random => (From: RandomColor(random), To: RandomColor(random)),
            input =>
            {
                CheckNear(input.From, Color.Lerp(input.From, input.To, 0f), "at 0");
                CheckNear(input.To, Color.Lerp(input.From, input.To, 1f), "at 1");
                Color middle = new Color((input.From.R + input.To.R) / 2f, (input.From.G + input.To.G) / 2f, (input.From.B + input.To.B) / 2f, (input.From.A + input.To.A) / 2f);
                CheckNear(middle, Color.Lerp(input.From, input.To, 0.5f), "half way");
            });
    }

    private static Color RandomColor(Random random) =>
        new Color(Gen.Number(random, 0f, 1f), Gen.Number(random, 0f, 1f), Gen.Number(random, 0f, 1f), Gen.Number(random, 0f, 1f));

    private static void CheckNear(Color expected, Color actual, string what)
    {
        Check.Near(expected.R, actual.R, $"{what} red", 1e-5f);
        Check.Near(expected.G, actual.G, $"{what} green", 1e-5f);
        Check.Near(expected.B, actual.B, $"{what} blue", 1e-5f);
        Check.Near(expected.A, actual.A, $"{what} alpha", 1e-5f);
    }
}
