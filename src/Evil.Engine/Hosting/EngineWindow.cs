using Evil.Engine.Capture;
using Evil.Engine.Graphics;
using Evil.Engine.Input;
using Evil.Engine.Textures;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using Vector2 = System.Numerics.Vector2;
using Vector2i = OpenTK.Mathematics.Vector2i;

namespace Evil.Engine.Hosting;

internal sealed class EngineWindow : GameWindow
{
    private readonly GameOptions _options;
    private readonly Keyboard _keyboard = new();
    private readonly Mouse _mouse = new();
    private BatchRenderer? _renderer;
    private Texture2D? _white;
    private TextureLibrary? _textures;
    private Canvas? _canvas;
    private GameContext? _game;
    private double _totalSeconds;
    private long _frame;
    private bool _closing;
    private bool _closeAfterRender;

    public EngineWindow(GameOptions options)
        : base(GameSettings(options.Window), NativeSettings(options.Window)) => _options = options;

    private Vector2 Screen => new(ClientSize.X, ClientSize.Y);

    protected override void OnLoad()
    {
        base.OnLoad();
        GL.Viewport(0, 0, FramebufferSize.X, FramebufferSize.Y);
        _renderer = new BatchRenderer();
        _white = Texture2D.Upload("white", 1, 1, [255, 255, 255, 255], TextureFilter.Nearest);
        _textures = new TextureLibrary(_options.ContentRoot, _options.TextureFilter);
        _canvas = new Canvas(_renderer, _white);
        _mouse.Place(new Vector2(MouseState.X, MouseState.Y));
        _game = new GameContext(_keyboard, _mouse, _textures) { ScreenSize = Screen };
        _game.Scenes.Push(_options.StartScene);
        _game.Scenes.ApplyChanges();
    }

    protected override void OnUpdateFrame(FrameEventArgs args)
    {
        base.OnUpdateFrame(args);
        if (_closing)
        {
            return;
        }

        var game = _game!;
        _totalSeconds += args.Time;
        game.ScreenSize = Screen;
        game.Time = new GameTime((float)args.Time, _totalSeconds, _frame);
        game.Scenes.Update(game.Time);
        game.Scenes.ApplyChanges();
        _keyboard.EndFrame();
        _mouse.EndFrame();
        if (_options.Snapshot is { } snapshot && snapshot.Frame == _frame)
        {
            game.CaptureFrame(snapshot.Path);
            _closeAfterRender = true;
        }

        _frame++;
        if (!game.IsExiting && game.Scenes.Count > 0)
        {
            return;
        }

        if (game.HasPendingCaptures && game.Scenes.Count > 0)
        {
            _closeAfterRender = true;
            return;
        }

        _closing = true;
        Close();
    }

    protected override void OnRenderFrame(FrameEventArgs args)
    {
        base.OnRenderFrame(args);
        if (_closing || ClientSize.X <= 0 || ClientSize.Y <= 0)
        {
            return;
        }

        var game = _game!;
        var clear = _options.ClearColor;
        GL.ClearColor(clear.R, clear.G, clear.B, clear.A);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        _renderer!.Begin(Screen);
        _canvas!.Begin(Screen);
        game.Scenes.Draw(_canvas);
        _canvas.End();
        _renderer.End();
        game.LastFrame = new FrameStats(_renderer.DrawCalls, _renderer.Triangles);
        while (game.TryTakeCapture(out var path))
        {
            FrameCapture.Save(path, FramebufferSize.X, FramebufferSize.Y);
        }

        SwapBuffers();
        if (_closeAfterRender)
        {
            _closing = true;
            Close();
        }
    }

    protected override void OnKeyDown(KeyboardKeyEventArgs e)
    {
        base.OnKeyDown(e);
        var key = (Key)(int)e.Key;
        if (Enum.IsDefined(key))
        {
            _keyboard.Press(key, e.IsRepeat);
        }
    }

    protected override void OnKeyUp(KeyboardKeyEventArgs e)
    {
        base.OnKeyUp(e);
        var key = (Key)(int)e.Key;
        if (Enum.IsDefined(key))
        {
            _keyboard.Release(key);
        }
    }

    protected override void OnMouseMove(MouseMoveEventArgs e)
    {
        base.OnMouseMove(e);
        _mouse.Move(new Vector2(e.X, e.Y));
    }

    protected override void OnMouseDown(MouseButtonEventArgs e)
    {
        base.OnMouseDown(e);
        _mouse.Press((MouseButton)(int)e.Button);
    }

    protected override void OnMouseUp(MouseButtonEventArgs e)
    {
        base.OnMouseUp(e);
        _mouse.Release((MouseButton)(int)e.Button);
    }

    protected override void OnMouseWheel(MouseWheelEventArgs e)
    {
        base.OnMouseWheel(e);
        _mouse.Wheel(e.OffsetY);
    }

    protected override void OnFramebufferResize(FramebufferResizeEventArgs e)
    {
        base.OnFramebufferResize(e);
        GL.Viewport(0, 0, e.Width, e.Height);
    }

    protected override void OnUnload()
    {
        try
        {
            _game?.Scenes.ExitAll();
        }
        finally
        {
            _textures?.Dispose();
            _white?.Dispose();
            _renderer?.Dispose();
            base.OnUnload();
        }
    }

    private static GameWindowSettings GameSettings(WindowSettings window) => new()
    {
        UpdateFrequency = window.UpdatesPerSecond,
    };

    private static NativeWindowSettings NativeSettings(WindowSettings window) => new()
    {
        Title = window.Title,
        ClientSize = new Vector2i(window.Width, window.Height),
        API = ContextAPI.OpenGL,
        APIVersion = new Version(3, 3),
        Profile = ContextProfile.Core,
        Flags = ContextFlags.ForwardCompatible,
        Vsync = window.VSync ? VSyncMode.On : VSyncMode.Off,
        WindowBorder = window.Resizable ? WindowBorder.Resizable : WindowBorder.Fixed,
    };
}
