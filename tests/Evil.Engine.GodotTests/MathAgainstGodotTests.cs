using Evil.Engine.Input;
using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests;

internal static class MathAgainstGodotTests
{
    [Test]
    public static void EulerTurnsMatchGodotsBasis()
    {
        Property.ForAll(
            nameof(EulerTurnsMatchGodotsBasis),
            random => (Degrees: Gen.Euler(random), Point: Gen.Point3(random, 10f)),
            input =>
            {
                Godot.Vector3 radians = new Godot.Vector3(Godot.Mathf.DegToRad(input.Degrees.X), Godot.Mathf.DegToRad(input.Degrees.Y), Godot.Mathf.DegToRad(input.Degrees.Z));
                Vector3 godots = (Godot.Basis.FromEuler(radians) * input.Point.ToGodot()).ToEvil();
                Vector3 ours = Rotations.Rotate(input.Point, Rotations.FromEulerDegrees(input.Degrees));
                Check.Near(godots, ours, "turned point", 1e-3f * (1f + input.Point.Length()));
            });
    }

    [Test]
    public static void LookAlongMatchesGodotsLookingAt()
    {
        Property.ForAll(
            nameof(LookAlongMatchesGodotsLookingAt),
            random => Gen.Direction(random),
            direction =>
            {
                if (MathF.Abs(direction.Y) > 0.99f)
                {
                    return;
                }

                Godot.Basis godots = Godot.Basis.LookingAt(direction.ToGodot(), Godot.Vector3.Up);
                Quaternion ours = Rotations.LookAlong(direction);
                Check.Near((godots * new Godot.Vector3(0f, 0f, -1f)).ToEvil(), Rotations.Rotate(Rotations.Forward, ours), "forward", 1e-4f);
                Check.Near((godots * Godot.Vector3.Right).ToEvil(), Rotations.Rotate(Vector3.UnitX, ours), "right", 1e-4f);
            });
    }

    [Test]
    public static void PosesMatchGodotsTransforms()
    {
        Property.ForAll(
            nameof(PosesMatchGodotsTransforms),
            random => (Pose: Gen.Placement(random, false), Point: Gen.Point3(random, 20f)),
            input =>
            {
                Godot.Basis basis = new Godot.Basis(input.Pose.Rotation.ToGodot()) * Godot.Basis.FromScale(input.Pose.Scale.ToGodot());
                Godot.Transform3D transform = new Godot.Transform3D(basis, input.Pose.Position.ToGodot());
                Vector3 godots = (transform * input.Point.ToGodot()).ToEvil();
                Check.Near(godots, input.Pose.ToWorld(input.Point), "to world", 1e-3f * (1f + godots.Length()));
                Check.Near((transform.AffineInverse() * input.Point.ToGodot()).ToEvil(), input.Pose.ToLocal(input.Point), "to local", 1e-3f * (1f + input.Point.Length()));
            });
    }

    [Test]
    public static void NestedObjectsMatchComposedPoses()
    {
        using Stage stage = Stage.Open();
        Property.ForAll(
            nameof(NestedObjectsMatchComposedPoses),
            random => (Outer: Gen.Placement(random, true), Middle: Gen.Placement(random, true), Inner: Gen.Placement(random, false), Point: Gen.Point3(random, 5f)),
            input =>
            {
                WorldObject outer = stage.Root.Add(new WorldObject("outer"));
                outer.Pose = input.Outer;
                WorldObject middle = outer.Add(new WorldObject("middle"));
                middle.Pose = input.Middle;
                WorldObject inner = middle.Add(new WorldObject("inner"));
                inner.Pose = input.Inner;

                Pose composed = input.Outer.ToWorld(input.Middle).ToWorld(input.Inner);
                Check.Near(composed.Position, inner.WorldPosition, "world position", 1e-3f * (1f + composed.Position.Length()));
                Check.Near(composed.ToWorld(input.Point), inner.ToWorld(input.Point), "a point inside", 1e-2f * (1f + composed.Position.Length()));
                Check.Near(composed.Forward, inner.Forward, "facing", 1e-3f);
                outer.Destroy();
            });
    }

    [Test]
    public static void BoundsMatchGodotsAabb()
    {
        Property.ForAll(
            nameof(BoundsMatchGodotsAabb),
            random => (A: Gen.Box(random, 10f), B: Gen.Box(random, 10f), Point: Gen.Point3(random, 15f)),
            input =>
            {
                Godot.Aabb a = ToAabb(input.A);
                Godot.Aabb b = ToAabb(input.B);
                Check.Equal(a.Intersects(b), input.A.Intersects(input.B), "intersects");
                Check.Equal(a.HasPoint(input.Point.ToGodot()), input.A.Contains(input.Point), "contains");
                Godot.Aabb merged = a.Merge(b);
                Bounds3 union = input.A.Union(input.B);
                Check.Near(merged.Position.ToEvil(), union.Min, "union min", 1e-4f);
                Check.Near(merged.End.ToEvil(), union.Max, "union max", 1e-4f);
            });
    }

