using Godot;

namespace Evil.Engine.Screen;

internal static class FrameCapture
{
    public static void Save(Viewport viewport, string path)
    {
        string? folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        Image image = viewport.GetTexture().GetImage();
        Error error = image.SavePng(path);
        if (error != Error.Ok)
        {
            throw new IOException($"Could not save the frame to '{path}': {error}.");
        }
    }
}
