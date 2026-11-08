using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rays;

internal static class RayRegressionTests
{
    [Test]
    public static void ASmallFarSphereIsHitAtItsEdge()
    {
        Ray3 ray = new Ray3(new Vector3(-27.376598f, -21.737553f, -14.57868f), new Vector3(0.76469314f, 0.50726354f, 0.39740166f));
        Vector3 center = new Vector3(9.059124f, 2.5482254f, 4.356447f);
        float? hit = ray.IntersectSphere(center, 0.10181996f);
        Check.True(hit is not null, "a ray passing 0.0999 from the center of a 0.1018 sphere 50 away hits it");
        Check.Near(0.10181996f, Vector3.Distance(center, ray.PointAt(hit!.Value)), "on the surface", 1e-3f);
    }
}
