namespace Evil.Engine.GodotTests;

internal sealed class Stage : IDisposable
{
    private Stage()
    {
        var host = Host ?? throw new InvalidOperationException("The runner has not set Stage.Host.");
        Root = new WorldObject("Stage");
        host.AddChild(Root.Node);
    }

    public static Godot.Node? Host { get; set; }

    public WorldObject Root { get; }

    public static Stage Open() => new();

    public void Dispose()
    {
        Root.Node.GetParent()?.RemoveChild(Root.Node);
        Root.Node.Free();
    }
}
