using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal sealed class BoundsMatchGodotsAabb : Property<BoxProbe>
{
    protected override BoxProbe Generate(Random random) => new BoxProbe(Gen.Box(random, 10f), Gen.Box(random, 10f), Gen.Point3(random, 15f));

    protected override void Verify(BoxProbe sample)
    {
        Godot.Aabb a = GodotGeometry.ToAabb(sample.A);
        Godot.Aabb b = GodotGeometry.ToAabb(sample.B);
        Check.Equal(a.Intersects(b), sample.A.Intersects(sample.B), "intersects");
        Check.Equal(a.HasPoint(sample.Point.ToGodot()), sample.A.Contains(sample.Point), "contains");
        Godot.Aabb merged = a.Merge(b);
        Bounds3 union = sample.A.Union(sample.B);
        Check.Near(merged.Position.ToEvil(), union.Min, "union min", 1e-4f);
        Check.Near(merged.End.ToEvil(), union.Max, "union max", 1e-4f);
    }
}
