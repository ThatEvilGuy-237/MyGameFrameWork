using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Poses;

internal sealed class ThePoseDirectionsAreTurnedAxes : Property<Pose>
{
    protected override Pose Generate(Random random) =>
        Gen.Placement(random, false);

    protected override void Verify(Pose sample)
    {
        Check.Near(Rotations.Rotate(-Vector3.UnitZ, sample.Rotation), sample.Forward, "forward", 1e-4f);
        Check.Near(0f, Vector3.Dot(sample.Right, sample.Up), "right and up are square", 1e-4f);
        Check.Near(-sample.Forward, Vector3.Cross(sample.Right, sample.Up), "right x up is back", 1e-4f);
    }
}
