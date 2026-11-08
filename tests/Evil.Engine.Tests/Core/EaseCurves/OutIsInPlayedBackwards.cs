using Evil.Engine.Core;

namespace Evil.Engine.Tests.Core.EaseCurves;

internal sealed class OutIsInPlayedBackwards : Property<EaseMirror>
{
    protected override EaseMirror Generate(Random random) =>
        Gen.Pick(random, EaseCurveKinds.Mirrors) with { Progress = Gen.Number(random, 0f, 1f) };

    protected override void Verify(EaseMirror sample)
    {
        float backwards = 1f - Ease.Apply(sample.In, 1f - sample.Progress);
        Check.Near(backwards, Ease.Apply(sample.Out, sample.Progress), $"{sample.Out} at {sample.Progress}", 1e-5f);
    }
}
