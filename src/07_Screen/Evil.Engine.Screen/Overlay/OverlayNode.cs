using Godot;

namespace Evil.Engine.Screen;

internal sealed partial class OverlayNode : Node2D
{
    private readonly GodotCanvasTarget _target;

    public OverlayNode() => _target = new GodotCanvasTarget(this);

    public IOverlayPainter? Painter { get; set; }

    public ICanvasTarget Target => _target;

    public override void _Draw() => Painter?.Paint(_target);
}
