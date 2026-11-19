using System.Globalization;
using Godot;

namespace Evil.Engine.Characters;

public sealed class CharacterClips
{
    private const float Slack = 0.05f;

    private readonly Dictionary<string, FaceTrack> _faces = new Dictionary<string, FaceTrack>(StringComparer.Ordinal);
    private readonly IReadOnlyDictionary<string, ClipDescription> _told;
    private readonly AnimationPlayer? _player;
    private readonly string _source;
    private string? _held;

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
            if (Math.Abs(animation.Length - clip.Value.Seconds) > Slack)
            {
                throw new InvalidDataException(string.Create(CultureInfo.InvariantCulture, $"Character: the clip '{clip.Key}' of {source} is {animation.Length:0.00} s long in its body file, its description says {clip.Value.Seconds:0.00} s."));
            }

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

    internal string Face => (Current ?? _held) is string clip ? _faces[clip].At((float)Player.CurrentAnimationPosition) : FaceTrack.Neutral;

    private AnimationPlayer Player => _player ?? throw new InvalidOperationException($"Character: {_source} came without animations.");

    public bool Has(string name) => _told.ContainsKey(name);

    public float Seconds(string name) => Told(name).Seconds;

    public bool Loops(string name) => Told(name).Loops;

    public void Play(string name) => Play(name, 0f);

    public void Play(string name, float from)
    {
        float start = Moment(name, from);
        _held = null;
        Player.ClearQueue();
        Player.Play(name, Math.Max(0f, Blend));
        Player.Seek(start, true);
    }

    public void Hold(string name, float at = 0f)
    {
        float moment = Moment(name, at);
        Player.ClearQueue();
        Player.Play(name, 0f);
        Player.Seek(moment, true);
        Player.Pause();
        _held = name;
    }

    public void Then(string name)
    {
        Told(name);
        Player.Queue(name);
    }

    private float Moment(string name, float seconds)
    {
        float length = Told(name).Seconds;
        if (!(seconds >= 0f && seconds <= length))
        {
            throw new ArgumentOutOfRangeException(nameof(seconds), seconds, $"The clip '{name}' of {_source} is {length} s long: a moment in it lies between 0 and that.");
        }

        return seconds;
    }

    private ClipDescription Told(string name) =>
        _told.TryGetValue(name, out ClipDescription? clip) ? clip : throw new ArgumentException($"{_source} has no clip named '{name}'. It has: {string.Join(", ", _told.Keys)}.", nameof(name));
}
