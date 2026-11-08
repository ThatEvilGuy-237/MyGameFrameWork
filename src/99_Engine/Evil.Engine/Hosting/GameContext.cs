using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Evil.Engine.Audio;
using Evil.Engine.Core;
using Evil.Engine.Input;
using Evil.Engine.Motion;
using Evil.Engine.Scenes;
using Evil.Engine.Screen;
using Evil.Engine.World;

namespace Evil.Engine.Hosting;

public sealed class GameContext
{
    private readonly Queue<string> _captures = new Queue<string>();

    internal GameContext(WorldObject worlds)
        : this(worlds, new Keyboard(), new Mouse(), new Display(new WindowSettings("Evil Engine")), new ContentLibrary("res://"), new AudioMixer(worlds.Node))
    {
    }

    internal GameContext(WorldObject worlds, Keyboard keyboard, Mouse mouse, Display display, ContentLibrary content, AudioMixer audio)
    {
        Keyboard = keyboard;
        Mouse = mouse;
        Display = display;
        Content = content;
        Audio = audio;
        Motion = new MotionPlayer();
        Scenes = new SceneStack(this, worlds);
    }

    public SceneStack Scenes { get; }

    public Keyboard Keyboard { get; }

    public Mouse Mouse { get; }

    public Display Display { get; }

    public ContentLibrary Content { get; }

    public MotionPlayer Motion { get; }

    public AudioMixer Audio { get; }

    public Vector2 ScreenSize { get; internal set; }

    public GameTime Time { get; internal set; }

    public bool IsExiting { get; private set; }

    public void Exit() => IsExiting = true;

    public void CaptureFrame(string path)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        _captures.Enqueue(Path.GetFullPath(path));
    }

    internal bool HasPendingCaptures => _captures.Count > 0;

    internal bool TryTakeCapture([NotNullWhen(true)] out string? path) => _captures.TryDequeue(out path);
}
