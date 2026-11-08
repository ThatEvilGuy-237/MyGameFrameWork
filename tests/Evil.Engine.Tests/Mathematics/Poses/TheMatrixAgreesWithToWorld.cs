using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Poses;

internal sealed class TheMatrixAgreesWithToWorld : Property<PosePoint>
{
    protected override PosePoint Generate(Random random) =>
        new PosePoint(Gen.Placement(random, false), Gen.Point3(random, 20f));

    protected override void Verify(PosePoint sample)
    {
        Vector3 expected = sample.Pose.ToWorld(sample.Point);
        Check.Near(expected, Vector3.Transform(sample.Point, sample.Pose.ToMatrix()), "matrix", 1e-3f * (1f + expected.Length()));
    }
}
