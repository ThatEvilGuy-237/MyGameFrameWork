using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.WorldObjects;

internal static class WorldDeepTests
{
    private static readonly Vector3[] Probes = [Vector3.UnitX, Vector3.UnitY, -Vector3.UnitZ, new Vector3(1f, 2f, 3f)];

    [Test]
    public static void OurEulerTurnsMatchGodots()
    {
        Vector3[] angles = [new Vector3(0f, 90f, 0f), new Vector3(30f, 0f, 0f), new Vector3(0f, 0f, 45f), new Vector3(20f, 75f, -10f), new Vector3(-60f, 130f, 35f)];
        foreach (Vector3 degrees in angles)
        {
            Quaternion ours = Rotations.FromEulerDegrees(degrees);
            Godot.Vector3 radians = new Godot.Vector3(Godot.Mathf.DegToRad(degrees.X), Godot.Mathf.DegToRad(degrees.Y), Godot.Mathf.DegToRad(degrees.Z));
            Godot.Basis godots = Godot.Basis.FromEuler(radians);
            foreach (Vector3 probe in Probes)
            {
                Check.Near((godots * probe.ToGodot()).ToEvil(), Rotations.Rotate(probe, ours), $"{probe} turned by {degrees}");
            }
        }
    }

    [Test]
    public static void OurLookAlongMatchesLookAt()
    {
        using Stage stage = Stage.Open();
        WorldObject eye = stage.Root.Add(new WorldObject("eye"));
        foreach (Vector3 target in new[] { new Vector3(5f, 0f, 0f), new Vector3(-3f, 4f, 2f), new Vector3(0f, -9f, 0f) })
        {
            eye.LookAt(target);
            Vector3 ours = Rotations.Rotate(Rotations.Forward, Rotations.LookAlong(target));
            Check.Near(ours, eye.Forward, $"looking at {target}");
        }
    }

    [Test]
    public static void OurPosesMatchTheWorld()
    {
        using Stage stage = Stage.Open();
        Pose sunPose = new Pose(new Vector3(1f, 0f, 2f), Rotations.FromEulerDegrees(new Vector3(0f, 90f, 0f)), new Vector3(2f, 2f, 2f));
        Pose planetPose = new Pose(new Vector3(3f, 1f, 0f), Rotations.FromEulerDegrees(new Vector3(10f, 40f, 0f)), Vector3.One);
        WorldObject sun = new WorldObject("sun");
        sun.Pose = sunPose;
        stage.Root.Add(sun);
        WorldObject planet = new WorldObject("planet");
        planet.Pose = planetPose;
        sun.Add(planet);
        Pose expected = sunPose.ToWorld(planetPose);
        Check.Near(expected.Position, planet.WorldPose.Position, "world position");
        Check.Near(expected.Forward, planet.WorldPose.Forward, "world facing");
        Check.Near(expected.Forward, planet.Forward, "Forward agrees");
    }

    [Test]
    public static void ToWorldAndToLocalInAndOutOfTheWorld()
    {
        WorldObject parent = new WorldObject("parent");
        parent.Position = new Vector3(10f, 0f, 0f);
        parent.RotateY(90f);
        WorldObject child = new WorldObject("child");
        child.Position = new Vector3(2f, 0f, 0f);
        parent.Add(child);
        Check.Near(new Vector3(10f, 0f, -2f), child.WorldPosition, "nested position outside the world");
        Check.Near(new Vector3(10f, 0f, -3f), child.ToWorld(new Vector3(1f, 0f, 0f)), "to world outside the world");

        using Stage stage = Stage.Open();
        stage.Root.Add(parent);
        Check.Near(new Vector3(10f, 0f, -3f), child.ToWorld(new Vector3(1f, 0f, 0f)), "to world inside the world");
        Check.Near(new Vector3(1f, 0f, 0f), child.ToLocal(new Vector3(10f, 0f, -3f)), "back to local");
    }

    [Test]
    public static void DirectionsFollowTurns()
    {
        WorldObject thing = new WorldObject("thing");
        thing.RotateY(90f);
        Check.Near(new Vector3(-1f, 0f, 0f), thing.Forward, "forward");
        Check.Near(new Vector3(0f, 0f, -1f), thing.Right, "right");
        Check.Near(Vector3.UnitY, thing.Up, "up");
        thing.Destroy();
    }

    [Test]
    public static void LookAtInsideATurnedParent()
    {
        using Stage stage = Stage.Open();
        WorldObject parent = stage.Root.Add(new WorldObject("parent"));
        parent.RotateY(90f);
        WorldObject eye = parent.Add(new WorldObject("eye"));
        eye.LookAt(new Vector3(10f, 0f, 0f));
        Check.Near(new Vector3(1f, 0f, 0f), eye.Forward, "looks at the world point, not the parent's");
    }

