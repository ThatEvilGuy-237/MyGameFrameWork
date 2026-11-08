using Evil.Engine.Core;

namespace Evil.Engine.Tests.Core.EaseCurves;

internal static class EaseCurveTests
{
    [Test]
    public static void EveryEasingStartsAtZeroAndEndsAtOne()
    {
        foreach (Easing easing in Enum.GetValues<Easing>())
        {
            Check.Near(0f, Ease.Apply(easing, 0f), $"{easing} at 0", 1e-5f);
            Check.Near(1f, Ease.Apply(easing, 1f), $"{easing} at 1", 1e-5f);
        }
    }
}
