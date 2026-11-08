using Evil.Engine.Mathematics;

namespace Evil.Engine.GodotTests.Maths;

internal static class GodotGeometry
{
    public static Godot.Aabb ToAabb(Bounds3 box) => new Godot.Aabb(box.Min.ToGodot(), box.Size.ToGodot());

    public static void CheckSame(Vector3? godotsPoint, Ray3 ray, float? ourDistance, string what)
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
