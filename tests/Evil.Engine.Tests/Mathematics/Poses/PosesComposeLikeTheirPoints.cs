using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Poses;

internal sealed class PosesComposeLikeTheirPoints : Property<NestedPoses>
{
    protected override NestedPoses Generate(Random random) =>
        new NestedPoses(Gen.Placement(random, true), Gen.Placement(random, false), Gen.Point3(random, 10f));

    protected override void Verify(NestedPoses sample)
    {
        Vector3 stepByStep = sample.Parent.ToWorld(sample.Child.ToWorld(sample.Point));
        Vector3 combined = sample.Parent.ToWorld(sample.Child).ToWorld(sample.Point);
        Check.Near(stepByStep, combined, "child in parent", 1e-3f * (1f + stepByStep.Length()));
    }
}
