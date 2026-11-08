using System.Numerics;
using Evil.Engine.Mathematics;

namespace Evil.Engine.Tests.Properties;

internal static class RayAndBoundsProperties
{
    private const float Step = 0.01f;
    private const float Reach = 120f;

    [Test]
    public static void BoundsFromPointsHoldEveryPointTightly()
    {
        Property.ForAll(
            nameof(BoundsFromPointsHoldEveryPointTightly),
            random => Enumerable.Range(0, random.Next(1, 20)).Select(_ => Gen.Point3(random, 50f)).ToArray(),
            points =>
            {
                Bounds3 box = Bounds3.FromPoints(points);
                Check.True(points.All(box.Contains), "every point inside");
                Check.Equal(points.Min(point => point.X), box.Min.X, "tight on the left");
                Check.Equal(points.Max(point => point.Y), box.Max.Y, "tight on the top");
            });
    }

    [Test]
    public static void UnionAndEncapsulateHoldBoth()
    {
        Property.ForAll(
            nameof(UnionAndEncapsulateHoldBoth),
            random => (A: Gen.Box(random, 20f), B: Gen.Box(random, 20f), Point: Gen.Point3(random, 40f)),
            input =>
            {
                Bounds3 union = input.A.Union(input.B);
                Check.True(union.Contains(input.A.Min) && union.Contains(input.A.Max) && union.Contains(input.B.Min) && union.Contains(input.B.Max), "union holds both");
                Check.True(input.A.Encapsulate(input.Point).Contains(input.Point), "encapsulate holds the point");
                Check.Equal(input.A.Intersects(input.B), input.B.Intersects(input.A), "intersects either way round");
                bool overlapEveryAxis = Overlap(input.A.Min.X, input.A.Max.X, input.B.Min.X, input.B.Max.X)
                    && Overlap(input.A.Min.Y, input.A.Max.Y, input.B.Min.Y, input.B.Max.Y)
                    && Overlap(input.A.Min.Z, input.A.Max.Z, input.B.Min.Z, input.B.Max.Z);
                Check.Equal(overlapEveryAxis, input.A.Intersects(input.B), "intersects means overlap on every axis");
            });
    }

    [Test]
    public static void ARayHitsABoxWhereWalkingAlongItFindsIt()
    {
        Property.ForAll(
            nameof(ARayHitsABoxWhereWalkingAlongItFindsIt),
            random =>
            {
                Bounds3 box = Gen.Box(random, 10f);
                Vector3 origin = Gen.Point3(random, 30f);
                Vector3 aim = Gen.Chance(random, 0.5) ? box.Center + Gen.Point3(random, 1f) * box.Size / 2f - origin : Gen.Direction(random);
                return (Box: box, Ray: new Ray3(origin, aim.LengthSquared() > 1e-6f ? aim : Vector3.UnitX));
            },
            input =>
            {
                float? hit = input.Ray.Intersect(input.Box);
                float? walked = FirstInside(input.Ray, input.Box.Expand(-0.02f));
                if (hit is not float distance)
                {
                    Check.True(walked is null, $"the box was missed, but walking found it {walked} along");
                    return;
                }

                Check.True(input.Box.Expand(1e-3f).Contains(input.Ray.PointAt(distance)), $"the hit at {distance} is not on the box");
                if (walked is float inside)
                {
                    Check.True(distance <= inside + Step, $"the hit at {distance} comes after walking found the box at {inside}");
                }
            });
    }

    [Test]
    public static void ARayHitsASphereOnItsSurface()
    {
        Property.ForAll(
            nameof(ARayHitsASphereOnItsSurface),
            random =>
            {
                Vector3 center = Gen.Point3(random, 10f);
                float radius = Gen.Number(random, 0.1f, 8f);
                Vector3 origin = Gen.Point3(random, 30f);
                Vector3 aim = Gen.Chance(random, 0.5) ? center + Gen.Point3(random, radius) - origin : Gen.Direction(random);
                return (Center: center, Radius: radius, Ray: new Ray3(origin, aim.LengthSquared() > 1e-6f ? aim : Vector3.UnitX));
            },
            input =>
            {
                float? hit = input.Ray.IntersectSphere(input.Center, input.Radius);
                float along = Vector3.Dot(input.Center - input.Ray.Origin, input.Ray.Direction);
                float closest = Vector3.Distance(input.Center, input.Ray.PointAt(MathF.Max(0f, along)));
                bool startsInside = Vector3.Distance(input.Center, input.Ray.Origin) <= input.Radius;
                if (hit is not float distance)
                {
                    Check.True(closest >= input.Radius - 1e-3f, $"missed, but the ray comes within {closest} of a {input.Radius} sphere");
                    return;
                }

                if (startsInside)
                {
                    Check.Equal(0f, distance, "a ray starting inside hits at once");
                    return;
                }

                Check.Near(input.Radius, Vector3.Distance(input.Center, input.Ray.PointAt(distance)), "on the surface", 1e-3f * (1f + input.Radius + distance));
                Check.True(distance <= along + 1e-3f, "the near side is hit first");
            });
    }

    [Test]
    public static void ASmallFarSphereIsHitAtItsEdge()
    {
        Ray3 ray = new Ray3(new Vector3(-27.376598f, -21.737553f, -14.57868f), new Vector3(0.76469314f, 0.50726354f, 0.39740166f));
        Vector3 center = new Vector3(9.059124f, 2.5482254f, 4.356447f);
        float? hit = ray.IntersectSphere(center, 0.10181996f);
        Check.True(hit is not null, "a ray passing 0.0999 from the center of a 0.1018 sphere 50 away hits it");
        Check.Near(0.10181996f, Vector3.Distance(center, ray.PointAt(hit!.Value)), "on the surface", 1e-3f);
    }

    [Test]
    public static void ARayHitsAPlaneOnThePlaneOrNotAtAll()
    {
        Property.ForAll(
            nameof(ARayHitsAPlaneOnThePlaneOrNotAtAll),
            random => (Point: Gen.Point3(random, 20f), Normal: Gen.Direction(random), Ray: new Ray3(Gen.Point3(random, 30f), Gen.Direction(random))),
            input =>
            {
                float? hit = input.Ray.IntersectPlane(input.Point, input.Normal);
                float facing = Vector3.Dot(input.Ray.Direction, input.Normal);
                float ahead = Vector3.Dot(input.Point - input.Ray.Origin, input.Normal);
                if (hit is not float distance)
                {
                    Check.True(MathF.Abs(facing) < 1e-6f || ahead / facing < 0f, "missed a plane that lies ahead");
                    return;
                }

                Check.True(distance >= 0f, "never behind the ray");
                Check.Near(0f, Vector3.Dot(input.Ray.PointAt(distance) - input.Point, input.Normal), "on the plane", 1e-3f * (1f + distance));
            });
    }

    private static float? FirstInside(Ray3 ray, Bounds3 box)
    {
        for (float distance = 0f; distance <= Reach; distance += Step)
        {
            if (box.Contains(ray.PointAt(distance)))
            {
                return distance;
            }
        }

        return null;
    }

    private static bool Overlap(float minA, float maxA, float minB, float maxB) => minA <= maxB && minB <= maxA;
}
