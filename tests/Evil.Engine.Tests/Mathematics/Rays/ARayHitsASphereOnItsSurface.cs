using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rays;

internal sealed class ARayHitsASphereOnItsSurface : Property<RayAtSphere>
{
    protected override RayAtSphere Generate(Random random)
    {
        Vector3 center = Gen.Point3(random, 10f);
        float radius = Gen.Number(random, 0.1f, 8f);
        return new RayAtSphere(center, radius, RayAim.Toward(random, Gen.Point3(random, 30f), center + Gen.Point3(random, radius)));
    }

    protected override void Verify(RayAtSphere sample)
    {
        float? hit = sample.Ray.IntersectSphere(sample.Center, sample.Radius);
        float along = Vector3.Dot(sample.Center - sample.Ray.Origin, sample.Ray.Direction);
        float closest = Vector3.Distance(sample.Center, sample.Ray.PointAt(MathF.Max(0f, along)));
        if (hit is not float distance)
        {
            Check.True(closest >= sample.Radius - 1e-3f, $"missed, but the ray comes within {closest} of a {sample.Radius} sphere");
            return;
        }

        if (Vector3.Distance(sample.Center, sample.Ray.Origin) <= sample.Radius)
        {
            Check.Equal(0f, distance, "a ray starting inside hits at once");
            return;
        }

        Check.Near(sample.Radius, Vector3.Distance(sample.Center, sample.Ray.PointAt(distance)), "on the surface", 1e-3f * (1f + sample.Radius + distance));
        Check.True(distance <= along + 1e-3f, "the near side is hit first");
    }
}
