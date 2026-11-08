using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed class RayHitsMatchGodots : Property<RayProbe>
{
    private const float Far = 1000f;

    protected override RayProbe Generate(Random random)
    {
        Bounds3 box = Gen.Box(random, 8f);
        Vector3 origin = Gen.Point3(random, 30f);
        Vector3 aim = Gen.Chance(random, 0.6) ? box.Center + Gen.Point3(random, 1.2f) * box.Size / 2f - origin : Gen.Direction(random);
        return new RayProbe(box, Gen.Number(random, 0.1f, 4f), new Ray3(origin, aim.LengthSquared() > 1e-6f ? aim : Vector3.UnitX));
    }

    protected override void Verify(RayProbe sample)
    {
        bool startsInside = sample.Box.Expand(0.01f).Contains(sample.Ray.Origin)
            || Vector3.Distance(sample.Ray.Origin, sample.Box.Center) <= sample.Radius + 0.01f;
        if (startsInside)
        {
            return;
        }

        Godot.Vector3 from = sample.Ray.Origin.ToGodot();
        Godot.Vector3 to = from + sample.Ray.Direction.ToGodot() * Far;
        bool godotHitsTheBox = GodotGeometry.ToAabb(sample.Box).IntersectsSegment(from, to);
        Check.Equal(godotHitsTheBox, sample.Ray.Intersect(sample.Box) is not null, "box hit or miss");

        Godot.Vector3[] sphere = Godot.Geometry3D.SegmentIntersectsSphere(from, to, sample.Box.Center.ToGodot(), sample.Radius);
        GodotGeometry.CheckSame(sphere.Length > 0 ? sphere[0].ToEvil() : null, sample.Ray, sample.Ray.IntersectSphere(sample.Box.Center, sample.Radius), "sphere");

        Godot.Plane plane = new Godot.Plane(Vector3.UnitY.ToGodot(), sample.Box.Center.ToGodot());
        Vector3? godotsPlaneHit = plane.IntersectsRay(from, sample.Ray.Direction.ToGodot())?.ToEvil();
        GodotGeometry.CheckSame(godotsPlaneHit, sample.Ray, sample.Ray.IntersectPlane(sample.Box.Center, Vector3.UnitY), "plane");
    }
}
