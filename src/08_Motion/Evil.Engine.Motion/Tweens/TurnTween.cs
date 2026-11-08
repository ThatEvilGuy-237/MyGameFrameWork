using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal sealed class TurnTween(WorldObject target, Quaternion to, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Quaternion _from;

    private protected override void Start() => _from = Target.Orientation;

    private protected override void Apply(float amount) => Target.Orientation = Quaternion.Slerp(_from, to, amount);
}
