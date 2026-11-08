using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests;

internal static class WorldDeepTests
{
    private static readonly Vector3[] Probes = [Vector3.UnitX, Vector3.UnitY, -Vector3.UnitZ, new(1f, 2f, 3f)];

    [Test]
    public static void OurEulerTurnsMatchGodots()
    {
        Vector3[] angles = [new(0f, 90f, 0f), new(30f, 0f, 0f), new(0f, 0f, 45f), new(20f, 75f, -10f), new(-60f, 130f, 35f)];
        foreach (var degrees in angles)
        {
            var ours = Rotations.FromEulerDegrees(degrees);
            var radians = new Godot.Vector3(Godot.Mathf.DegToRad(degrees.X), Godot.Mathf.DegToRad(degrees.Y), Godot.Mathf.DegToRad(degrees.Z));
            var godots = Godot.Basis.FromEuler(radians);
            foreach (var probe in Probes)
            {
                Check.Near((godots * probe.ToGodot()).ToEvil(), Rotations.Rotate(probe, ours), $"{probe} turned by {degrees}");
            }
        }
    }

    [Test]
    public static void OurLookAlongMatchesLookAt()
    {
        using var stage = Stage.Open();
        var eye = stage.Root.Add(new WorldObject("eye"));
        foreach (var target in new[] { new Vector3(5f, 0f, 0f), new Vector3(-3f, 4f, 2f), new Vector3(0f, -9f, 0f) })
        {
            eye.LookAt(target);
            var ours = Rotations.Rotate(Rotations.Forward, Rotations.LookAlong(target));
            Check.Near(ours, eye.Forward, $"looking at {target}");
        }
    }

    [Test]
    public static void OurPosesMatchTheWorld()
    {
        using var stage = Stage.Open();
        var sunPose = new Pose(new Vector3(1f, 0f, 2f), Rotations.FromEulerDegrees(new Vector3(0f, 90f, 0f)), new Vector3(2f, 2f, 2f));
        var planetPose = new Pose(new Vector3(3f, 1f, 0f), Rotations.FromEulerDegrees(new Vector3(10f, 40f, 0f)), Vector3.One);
        var sun = stage.Root.Add(new WorldObject("sun") { Pose = sunPose });
        var planet = sun.Add(new WorldObject("planet") { Pose = planetPose });
        var expected = sunPose.ToWorld(planetPose);
        Check.Near(expected.Position, planet.WorldPose.Position, "world position");
        Check.Near(expected.Forward, planet.WorldPose.Forward, "world facing");
        Check.Near(expected.Forward, planet.Forward, "Forward agrees");
    }

    [Test]
    public static void ToWorldAndToLocalInAndOutOfTheWorld()
    {
        var parent = new WorldObject("parent") { Position = new Vector3(10f, 0f, 0f) };
        parent.RotateY(90f);
        var child = parent.Add(new WorldObject("child") { Position = new Vector3(2f, 0f, 0f) });
        Check.Near(new Vector3(10f, 0f, -2f), child.WorldPosition, "nested position outside the world");
        Check.Near(new Vector3(10f, 0f, -3f), child.ToWorld(new Vector3(1f, 0f, 0f)), "to world outside the world");

        using var stage = Stage.Open();
        stage.Root.Add(parent);
        Check.Near(new Vector3(10f, 0f, -3f), child.ToWorld(new Vector3(1f, 0f, 0f)), "to world inside the world");
        Check.Near(new Vector3(1f, 0f, 0f), child.ToLocal(new Vector3(10f, 0f, -3f)), "back to local");
    }

    [Test]
    public static void DirectionsFollowTurns()
    {
        var thing = new WorldObject("thing");
        thing.RotateY(90f);
        Check.Near(new Vector3(-1f, 0f, 0f), thing.Forward, "forward");
        Check.Near(new Vector3(0f, 0f, -1f), thing.Right, "right");
        Check.Near(Vector3.UnitY, thing.Up, "up");
        thing.Destroy();
    }

    [Test]
    public static void LookAtInsideATurnedParent()
    {
        using var stage = Stage.Open();
        var parent = stage.Root.Add(new WorldObject("parent"));
        parent.RotateY(90f);
        var eye = parent.Add(new WorldObject("eye"));
        eye.LookAt(new Vector3(10f, 0f, 0f));
        Check.Near(new Vector3(1f, 0f, 0f), eye.Forward, "looks at the world point, not the parent's");
    }

