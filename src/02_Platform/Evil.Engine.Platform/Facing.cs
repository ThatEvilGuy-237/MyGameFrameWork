using Godot;

namespace Evil.Engine.Platform;

internal static class Facing
{
    public static Basis Along(Vector3 direction)
    {
        var forward = direction.Normalized();
        var up = Mathf.Abs(forward.Dot(Vector3.Up)) > 0.999f ? Vector3.Forward : Vector3.Up;
        return Basis.LookingAt(forward, up);
    }
}
