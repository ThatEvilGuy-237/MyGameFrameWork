using Godot;
using GodotWindow = Godot.Window;

namespace Evil.Engine.Screen;

public sealed class Display
{
    private readonly GodotWindow? _window;
    private string _title;

    internal Display(WindowSettings settings, GodotWindow? window = null)
    {
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
