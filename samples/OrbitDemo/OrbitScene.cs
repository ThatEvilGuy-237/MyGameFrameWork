namespace OrbitDemo;

internal sealed class OrbitScene : Scene
{
    private Box? _red;
    private Box? _green;
    private Box? _blue;
    private Sphere? _yellow;

    protected override void Enter()
    {
        Camera.Position = new Vector3(0f, 7f, 11f);
        Camera.FieldOfView = 50f;
        Camera.LookAt(new Vector3(0f, 0.5f, 0f));

        World.Add(new Sun(new Vector3(-0.6f, -1f, -0.4f)));
        World.Add(new Floor(new Vector2(24f, 24f), Color.FromHex("#3C5A5A")));

        _red = World.Add(new Box(new Vector3(1.4f, 1.4f, 1.4f), Color.FromHex("#E5484D")));
        _red.Position = new Vector3(-5f, 0.7f, -2f);

        _green = World.Add(new Box(new Vector3(1.2f, 2.4f, 1.2f), Color.FromHex("#30A46C")));
        _green.Position = new Vector3(5f, 1.2f, -2f);

        _blue = World.Add(new Box(new Vector3(1f, 1f, 1f), Color.FromHex("#3E63DD")));
        _blue.Position = new Vector3(0f, 1f, 1f);

        _yellow = _blue.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
        _yellow.Position = new Vector3(3f, 0f, 0f);

        var moon = _yellow.Add(new Sphere(0.22f, Color.White));
        moon.Position = new Vector3(1.2f, 0f, 0f);
    }

    protected override void Update(GameTime time)
    {
        var step = time.DeltaSeconds;
        _red!.RotateY(60f * step);
        _green!.RotateY(-90f * step);
        _blue!.RotateY(-50f * step);
        _yellow!.RotateY(160f * step);
    }
}
