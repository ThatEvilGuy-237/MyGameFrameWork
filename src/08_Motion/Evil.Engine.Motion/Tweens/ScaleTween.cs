using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal sealed class ScaleTween(WorldObject target, Vector3 to, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Vector3 _from;

    private protected override void Start() => _from = Target.Scale;

    private protected override void Apply(float amount) => Target.Scale = Vector3.Lerp(_from, to, amount);
}
