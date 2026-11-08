using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using Evil.Engine.Input;
using Evil.Engine.Scenes;
using Evil.Engine.Textures;

namespace Evil.Engine.Hosting;

public sealed class GameContext
{
    private readonly Queue<string> _captures = new();

    internal GameContext(Keyboard keyboard, Mouse mouse, TextureLibrary textures)
    {
        Keyboard = keyboard;
        Mouse = mouse;
        Textures = textures;
        Scenes = new SceneStack(this);
    }

    public Keyboard Keyboard { get; }

    public Mouse Mouse { get; }

    public TextureLibrary Textures { get; }

    public SceneStack Scenes { get; }

    public Vector2 ScreenSize { get; internal set; }

    public GameTime Time { get; internal set; }

    public FrameStats LastFrame { get; internal set; }

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

public readonly record struct FrameStats(int DrawCalls, int Triangles);
