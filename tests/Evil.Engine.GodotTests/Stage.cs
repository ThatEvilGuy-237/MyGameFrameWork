namespace Evil.Engine.GodotTests;

internal sealed class Stage : IDisposable
{
    private Stage()
    {
        Godot.Node host = Host ?? throw new InvalidOperationException("The runner has not set Stage.Host.");
        Root = new WorldObject("Stage");
        host.AddChild(Root.Node);
    }

    public static Godot.Node? Host { get; set; }

    public static Vector2 ScreenMiddle => Host!.GetViewport().GetVisibleRect().Size.ToEvil() / 2f;

    public WorldObject Root { get; }

    public static Stage Open() => new Stage();

    public Camera AddCamera(Vector3 position)
    {
        Camera camera = new Camera();
        camera.Position = position;
        Root.Add(camera);
        camera.LookAt(Vector3.Zero);
        camera.MakeCurrent();
        return camera;
    }

    public void Dispose()
    {
        Root.Node.GetParent()?.RemoveChild(Root.Node);
        Root.Node.Free();
    }
}
