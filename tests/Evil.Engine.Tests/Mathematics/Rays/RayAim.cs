using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Mathematics.Rays;

internal static class RayAim
{
    public static Ray3 Toward(Random random, Vector3 origin, Vector3 target)
    {
        Vector3 aim = Gen.Chance(random, 0.5) ? target - origin : Gen.Direction(random);
        return new Ray3(origin, aim.LengthSquared() > 1e-6f ? aim : Vector3.UnitX);
    }

    public static float? FirstInside(Ray3 ray, Bounds3 box, float step, float reach)
    {
        for (float distance = 0f; distance <= reach; distance += step)
        {
            if (box.Contains(ray.PointAt(distance)))
            {
                return distance;
            }
        }

        return null;
    }
}
