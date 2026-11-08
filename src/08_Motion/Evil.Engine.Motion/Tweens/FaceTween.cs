using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal sealed class FaceTween(WorldObject target, Vector3 point, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Quaternion _from;
    private Quaternion _to;

    public static float YawToward(Vector3 from, Vector3 point)
    {
        Vector3 flat = new Vector3(point.X - from.X, 0f, point.Z - from.Z);
        return flat.LengthSquared() < 1e-10f ? float.NaN : Angle.ToDegrees(MathF.Atan2(-flat.X, -flat.Z));
    }

    private protected override void Start()
    {
        _from = Target.Orientation;
        float yaw = YawToward(Target.Position, point);
        _to = float.IsNaN(yaw) ? _from : Rotations.AroundAxis(Vector3.UnitY, yaw);
    }

    private protected override void Apply(float amount) => Target.Orientation = Quaternion.Slerp(_from, _to, amount);
}
