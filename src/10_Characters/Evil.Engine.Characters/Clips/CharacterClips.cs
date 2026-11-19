using Godot;

namespace Evil.Engine.Characters;

public sealed class CharacterClips
{
    private readonly Dictionary<string, FaceTrack> _faces = new Dictionary<string, FaceTrack>(StringComparer.Ordinal);
    private readonly IReadOnlyDictionary<string, ClipDescription> _told;
    private readonly AnimationPlayer? _player;
    private readonly string _source;

    internal CharacterClips(AnimationPlayer? player, IReadOnlyDictionary<string, ClipDescription> told, string source)
    {
        _player = player;
        _told = told;
        _source = source;
        foreach (KeyValuePair<string, ClipDescription> clip in told)
        {
            Animation animation = Player.HasAnimation(clip.Key)
                ? Player.GetAnimation(clip.Key)
                : throw new InvalidDataException($"Character: {source} describes the clip '{clip.Key}', but its body file has no such animation.");
            animation.LoopMode = clip.Value.Loops ? Animation.LoopModeEnum.Linear : Animation.LoopModeEnum.None;
            _faces[clip.Key] = new FaceTrack(clip.Value.Faces);
        }

        if (player is not null)
        {
            player.Deterministic = true;
        }
    }

    public IReadOnlyList<string> Names => [.. _told.Keys];

    public string? Current
    {
        get
        {
            string name = _player?.CurrentAnimation.ToString() ?? string.Empty;
            return name.Length == 0 ? null : name;
        }
    }

    public float Blend { get; set; } = 0.25f;

    internal string Face => Current is string clip ? _faces[clip].At((float)Player.CurrentAnimationPosition) : FaceTrack.Neutral;

    private AnimationPlayer Player => _player ?? throw new InvalidOperationException($"Character: {_source} came without animations.");

    public bool Has(string name) => _told.ContainsKey(name);

    public float Seconds(string name) => Told(name).Seconds;

    public bool Loops(string name) => Told(name).Loops;

    public void Play(string name)
    {
        Told(name);
        Player.ClearQueue();
        Player.Play(name, Math.Max(0f, Blend));
    }

    public void Then(string name)
    {
        Told(name);
        Player.Queue(name);
    }

    private ClipDescription Told(string name) =>
        _told.TryGetValue(name, out ClipDescription? clip) ? clip : throw new ArgumentException($"{_source} has no clip named '{name}'. It has: {string.Join(", ", _told.Keys)}.", nameof(name));
}
