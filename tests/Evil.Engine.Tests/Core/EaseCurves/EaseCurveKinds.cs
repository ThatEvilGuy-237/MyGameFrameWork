using Evil.Engine.Core;

namespace Evil.Engine.Tests.Core.EaseCurves;

internal static class EaseCurveKinds
{
    public static readonly Easing[] Every = Enum.GetValues<Easing>();

    public static readonly Easing[] NeverOvershoot =
        [Easing.Linear, Easing.InQuad, Easing.OutQuad, Easing.InOutQuad, Easing.InCubic, Easing.OutCubic, Easing.InOutCubic, Easing.InOutSine];

    public static readonly Easing[] TheSameFromBothEnds = [Easing.InOutQuad, Easing.InOutCubic, Easing.InOutSine];

    public static readonly EaseMirror[] Mirrors = [new EaseMirror(Easing.InQuad, Easing.OutQuad, 0f), new EaseMirror(Easing.InCubic, Easing.OutCubic, 0f)];
}
