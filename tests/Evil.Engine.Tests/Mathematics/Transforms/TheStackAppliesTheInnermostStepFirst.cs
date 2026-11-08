using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Transforms;

internal sealed class TheStackAppliesTheInnermostStepFirst : Property<MoveTurnScale>
{
    protected override MoveTurnScale Generate(Random random) =>
        new MoveTurnScale(Gen.Point2(random, 100f), Gen.Number(random, -360f, 360f), ScaleStep.RandomFactor(random), Gen.Point2(random, 50f));

    protected override void Verify(MoveTurnScale sample)
    {
        TransformStack stack = new TransformStack();
        stack.Translate(sample.Move);
        stack.Rotate(sample.Degrees);
        stack.Scale(sample.Scale);
        Vector2 expected = sample.Move + PlaneTurn.Apply(sample.Point * sample.Scale, sample.Degrees);
        Check.Near(expected, stack.ToWorld(sample.Point), "move after turn after scale", 1e-3f * (1f + expected.Length()));
    }
}
