namespace Evil.Engine.Hosting;

public sealed record WindowSettings(
    string Title,
    int Width = 1280,
    int Height = 720,
    bool VSync = true,
    bool Resizable = true,
    double UpdatesPerSecond = 60);