    [Test]
    public static void OrientationAndPoseRoundTrip()
    {
        var thing = new WorldObject("thing");
        var turn = Rotations.FromEulerDegrees(new Vector3(15f, 60f, 5f));
        thing.Orientation = turn;
        Check.Near(Rotations.Rotate(Vector3.UnitX, turn), Rotations.Rotate(Vector3.UnitX, thing.Orientation), "orientation");
        var pose = new Pose(new Vector3(1f, 2f, 3f), turn, new Vector3(1f, 2f, 1f));
        thing.Pose = pose;
        Check.Near(pose.Position, thing.Pose.Position, "pose position");
        Check.Near(pose.Scale, thing.Pose.Scale, "pose scale");
        Check.Near(pose.Forward, thing.Pose.Forward, "pose facing");
        thing.Destroy();
    }

    [Test]
    public static void DestroyTakesItAndItsChildrenOut()
    {
        using var stage = Stage.Open();
        var arm = stage.Root.Add(new WorldObject("arm"));
        var hand = arm.Add(new WorldObject("hand"));
        var finger = hand.Add(new WorldObject("finger"));
        hand.Destroy();
        Check.Equal(0, arm.Children.Count, "gone from the parent");
        Check.True(hand.IsDestroyed && finger.IsDestroyed, "it and its children are destroyed");
        Check.True(hand.Node.IsQueuedForDeletion(), "Godot frees it at the end of the frame");
        Check.Throws<ObjectDisposedException>(() => _ = hand.Position, "'hand' was destroyed");
        hand.Destroy();
        Check.Throws<ObjectDisposedException>(() => arm.Add(finger));
    }

    [Test]
    public static void FindLooksNearestFirst()
    {
        var root = new WorldObject("root");
        var arm = root.Add(new WorldObject("arm"));
        var deep = arm.Add(new WorldObject("target"));
        var near = root.Add(new WorldObject("target"));
        root.Add(new Box());
        arm.Add(new Box());
        Check.True(ReferenceEquals(near, root.Find("target")), "the closer one wins");
        Check.True(ReferenceEquals(deep, arm.Find("target")), "search inside a branch");
        Check.True(root.Find("missing") is null, "nothing found");
        Check.Equal(2, root.Descendants<Box>().Count(), "every box below");
        root.Destroy();
    }

    [Test]
    public static void NewShapesKeepTheirSizes()
    {
        var cylinder = new Cylinder(0.4f, 2f, Color.Gray);
        Check.Near(0.4f, cylinder.Radius, "cylinder radius");
        Check.Near(2f, cylinder.Height, "cylinder height");
        var cone = new Cone(0.6f, 1.5f, Color.Gray);
        Check.Near(0.6f, cone.Radius, "cone radius");
        var capsule = new Capsule(0.3f, 1.2f, Color.Gray);
        Check.Near(1.2f, capsule.Height, "capsule height");
        var torus = new Torus(0.5f, 0.8f, Color.Gray);
        Check.Near(0.8f, torus.OuterRadius, "torus outer radius");
        Check.Throws<ArgumentOutOfRangeException>(() => _ = new Capsule(1f, 1f, Color.Gray), "twice the radius");
        Check.Throws<ArgumentOutOfRangeException>(() => _ = new Torus(1f, 0.5f, Color.Gray), "bigger than the inner one");
        foreach (var shape in new WorldObject[] { cylinder, cone, capsule, torus })
        {
            shape.Destroy();
        }
    }

    [Test]
    public static void MaterialsShareAndGlow()
    {
        var stone = Material.Matte(Color.Gray);
        var left = new Box { Material = stone };
        var right = new Box { Material = stone };
        left.Color = Color.Red;
        Check.Equal(Color.Red, right.Color, "a shared material changes for both");

        var metal = Material.Metal(Color.White);
        Check.Near(1f, metal.Metallic, "metal");
        var lamp = Material.Glowing(Color.Yellow, 3f);
        Check.Equal(Color.Yellow, lamp.Glow, "glow colour");
        Check.Near(3f, lamp.GlowStrength, "glow strength");
        Check.Equal(Color.Black, new Material().Glow, "no glow by default");
        Check.Near(1f, new Material { Roughness = 4f }.Roughness, "roughness stays in 0..1");
        Check.True(new Material { Unshaded = true }.Unshaded, "unshaded");
        left.Destroy();
        right.Destroy();
    }
}
