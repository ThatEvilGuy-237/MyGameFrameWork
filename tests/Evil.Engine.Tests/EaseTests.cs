using Evil.Engine.Core;

namespace Evil.Engine.Tests;

internal static class EaseTests
{
    [Test]
    public static void EveryCurveStartsAtZeroAndEndsAtOne()
    {
        foreach (Easing easing in Enum.GetValues<Easing>())
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
        Check.True(OutBackOvershoots(), "back overshoots");
        Check.True(InOutSineNeverGoesBack(), "sine never goes back");
    }

    private static bool OutBackOvershoots()
    {
        for (int step = 1; step < 10; step++)
        {
            if (Ease.Apply(Easing.OutBack, step / 10f) > 1f)
            {
                return true;
            }
        }

        return false;
    }

    private static bool InOutSineNeverGoesBack()
    {
        float last = 0f;
        for (int step = 0; step <= 20; step++)
        {
            float now = Ease.Apply(Easing.InOutSine, step / 20f);
            if (now < last)
            {
                return false;
            }

            last = now;
        }

        return true;
    }
}
