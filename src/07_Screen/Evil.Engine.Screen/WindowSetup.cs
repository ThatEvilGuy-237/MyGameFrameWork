using Godot;

namespace Evil.Engine.Screen;

internal static class WindowSetup
{
    public static void Apply(WindowSettings settings)
    {
        DisplayServer.WindowSetTitle(settings.Title);
        DisplayServer.WindowSetVsyncMode(settings.VSync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VSyncMode.Disabled);
        DisplayServer.WindowSetFlag(DisplayServer.WindowFlags.ResizeDisabled, !settings.Resizable);
        if (settings.Fullscreen)
        {
            DisplayServer.WindowSetMode(DisplayServer.WindowMode.Fullscreen);
            return;
        }

        DisplayServer.WindowSetMode(DisplayServer.WindowMode.Windowed);
        DisplayServer.WindowSetSize(new Vector2I(settings.Width, settings.Height));
    }
}
