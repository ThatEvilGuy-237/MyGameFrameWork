using Evil.Engine.Audio;
using Evil.Engine.Core;
using Evil.Engine.Input;
using Evil.Engine.Platform;
using Evil.Engine.Screen;
using Evil.Engine.World;
using Godot;

namespace Evil.Engine.Hosting;

public abstract partial class GameMain : Node, IOverlayPainter
{
    private readonly Keyboard _keyboard = new Keyboard();
    private readonly Mouse _mouse = new Mouse();
    private GameOptions? _options;
    private OverlayNode? _overlay;
    private Canvas? _canvas;
    private GameContext? _game;
    private double _totalSeconds;
    private long _frame;
    private bool _quitAfterCapture;
    private bool _quitting;
    private int _framesUntilQuit;

    protected IReadOnlyList<string> Arguments => OS.GetCmdlineUserArgs();

    public override void _Ready()
    {
        GetTree().AutoAcceptQuit = false;
        _options = CreateGame().Options;
        WindowSetup.Apply(_options.Window, GetTree().Root);
        RenderingServer.SetDefaultClearColor(_options.ClearColor.ToGodot());
        WorldObject worlds = new WorldObject("Worlds");
        AddChild(worlds.Node);
        Node audio = new Node();
        audio.Name = "Audio";
        AddChild(audio);
        CanvasLayer layer = new CanvasLayer();
        layer.Name = "Overlay";
        layer.Layer = 1;
        AddChild(layer);
        _overlay = new OverlayNode();
        _overlay.Name = "Canvas";
        _overlay.Painter = this;
        layer.AddChild(_overlay);
        _canvas = new Canvas(_overlay.Target);
        _mouse.Place(GetViewport().GetMousePosition().ToEvil());
        _game = new GameContext(worlds, _keyboard, _mouse, new Display(_options.Window, GetTree().Root), new ContentLibrary(_options.ContentRoot), new AudioMixer(audio));
        _game.ScreenSize = ScreenSize();
        RenderingServer.FramePostDraw += OnFrameDrawn;
        _game.Scenes.Push(_options.StartScene);
        _game.Scenes.ApplyChanges();
    }

    public override void _Process(double delta)
    {
        if (_quitting)
        {
            _framesUntilQuit--;
            if (_framesUntilQuit <= 0)
            {
                GetTree().Quit();
            }

            return;
        }

        if (_game is null || _options is null)
        {
            return;
        }

        _totalSeconds += delta;
        _game.ScreenSize = ScreenSize();
        _game.Time = new GameTime((float)delta, _totalSeconds, _frame);
        _game.Scenes.Update(_game.Time);
        _game.Scenes.ApplyChanges();
        _game.Motion.Update((float)delta);
        _game.Audio.Update((float)delta);
        _keyboard.EndFrame();
        _mouse.EndFrame();
        _overlay?.QueueRedraw();
        if (_options.Snapshot is { } snapshot && snapshot.Frame == _frame)
        {
            _game.CaptureFrame(snapshot.Path);
            _quitAfterCapture = true;
        }

        _frame++;
        if (!_game.IsExiting && _game.Scenes.Count > 0)
        {
            return;
        }

        if (_game.HasPendingCaptures && _game.Scenes.Count > 0)
        {
            _quitAfterCapture = true;
            return;
        }

        Quit();
    }

    public override void _Input(InputEvent @event) => InputFeed.Feed(@event, _keyboard, _mouse);

    public override void _Notification(int what)
    {
        if (what == NotificationWMCloseRequest)
        {
            Quit();
            return;
        }

        if (what == NotificationApplicationFocusOut)
        {
            _keyboard.ReleaseAll();
            _mouse.ReleaseAll();
        }
    }

    public override void _ExitTree()
    {
        RenderingServer.FramePostDraw -= OnFrameDrawn;
        _game?.Scenes.ExitAll();
        _game?.Audio.Close();
        _game?.Content.Close();
    }

    protected abstract GameHost CreateGame();

    void IOverlayPainter.Paint(ICanvasTarget target)
    {
        if (_game is null || _canvas is null || _quitting)
        {
            return;
        }

        _canvas.Begin(ScreenSize());
        try
        {
            _game.Scenes.Draw(_canvas);
        }
        finally
        {
            _canvas.End();
        }
    }

    private void OnFrameDrawn()
    {
        if (_quitting || _game is null)
        {
            return;
        }

        while (_game.TryTakeCapture(out string? path))
        {
            FrameCapture.Save(GetViewport(), path);
        }

        if (_quitAfterCapture)
        {
            Quit();
        }
    }

    private void Quit()
    {
        if (_quitting)
        {
            return;
        }

        _quitting = true;
        _framesUntilQuit = 3;
        _game?.Audio.StopAll();
    }

    private System.Numerics.Vector2 ScreenSize() => GetViewport().GetVisibleRect().Size.ToEvil();
}