    [Test]
    public static void RayHitsMatchGodots()
    {
        Property.ForAll(
            nameof(RayHitsMatchGodots),
            random =>
            {
                Bounds3 box = Gen.Box(random, 8f);
                Vector3 origin = Gen.Point3(random, 30f);
                Vector3 aim = Gen.Chance(random, 0.6) ? box.Center + Gen.Point3(random, 1.2f) * box.Size / 2f - origin : Gen.Direction(random);
                return (Box: box, Radius: Gen.Number(random, 0.1f, 4f), Ray: new Ray3(origin, aim.LengthSquared() > 1e-6f ? aim : Vector3.UnitX));
            },
            input =>
            {
                if (input.Box.Expand(0.01f).Contains(input.Ray.Origin) || Vector3.Distance(input.Ray.Origin, input.Box.Center) <= input.Radius + 0.01f)
                {
                    return;
                }

                Godot.Vector3 from = input.Ray.Origin.ToGodot();
                Godot.Vector3 direction = input.Ray.Direction.ToGodot();
                bool godotHitsTheBox = ToAabb(input.Box).IntersectsSegment(from, from + direction * 1000f);
                Check.Equal(godotHitsTheBox, input.Ray.Intersect(input.Box) is not null, "box hit or miss");

                Godot.Vector3[] sphere = Godot.Geometry3D.SegmentIntersectsSphere(from, from + direction * 1000f, input.Box.Center.ToGodot(), input.Radius);
                CheckSame(sphere.Length > 0 ? sphere[0].ToEvil() : null, input.Ray, input.Ray.IntersectSphere(input.Box.Center, input.Radius), "sphere");

                Godot.Plane plane = new Godot.Plane(Vector3.UnitY.ToGodot(), input.Box.Center.ToGodot());
                CheckSame(plane.IntersectsRay(from, direction)?.ToEvil(), input.Ray, input.Ray.IntersectPlane(input.Box.Center, Vector3.UnitY), "plane");
            });
    }

    [Test]
    public static void AZeroScaledShapeCannotBeClickedAndDoesNotBreakPicking()
    {
        using Stage stage = Stage.Open();
        Camera camera = new Camera();
        camera.Position = new Vector3(0f, 0f, 10f);
        stage.Root.Add(camera);
        camera.LookAt(Vector3.Zero);
        camera.MakeCurrent();
        Box popping = stage.Root.Add(new Box());
        popping.Name = "popping";
        popping.Scale = Vector3.Zero;
        Box behind = stage.Root.Add(new Box());
        behind.Name = "behind";
        behind.Position = new Vector3(0f, 0f, -4f);
        Vector2 middle = Stage.Host!.GetViewport().GetVisibleRect().Size.ToEvil() / 2f;
        Hit? hit = Picker.Pick(camera, stage.Root, middle);
        Check.True(hit is not null && ReferenceEquals(behind, hit.Target), "the click goes to the box behind");
    }

    [Test]
    public static void BrokenTransformsAreRefusedWithTheObjectsName()
    {
        WorldObject pawn = new WorldObject("pawn-red");
        Check.Throws<ArgumentException>(() => pawn.Position = new Vector3(float.NaN, 0f, 0f), "pawn-red's Position must be finite");
        Check.Throws<ArgumentException>(() => pawn.Rotation = new Vector3(0f, float.PositiveInfinity, 0f), "pawn-red's Rotation");
        Check.Throws<ArgumentException>(() => pawn.Scale = new Vector3(1f, float.NaN, 1f), "pawn-red's Scale");
        Check.Throws<ArgumentException>(() => pawn.Orientation = new Quaternion(float.NaN, 0f, 0f, 1f), "pawn-red's Orientation");
        Check.Throws<ArgumentException>(() => pawn.Orientation = new Quaternion(0f, 0f, 0f, 0f), "no length");
        Check.Near(Vector3.Zero, pawn.Position, "a refused value changes nothing");
        pawn.Scale = Vector3.Zero;
        Check.Near(Vector3.Zero, pawn.Scale, "a zero scale is allowed: things grow from nothing");
        pawn.Destroy();
    }

    private static Godot.Aabb ToAabb(Bounds3 box) => new Godot.Aabb(box.Min.ToGodot(), box.Size.ToGodot());

    private static void CheckSame(Vector3? godotsPoint, Ray3 ray, float? ourDistance, string what)
    {
        if (godotsPoint is null || ourDistance is null)
        {
            Check.Equal(godotsPoint is null, ourDistance is null, $"{what}: Godot {(godotsPoint is null ? "misses" : "hits")}, we {(ourDistance is null ? "miss" : "hit")}");
            return;
        }

        Vector3 ours = ray.PointAt(ourDistance.Value);
        Check.Near(godotsPoint.Value, ours, $"{what} hit point", 2e-3f * (1f + ourDistance.Value));
    }
}
