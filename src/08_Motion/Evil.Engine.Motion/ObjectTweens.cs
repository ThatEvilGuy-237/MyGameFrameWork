using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal abstract class ObjectTween(WorldObject target, float seconds, Easing easing) : Tween(seconds, easing)
{
    protected WorldObject Target { get; } = target;

    internal override bool TargetIsGone => Target.IsDestroyed;
}

internal sealed class MoveTween(WorldObject target, Vector3 to, float height, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Vector3 _from;

    private protected override void Start() => _from = Target.Position;

    private protected override void Apply(float amount)
    {
        var lift = height * 4f * amount * (1f - amount);
        Target.Position = Vector3.Lerp(_from, to, amount) + new Vector3(0f, lift, 0f);
    }
}

internal sealed class TurnTween(WorldObject target, Quaternion to, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Quaternion _from;

    private protected override void Start() => _from = Target.Orientation;

    private protected override void Apply(float amount) => Target.Orientation = Quaternion.Slerp(_from, to, amount);
}

internal sealed class FaceTween(WorldObject target, Vector3 point, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Quaternion _from;
    private Quaternion _to;

    public static float YawToward(Vector3 from, Vector3 point)
    {
        var flat = new Vector3(point.X - from.X, 0f, point.Z - from.Z);
        return flat.LengthSquared() < 1e-10f ? float.NaN : Angle.ToDegrees(MathF.Atan2(-flat.X, -flat.Z));
    }

    private protected override void Start()
    {
        _from = Target.Orientation;
        var yaw = YawToward(Target.Position, point);
        _to = float.IsNaN(yaw) ? _from : Rotations.AroundAxis(Vector3.UnitY, yaw);
    }

    private protected override void Apply(float amount) => Target.Orientation = Quaternion.Slerp(_from, _to, amount);
}

internal sealed class SpinTween(WorldObject target, float degrees, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Quaternion _from;

    private protected override void Start() => _from = Target.Orientation;

    private protected override void Apply(float amount) =>
        Target.Orientation = Rotations.AroundAxis(Vector3.UnitY, degrees * amount) * _from;
}

internal sealed class ScaleTween(WorldObject target, Vector3 to, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Vector3 _from;

    private protected override void Start() => _from = Target.Scale;

    private protected override void Apply(float amount) => Target.Scale = Vector3.Lerp(_from, to, amount);
}

internal sealed class WaitTween(WorldObject target, float seconds) : ObjectTween(target, seconds, Easing.Linear)
{
    private protected override void Start()
    {
    }

    private protected override void Apply(float amount)
    {
    }
}

internal sealed class ColorTween(Material material, Color to, float seconds, Easing easing) : Tween(seconds, easing)
{
    private Color _from;

    private protected override void Start() => _from = material.Color;

    private protected override void Apply(float amount) => material.Color = Color.Lerp(_from, to, amount);
}
