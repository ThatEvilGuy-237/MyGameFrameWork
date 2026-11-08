using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rays;

internal sealed class ARayHitsAPlaneOnThePlaneOrNotAtAll : Property<RayAtPlane>
{
    protected override RayAtPlane Generate(Random random) =>
        new RayAtPlane(Gen.Point3(random, 20f), Gen.Direction(random), new Ray3(Gen.Point3(random, 30f), Gen.Direction(random)));

    protected override void Verify(RayAtPlane sample)
    {
        float? hit = sample.Ray.IntersectPlane(sample.Point, sample.Normal);
        float facing = Vector3.Dot(sample.Ray.Direction, sample.Normal);
        float ahead = Vector3.Dot(sample.Point - sample.Ray.Origin, sample.Normal);
        if (hit is not float distance)
        {
            Check.True(MathF.Abs(facing) < 1e-6f || ahead / facing < 0f, "missed a plane that lies ahead");
            return;
        }

        Check.True(distance >= 0f, "never behind the ray");
        Check.Near(0f, Vector3.Dot(sample.Ray.PointAt(distance) - sample.Point, sample.Normal), "on the plane", 1e-3f * (1f + distance));
    }
}
