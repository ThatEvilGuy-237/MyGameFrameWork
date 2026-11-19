using Godot;
using GodotWindow = Godot.Window;

namespace Evil.Engine.Screen;

public sealed class Display
{
    private readonly WindowSettings _settings;
    private readonly GodotWindow? _window;
    private string _title;
    private float _sideRoom;

    internal Display(WindowSettings settings, GodotWindow? window = null)
    {
        _settings = settings;
        _title = settings.Title;
        _window = window;
    }

    public string Title
    {
        get => _title;
        set
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(value);
            _title = value;
            if (_window is not null)
            {
                _window.Title = value;
            }
        }
    }

    public bool Fullscreen
    {
        get => _window?.Mode is GodotWindow.ModeEnum.Fullscreen or GodotWindow.ModeEnum.ExclusiveFullscreen;
        set
        {
            if (_window is not null)
            {
                _window.Mode = value ? GodotWindow.ModeEnum.Fullscreen : GodotWindow.ModeEnum.Windowed;
            }
        }
    }

    public bool VSync
    {
        get => DisplayServer.WindowGetVsyncMode() != DisplayServer.VSyncMode.Disabled;
        set => DisplayServer.WindowSetVsyncMode(value ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
    }

    public float SideRoom
    {
        get => _sideRoom;
        set
        {
            if (!(value >= 0f))
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Display.SideRoom is a width of 0 or more.");
            }

            if (value > 0f && !_settings.ScalesWithWindow)
            {
                throw new InvalidOperationException("Display.SideRoom needs a window that scales its picture: WindowSettings(..., ScalesWithWindow: true).");
            }

            _sideRoom = MathF.Round(value);
            if (_window is not null && _settings.ScalesWithWindow)
            {
                _window.ContentScaleSize = new Vector2I(_settings.Width + (int)_sideRoom, _settings.Height);
            }
        }
    }

    public void Resize(int width, int height)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), $"Display.Resize needs a positive size, got {width}x{height}.");
        }

        if (_window is null)
        {
            return;
        }

        if (Fullscreen)
        {
            Fullscreen = false;
        }

        _window.Size = new Vector2I(width, height);
    }
}
