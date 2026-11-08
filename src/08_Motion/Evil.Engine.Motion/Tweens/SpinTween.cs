using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.Mathematics;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal sealed class SpinTween(WorldObject target, float degrees, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Quaternion _from;

    private protected override void Start() => _from = Target.Orientation;

    private protected override void Apply(float amount) =>
        Target.Orientation = Rotations.AroundAxis(Vector3.UnitY, degrees * amount) * _from;
}
