using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Transforms;

internal sealed class ToLocalUndoesToWorld : Property<StepsAndPoint>
{
    protected override StepsAndPoint Generate(Random random) =>
        new StepsAndPoint(TransformStep.RandomSequence(random), Gen.Point2(random, 50f));

    protected override void Verify(StepsAndPoint sample)
    {
        TransformStack stack = new TransformStack();
        TransformStep.ApplyAll(stack, sample.Steps);
        Vector2 back = stack.ToLocal(stack.ToWorld(sample.Point));
        Check.Near(sample.Point, back, "there and back", 1e-2f * (1f + sample.Point.Length()));
    }
}
