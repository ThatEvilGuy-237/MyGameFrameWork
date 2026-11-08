namespace OrbitDemo;

internal sealed class OrbitScene : Scene
{
    private Box? _red;
    private Box? _green;
    private Box? _blue;
    private Sphere? _yellow;
    private OrbitCamera? _view;
    private Torus? _marker;

    protected override void Enter()
    {
        Camera.FieldOfView = 50f;
        _view = new OrbitCamera(Camera, new OrbitLimits(MinDistance: 6f, MaxDistance: 30f))
        {
            Target = new Vector3(0f, 0f, 2.2f),
            Distance = 13f,
            Pitch = 32f,
            Yaw = -15f,
        };
        _view.Snap();

        Atmosphere = Atmosphere.Dusk();
        World.Add(Sun.Golden());
        World.Add(new Floor(new Vector2(24f, 24f), Color.FromHex("#3C5A5A")));

        _red = World.Add(new Box(new Vector3(1.4f, 1.4f, 1.4f), Color.FromHex("#E5484D")));
        _red.Position = new Vector3(-5f, 0.7f, -2f);

        _green = World.Add(new Box(new Vector3(1.2f, 2.4f, 1.2f), Color.FromHex("#30A46C")));
        _green.Position = new Vector3(5f, 1.2f, -2f);

        _blue = World.Add(new Box(new Vector3(1f, 1f, 1f), Color.FromHex("#3E63DD")));
        _blue.Position = new Vector3(0f, 1f, 1f);

        _yellow = _blue.Add(new Sphere(0.55f, Color.FromHex("#FFC53D")));
        _yellow.Position = new Vector3(3f, 0f, 0f);

        var ring = _yellow.Add(new Torus(0.8f, 0.95f, Color.FromHex("#E8D9A8")));
        ring.Rotation = new Vector3(20f, 0f, 10f);

        var moon = _yellow.Add(new Sphere(0.22f, Color.White) { Material = Material.Glowing(Color.FromHex("#DDEBFF"), 1.5f) });
        moon.Position = new Vector3(1.4f, 0f, 0f);

        var stone = Material.Matte(Color.FromHex("#8C8A80"));
        foreach (var x in new[] { -7f, 7f })
        {
            var pillar = World.Add(new Cylinder(0.45f, 3f, Color.White) { Material = stone });
            pillar.Position = new Vector3(x, 1.5f, -5f);
            var flame = pillar.Add(new Sphere(0.18f, Color.White) { Material = Material.Glowing(Color.FromHex("#FFB45E"), 2.5f), CastsShadow = false });
            flame.Position = new Vector3(0f, 1.75f, 0f);
            flame.Add(Lamp.Warm(7f));
        }

        var spot = World.Add(new Spotlight(Color.FromHex("#CFE3FF"), 14f, 24f) { Shadows = true, Intensity = 2f });
        spot.Position = new Vector3(0f, 9f, 3.5f);

        AddPawn(new Vector3(-2.5f, 0f, 3.5f), Color.FromHex("#E5484D"));
        AddPawn(new Vector3(2.5f, 0f, 3.5f), Color.FromHex("#3E63DD"));

        _marker = World.Add(new Torus(0.22f, 0.34f, Color.White) { Material = Material.Glowing(Color.FromHex("#FFC53D"), 1f), CastsShadow = false });
    }

    private void AddPawn(Vector3 place, Color team)
    {
        var pawn = World.Add(new Capsule(0.35f, 1.4f, team));
        pawn.Position = place + new Vector3(0f, 0.7f, 0f);
        var hat = pawn.Add(new Cone(0.3f, 0.45f, Color.FromHex("#F0F0F0")));
        hat.Position = new Vector3(0f, 0.9f, 0f);
    }

    protected override void Update(GameTime time)
    {
        var step = time.DeltaSeconds;
        _red!.RotateY(60f * step);
        _green!.RotateY(-90f * step);
        _blue!.RotateY(-50f * step);
        _yellow!.RotateY(160f * step);

        _view!.Turn(12f * step);
        _view.Update(time);
        if (Camera.PointOnGround(Game.ScreenSize / 2f) is { } ground)
        {
            _marker!.Position = ground + new Vector3(0f, 0.02f, 0f);
        }
    }
}
