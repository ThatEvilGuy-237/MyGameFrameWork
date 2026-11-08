using Evil.Engine.Core;

namespace Evil.Engine.Tests;

internal static class EaseTests
{
    [Test]
    public static void EveryCurveStartsAtZeroAndEndsAtOne()
    {
        foreach (var easing in Enum.GetValues<Easing>())
        {
            Check.Near(0f, Ease.Apply(easing, 0f), $"{easing} at 0");
            Check.Near(1f, Ease.Apply(easing, 1f), $"{easing} at 1");
            Check.Near(1f, Ease.Apply(easing, 3f), $"{easing} past the end stays at the end");
            Check.Near(0f, Ease.Apply(easing, -1f), $"{easing} before the start stays at the start");
        }
    }

    [Test]
    public static void CurvesHaveTheirShape()
    {
        Check.Near(0.5f, Ease.Apply(Easing.Linear, 0.5f), "linear halfway");
        Check.Near(0.5f, Ease.Apply(Easing.InOutCubic, 0.5f), "in-out is symmetric");
        Check.True(Ease.Apply(Easing.InCubic, 0.3f) < 0.3f, "in starts slow");
        Check.True(Ease.Apply(Easing.OutCubic, 0.3f) > 0.3f, "out starts fast");
        Check.True(Enumerable.Range(1, 9).Any(i => Ease.Apply(Easing.OutBack, i / 10f) > 1f), "back overshoots");
        var steps = Enumerable.Range(0, 21).Select(i => Ease.Apply(Easing.InOutSine, i / 20f)).ToList();
        Check.True(steps.Zip(steps.Skip(1)).All(pair => pair.Second >= pair.First), "sine never goes back");
    }
}
