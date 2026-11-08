using Evil.Engine.Core;

namespace Evil.Engine.Motion;

public sealed class ValueTween : Tween
{
    private readonly float _from;
    private readonly float _to;

    internal ValueTween(float from, float to, float seconds, Easing easing)
        : base(seconds, easing)
    {
        _from = from;
        _to = to;
        Current = from;
    }

    public float Current { get; private set; }

    private protected override void Start() => Current = _from;

    private protected override void Apply(float amount) => Current = _from + (_to - _from) * amount;
}
