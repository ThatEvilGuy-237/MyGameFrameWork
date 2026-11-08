using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Poses;

internal sealed class ToLocalUndoesToWorld : Property<PosePoint>
{
    protected override PosePoint Generate(Random random) =>
        new PosePoint(Gen.Placement(random, false), Gen.Point3(random, 20f));

    protected override void Verify(PosePoint sample)
    {
        Check.Near(sample.Point, sample.Pose.ToLocal(sample.Pose.ToWorld(sample.Point)), "there and back", 1e-3f * (1f + sample.Point.Length()));
    }
}
