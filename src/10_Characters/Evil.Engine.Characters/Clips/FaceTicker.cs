using Godot;

namespace Evil.Engine.Characters;

internal sealed partial class FaceTicker : Node
{
    private readonly CharacterClips? _clips;
    private readonly CharacterLook? _look;
    private readonly IReadOnlyDictionary<string, ExpressionDescription>? _expressions;
    private string _shown = FaceTrack.Neutral;

    public FaceTicker()
    {
    }

    public FaceTicker(CharacterClips clips, CharacterLook look, IReadOnlyDictionary<string, ExpressionDescription> expressions)
    {
        _clips = clips;
        _look = look;
        _expressions = expressions;
    }

    public override void _Process(double delta) => Tick();

    public void Tick()
    {
        if (_clips is null || _look is null || _expressions is null || _clips.Face == _shown)
        {
            return;
        }

        _shown = _clips.Face;
        if (_expressions.TryGetValue(_shown, out ExpressionDescription? expression))
        {
            _look.Show(expression.Pictures);
        }
    }
}
