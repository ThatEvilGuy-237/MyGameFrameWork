using Evil.Engine.Core;
using Evil.Engine.World;

namespace Evil.Engine.Motion;

internal sealed class ColorTween(Material material, Color to, float seconds, Easing easing) : Tween(seconds, easing)
{
    private Color _from;

    private protected override void Start() => _from = material.Color;

    private protected override void Apply(float amount) => material.Color = Color.Lerp(_from, to, amount);
}
