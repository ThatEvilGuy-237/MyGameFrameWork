using Evil.Engine.Core;

namespace Evil.Engine.Tests.Core.Colors;

internal sealed class LerpHitsBothEndsAndTheMiddle : Property<ColorPair>
{
    protected override ColorPair Generate(Random random) =>
        new ColorPair(RandomColor(random), RandomColor(random));

    protected override void Verify(ColorPair sample)
    {
        CheckNear(sample.From, Color.Lerp(sample.From, sample.To, 0f), "at 0");
        CheckNear(sample.To, Color.Lerp(sample.From, sample.To, 1f), "at 1");
        Color middle = new Color((sample.From.R + sample.To.R) / 2f, (sample.From.G + sample.To.G) / 2f, (sample.From.B + sample.To.B) / 2f, (sample.From.A + sample.To.A) / 2f);
        CheckNear(middle, Color.Lerp(sample.From, sample.To, 0.5f), "half way");
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
