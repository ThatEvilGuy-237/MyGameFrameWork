using System.Numerics;
using Evil.Engine.Core;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal sealed class MoveTween(WorldObject target, Vector3 to, float height, float seconds, Easing easing)
    : ObjectTween(target, seconds, easing)
{
    private Vector3 _from;

    private protected override void Start() => _from = Target.Position;

    private protected override void Apply(float amount)
    {
        float lift = height * 4f * amount * (1f - amount);
        Target.Position = Vector3.Lerp(_from, to, amount) + new Vector3(0f, lift, 0f);
    }
}
