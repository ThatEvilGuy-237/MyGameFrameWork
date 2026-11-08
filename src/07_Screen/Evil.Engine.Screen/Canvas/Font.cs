namespace Evil.Engine.Screen;

public sealed class Font
{
    internal Font(Godot.Font native, string name)
    {
        Native = native;
        Name = name;
    }

    public string Name { get; }

    internal Godot.Font Native { get; }
}
