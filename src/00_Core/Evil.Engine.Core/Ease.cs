namespace Evil.Engine.Core;

public static class Ease
{
    public static float Apply(Easing easing, float progress)
    {
        var t = Math.Clamp(progress, 0f, 1f);
        return easing switch
        {
            Easing.InQuad => t * t,
            Easing.OutQuad => 1f - (1f - t) * (1f - t),
            Easing.InOutQuad => t < 0.5f ? 2f * t * t : 1f - MathF.Pow(-2f * t + 2f, 2f) / 2f,
            Easing.InCubic => t * t * t,
            Easing.OutCubic => 1f - MathF.Pow(1f - t, 3f),
            Easing.InOutCubic => t < 0.5f ? 4f * t * t * t : 1f - MathF.Pow(-2f * t + 2f, 3f) / 2f,
            Easing.InOutSine => -(MathF.Cos(MathF.PI * t) - 1f) / 2f,
            Easing.OutBack => OutBack(t),
            Easing.OutBounce => OutBounce(t),
            _ => t,
        };
    }

    private static float OutBack(float t)
    {
        const float overshoot = 1.70158f;
        const float lift = overshoot + 1f;
        return 1f + lift * MathF.Pow(t - 1f, 3f) + overshoot * MathF.Pow(t - 1f, 2f);
    }

    private static float OutBounce(float t)
    {
        const float strength = 7.5625f;
        const float width = 2.75f;
        if (t < 1f / width)
        {
            return strength * t * t;
        }

        if (t < 2f / width)
        {
            t -= 1.5f / width;
            return strength * t * t + 0.75f;
        }

        if (t < 2.5f / width)
        {
            t -= 2.25f / width;
            return strength * t * t + 0.9375f;
        }

        t -= 2.625f / width;
        return strength * t * t + 0.984375f;
    }
}
