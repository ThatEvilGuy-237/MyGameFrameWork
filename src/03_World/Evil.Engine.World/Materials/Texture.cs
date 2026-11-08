using Godot;

namespace Evil.Engine.World;

public sealed class Texture
{
    internal Texture(Texture2D native, string name)
    {
        Native = native;
        Name = name;
    }

    public string Name { get; }

    public int Width => Native.GetWidth();

    public int Height => Native.GetHeight();

    internal Texture2D Native { get; }
}
