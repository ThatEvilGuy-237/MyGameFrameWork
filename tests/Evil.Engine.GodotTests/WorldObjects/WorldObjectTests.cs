namespace Evil.Engine.GodotTests.WorldObjects;

internal static class WorldObjectTests
{
    [Test]
    public static void AddPutsTheChildInsideTheParent()
    {
        WorldObject parent = new WorldObject("parent");
        WorldObject child = parent.Add(new WorldObject("child"));
        Check.True(ReferenceEquals(parent, child.Parent), "parent is set");
        Check.Equal(1, parent.Children.Count, "children");
        Check.True(ReferenceEquals(parent.Node, child.Node.GetParent()), "the Godot node sits inside the parent's node");
        parent.Node.Free();
    }

    [Test]
    public static void AnObjectHasOneParentAndNoCycles()
    {
        WorldObject first = new WorldObject("first");
        WorldObject second = new WorldObject("second");
        WorldObject child = first.Add(new WorldObject("child"));
        void AddToASecondParent() => second.Add(child);
        Check.Throws<InvalidOperationException>(AddToASecondParent, "already belongs to 'first'");
        void PutTheParentInTheChild() => child.Add(first);
        Check.Throws<InvalidOperationException>(PutTheParentInTheChild, "cannot go inside itself");
        void PutItInsideItself() => first.Add(first);
        Check.Throws<InvalidOperationException>(PutItInsideItself, "cannot go inside itself");
        first.Node.Free();
        second.Node.Free();
    }

    [Test]
    public static void RemoveTakesTheChildOut()
    {
        WorldObject parent = new WorldObject("parent");
        WorldObject child = parent.Add(new WorldObject("child"));
        parent.Remove(child);
        Check.True(child.Parent is null, "no parent");
        Check.True(child.Node.GetParent() is null, "the Godot node is out too");
        void RemoveAStranger() => parent.Remove(child);
        Check.Throws<InvalidOperationException>(RemoveAStranger, "is not inside 'parent'. Remove takes direct children only: call Remove on");
        parent.Node.Free();
        child.Node.Free();
    }

    [Test]
    public static void TransformsRoundTrip()
    {
        WorldObject thing = new WorldObject("thing");
        thing.Position = new Vector3(1f, 2f, 3f);
        thing.Rotation = new Vector3(0f, 45f, 0f);
        thing.Scale = new Vector3(2f, 2f, 2f);
        thing.Visible = false;
        Check.Near(new Vector3(1f, 2f, 3f), thing.Position, "position");
        Check.Near(new Vector3(0f, 45f, 0f), thing.Rotation, "rotation in degrees");
        Check.Near(new Vector3(2f, 2f, 2f), thing.Scale, "scale");
        Check.True(!thing.Visible, "hidden");
        Check.Equal("thing", thing.Name, "name");
        thing.Translate(new Vector3(1f, 0f, 0f));
        Check.Near(new Vector3(2f, 2f, 3f), thing.Position, "translate adds");
        thing.Node.Free();
    }

    [Test]
    public static void ChildrenFollowTheParentsTurn()
    {
        using Stage stage = Stage.Open();
        WorldObject arm = stage.Root.Add(new WorldObject("arm"));
        arm.Position = new Vector3(1f, 0f, 0f);
        arm.RotateY(90f);
        WorldObject hand = arm.Add(new WorldObject("hand"));
        hand.Position = new Vector3(2f, 0f, 0f);
        Check.Near(new Vector3(1f, 0f, -2f), hand.WorldPosition, "a quarter turn left swings +X to -Z");
    }

    [Test]
    public static void ThreeLevelsFollowEveryParent()
    {
        using Stage stage = Stage.Open();
        WorldObject sun = stage.Root.Add(new WorldObject("sun"));
        sun.RotateY(90f);
        WorldObject planet = sun.Add(new WorldObject("planet"));
        planet.Position = new Vector3(10f, 0f, 0f);
        WorldObject moon = planet.Add(new WorldObject("moon"));
        moon.Position = new Vector3(5f, 0f, 0f);
        Check.Near(new Vector3(0f, 0f, -15f), moon.WorldPosition, "the moon follows the sun's turn");
    }

    [Test]
    public static void ParentScaleMovesChildren()
    {
        using Stage stage = Stage.Open();
        WorldObject group = stage.Root.Add(new WorldObject("group"));
        group.Scale = new Vector3(2f, 2f, 2f);
        WorldObject child = group.Add(new WorldObject("child"));
        child.Position = new Vector3(1f, 0f, 0f);
        Check.Near(new Vector3(2f, 0f, 0f), child.WorldPosition, "offset doubles");
    }

    [Test]
    public static void LookAtTurnsTheFrontToTheTarget()
    {
        using Stage stage = Stage.Open();
        WorldObject eye = stage.Root.Add(new WorldObject("eye"));
        eye.LookAt(new Vector3(5f, 0f, 0f));
        Check.Near(new Vector3(1f, 0f, 0f), Forward(eye), "in the world");

        WorldObject loose = new WorldObject("loose");
        loose.LookAt(new Vector3(0f, 0f, -5f));
        Check.Near(new Vector3(0f, 0f, -1f), Forward(loose), "outside the world");
        loose.Node.Free();
    }

    [Test]
    public static void LookAtStraightUpAndKeepsScale()
    {
        using Stage stage = Stage.Open();
        WorldObject eye = stage.Root.Add(new WorldObject("eye"));
        eye.Scale = new Vector3(2f, 2f, 2f);
        eye.LookAt(new Vector3(0f, 10f, 0f));
        Check.Near(new Vector3(0f, 1f, 0f), Forward(eye), "looks up without breaking");
        Check.Near(new Vector3(2f, 2f, 2f), eye.Scale, "scale kept");
    }

    [Test]
    public static void RotateNeedsAnAxis()
    {
        WorldObject thing = new WorldObject("thing");
        void TurnAroundNothing() => thing.Rotate(Vector3.Zero, 10f);
        Check.Throws<ArgumentException>(TurnAroundNothing, "no length");
        thing.Node.Free();
    }

    private static Vector3 Forward(WorldObject thing)
    {
        Godot.Basis basis = thing.Node.IsInsideTree() ? thing.Node.GlobalTransform.Basis : thing.Node.Basis;
        return Vector3.Normalize((-basis.Z).ToEvil());
    }
}
