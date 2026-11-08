using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed class NestedObjectsMatchComposedPoses : Property<NestedProbe>
{
    private Stage? _stage;

    public override void Execute()
    {
        using Stage stage = Stage.Open();
        _stage = stage;
        base.Execute();
    }

    protected override NestedProbe Generate(Random random) =>
        new NestedProbe(Gen.Placement(random, true), Gen.Placement(random, true), Gen.Placement(random, false), Gen.Point3(random, 5f));

    protected override void Verify(NestedProbe sample)
    {
        WorldObject outer = _stage!.Root.Add(new WorldObject("outer"));
        outer.Pose = sample.Outer;
        WorldObject middle = outer.Add(new WorldObject("middle"));
        middle.Pose = sample.Middle;
        WorldObject inner = middle.Add(new WorldObject("inner"));
        inner.Pose = sample.Inner;

        Pose composed = sample.Outer.ToWorld(sample.Middle).ToWorld(sample.Inner);
        Check.Near(composed.Position, inner.WorldPosition, "world position", 1e-3f * (1f + composed.Position.Length()));
        Check.Near(composed.ToWorld(sample.Point), inner.ToWorld(sample.Point), "a point inside", 1e-2f * (1f + composed.Position.Length()));
        Check.Near(composed.Forward, inner.Forward, "facing", 1e-3f);
        outer.Destroy();
    }
}
