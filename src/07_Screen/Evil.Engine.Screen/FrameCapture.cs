using Godot;

namespace Evil.Engine.Screen;

internal static class FrameCapture
{
    public static void Save(Viewport viewport, string path)
    {
        var folder = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var image = viewport.GetTexture().GetImage();
        var error = image.SavePng(path);
        if (error != Error.Ok)
        {
            throw new IOException($"Could not save the frame to '{path}': {error}.");
        }
    }
}
