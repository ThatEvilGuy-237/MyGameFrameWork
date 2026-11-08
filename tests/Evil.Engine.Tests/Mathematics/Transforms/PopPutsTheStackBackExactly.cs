using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Transforms;

internal sealed class PopPutsTheStackBackExactly : Property<StepsAroundAGroup>
{
    protected override StepsAroundAGroup Generate(Random random) =>
        new StepsAroundAGroup(TransformStep.RandomSequence(random), TransformStep.RandomSequence(random));

    protected override void Verify(StepsAroundAGroup sample)
    {
        TransformStack stack = new TransformStack();
        TransformStep.ApplyAll(stack, sample.Before);
        Matrix3x2 saved = stack.Current;
        using (stack.Group())
        {
            TransformStep.ApplyAll(stack, sample.Inside);
        }

        Check.Equal(saved, stack.Current, "after the group");
        Check.Equal(0, stack.Depth, "depth");
    }
}