    [Test]
    public static void OrientationAndPoseRoundTrip()
    {
        WorldObject thing = new WorldObject("thing");
        Quaternion turn = Rotations.FromEulerDegrees(new Vector3(15f, 60f, 5f));
        thing.Orientation = turn;
        Check.Near(Rotations.Rotate(Vector3.UnitX, turn), Rotations.Rotate(Vector3.UnitX, thing.Orientation), "orientation");
        Pose pose = new Pose(new Vector3(1f, 2f, 3f), turn, new Vector3(1f, 2f, 1f));
        thing.Pose = pose;
        Check.Near(pose.Position, thing.Pose.Position, "pose position");
        Check.Near(pose.Scale, thing.Pose.Scale, "pose scale");
        Check.Near(pose.Forward, thing.Pose.Forward, "pose facing");
        thing.Destroy();
    }

    [Test]
    public static void DestroyTakesItAndItsChildrenOut()
    {
        using Stage stage = Stage.Open();
        WorldObject arm = stage.Root.Add(new WorldObject("arm"));
        WorldObject hand = arm.Add(new WorldObject("hand"));
        WorldObject finger = hand.Add(new WorldObject("finger"));
        hand.Destroy();
        Check.Equal(0, arm.Children.Count, "gone from the parent");
        Check.True(hand.IsDestroyed && finger.IsDestroyed, "it and its children are destroyed");
        Check.True(hand.Node.IsQueuedForDeletion(), "Godot frees it at the end of the frame");
        void ReadADestroyedHand() => _ = hand.Position;
        Check.Throws<ObjectDisposedException>(ReadADestroyedHand, "'hand' was destroyed: Destroy() took it out of the world for good.");
        hand.Destroy();
        void AddToADestroyedArm() => arm.Add(finger);
        Check.Throws<ObjectDisposedException>(AddToADestroyedArm);
    }

    [Test]
    public static void FindLooksNearestFirst()
    {
        WorldObject root = new WorldObject("root");
        WorldObject arm = root.Add(new WorldObject("arm"));
        WorldObject deep = arm.Add(new WorldObject("target"));
        WorldObject near = root.Add(new WorldObject("target"));
        root.Add(new Box());
        arm.Add(new Box());
        Check.True(ReferenceEquals(near, root.Find("target")), "the closer one wins");
        Check.True(ReferenceEquals(deep, arm.Find("target")), "search inside a branch");
        Check.True(root.Find("missing") is null, "nothing found");
        Check.Equal(2, new List<Box>(root.Descendants<Box>()).Count, "every box below");
        root.Destroy();
    }

    [Test]
    public static void NewShapesKeepTheirSizes()
    {
        Cylinder cylinder = new Cylinder(0.4f, 2f, Color.Gray);
        Check.Near(0.4f, cylinder.Radius, "cylinder radius");
        Check.Near(2f, cylinder.Height, "cylinder height");
        Cone cone = new Cone(0.6f, 1.5f, Color.Gray);
        Check.Near(0.6f, cone.Radius, "cone radius");
        Capsule capsule = new Capsule(0.3f, 1.2f, Color.Gray);
        Check.Near(1.2f, capsule.Height, "capsule height");
        Torus torus = new Torus(0.5f, 0.8f, Color.Gray);
        Check.Near(0.8f, torus.OuterRadius, "torus outer radius");
        void MakeAShortCapsule() => _ = new Capsule(1f, 1f, Color.Gray);
        Check.Throws<ArgumentOutOfRangeException>(MakeAShortCapsule, "twice the radius");
        void MakeAnInsideOutTorus() => _ = new Torus(1f, 0.5f, Color.Gray);
        Check.Throws<ArgumentOutOfRangeException>(MakeAnInsideOutTorus, "bigger than the inner one");
        foreach (WorldObject shape in new WorldObject[] { cylinder, cone, capsule, torus })
        {
            shape.Destroy();
        }
    }

    [Test]
    public static void MaterialsShareAndGlow()
    {
        Material stone = Material.Matte(Color.Gray);
        Box left = new Box();
        left.Material = stone;
        Box right = new Box();
        right.Material = stone;
        left.Color = Color.Red;
        Check.Equal(Color.Red, right.Color, "a shared material changes for both");

        Material metal = Material.Metal(Color.White);
        Check.Near(1f, metal.Metallic, "metal");
        Material lamp = Material.Glowing(Color.Yellow, 3f);
        Check.Equal(Color.Yellow, lamp.Glow, "glow colour");
        Check.Near(3f, lamp.GlowStrength, "glow strength");
        Check.Equal(Color.Black, new Material().Glow, "no glow by default");
        Material rough = new Material();
        rough.Roughness = 4f;
        Check.Near(1f, rough.Roughness, "roughness stays in 0..1");
        Material flat = new Material();
        flat.Unshaded = true;
        Check.True(flat.Unshaded, "unshaded");
        left.Destroy();
        right.Destroy();
    }

    [Test]
    public static void SelfOrAncestorFindsTheClickedShapeOrItsGroup()
    {
        Capsule pawn = new Capsule();
        pawn.Name = "pawn";
        Cone hat = pawn.Add(new Cone());
        Check.True(ReferenceEquals(pawn, pawn.SelfOrAncestor<Capsule>()), "the pawn itself");
        Check.True(ReferenceEquals(pawn, hat.SelfOrAncestor<Capsule>()), "from its hat");
        Check.True(pawn.Ancestor<Capsule>() is null, "Ancestor alone starts above it");
        Check.True(hat.SelfOrAncestor<Box>() is null, "nothing of that kind");
        pawn.Destroy();
    }
}
