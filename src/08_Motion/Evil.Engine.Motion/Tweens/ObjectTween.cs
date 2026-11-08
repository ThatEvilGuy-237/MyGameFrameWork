using Evil.Engine.Core;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal abstract class ObjectTween(WorldObject target, float seconds, Easing easing) : Tween(seconds, easing)
{
    protected WorldObject Target { get; } = target;

    internal override bool TargetIsGone => Target.IsDestroyed;
}
