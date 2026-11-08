using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Orbits;

internal sealed class AnOrbitSitsAtItsDistanceAndLooksAtTheTarget : Property<OrbitSpot>
{
    protected override OrbitSpot Generate(Random random) =>
        new OrbitSpot(Gen.Point3(random, 50f), Gen.Number(random, -360f, 360f), Gen.Number(random, -89f, 89f), Gen.Number(random, 0.5f, 100f));

    protected override void Verify(OrbitSpot sample)
    {
        Pose eye = Orbit.PoseFor(sample.Target, sample.Yaw, sample.Pitch, sample.Distance);
        Check.Near(sample.Distance, Vector3.Distance(eye.Position, sample.Target), "distance", 1e-3f * sample.Distance);
        Check.Near(Vector3.Normalize(sample.Target - eye.Position), eye.Forward, "looks at the target", 1e-3f);
        Check.Near(sample.Distance * MathF.Sin(Angle.ToRadians(sample.Pitch)), eye.Position.Y - sample.Target.Y, "height from the pitch", 1e-3f * sample.Distance);
    }
}
