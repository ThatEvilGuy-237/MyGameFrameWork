using Godot;
using GodotWindow = Godot.Window;

namespace Evil.Engine.Screen;

internal static class WindowSetup
{
    public static void Apply(WindowSettings settings, GodotWindow window)
    {
        window.Title = settings.Title;
        window.Unresizable = !settings.Resizable;
        DisplayServer.WindowSetVsyncMode(settings.VSync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
        if (settings.Fullscreen)
        {
            window.Mode = GodotWindow.ModeEnum.Fullscreen;
            return;
        }

        window.Mode = GodotWindow.ModeEnum.Windowed;
        window.Size = new Vector2I(settings.Width, settings.Height);
    }
}
