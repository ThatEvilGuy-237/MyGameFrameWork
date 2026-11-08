using Evil.Engine.Core;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal sealed class WaitTween(WorldObject target, float seconds) : ObjectTween(target, seconds, Easing.Linear)
{
    private protected override void Start()
    {
    }

    private protected override void Apply(float amount)
    {
    }
}
