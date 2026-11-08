namespace Evil.Engine.Screen;

public sealed record WindowSettings(
    string Title,
    int Width = 1280,
    int Height = 720,
    bool VSync = true,
    bool Resizable = true,
    bool Fullscreen = false);
